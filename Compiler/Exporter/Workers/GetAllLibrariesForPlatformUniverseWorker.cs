using Common;
using Parser;

namespace Exporter.Workers
{
    // libraryMetadataList = Exporter::GetAllLibrariesForPlatformUniverse()
    public class GetAllLibrariesForPlatformUniverseWorker : AbstractCrayonWorker
    {
        public override CrayonWorkerResult DoWorkImpl(CrayonWorkerResult[] args)
        {
            LibraryMetadata[] allLibraries = new LibraryFinder().LibraryFlatList;
            return new CrayonWorkerResult() { Value = allLibraries };
        }
    }
}
