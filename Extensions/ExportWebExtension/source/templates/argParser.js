const argParse = args => {
    let extensionArgsByExtensionName = {};
    let crayonArgsByName = {};
    let runtimeArgs = [];
    let buildArgList = [];
    let targetFile = null;

    for (let i = 0; i < args.length; i++) {
        let arg = args[i];
        if (arg[0] == '-') {
            if (arg.startsWith("-crayon:")) {
                let parts = arg.substr("-crayon:".length).split('=', 2);
                let argName = parts[0];
                let argValue = parts.length == 1 ? "" : parts[1];
                crayonArgsByName[argName] = argValue;
            } else if (arg.startsWith("-build:")) {
                let parts = arg.substr("-build:".length).split('=', 2);
                let argName = parts[0];
                let argValue = parts.Length == 1 ? "" : parts[1];
                buildArgList.push({ name: argName, value: argValue });
            } else if (arg.startsWith("-ext:")) {
                let extArgName = null;
                let extArgValue = null;
                let parts = arg.substr("-ext:".length).split('.', 2);
                let extName = parts[0];
                if (parts.Length > 1) {
                    parts = parts[1].split('=', 2);
                    extArgName = parts[0];
                    extArgValue = parts.length == 1 ? "" : parts[1];
                }

                if (!extensionArgsByExtensionName[extName]) {
                    extensionArgsByExtensionName.push(extName, {});
                }
                extensionArgsByExtensionName[extName][extArgName] = extArgValue;
            } else {
                runtimeArgs.push(arg);
            }
        } else if (targetFile == null &&
            (arg.toLowerCase().endsWith(".build") || arg.toLowerCase().endsWith(".cbx"))) {
            targetFile = arg;
        } else {
            runtimeArgs.Add(arg);
        }
    }

    let output = { buildArgs: buildArgList };
    if (targetFile != null) {
        if (targetFile.endsWith(".cbx")) output.cbxFile = targetFile;
        else output.buildFile = targetFile;
    }

    output.runtimeArgs = runtimeArgs;

    let extensionArgList = [];
    let extensionNames = [...Object.keys(extensionArgsByExtensionName)];
    extensionNames.sort();
    for (let extension of extensionNames) {
        let extensionArgs = extensionArgsByExtensionName[extension];
        if (Object.keys(extensionArgs).length == 0) {
            extensionArgList.push({ extension });
        }
        let argNames = [...Object.keys(extensionArgs)];
        argNames.sort();
        for (let argName of argNames) {
            extensionArgList.push({ extension, name: argName, value: extensionArgs[argName] });
        }
    }
    output.extensionArgs = extensionArgList;

    let toolchainArgList = [];
    let toolchainArgNames = [...Object.keys(crayonArgsByName)];
    toolchainArgNames.sort();
    for (let toolchainArg of toolchainArgNames) {
        toolchainArgList.push({ name: toolchainArg, value: crayonArgsByName[toolchainArg] });
    }
    output.toolchainArgs = toolchainArgList;

    return output;
};
