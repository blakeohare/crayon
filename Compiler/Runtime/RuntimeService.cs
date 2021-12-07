using System.Collections.Generic;
using System.Threading.Tasks;
using Wax;
using Wax.Util.Disk;

namespace Runtime
{
    public class RuntimeService : WaxService
    {
        public RuntimeService() : base("runtime") { }

        public override Task<Dictionary<string, object>> HandleRequest(Dictionary<string, object> request)
        {
            bool realTimePrint = (bool)request["realTimePrint"];
            bool showLibStack = (bool)request["showLibStack"];
            bool useOutputPrefixes = (bool)request["useOutputPrefixes"];
            string[] args = (string[])request["args"];

            CbxBundle cbxBundle;
            Interpreter.ResourceReader resourceReader;

            if (request.ContainsKey("cbxPath"))
            {
                string cbxPath = (string)request["cbxPath"];
                string cbxFile = FileUtil.GetPlatformPath(cbxPath);
                CbxDecoder cbxDecoder = new CbxDecoder(System.IO.File.ReadAllBytes(cbxFile));
                string byteCode = cbxDecoder.ByteCode;
                string resourceManifest = cbxDecoder.ResourceManifest;
                string imageManifest = cbxDecoder.ImageManifest;
                cbxBundle = new CbxBundle()
                {
                    ByteCode = byteCode,
                    ResourceDB = new ResourceDatabase()
                    {
                        ResourceManifestFile = new FileOutput() { Type = FileOutputType.Text, TextContent = resourceManifest },
                        ImageResourceManifestFile = new FileOutput() { Type = FileOutputType.Text, TextContent = imageManifest },
                    },
                };
                List<string> resourceNames = new List<string>();
                List<FileOutput> resourceBytes = new List<FileOutput>();
                foreach (string resourceName in cbxDecoder.ResourceNames)
                {
                    resourceNames.Add("res/" + resourceName);
                    byte[] content = cbxDecoder.GetResourceBytes(resourceName);
                    resourceBytes.Add(new FileOutput() { Type = FileOutputType.Binary, BinaryContent = content });
                }
                cbxBundle.ResourceDB.ConvertToFlattenedFileData();
                cbxBundle.ResourceDB.FlatFileNames = resourceNames.ToArray();
                cbxBundle.ResourceDB.FlatFiles = resourceBytes.ToArray();
                resourceReader = new Interpreter.InMemoryResourceReader(cbxBundle.ResourceDB.FlatFileNames, cbxBundle.ResourceDB.FlatFiles);
            }
            else if (request.ContainsKey("cbxBundle"))
            {
                cbxBundle = new CbxBundle((Dictionary<string, object>)request["cbxBundle"]);
                resourceReader = new Interpreter.InMemoryResourceReader(cbxBundle.ResourceDB.FlatFileNames, cbxBundle.ResourceDB.FlatFiles);
            }
            else
            {
                throw new System.ArgumentException(); // no valid CBX to run
            }

            string extensionArgsJson = "";
            if (request.ContainsKey("extArgsJson"))
            {
                extensionArgsJson = (string)request["extArgsJson"];
            }

            return this.RunInlineVm(args, extensionArgsJson, cbxBundle, showLibStack, useOutputPrefixes, resourceReader);
        }

        private Task<Dictionary<string, object>> RunInlineVm(string[] runtimeArgs, string extensionArgsJson, CbxBundle cbxBundle, bool showLibStack, bool showOutputPrefixes, Interpreter.ResourceReader resourceReader)
        {
            string byteCode = cbxBundle.ByteCode;
            string resourceManifest = cbxBundle.ResourceDB.ResourceManifestFile.TextContent;
            string imageManifest = cbxBundle.ResourceDB.ImageResourceManifestFile.TextContent;

            Interpreter.Structs.VmContext vm = Interpreter.Vm.CrayonWrapper.createVm(byteCode, resourceManifest, imageManifest);
            Interpreter.Vm.CrayonWrapper.vmEnvSetCommandLineArgs(vm, runtimeArgs);
            Interpreter.Vm.CrayonWrapper.vmSetResourceReaderObj(vm, resourceReader);
            Interpreter.Vm.CrayonWrapper.vmSetWaxHub(vm, this.Hub);
            Interpreter.Vm.CrayonWrapper.vmSetWaxPayload(vm, extensionArgsJson);

            vm.environment.stdoutPrefix = showOutputPrefixes ? "STDOUT" : null;
            vm.environment.stacktracePrefix = showOutputPrefixes ? "STACKTRACE" : null;

            if (showLibStack)
            {
                Interpreter.Vm.CrayonWrapper.vmEnableLibStackTrace(vm);
            }
            TaskCompletionSource<Dictionary<string, object>> tcs = new TaskCompletionSource<Dictionary<string, object>>();

            Interpreter.Vm.EventLoop eventLoop = new Interpreter.Vm.EventLoop(vm, () =>
            {
                string response = Interpreter.Vm.CrayonWrapper.vmGetWaxResponse(vm) ?? "{}";
                Dictionary<string, object> taskResponse = new Dictionary<string, object>(new Wax.Util.JsonParser(response).ParseAsDictionary());
                tcs.SetResult(taskResponse);

                return true;
            });

            eventLoop.StartInterpreter();
            return tcs.Task;
        }
    }
}
