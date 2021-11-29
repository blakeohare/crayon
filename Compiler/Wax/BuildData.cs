﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Wax
{
    public class BuildData : JsonBasedObject
    {
        public BuildData() : base() { }
        public BuildData(IDictionary<string, object> data) : base(data) { }

        public CbxBundle CbxBundle { get { return this.GetObjectAsType<CbxBundle>("cbxBundle"); } set { this.SetObject("cbxBundle", value); } }
        public ExportProperties ExportProperties { get { return this.GetObjectAsType<ExportProperties>("exportProperties"); } set { this.SetObject("exportProperties", value); } }

        // TODO: move me to ExportProperties
        public bool UsesU3 { get { return this.GetBoolean("usesU3"); } set { this.SetBoolean("usesU3", value); } }

        public Error[] Errors
        {
            get { return this.GetObjectsAsType<Error>("errors"); }
            set { this.SetObjects("errors", value); }
        }
        public bool HasErrors { get { return this.HasObjects("errors"); } }
    }
}