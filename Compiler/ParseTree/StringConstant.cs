﻿using System.Collections.Generic;
using System.Text;

namespace Crayon.ParseTree
{
    internal class StringConstant : Expression, IConstantValue
    {
        internal override Expression PastelResolve(Parser parser)
        {
            return this;
        }

        public override bool CanAssignTo { get { return false; } }

        public string Value { get; private set; }
        public StringConstant(Token token, string value, Executable owner)
            : base(token, owner)
        {
            this.Value = value;
        }

        public override bool IsLiteral { get { return true; } }

        public static string ParseOutRawValue(Token stringToken)
        {
            string rawValue = stringToken.Value;
            rawValue = rawValue.Substring(1, rawValue.Length - 2);
            StringBuilder sb = new StringBuilder();
            char c;
            for (int i = 0; i < rawValue.Length; ++i)
            {
                c = rawValue[i];
                if (c == '\\')
                {
                    c = rawValue[++i];
                    switch (c)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case '0': sb.Append('\0'); break;
                        case 't': sb.Append('\t'); break;
                        case 'b': sb.Append('\b'); break;
                        case '\'': sb.Append("'"); break;
                        case '"': sb.Append("\""); break;
                        case '\\': sb.Append("\\"); break;
                        default: throw new ParserException(stringToken, "Invalid escape sequence: \\" + c);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        internal override Expression Resolve(Parser parser)
        {
            return this;
        }

        internal override Expression ResolveNames(Parser parser, Dictionary<string, Executable> lookup, string[] imports)
        {
            return this;
        }

        public Expression CloneValue(Token token, Executable owner)
        {
            return new StringConstant(token, this.Value, owner);
        }

        internal override void GetAllVariablesReferenced(HashSet<Variable> vars) { }
        internal override void PerformLocalIdAllocation(VariableIdAllocator varIds, VariableIdAllocPhase phase) { }
    }
}
