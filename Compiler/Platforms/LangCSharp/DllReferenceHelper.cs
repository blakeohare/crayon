using CommonUtil;
using System;
using System.Collections.Generic;
namespace LangCSharp
{
    public static class DllReferenceHelper
    {
        public static void AddDllReferencesToProjectBasedReplacements(
            Dictionary<string, string> replacements,
            List<DllFile> dlls)
        {
            System.Text.StringBuilder strongDllReferences = new System.Text.StringBuilder();
            System.Text.StringBuilder dllsByHint = new System.Text.StringBuilder();
            foreach (DllFile dll in dlls)
            {
                if (dll.IsStrongReference)
                {
                    if (dll.SpecificVersion) throw new NotImplementedException();

                    strongDllReferences.Append(StringUtil.JoinLines(
                        "",
                        "    <Reference Include=\"" + dll.Name + ", Version=" + dll.Version + ", Culture=" + dll.Culture + ", PublicKeyToken=" + dll.PublicKeyToken + ", processorArchitecture=" + dll.ProcessorArchitecture + "\">",
                        "      <SpecificVersion>False</SpecificVersion>",
                        "      <HintPath>.\\" + dll.HintPath + "</HintPath>",
                        "    </Reference>"));
                }
                dllsByHint.Append(StringUtil.JoinLines(
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
