﻿using System;
using System.Buffers;

namespace WebGL.WebSockets
{
    public readonly struct BufferWithLifeTime : IDisposable
    {
        private readonly ArraySegment<byte> _buffer;
        internal BufferWithLifeTime(ArraySegment<byte> buffer)
        {
            _buffer = buffer;
        }
        public void Dispose()
        {
            if (_buffer != null)
            {
                ArrayPool<byte>.Shared.Return(_buffer.Array);
            }
        }
        public static implicit operator ArraySegment<byte>(BufferWithLifeTime self)
        {
            return self._buffer;
        }
    }
    internal static class WebSocketTools
    {
        public static WebSocketCloseCode ParseCloseCodeEnum(int closeCode)
        {

            if (Enum.IsDefined(typeof(WebSocketCloseCode), closeCode))
            {
                return (WebSocketCloseCode)closeCode;
            }
            else
            {
                return WebSocketCloseCode.Undefined;
            }

        }

        public static WebSocketException GetErrorMessageFromCode(int errorCode, Exception inner)
        {
            switch (errorCode)
            {
                case -1:
                    return new WebSocketUnexpectedException("WebSocket instance not found.", inner);
                case -2:
                    return new WebSocketInvalidStateException("WebSocket is already connected or in connecting state.", inner);
                case -3:
                    return new WebSocketInvalidStateException("WebSocket is not connected.", inner);
                case -4:
                    return new WebSocketInvalidStateException("WebSocket is already closing.", inner);
                case -5:
                    return new WebSocketInvalidStateException("WebSocket is already closed.", inner);
                case -6:
                    return new WebSocketInvalidStateException("WebSocket is not in open state.", inner);
                case -7:
                    return new WebSocketInvalidArgumentException("Cannot close WebSocket. An invalid code was specified or reason is too long.", inner);
                default:
                    return new WebSocketUnexpectedException("Unknown error.", inner);
            }
        }
    }
}