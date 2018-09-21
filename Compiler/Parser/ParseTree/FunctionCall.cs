using Parser.Resolver;
using System.Collections.Generic;
using System.Linq;

namespace Parser.ParseTree
{
    public class FunctionCall : Expression
    {
        public Expression Root { get; private set; }
        public Token ParenToken { get; private set; }
        public Expression[] Args { get; private set; }

        public FunctionCall(Expression root, Token parenToken, IList<Expression> args, Node owner)
            : base(root.FirstToken, owner)
        {
            this.Root = root;
            this.ParenToken = parenToken;
            this.Args = args.ToArray();
        }

        internal override IEnumerable<Expression> Descendants { get { return new Expression[] { this.Root }.Concat(this.Args); } }

        // This will check floating point noise to see if it should be returning a round number.
        private double CalculateLogWithIntegerBase(double input, int b)
        {
            double output = System.Math.Log(input) / System.Math.Log(b);

            if (input % 1 == 0)
            {
                double intCheck = System.Math.Pow(b, output) % 1;
                if (intCheck > 0.5) intCheck = 1.0 - intCheck;
                if (intCheck < 0.000000000000001)
                {
                    return System.Math.Floor(output + .1);
                }
            }

            return output;
        }

        private const double TWO_PI = System.Math.PI * 2;

        private double GetValueOfExpr(Expression expr)
        {
            if (expr is IntegerConstant) return ((IntegerConstant)expr).Value;
            return ((FloatConstant)expr).Value;
        }

        private IConstantValue SimplifyFunctionCall(FunctionDefinition func, Expression[] args)
        {
            if (args.Length == 0) return null;

            for (int i = 0; i < args.Length; ++i)
            {
                if (!(args[i] is IConstantValue))
                {
                    return null;
                }
            }

            int iValue = 0;
            double fValue = 0.0;
            bool bValue = false;
            double output = 0.0;
            string lib = func.Assembly.CanonicalKey;
            string name = func.NameToken.Value;
            Expression winner;
            switch (lib)
            {
                case "en:Core":
                    switch (this.Args.Length + ":" + name)
                    {
                        // TODO: implement these...
                        case "1:chr":
                        case "1:ord":
                        case "1:parseFloat":
                        case "1:parseInt":
                        case "1:typeof":
                            return null;

                        case "1:isString":
                        case "1:isNumber":
                            if (this.Args[0] is StringConstant && name == "isString")
                            {
                                bValue = true;
                            }
                            else if (this.Args[0] is IntegerConstant || this.Args[0] is FloatConstant && name == "isNumber")
                            {
                                bValue = true;
                            }
                            return new BooleanConstant(this.FirstToken, bValue, this.Owner);
                    }
                    break;

                case "en:Math":
                    double[] nums = new double[this.Args.Length];
                    // Make sure we're only dealing with numbers.
                    for (int i = 0; i < this.Args.Length; ++i)
                    {
                        if (this.Args[i] is IntegerConstant) nums[i] = ((IntegerConstant)this.Args[i]).Value;
                        else if (this.Args[i] is FloatConstant) nums[i] = ((FloatConstant)this.Args[i]).Value;
                        else throw new ParserException(this.Args[i], "This is not a valid input for " + name + ".");
                    }

                    switch (this.Args.Length + ":" + name)
                    {
                        case "3:ensureRange":
                            double value = nums[0];
                            double r1 = nums[1];
                            double r2 = nums[2];
                            if (r1 > r2) // swap the values if given in wrong order
                            {
                                winner = value < r2 ? args[2] : value > r1 ? args[1] : args[0];
                            }
                            else
                            {
                                winner = value < r1 ? args[1] : value > r2 ? args[2] : args[0];
                            }
                            if (winner is IntegerConstant) return new IntegerConstant(this.FirstToken, ((IntegerConstant)winner).Value, this.Owner);
                            return new FloatConstant(this.FirstToken, ((FloatConstant)winner).Value, this.Owner);

                        case "1:abs":
                            if (args[0] is IntegerConstant)
                            {
                                iValue = ((IntegerConstant)args[0]).Value;
                                return new IntegerConstant(this.FirstToken, iValue < 0 ? -iValue : iValue, this.Owner);
                            }
                            fValue = nums[0];
                            return new FloatConstant(this.FirstToken, fValue < 0 ? -fValue : fValue, this.Owner);

                        case "2:arctan":
                        case "2:min":
                        case "2:max":
                            double arg1 = nums[0];
                            double arg2 = nums[1];

                            if (name == "arctan")
                            {
                                output = System.Math.Atan2(arg1, arg2);
                                if (output < 0) output += TWO_PI;
                                return new FloatConstant(this.FirstToken, output, this.Owner);
                            }

                            winner = (arg1 == arg2)
                                ? args[0]
                                : name == "min"
                                    ? (arg1 < arg2 ? args[0] : args[1])
                                    : (arg1 < arg2 ? args[1] : args[0]);
                            if (winner is IntegerConstant)
                            {
                                return new IntegerConstant(this.FirstToken, ((IntegerConstant)winner).Value, this.Owner);
                            }
                            return new FloatConstant(this.FirstToken, ((FloatConstant)winner).Value, this.Owner);

                        case "1:arctan":
                        case "1:arcsin":
                        case "1:arccos":
                        case "1:floor":
                        case "1:cos":
                        case "1:log2":
                        case "1:log10":
                        case "1:sign":
                        case "1:sin":
                        case "1:tan":
                        case "1:ln":
                            if (args.Length != 1) return null;
                            double input = nums[0];
                            switch (name)
                            {
                                case "floor":
                                    return new IntegerConstant(this.FirstToken, (int)System.Math.Floor(input), this.Owner);

                                case "arctan":
                                    output = System.Math.Atan(input);
                                    if (output < 0) output += TWO_PI;
                                    break;

                                case "cos": output = System.Math.Cos(input); break;
                                case "sin": output = System.Math.Sin(input); break;

                                case "arccos":
                                case "arcsin":
                                    if (input < -1 || input > 1)
                                    {
                                        throw new ParserException(
                                            this,
                                            "Cannot calculate " + name + " of a value outside of the range of -1 and 1");
                                    }
                                    output = name == "arccos"
                                        ? System.Math.Acos(input)
                                        : System.Math.Asin(input);
                                    if (output < 0) output += TWO_PI;
                                    break;

                                case "sign":
                                    return new IntegerConstant(
                                        this.FirstToken,
                                        input < 0 ? -1 : input > 0 ? 1 : 0,
                                        this.Owner);

                                case "tan":
                                    try
                                    {
                                        output = System.Math.Tan(input);
                                    }
                                    catch (System.Exception)
                                    {
                                        throw new ParserException(this, "Cannot calculate tan of this value. tan produces undefined results.");
                                    }
                                    break;

                                case "log10":
                                case "log2":
                                case "ln":
                                    if (input < 0) throw new ParserException(this, "Cannot calculate the log of a negative number.");
                                    output = name == "log10"
                                        ? CalculateLogWithIntegerBase(input, 10)
                                        : name == "log2"
                                            ? CalculateLogWithIntegerBase(input, 2)
                                            : System.Math.Log(input);
                                    break;
                            }

                            return new FloatConstant(this.FirstToken, output, this.Owner);
                    }
                    break;
            }

            return null;
        }

        internal override Expression Resolve(ParserContext parser)
        {
            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].Resolve(parser);
            }

            this.Root = this.Root.Resolve(parser);

            // TODO: this is hardcoded just for Math.floor(numeric constant). Eventually, it'd be nice
            // for a few common functions to have a compile-time codepath here.
            // e.g. Core.parseInt, Math.sin, etc.
            if (this.Root is FunctionReference)
            {
                FunctionDefinition funcDef = ((FunctionReference)this.Root).FunctionDefinition;
                IConstantValue cv = this.SimplifyFunctionCall(funcDef, this.Args);
                if (cv != null)
                {
                    return (Expression)cv;
                }
            }

            if (this.Root is SpecialEntity)
            {
                if (this.Root is SpecialEntity.EnumMaxFunction)
                {
                    int max = ((SpecialEntity.EnumMaxFunction)this.Root).GetMax();
                    return new IntegerConstant(this.Root.FirstToken, max, this.Owner);
                }

                if (this.Root is SpecialEntity.EnumValuesFunction)
                {
                    int[] rawValues = ((SpecialEntity.EnumValuesFunction)this.Root).GetValues();
                    List<Expression> values = new List<Expression>();
                    foreach (int rawValue in rawValues)
                    {
                        values.Add(new IntegerConstant(this.Root.FirstToken, rawValue, this.Owner));
                    }
                    return new ListDefinition(this.FirstToken, values, AType.Integer(this.FirstToken), this.Owner);
                }
            }

            return this;
        }

        internal override Expression ResolveEntityNames(ParserContext parser)
        {
            if (this.Root is Variable)
            {
                string varName = ((Variable)this.Root).Name;
                if (varName.StartsWith("$$$"))
                {
                    this.BatchExpressionEntityNameResolver(parser, this.Args);

                    return new CoreFunctionInvocation(this.FirstToken, this.Args, this.Owner);
                }
                else if (varName.StartsWith("$") && !varName.StartsWith("$$"))
                {
                    this.BatchExpressionEntityNameResolver(parser, this.Args);
                    return new CniFunctionInvocation(this.FirstToken, this.Args, this.Owner).ResolveEntityNames(parser);
                }
            }

            this.Root = this.Root.ResolveEntityNames(parser);
            this.BatchExpressionEntityNameResolver(parser, this.Args);

            if (this.Root is DotField ||
                this.Root is Variable ||
                this.Root is FieldReference ||
                this.Root is FunctionReference ||
                this.Root is BracketIndex ||
                this.Root is BaseMethodReference)
            {
                return this;
            }

            if (this.Root is IConstantValue)
            {
                if (this.Args.Length == 1 && this.Args[0] is OpChain)
                {
                    throw new ParserException(this.ParenToken, "Constants cannot be invoked like functions. Although it sort of looks like you're missing an op here.");
                }
                throw new ParserException(this.ParenToken, "Constants cannot be invoked like functions.");
            }

            if (this.Root is ClassReference)
            {
                throw new ParserException(this.Root, "Classes cannot be invoked like a function. If you meant to instantiate a new instance, use the 'new' keyword.");
            }

            throw new ParserException(this.ParenToken, "This cannot be invoked like a function.");
        }

        internal override Expression ResolveTypes(ParserContext parser, TypeResolver typeResolver)
        {
            this.Root = this.Root.ResolveTypes(parser, typeResolver);

            for (int i = 0; i < this.Args.Length; ++i)
            {
                this.Args[i] = this.Args[i].ResolveTypes(parser, typeResolver);
            }

            ResolvedType rootType = this.Root.ResolvedType;

            if (rootType == ResolvedType.ANY)
            {
                this.ResolvedType = ResolvedType.ANY;
                return this;
            }

            if (rootType.Category == ResolvedTypeCategory.FUNCTION_POINTER)
            {
                int maxArgCount = rootType.FunctionArgs.Length;
                int minArgCount = maxArgCount - rootType.FunctionOptionalArgCount;
                if (this.Args.Length < minArgCount || this.Args.Length > maxArgCount)
                {
                    throw new ParserException(this.ParenToken, "This function has the incorrect number of arguments.");
                }

                for (int i = 0; i < this.Args.Length; ++i)
                {
                    if (!this.Args[i].ResolvedType.CanAssignToA(rootType.FunctionArgs[i]))
                    {
                        throw new ParserException(this.Args[i], "Incorrect argument type.");
                    }
                }
                this.ResolvedType = rootType.FunctionReturnType;

                // TODO: this is temporary until the Math library is converted to Acrylic
                if (this.ResolvedType == ResolvedType.ANY &&
                    this.Root is FunctionReference &&
                    ((FunctionReference)this.Root).FunctionDefinition.CompilationScope.Metadata.ID == "Math")
                {
                    FunctionReference func = (FunctionReference)this.Root;
                    switch (func.FunctionDefinition.NameToken.Value)
                    {
                        case "abs":
                        case "min":
                        case "max":
                        case "ensureRange":
                            if (this.Args.Where(ex => ex.ResolvedType == ResolvedType.FLOAT).FirstOrDefault() != null)
                            {
                                this.ResolvedType = ResolvedType.FLOAT;
                            }
                            this.ResolvedType = ResolvedType.INTEGER;
                            break;

                        case "floor":
                        case "sign":
                            this.ResolvedType = ResolvedType.INTEGER;
                            break;

                        default:
                            this.ResolvedType = ResolvedType.FLOAT;
                            break;
                    }
                }

                if (this.ResolvedType == ResolvedType.ANY &&
                    this.CompilationScope.IsStaticallyTyped)
                {
                    // ANY types are not allowed in statically typed compilation scopes.
                    // Convert this into an object and require the user to perform any specific casts.
                    this.ResolvedType = ResolvedType.OBJECT;
                }

                return this;
            }

            throw new System.NotImplementedException();
        }

        internal override void PerformLocalIdAllocation(ParserContext parser, VariableScope varIds, VariableIdAllocPhase phase)
        {
            if ((phase & VariableIdAllocPhase.ALLOC) != 0)
            {
                this.Root.PerformLocalIdAllocation(parser, varIds, phase);
                foreach (Expression arg in this.Args)
                {
                    arg.PerformLocalIdAllocation(parser, varIds, phase);
                }
            }
        }

        internal static int CountOptionalArgs(Expression[] nullableDefaultValues)
        {
            int length = nullableDefaultValues.Length;
            if (length == 0) return 0;
            int optionalArgs = 0;
            for (int i = length - 1; i >= 0; --i)
            {
                if (nullableDefaultValues[i] == null) return optionalArgs;
                optionalArgs++;
            }
            return optionalArgs;
        }
    }
}
