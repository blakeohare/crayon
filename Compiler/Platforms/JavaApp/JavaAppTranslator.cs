using Platform;
using Pastel.Nodes;
using System.Text;

namespace JavaApp
{
    public class JavaAppTranslator : LangJava.JavaTranslator
    {
        public JavaAppTranslator(AbstractPlatform platform)
            : base(platform)
        { }

        public override void TranslateCommandLineArgs(StringBuilder sb)
        {
            sb.Append("TranslationHelper.getCommandLineArgs()");
        }

        public override void TranslatePrintStdErr(StringBuilder sb, Expression value)
        {
            sb.Append("System.err.println(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslatePrintStdOut(StringBuilder sb, Expression value)
        {
            sb.Append("System.out.println(");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateReadByteCodeFile(StringBuilder sb)
        {
            sb.Append("AwtTranslationHelper.getRawByteCodeString()");
        }

        public override void TranslateVmEndProcess(StringBuilder sb)
        {
            sb.Append("System.exit(0)");
        }
    }
}
