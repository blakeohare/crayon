using Pastel.Nodes;
using System.Text;

namespace JavaAppAndroid
{
    public class JavaAppAndroidTranslator : LangJava.JavaTranslator
    {
        public JavaAppAndroidTranslator(Platform.AbstractPlatform platform) : base(platform)
        {
        }

        public override void TranslateVmEndProcess(StringBuilder sb)
        {
            sb.Append("System.exit(0)");
        }
    }
}
