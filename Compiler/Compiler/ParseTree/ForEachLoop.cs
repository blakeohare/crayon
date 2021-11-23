using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    internal class ForEachLoop : Executable
    {
        public Token IterationVariable { get; private set; }
        public VariableId IterationVariableId { get; private set; }
        public VariableId IndexLocalId { get; private set; }
        public VariableId ListLocalId { get; private set; }
        public Expression IterationExpression { get; private set; }
        public Executable[] Code { get; private set; }
        public AType IterationType { get; private set; }
        public ResolvedType IterationResolvedType { get; private set; }

        public ForEachLoop(Token forToken, AType iterationType, Token iterationVariable, Expression iterationExpression, IList<Executable> code, Node owner)
            : base(forToken, owner)
        {
            this.IterationVariable = iterationVariable;
            this.IterationType = iterationType;
            this.IterationExpression = iterationExpression;
            this.Code = code.ToArray();
        }

        internal override IList<Executable> Resolve(ParserContext parser)
        {
            this.IterationExpression = this.IterationExpression.Resolve(parser);
            parser.ValueStackDepth += 3;
            this.Code = Resolve(parser, this.Code).ToArray();
            parser.ValueStackDepth -= 3;
            return Listify(this);
        }

        internal override void ResolveVariableOrigins(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            this.IterationExpression.ResolveVariableOrigins(parser, varIds, phase);

            if ((phase & VariableIdAllocPhase.REGISTER) != 0)
            {
                varIds.RegisterVariable(this.IterationType, this.IterationVariable.Value);
                this.IndexLocalId = varIds.RegisterSyntheticVariable(AType.Integer(this.FirstToken));
                this.ListLocalId = varIds.RegisterSyntheticVariable(AType.Any(this.FirstToken));
            }

            if (phase != VariableIdAllocPhase.REGISTER_AND_ALLOC)
            {
                foreach (Executable ex in this.Code)
                {
                    ex.ResolveVariableOrigins(parser, varIds, phase);
                }
            }
            else
            {
                foreach (Executable ex in this.Code)
                {
                    ex.ResolveVariableOrigins(parser, varIds, VariableIdAllocPhase.REGISTER);
                }

                foreach (Executable ex in this.Code)
                {
                    ex.ResolveVariableOrigins(parser, varIds, VariableIdAllocPhase.ALLOC);
                }
            }

            this.IterationVariableId = varIds.GetVarId(this.IterationVariable);
        }

        internal override Executable ResolveEntityNames(ParserContext parser)
        {
            string iterationVariableName = this.IterationVariable.Value;
            TopLevelEntity exec = this.Owner.FileScope.FileScopeEntityLookup.DoEntityLookup(iterationVariableName, this.Owner);
            if (exec != null)
            {
                throw new ParserException(this.IterationVariable, "The name '" + iterationVariableName + "' collides with an existing definition.");
            }

            this.IterationExpression = this.IterationExpression.ResolveEntityNames(parser);
            this.BatchExecutableEntityNameResolver(parser, this.Code);
            return this;
        }

        internal override void ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.IterationExpression = this.IterationExpression.ResolveTypes(parser, typeResolver);
            this.IterationResolvedType = typeResolver.ResolveType(this.IterationType);
            this.IterationVariableId.ResolvedType = this.IterationResolvedType;
            ResolvedType exprType = this.IterationExpression.ResolvedType;
            if (exprType.Category == ResolvedTypeCategory.ANY)
            {
                // This is fine.
            }
            else if (exprType.Category == ResolvedTypeCategory.LIST)
            {
                if (!exprType.ListItemType.CanAssignToA(this.IterationResolvedType))
                {
                    throw new ParserException(this.IterationExpression, "The list item type cannot be applied to the iterator variable's type.");
                }
            }
            else if (exprType.Category == ResolvedTypeCategory.STRING)
            {
                if (!ResolvedType.STRING.CanAssignToA(this.IterationResolvedType))
                {
                    throw new ParserException(this.IterationExpression, "String characters must be assigned to string types in for loops.");
                }
            }
            else
            {
                throw new ParserException(this.IterationExpression, "Cannot iterate on this type.");
            }

            foreach (Executable ex in this.Code)
            {
                ex.ResolveTypes(parser, typeResolver);
            }
        }
    }
}
