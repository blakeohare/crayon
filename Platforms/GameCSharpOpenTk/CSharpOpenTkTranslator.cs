using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pastel.Nodes;

namespace GameCSharpOpenTk
{
    public class CSharpOpenTkTranslator : LangCSharp.CSharpTranslator
    {
        public CSharpOpenTkTranslator(Platform.AbstractPlatform platform)
            : base(platform)
        {

        }

        public override void TranslateCommandLineArgs(StringBuilder sb)
        {
            sb.Append("CSharpOpenTkTranslationHelper.GetCommandLineArgs()");
        }

        public override void TranslateGetResourceManifest(StringBuilder sb)
        {
            sb.Append("TranslationHelper.TODO(\"GET_RESOURCE_MANIFEST\")");
        }

        public override void TranslateInvokeDynamicLibraryFunction(StringBuilder sb, Expression functionId, Expression argsArray)
        {
            sb.Append("TranslationHelper.TODO(\"INVOKE_DYNAMIC_LIBRARY_FUNCTION\", ");
            this.TranslateExpression(sb, functionId);
            sb.Append(", ");
            this.TranslateExpression(sb, argsArray);
            sb.Append(")");
        }

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

        public override void TranslateReadByteCodeFile(StringBuilder sb)
        {
            sb.Append("TranslationHelper.TODO(\"READ_BYTE_CODE_FILE\")");
        }

        public override void TranslateThreadSleep(StringBuilder sb, Expression seconds)
        {
            sb.Append("System.Threading.Thread.Sleep((int)(1000 * ");
            this.TranslateExpression(sb, seconds);
            sb.Append("))");
        }
    }
}
