using System.Collections.Generic;
using System.Linq;

namespace Parser
{
    internal enum VariableIdAllocPhase
    {
        REGISTER = 0x1,

        ALLOC = 0x2,

        REGISTER_AND_ALLOC = 0x3,
    }

    internal class VariableId
    {
        public VariableId(AType type, string name)
        {
            if (type == null) throw new System.Exception(); // no nulls
            this.Type = type;
            this.Name = name;
            this.UsedByClosure = false;
        }

        public override string ToString()
        {
            return (this.UsedByClosure
                ? ("Closure Var #" + this.ClosureID)
                : ("Local Var #" + this.ID))
                + ": \"" + this.Name + "\"";
        }

        public int ID { get; set; }
        public string Name { get; private set; }
        public bool UsedByClosure { get; set; }
        public int ClosureID { get; set; }
        public AType Type { get; private set; }
        public ResolvedType ResolvedType { get; set; }
    }

    internal class VariableScope
    {
        private bool requireExplicitDeclarations;

        private VariableScope parentScope = null;
        private VariableScope rootScope = null;
        private VariableScope closureScope = null;
        private VariableScope closureRootScope = null;
        private int closureIdAlloc = 1;
        private int syntheticIdAlloc = 0;

        // A lookup of all ID's that have been registered in this scope up until now.
        private readonly Dictionary<string, VariableId> idsByVar = new Dictionary<string, VariableId>();

        // The following fields are only used in the root scope.
        // flattenedIds is a lookup of all ID's of children branches. Unlike idsByVar, this cannot be used
        // to see if a variable is declared as it is possible that a variable is declared in another branch.
        private HashSet<string> usedVariableNames = null;
        // The order that variables are encountered.
        private List<VariableId> rootScopeOrder;

        // This is for when you declare a variable in, say, an if statement and also in its else statement
        // and also use the variable afterwards...
        // if (condition) {
        //   x = 1;
        // } else {
        //   x = 2;
        // }
        // print(x);
        // Previous definitions must be looked up, even though that scope was popped.
        // This dictionary gets re-instantiated in every closure scope.
        private Dictionary<string, VariableId> variableToUseIfNotFoundInDirectParent = null;

        public int Size { get { return this.rootScopeOrder.Count; } }

        private VariableScope() { }

        public VariableId[] GetAllLocals()
        {
            return rootScope.rootScopeOrder.ToArray();
        }

        public static VariableScope NewEmptyScope(bool requireExplicitDeclarations)
        {
            VariableScope scope = new VariableScope()
            {
                requireExplicitDeclarations = requireExplicitDeclarations,
                rootScopeOrder = new List<VariableId>(),
                usedVariableNames = new HashSet<string>(),
                variableToUseIfNotFoundInDirectParent = new Dictionary<string, VariableId>(),
            };
            scope.rootScope = scope;
            scope.closureRootScope = scope;
            return scope;
        }

        public static VariableScope CreatedNestedBlockScope(VariableScope parent)
        {
            return new VariableScope()
            {
                requireExplicitDeclarations = parent.requireExplicitDeclarations,
                parentScope = parent,
                rootScope = parent.rootScope,
                closureScope = parent.closureScope,
                closureRootScope = parent.closureRootScope,
                variableToUseIfNotFoundInDirectParent = parent.variableToUseIfNotFoundInDirectParent,
            };
        }

        public static VariableScope CreateClosure(VariableScope parent)
        {
            VariableScope scope = NewEmptyScope(parent.requireExplicitDeclarations);
            scope.closureScope = parent;
            scope.closureRootScope = parent.closureRootScope;
            return scope;
        }

        public void FinalizeScopeIds()
        {
            int id = 0;
            foreach (VariableId varId in this.rootScopeOrder)
            {
                if (!varId.UsedByClosure)
                {
                    varId.ID = id++;
                }
                else
                {
                    // You must still skip over the ID# even if the variable is used by a closure
                    // because arguments are always sequentially allocated ID#'s regardless of their closure status.
                    id++;
                }
            }
        }

        public VariableId[] GetClosureIds()
        {
            return this.idsByVar.Values
                .Where(vid => vid.UsedByClosure)
                .OrderBy(vid => vid.ClosureID)
                .ToArray();
        }

        private void MarkVarAsClosureVarThroughParentChain(VariableScope fromScope, VariableScope toScope, VariableId varId)
        {
            if (!varId.UsedByClosure)
            {
                varId.ClosureID = fromScope.closureRootScope.closureIdAlloc++;
                varId.UsedByClosure = true;
            }

            do
            {
                fromScope.idsByVar[varId.Name] = varId;
                fromScope = fromScope.closureScope;
            } while (fromScope != toScope);
        }

        public VariableId RegisterSyntheticVariable(AType type)
        {
            return this.RegisterVariable(type, "." + this.closureRootScope.syntheticIdAlloc++);
        }

        public VariableId RegisterVariableForcedReDeclaration(AType type, string name, Token throwToken)
        {
            if (this.idsByVar.ContainsKey(name))
            {
                throw new ParserException(throwToken, "This variable has already been used in this scope.");
            }
            VariableId varId = new VariableId(type, name);
            this.idsByVar[name] = varId;
            this.rootScope.usedVariableNames.Add(name);
            this.variableToUseIfNotFoundInDirectParent[name] = varId;
            this.rootScope.rootScopeOrder.Add(varId);
            return varId;
        }

        public VariableId RegisterVariable(AType type, string name)
        {
            return this.RegisterVariable(type, name, true);
        }

        public VariableId RegisterVariable(AType type, string name, bool allowSameScopeCollisions)
        {
            // Before anything else, check to see if this is coming from the closure.
            VariableScope closureWalker = this.closureScope;
            while (closureWalker != null)
            {
                VariableId closureVarId;
                if (closureWalker.idsByVar.TryGetValue(name, out closureVarId))
                {
                    MarkVarAsClosureVarThroughParentChain(this, closureWalker, closureVarId);
                    return closureVarId;
                }
                closureWalker = closureWalker.closureScope;
            }

            VariableId varId;

            // Check if variable is already declared in this or a parent scope already.
            if (rootScope.usedVariableNames.Contains(name))
            {
                // The above if statement is a quick check to see if variable used before, anywhere,
                // even if in a parallel branch. This will prevent many unnecessary walks up the parent chain.

                // Variable is already known by this scope. Nothing to do.
                if (this.idsByVar.TryGetValue(name, out varId))
                {
                    return varId;
                }

                if (this.variableToUseIfNotFoundInDirectParent.TryGetValue(name, out varId))
                {
                    return varId;
                }

                // Check to see if this variable was used by this or any parent scope.
                VariableScope walker = this.parentScope;
                while (walker != null)
                {
                    if (walker.idsByVar.TryGetValue(name, out varId))
                    {
                        // cache this value in the current scope to make the lookup faster in the future
                        this.idsByVar[name] = varId;
                        return varId;
                    }
                    walker = walker.parentScope;
                }

                // Do it again but using the fuzzy fallback
                walker = this;
                while (walker != null)
                {
                    if (walker.variableToUseIfNotFoundInDirectParent.ContainsKey(name))
                    {
                        varId = walker.variableToUseIfNotFoundInDirectParent[name];
                        this.idsByVar[name] = varId;
                        return varId;
                    }
                }

                // This should not happen.
#if DEBUG
                throw new System.InvalidOperationException("Couldn't find variable declaration: " + name);
#endif

            }

            // Variable has never been used anywhere. Create a new one and put it in the root bookkeeping.
            varId = new VariableId(type, name);
            this.variableToUseIfNotFoundInDirectParent.Add(name, varId);
            this.idsByVar[name] = varId;
            this.rootScope.usedVariableNames.Add(name);
            this.rootScope.rootScopeOrder.Add(varId);
            return varId;
        }

        public VariableId GetVarId(Token variableToken)
        {
            VariableScope walker;
            VariableId varId;
            string name = variableToken.Value;

            // Most common case. Nothing to do if you find it here.
            if (this.idsByVar.TryGetValue(name, out varId))
            {
                return varId;
            }

            if (this.variableToUseIfNotFoundInDirectParent.TryGetValue(name, out varId))
            {
                return varId;
            }

            // Check closures
            walker = this.closureScope;
            while (walker != null)
            {
                // Note that by the time the lambda var ID allocation begins, the containing scope
                // has already been finished and flattened, so there's no concept of parent scopes here
                // aside from the closure chain.
                if (walker.idsByVar.TryGetValue(name, out varId))
                {
                    MarkVarAsClosureVarThroughParentChain(this, walker, varId);
                    return varId;
                }
                walker = walker.closureScope;
            }

            // Check parent chain
            walker = this;
            while (walker != null)
            {
                if (walker.idsByVar.TryGetValue(name, out varId))
                {
                    return varId;
                }
                walker = walker.parentScope;
            }

            return null;
        }

        public void MergeToParent()
        {
            // variable scope closing means anything that was declared in it is gone. That name is now free again.
            if (this.requireExplicitDeclarations) return;

            foreach (VariableId v in this.idsByVar.Values)
            {
                this.parentScope.idsByVar[v.Name] = v;
            }
        }
    }
}
