# Native Tunnel

This library allows Crayon code to interop with code written directly in the platform you are exporting your project to. So, for example, if you are planning to export your project to C#, you can write extra C# code that can be invoked by Crayon.

This library only works when exporting projects to a platform. The pre-compiled runtime ignores the NativeTunnel requests.

## On the Crayon side

First your project should import the `NativeTunnel` library:

`import NativeTunnel;`

Then call the `NativeTunnel.Tunnel.send(type, arg, callback)` function. 

* `type` - this is a string. This tells the platform code what type of command you are sending. This can be anything you want (since you will be implementing the code that handles this).
* `arg` - this is also a string value. To send multiple values, using a comma-delimited format or JSON is recommended. This can also be anything you want (again, you're implementing the code that will handle this on the platform side).
* `callback` - this is a function that takes in one argument for a response value. 

Suppose you want to invoke the `window.prompt(msg)` dialog in your project and get the response from the user, but only when it's exported to the web. Your `Tunnel.send` call might look something like this:

```
NativeTunnel.Tunnel.send('show-prompt', "How are you today?", response => {
    print("Received this response from the user: " + response);
});
```

In order for the response callback to be invoked, at some point in your code, you must repeatedly invoke the `NativeTunnel.Tunnel.flushRecvQueue()` function. For games that use the `NativeTunnel`, it's probably good to put one `flushRecvQueue()` invocation once in your main game loop. For UI-based games, consider having a recurring invocation based on `timedCallback`. 

## On the platform side (C#)

When you export your project to C#, there will be a `Program.cs` file that starts off all execution. The last line of this function is `EventLoop.StartInterpreter();` Right before this line, insert the following two lines:

```
Interpreter.Vm.CrayonWrapper.PST_RegisterExtensibleCallback("nativeTunnelSend", HandleSdlRequest);
Interpreter.Vm.CrayonWrapper.PST_RegisterExtensibleCallback("nativeTunnelRecv", HandleSdlFlush);
```

`HandleSdlRequest` and `HandleSdlFlush` will be invoked when the Crayon VM invokes `send()` and `flushRecvQueue()`. However, these two functions are not defined. You can define them something like this:

```

private static int uniqueIdAllocator = 1;

private static object HandleSdlRequest(object[] request)
{
    string messageType = request[0].ToString();
    string payload = request[1].ToString();
    int id = uniqueIdAllocator++;

    HandleMessage(id, messageType, payload);

    return id;
}

private static object HandleSdlFlush(object[] dataOut)
{
    bool completedMessagesReady = ...
    if (completedMessagesReady) {
        int id = ...
        int status = ...
        string outgoingPayload = ...
        bool recurring = ...

        dataOut[0] = id;
        dataOut[1] = status;
        dataOut[2] = outgoingPayload;
        dataOut[3] = recurring;

        return true;
    } else {
        return false;
    }
}
```

It is up to you on how to implement `HandleMessage`. When you pass the ID number, you must keep track of it, along with the result that occurs. When `HandleSdlFlush` is invoked, if there are any messages that were completed, `true` should be returned and the response information should be assigned to the `dataOut` array. This is how the information is relayed back to the VM. The `dataOut` array should have four elements assigned to it:
* `dataOut[0]` - this is the ID number that you returned from `HandleSdlRequest`
* `dataOut[1]` - this is an integer indicating the status of the response. `0` means it failed, `1` means it's successful, `2` means that the message type is unknown.
* `dataOut[2]` - this is a string with the outgoing payload. This string will be received by the Crayon callback you passed to `Tunnel.send()`. 
* `dataOut[3]` - this is a boolean. If set to `true`, the NativeTunnel will keep the callback registered and expect that more responses will be sent with the same ID number. If set to `false`, the NativeTunnel will unregister the callback after it's been invoked and future responses with this ID number will be ignored. 

## On the platform side (JavaScript)

TODO: fill this in.
