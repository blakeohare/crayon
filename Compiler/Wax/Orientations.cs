using System.Collections.Generic;

namespace Wax
{
    public class Orientations : JsonBasedObject
    {
        public Orientations() : base() { }
        public Orientations(IDictionary<string, object> data) : base(data) { }

        public bool SupportsPortrait { get { return this.GetBoolean("portrait"); } set { this.SetTrueBoolean("portrait", value); } }
        public bool SupportsUpsideDown { get { return this.GetBoolean("invPortrait"); } set { this.SetTrueBoolean("invPortrait", value); } }
        public bool SupportsLandscapeLeft { get { return this.GetBoolean("landscapeLeft"); } set { this.SetTrueBoolean("landscapeLeft", value); } }
        public bool SupportsLandscapeRight { get { return this.GetBoolean("landscapeRight"); } set { this.SetTrueBoolean("landscapeRight", value); } }
    }
}
