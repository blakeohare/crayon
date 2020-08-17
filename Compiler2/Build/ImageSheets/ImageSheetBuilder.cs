﻿using System.Collections.Generic;
using System.Linq;

namespace Build.ImageSheets
{
    public class ImageSheetBuilder
    {
        public PrefixMatcher PrefixMatcher { get; set; }

        public ImageSheetBuilder()
        {
            this.PrefixMatcher = new PrefixMatcher();
        }

        public Sheet[] Generate(Build.ResourceDatabase resDB)
        {
            // PrefixMatcher has already been configured by the build file at this point.
            // This is done in the AbstractPlatform just before invoking this method.

            Dictionary<string, List<Common.FileOutput>> files = this.PrefixMatcher.MatchAndRemoveFiles(resDB);

            List<Sheet> sheets = new List<Sheet>();

            foreach (string sheetId in files.Keys.OrderBy(k => k))
            {
                Sheet sheet = new Sheet() { ID = sheetId };
                sheet.Chunks.AddRange(RectangleAllocator.Allocate(files[sheetId]
                    .OrderBy(f => f.OriginalPath.ToLowerInvariant())
                    .Select(f => new Image(f))));
                sheets.Add(sheet);
            }

            return sheets.ToArray();
        }
    }
}
