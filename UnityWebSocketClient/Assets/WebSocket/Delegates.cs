using System;

namespace WebGL.WebSockets
{
    public delegate void WebSocketOpenEventHandler();
    public delegate void WebSocketMessageEventHandler(BufferWithLifeTime buffer);
    public delegate void WebSocketErrorEventHandler(string errorMsg);
    public delegate void WebSocketCloseEventHandler(WebSocketCloseCode closeCode);
}