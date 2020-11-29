using Common;
using CommonUtil.Disk;
using System.Collections.Generic;

namespace Build
{
    public class AssemblyContext
    {
        private BuildContext buildContext;
        public AssemblyContext(BuildContext buildContext)
        {
            this.buildContext = buildContext;
        }

        // The following are things that ought to be on a assembly-specific object
        public FilePath[] SourceFolders { get; set; }
        public Dictionary<string, BuildVarCanonicalized> BuildVariableLookup { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public ProgrammingLanguage ProgrammingLanguage { get; set; }

        public Dictionary<string, string> GetCodeFiles()
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            string fileExtension = this.ProgrammingLanguage == ProgrammingLanguage.ACRYLIC
                ? ".acr"
                : ".cry";
            foreach (FilePath sourceDir in this.SourceFolders)
            {
                string[] files = FileUtil.GetAllAbsoluteFilePathsDescendentsOf(sourceDir.AbsolutePath);
                foreach (string filepath in files)
                {
                    if (filepath.ToLowerInvariant().EndsWith(fileExtension))
                    {
                        string relativePath = FileUtil.ConvertAbsolutePathToRelativePath(
                            filepath,
                            this.buildContext.ProjectDirectory);
                        output[relativePath] = FileUtil.ReadFileText(filepath);
                    }
                }
            }
            return output;
        }
    }
}
