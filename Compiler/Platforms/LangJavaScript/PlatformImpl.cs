﻿using Common;
using Platform;
using System;
using System.Collections.Generic;

namespace LangJavaScript
{
    public class PlatformImpl : AbstractPlatform
    {
        public override string Name { get { return "lang-javascript"; } }
        public override string InheritsFrom { get { return null; } }
        public override string NL { get { return "\n"; } }

        public PlatformImpl()
            : base("JAVASCRIPT")
        { }

        public override void ExportStandaloneVm(Dictionary<string, FileOutput> output)
        {
            throw new NotImplementedException();
        }

        public override void ExportProject(
            Dictionary<string, FileOutput> output,
            string byteCode,
            Build.ResourceDatabase resourceDatabase,
            Options options)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GenerateReplacementDictionary(
            Options options,
            Build.ResourceDatabase resDb)
        {
            return AbstractPlatform.GenerateGeneralReplacementsDictionary(options);
        }

        public override IDictionary<string, object> GetConstantFlags()
        {
            return new Dictionary<string, object>();
        }
    }
}
