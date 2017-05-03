using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace LangCSharp
{
    public static class DllReferenceHelper
    {
        public static void AddDllReferencesToProjectBasedReplacements(
            Dictionary<string, string> replacements, 
            List<DllFile> dlls,
            Dictionary<string, string> referenceLibraryNamesToGuids)
        {
            StringBuilder strongDllReferences = new StringBuilder();
            StringBuilder dllsByHint = new StringBuilder();
            foreach (DllFile dll in dlls)
            {
                if (dll.IsStrongReference)
                {
                    if (dll.SpecificVersion) throw new NotImplementedException();

                    strongDllReferences.Append(Util.JoinLines(
                        "",
                        "    <Reference Include=\"" + dll.Name + ", Version=" + dll.Version + ", Culture=" + dll.Culture + ", PublicKeyToken=" + dll.PublicKeyToken + ", processorArchitecture=" + dll.ProcessorArchitecture + "\">",
                        "      <SpecificVersion>False</SpecificVersion>",
                        "      <HintPath>.\\" + dll.HintPath + "</HintPath>",
                        "    </Reference>"));
                }
                dllsByHint.Append(Util.JoinLines(
                    "",
                    "    <None Include=\"" + dll.HintPath + "\">",
                    "      <CopyToOutputDirectory>Always</CopyToOutputDirectory>",
                    "    </None>"));
            }

            foreach (string libName in referenceLibraryNamesToGuids.Keys.OrderBy(n => n.ToLower()))
            {
                string libGuid = referenceLibraryNamesToGuids[libName];
                strongDllReferences.Append(Util.JoinLines(
                    "",
                    "    <ProjectReference Include=\"..\\" + libName + "\\" + libName + ".csproj\">",
                    "      <Project>{" + libGuid.ToLower() + "}</Project>",
                    "      <Name>Game</Name>",
                    "    </ProjectReference>"));
            }

            replacements["DLL_REFERENCES"] = strongDllReferences.ToString().Trim();
            replacements["DLLS_COPIED"] = dllsByHint.ToString().Trim();
        }
    }
}
