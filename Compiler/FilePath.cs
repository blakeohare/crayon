namespace Crayon
{
    // Represents a source path root in the build file that may have an alias.
    public class FilePath
    {
        private string[] pathRelativeToRoot;
        private string[] absolutePath;
        private string absolutePathString;
        private string canonicalAbsolutePath;
        private string nullableAlias; // any resources or code files that are generated from this FilePath should alias this FilePath's absolute path to this string instead.

        public FilePath(string pathRelativeToProjectRoot, string projectRootDirectory, string alias)
        {
            pathRelativeToProjectRoot = FileUtil.GetCanonicalizeUniversalPath(pathRelativeToProjectRoot);
            projectRootDirectory = FileUtil.GetCanonicalizeUniversalPath(projectRootDirectory);

            this.canonicalAbsolutePath = FileUtil.GetCanonicalizeUniversalPath(projectRootDirectory + "/" + pathRelativeToProjectRoot);
            this.absolutePathString = this.canonicalAbsolutePath;
            this.absolutePath = this.canonicalAbsolutePath.Split('/');
            this.pathRelativeToRoot = pathRelativeToProjectRoot.Split('/');
            alias = (alias ?? "").Trim();
            this.nullableAlias = alias.Length == 0 ? null : alias;
        }

        public override bool Equals(object obj)
        {
            if (obj is FilePath)
            {
                return ((FilePath)obj).canonicalAbsolutePath == this.canonicalAbsolutePath;
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

        public string GetAliasedOrRelativePathh(string absolutePath)
        {
            if (this.nullableAlias == null)
            {
                return FileUtil.ConvertAbsolutePathToRelativePath(absolutePath, this.absolutePathString);
            }

            if (absolutePath.StartsWith(this.absolutePathString))
            {
                string relativeToAliasPath = absolutePath.Substring(this.absolutePathString.Length).TrimStart('/');
                return this.nullableAlias + ":" + relativeToAliasPath;
            }

            // why was this FilePath called with this absolute path?
            // If this turns out to be a valid use case, return null instead of throwing.
            throw new System.InvalidOperationException();
        }
    }
}
