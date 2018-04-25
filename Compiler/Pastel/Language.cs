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
    }
}
