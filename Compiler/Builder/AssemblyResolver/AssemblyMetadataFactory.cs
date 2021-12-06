using Builder.Localization;
using System.Collections.Generic;
using System.Linq;
using Wax.Util.Disk;

namespace AssemblyResolver
{
    internal static class AssemblyMetadataFactory
    {
        public static InternalAssemblyMetadata CreateLibrary(string directory, string id)
        {
            Wax.Util.JsonLookup manifest;
            string manifestText = FileUtil.ReadFileText(FileUtil.JoinPath(directory, "manifest.json"));
            try
            {
                manifest = new Wax.Util.JsonLookup(new Wax.Util.JsonParser(manifestText)
                    .AddOption(Wax.Util.JsonOption.ALLOW_TRAILING_COMMA)
                    .AddOption(Wax.Util.JsonOption.ALLOW_COMMENTS)
                    .ParseAsDictionary());
            }
            catch (Wax.Util.JsonParser.JsonParserException jpe)
            {
                throw new System.InvalidOperationException("Syntax error while parsing the library manifest for '" + id + "'.", jpe);
            }

            Locale internalLocale = Locale.Get(manifest.GetAsString("localization.default", "en"));
            InternalAssemblyMetadata m = new InternalAssemblyMetadata(id, internalLocale, directory);

            IDictionary<string, object> namesByLocale = manifest.GetAsDictionary("localization.names");
            foreach (string localeId in namesByLocale.Keys)
            {
                string name = namesByLocale[localeId] as string;
                m.NameByLocale[localeId] = name ?? m.ID;
            }

            HashSet<Locale> supportedLocales = new HashSet<Locale>(manifest.GetAsDictionary("localization.names").Keys.Select(localeName => Locale.Get(localeName)));
            supportedLocales.Add(m.InternalLocale);
            m.SupportedLocales = supportedLocales.OrderBy(loc => loc.ID).ToArray();

            m.OnlyImportableFrom = new HashSet<string>(manifest.GetAsList("onlyAllowImportFrom").Cast<string>()).ToArray();

            return m;
        }
    }
}
