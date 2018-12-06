using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace LangCSharp
{
    public static class DllReferenceHelper
    {
        public static void AddDllReferencesToProjectBasedReplacements(
            Dictionary<string, string> replacements,
            List<DllFile> dlls)
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

            replacements["DLL_REFERENCES"] = strongDllReferences.ToString().Trim();
            replacements["DLLS_COPIED"] = dllsByHint.ToString().Trim();
        }
    }
}
