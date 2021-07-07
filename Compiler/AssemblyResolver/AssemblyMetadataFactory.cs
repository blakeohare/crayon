using CommonUtil.Disk;
using CommonUtil.Json;
using Common.Localization;
using System.Collections.Generic;
using System.Linq;

namespace AssemblyResolver
{
    internal static class AssemblyMetadataFactory
    {
        public static InternalAssemblyMetadata CreateLibrary(string directory, string id)
        {
            InternalAssemblyMetadata m = new InternalAssemblyMetadata();
            m.Directory = directory;
            m.ID = id;
            JsonLookup manifest;

            string manifestText = FileUtil.ReadFileText(FileUtil.JoinPath(directory, "manifest.json"));
            try
            {
                manifest = new JsonLookup(new JsonParser(manifestText)
                    .AddOption(JsonOption.ALLOW_TRAILING_COMMA)
                    .AddOption(JsonOption.ALLOW_COMMENTS)
                    .ParseAsDictionary());
            }
            catch (JsonParser.JsonParserException jpe)
            {
                throw new System.InvalidOperationException("Syntax error while parsing the library manifest for '" + id + "'.", jpe);
            }

            m.InternalLocale = Locale.Get(manifest.GetAsString("localization.default", "en"));

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
