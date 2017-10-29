using Localization;
using System.Collections.Generic;
using System.Linq;

namespace Crayon
{
    public class LibraryMetadata
    {
        public string Directory { get; private set; }
        public string Name { get; private set; }
        public IDictionary<string, object> Manifest { get; private set; }
        public Locale InternalLocale { get; private set; }
        public string CanonicalKey { get; private set; }
        public HashSet<Locale> SupportedLocales { get; private set; }
        public bool IsImportRestricted { get { return this.OnlyImportableFrom != null; } }
        public HashSet<string> OnlyImportableFrom { get; private set; }

        public LibraryMetadata(string directory, string name)
        {
            this.Directory = directory;
            this.Name = name;

            string manifestText = System.IO.File.ReadAllText(System.IO.Path.Combine(directory, "manifest.json"));
            try
            {
                this.Manifest = new Common.JsonParser(manifestText)
                    .AddOption(Common.JsonOption.ALLOW_TRAILING_COMMA)
                    .AddOption(Common.JsonOption.ALLOW_COMMENTS)
                    .ParseAsDictionary();
            }
            catch (Common.JsonParser.JsonParserException jpe)
            {
                throw new System.InvalidOperationException("Syntax error while parsing the library manifest for '" + name + "'.", jpe);
            }

            this.InternalLocale = Locale.Get("en"); Common.TODO.NotAllLibrariesAreWrittenInEnglish();
            this.CanonicalKey = this.InternalLocale.ID + ":" + this.Name;
            this.SupportedLocales = new HashSet<Locale>();
            this.SupportedLocales.Add(this.InternalLocale);

            if (this.Manifest.ContainsKey("only_allow_import_from"))
            {
                object[] libs = (object[])this.Manifest["only_allow_import_from"];
                this.OnlyImportableFrom = new HashSet<string>(libs.Select(o => o.ToString()));
            }

            switch (this.Name)
            {
                case "Game":
                case "Graphics2D":
                case "Math":
                case "Random":
                    this.SupportedLocales.Add(Locale.Get("es"));
                    break;
            }
        }

        public string GetName(Locale locale)
        {
            Common.TODO.LibraryNameLocalization();
            // TODO: This is just a test. Remove promptly.
            switch (locale.ID + ":" + this.Name)
            {
                case "es:Game": return "Juego";
                case "es:Graphics2D": return "Graficos2D";
                case "es:Math": return "Mates";
                case "es:Random": return "Aleatorio";
                default: return this.Name;
            }
        }
    }
}
