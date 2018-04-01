using Common;
using Exporter;
using Parser;
using System;
using System.Linq;

namespace Crayon
{
    internal class ShowLibraryDepsWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            CompilationBundle compilationResult = (CompilationBundle)args[0].Value;

            LibraryMetadata[] libraryMetadata = compilationResult
                .UserCodeScope
                .Dependencies
                .Select(libLocView => libLocView.LibraryScope.Library)
                .ToArray();

            string libs = LibraryDependencyResolver.GetDependencyTreeLog(libraryMetadata);
            Console.WriteLine("<LibraryDependencies>");
            Console.WriteLine(libs.Trim());
            Console.WriteLine("</LibraryDependencies>");

            return new CrayonWorkerResult();
        }
    }
}
