using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace WebGL.WebSockets
{
    ///
    /// Factory
    ///
    /// <summary>
    /// Class providing static access methods to work with JSLIB WebSocket or WebSocketSharp interface
    /// </summary>
    internal static class WebSocketFactory
    {
        /* Map of websocket instances */
        public static readonly Dictionary<Int32, WebSocket> instances = new Dictionary<Int32, WebSocket>();

        /* Delegates */
        public delegate void OnOpenCallback(int instanceId);

        public delegate void OnMessageCallback(int instanceId, IntPtr msgPtr, int msgSize);

        public delegate void OnErrorCallback(int instanceId, IntPtr errorPtr);

        public delegate void OnCloseCallback(int instanceId, int closeCode);

        /* WebSocket JSLIB callback setters and other functions */
        [DllImport("__Internal")]
        public static extern int WebSocketAllocate(string url);

        [DllImport("__Internal")]
        public static extern int WebSocketAddSubProtocol(int instanceId, string subprotocol);

        [DllImport("__Internal")]
        private static extern void WebSocketFree(int instanceId);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnOpen(OnOpenCallback callback);

        [DllImport("__Internal")]
        private static extern void WebSocketSetOnMessage(OnMessageCallback callback);

        [DllImport("__Internal")]
        private static extern void WebSocketSetOnError(OnErrorCallback callback);

        [DllImport("__Internal")]
        private static extern void WebSocketSetOnClose(OnCloseCallback callback);

        /* If callbacks was initialized and set */
        public static bool isInitialized = false;

        /*
         * Initialize WebSocket callbacks to JSLIB
         */
        public static void Initialize()
        {
            if (Application.platform != RuntimePlatform.WebGLPlayer) return;

            WebSocketSetOnOpen(DelegateOnOpenEvent);
            WebSocketSetOnMessage(DelegateOnMessageEvent);
            WebSocketSetOnError(DelegateOnErrorEvent);
            WebSocketSetOnClose(DelegateOnCloseEvent);
            isInitialized = true;
        }

        /// <summary>
        /// Called when instance is destroyed (by destructor)
        /// Method removes instance from map and free it in JSLIB implementation
        /// </summary>
        /// <param name="instanceId">Instance identifier.</param>
        internal static void HandleInstanceDestroy(int instanceId)
        {
            instances.Remove(instanceId);
            WebSocketFree(instanceId);
        }

        [MonoPInvokeCallback(typeof(OnOpenCallback))]
        internal static void DelegateOnOpenEvent(int instanceId)
        {
            if (instances.TryGetValue(instanceId, out var instanceRef))
            {
                instanceRef.DelegateOnOpenEvent();
            }
        }

        [MonoPInvokeCallback(typeof(OnMessageCallback))]
        internal static void DelegateOnMessageEvent(int instanceId, IntPtr msgPtr, int msgSize)
        {
            if (instances.TryGetValue(instanceId, out var instanceRef))
            {
                var bytes = new ArraySegment<byte>(ArrayPool<byte>.Shared.Rent(msgSize),0,msgSize);
                Marshal.Copy(msgPtr, bytes.Array, 0, msgSize);
                instanceRef.DelegateOnMessageEvent(new BufferWithLifeTime(bytes));
            }
        }

        [MonoPInvokeCallback(typeof(OnErrorCallback))]
        internal static void DelegateOnErrorEvent(int instanceId, IntPtr errorPtr)
        {
            if (instances.TryGetValue(instanceId, out var instanceRef))
            {
                string errorMsg = Marshal.PtrToStringAuto(errorPtr);
                instanceRef.DelegateOnErrorEvent(errorMsg);
            }
        }

        [MonoPInvokeCallback(typeof(OnCloseCallback))]
        internal static void DelegateOnCloseEvent(int instanceId, int closeCode)
        {
            if (instances.TryGetValue(instanceId, out var instanceRef))
            {
                instanceRef.DelegateOnCloseEvent(closeCode);
            }
        }
    }
}