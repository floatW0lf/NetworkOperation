using System;

namespace WebGL.WebSockets
{
    public delegate void WebSocketOpenEventHandler();
    public delegate void WebSocketMessageEventHandler(ArraySegment<byte> data, BufferLifeTime lifeTime);
    public delegate void WebSocketErrorEventHandler(string errorMsg);
    public delegate void WebSocketCloseEventHandler(WebSocketCloseCode closeCode);
}