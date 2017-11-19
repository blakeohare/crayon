using System;
using System.Text;
using Pastel.Nodes;

namespace JavaAppAndroid
{
    public class JavaAppAndroidTranslator : LangJava.JavaTranslator
    {
        public JavaAppAndroidTranslator(Platform.AbstractPlatform platform) : base(platform)
        {
        }

        public override void TranslateCommandLineArgs(StringBuilder sb)
        {
            sb.Append("new String[0]");
        }

        public override void TranslatePrintStdErr(StringBuilder sb, Expression value)
        {
            sb.Append("android.util.Log.e(\"STDERR\", ");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslatePrintStdOut(StringBuilder sb, Expression value)
        {
            sb.Append("android.util.Log.d(\"STDOUT\", ");
            this.TranslateExpression(sb, value);
            sb.Append(')');
        }

        public override void TranslateReadByteCodeFile(StringBuilder sb)
        {
            sb.Append("TranslationHelper.getByteCode()");
        }

        public override void TranslateVmEndProcess(StringBuilder sb)
        {
            sb.Append("System.exit(0)");
        }
    }
}
