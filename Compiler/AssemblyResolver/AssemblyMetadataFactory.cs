﻿using Localization;
using System.Collections.Generic;
using System.Linq;
using CommonUtil.Disk;
using CommonUtil.Json;

namespace AssemblyResolver
{
    public static class AssemblyMetadataFactory
    {
        public static AssemblyMetadata CreateUserDefined(Locale locale)
        {
            AssemblyMetadata m = new AssemblyMetadata();
            m.ID = ".";
            m.InternalLocale = locale;
            m.CanonicalKey = ".";
            m.SupportedLocales = new HashSet<Locale>() { locale };
            m.OnlyImportableFrom = new HashSet<string>();
            m.IsUserDefined = true;
            m.CniFunctions = new Dictionary<string, int>();
            return m;
        }

        public static AssemblyMetadata CreateLibrary(string directory, string id)
        {
            AssemblyMetadata m = new AssemblyMetadata();
            m.Directory = directory;
            m.ID = id;
            m.IsUserDefined = false;
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

            m.CanonicalKey = m.InternalLocale.ID + ":" + m.ID;
            m.SupportedLocales = new HashSet<Locale>(manifest.GetAsDictionary("localization.names").Keys.Select(localeName => Locale.Get(localeName)));
            m.SupportedLocales.Add(m.InternalLocale);
            m.OnlyImportableFrom = new HashSet<string>(manifest.GetAsList("onlyAllowImportFrom").Cast<string>());
            m.CniFunctions = new Dictionary<string, int>();
            foreach (IDictionary<string, object> cniEntry in manifest.GetAsList("cni").OfType<IDictionary<string, object>>())
            {
                if (cniEntry.ContainsKey("name") && cniEntry.ContainsKey("argc"))
                {
                    string name = cniEntry["name"].ToString();
                    int argc = (int)cniEntry["argc"];
                    m.CniFunctions[name] = argc;
                }
            }
            m.CniStartupFunction = manifest.GetAsString("cni-startup");
            return m;
        }
    }
}