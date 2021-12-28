using System;

namespace WebGL.WebSockets
{
    public delegate void WebSocketOpenEventHandler();
    public delegate void WebSocketMessageEventHandler(Span<byte> data);
    public delegate void WebSocketErrorEventHandler(string errorMsg);
    public delegate void WebSocketCloseEventHandler(WebSocketCloseCode closeCode);
}