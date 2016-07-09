namespace Crayon
{
    enum AsyncMessageType
    {
        HTTP_RESPONSE = 1, // args: [request object, status code, status message, content, headers (dict<string, string[]>)]
    }
}
