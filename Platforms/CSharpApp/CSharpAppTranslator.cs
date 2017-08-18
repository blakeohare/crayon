using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pastel.Nodes;

namespace CSharpApp
{
    public class CSharpAppTranslator : LangCSharp.CSharpTranslator
    {
        public CSharpAppTranslator(Platform.AbstractPlatform platform)
            : base(platform)
        { }

        public override void TranslatePrintStdErr(StringBuilder sb, Expression value)
        {
            sb.Append("System.Console.Error.WriteLine(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslatePrintStdOut(StringBuilder sb, Expression value)
        {
            sb.Append("System.Console.WriteLine(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateThreadSleep(StringBuilder sb, Expression seconds)
        {
            sb.Append("System.Threading.Thread.Sleep((int)(1000 * ");
            this.TranslateExpression(sb, seconds);
            sb.Append("))");
        }
    }
}
