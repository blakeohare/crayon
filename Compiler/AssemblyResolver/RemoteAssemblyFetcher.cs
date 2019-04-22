using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyResolver
{
    public enum FetchAssemblyStatus
    {
        INVALID_URL,
        NO_CONNECTION,
        SERVER_NOT_RESPONDING,
        UNKNOWN_RESPONSE,
        LIBRARY_NOT_FOUND,
        SUCCESS,
    }

    internal class RemoteAssemblyFetcher
    {
        public FetchAssemblyStatus EnsureAssemblyFetched(string url)
        {
            RemoteAssemblyUrl structuredUrl = RemoteAssemblyUrl.FromUrl(url);
            if (!structuredUrl.IsValid) return FetchAssemblyStatus.INVALID_URL;
            
            throw new NotImplementedException();
        }
        
    }
}
