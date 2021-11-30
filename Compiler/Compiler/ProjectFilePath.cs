using CommonUtil.Disk;

namespace Build
{
    // Represents a source path root in the build file that may have an alias.
    public class ProjectFilePath
    {
        private string[] pathRelativeToRoot;
        private string absolutePathString;
        private string canonicalAbsolutePath;

        public ProjectFilePath(string pathRelativeToProjectRoot, string projectRootDirectory)
        {
            pathRelativeToProjectRoot = FileUtil.GetCanonicalizeUniversalPath(pathRelativeToProjectRoot);
            projectRootDirectory = FileUtil.GetCanonicalizeUniversalPath(projectRootDirectory);

            this.canonicalAbsolutePath = FileUtil.GetCanonicalizeUniversalPath(projectRootDirectory + "/" + pathRelativeToProjectRoot);
            this.absolutePathString = this.canonicalAbsolutePath;
            this.pathRelativeToRoot = pathRelativeToProjectRoot.Split('/');
        }

        public void AddSuffix(string suffix)
        {
            this.canonicalAbsolutePath += suffix;
            this.absolutePathString += suffix;
            this.pathRelativeToRoot[this.pathRelativeToRoot.Length - 1] += suffix;
        }

        public override bool Equals(object obj)
        {
            if (obj is ProjectFilePath)
            {
                return ((ProjectFilePath)obj).canonicalAbsolutePath == this.canonicalAbsolutePath;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.canonicalAbsolutePath.GetHashCode();
        }

        public override string ToString()
        {
            return "PATH<" + string.Join("/", this.pathRelativeToRoot) + ">";
        }

        public string AbsolutePath { get { return this.absolutePathString; } }

        public string GetAliasedOrRelativePath(string absolutePath)
        {
            return FileUtil.ConvertAbsolutePathToRelativePath(absolutePath, this.absolutePathString);
        }
    }
}
