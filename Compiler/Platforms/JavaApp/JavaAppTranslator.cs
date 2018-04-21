using Platform;
using System.Text;

namespace JavaApp
{
    public class JavaAppTranslator : LangJava.JavaTranslator
    {
        public JavaAppTranslator(AbstractPlatform platform)
            : base(platform)
        { }
        
        public override void TranslateVmEndProcess(StringBuilder sb)
        {
            sb.Append("System.exit(0)");
        }
    }
}
