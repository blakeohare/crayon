using Localization;
using System.Collections.Generic;

namespace Crayon
{
    public class LibraryMetadata
    {
        public string Directory { get; private set; }
        public string Name { get; private set; }
        public string Manifest { get; private set; }
        public Locale InternalLocale { get; private set; }
        public string CanonicalKey { get; private set; }
        public HashSet<Locale> SupportedLocales { get; private set; }

        public LibraryMetadata(string directory, string name)
        {
            this.Directory = directory;
            this.Name = name;
            this.Manifest = System.IO.File.ReadAllText(System.IO.Path.Combine(directory, "manifest.txt"));
            this.InternalLocale = Locale.Get("en"); Common.TODO.NotAllLibrariesAreWrittenInEnglish();
            this.CanonicalKey = this.InternalLocale.ID + ":" + this.Name;
            this.SupportedLocales = new HashSet<Locale>();
            this.SupportedLocales.Add(this.InternalLocale);

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
