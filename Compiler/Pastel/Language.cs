using Pastel.Transpilers;
using System.Collections.Generic;

namespace Pastel
{
    public enum Language
    {
        C,
        CSHARP,
        JAVA,
        JAVA6,
        JAVASCRIPT,
        PYTHON,
    }

    internal static class LanguageUtil
    {
        private static readonly Dictionary<Language, AbstractTranslator> singletons = new Dictionary<Language, AbstractTranslator>();

        internal static AbstractTranslator GetTranspiler(Language language)
        {
            if (singletons.ContainsKey(language))
            {
                return singletons[language];
            }

            AbstractTranslator t;
            switch (language)
            {
                case Language.C: t = new CTranslator(); break;
                case Language.CSHARP: t = new CSharpTranslator(); break;
                case Language.JAVA: t = new JavaTranslator(false); break;
                case Language.JAVA6: t = new JavaTranslator(true); break;
                case Language.JAVASCRIPT: t = new JavaScriptTranslator(); break;
                case Language.PYTHON: t = new PythonTranslator(); break;
                default: throw new System.Exception();
            }
            singletons[language] = t;
            return t;
        }

        internal static Dictionary<string, object> GetLanguageConstants(Language lang)
        {
            Dictionary<string, object> output = new Dictionary<string, object>();

            output["ARRAY_IS_LIST"] = lang == Language.PYTHON || lang == Language.JAVASCRIPT;
            output["HAS_INCREMENT"] = lang != Language.PYTHON;
            output["INT_IS_FLOOR"] = lang == Language.JAVASCRIPT || lang == Language.C;
            output["IS_C"] = lang == Language.C;
            output["IS_CHAR_A_NUMBER"] = lang == Language.C || lang == Language.CSHARP || lang == Language.JAVA || lang == Language.JAVA6;
            output["IS_JAVASCRIPT"] = lang == Language.JAVASCRIPT;
            output["IS_PYTHON"] = lang == Language.PYTHON;
            output["PLATFORM_SUPPORTS_LIST_CLEAR"] = lang != Language.PYTHON;
            output["STRONGLY_TYPED"] = lang == Language.C || lang == Language.CSHARP || lang == Language.JAVA || lang == Language.JAVA6;

            return output;
        }
    }
}
