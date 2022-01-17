using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WebGL.WebSockets
{ /// <summary>
  /// WebSocket class bound to JSLIB.
  /// </summary>
  public class WebSocket {

    /* WebSocket JSLIB functions */
    [DllImport ("__Internal")]
    private static extern int WebSocketConnect (int instanceId);

    [DllImport ("__Internal")]
    private static extern int WebSocketClose (int instanceId, int code, string reason);

    [DllImport ("__Internal")]
    private static extern int WebSocketSend (int instanceId, byte[] dataPtr, int dataLength);

    [DllImport ("__Internal")]
    private static extern int WebSocketSendText (int instanceId, string message);

    [DllImport ("__Internal")]
    private static extern int WebSocketGetState (int instanceId);

    private int instanceId;

    public event WebSocketOpenEventHandler OnOpen;
    public event WebSocketMessageEventHandler OnMessage;
    public event WebSocketErrorEventHandler OnError;
    public event WebSocketCloseEventHandler OnClose;

    public WebSocket (string url) {
      if (!WebSocketFactory.isInitialized) {
        WebSocketFactory.Initialize ();
      }
      int instanceId = WebSocketFactory.WebSocketAllocate (url);
      WebSocketFactory.instances.Add (instanceId, this);

      this.instanceId = instanceId;
    }
    
    public WebSocket (string url, string subprotocol) {
      if (!WebSocketFactory.isInitialized) {
        WebSocketFactory.Initialize ();
      }

      int instanceId = WebSocketFactory.WebSocketAllocate (url);
      WebSocketFactory.instances.Add (instanceId, this);

      WebSocketFactory.WebSocketAddSubProtocol(instanceId, subprotocol);

      this.instanceId = instanceId;
    }

    public WebSocket (string url, List<string> subprotocols) {
      if (!WebSocketFactory.isInitialized) {
        WebSocketFactory.Initialize ();
      }

      int instanceId = WebSocketFactory.WebSocketAllocate (url);
      WebSocketFactory.instances.Add (instanceId, this);

      foreach (string subprotocol in subprotocols) {
        WebSocketFactory.WebSocketAddSubProtocol(instanceId, subprotocol);
      }

      this.instanceId = instanceId;
    }

    ~WebSocket () {
      WebSocketFactory.HandleInstanceDestroy (this.instanceId);
    }

    public int GetInstanceId () {
      return this.instanceId;
    }

    public void Connect() {
      int ret = WebSocketConnect (this.instanceId);

      if (ret < 0)
        throw WebSocketTools.GetErrorMessageFromCode (ret, null);
    }

	public void CancelConnection () {
		if (State == WebSocketState.Open)
			Close (WebSocketCloseCode.Abnormal);
	}

    public void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null) {
      int ret = WebSocketClose (this.instanceId, (int) code, reason);

      if (ret < 0)
        throw WebSocketTools.GetErrorMessageFromCode (ret, null);
    }

    public void Send (byte[] data, int lenght = -1) {
      int ret = WebSocketSend (this.instanceId, data, lenght == -1 ? data.Length : lenght);

      if (ret < 0)
        throw WebSocketTools.GetErrorMessageFromCode (ret, null);
    }

    public void SendText (string message) {
      int ret = WebSocketSendText (this.instanceId, message);

      if (ret < 0)
        throw WebSocketTools.GetErrorMessageFromCode (ret, null);
    }

    public WebSocketState State {
      get {
        int state = WebSocketGetState (this.instanceId);

        if (state < 0)
          throw WebSocketTools.GetErrorMessageFromCode (state, null);

        switch (state) {
          case 0:
            return WebSocketState.Connecting;

          case 1:
            return WebSocketState.Open;

          case 2:
            return WebSocketState.Closing;

          case 3:
            return WebSocketState.Closed;

          default:
            return WebSocketState.Closed;
        }
      }
    }

    internal void DelegateOnOpenEvent () {
      this.OnOpen?.Invoke ();
    }

    internal void DelegateOnMessageEvent (BufferWithLifeTime withLifeTime) {
      this.OnMessage?.Invoke (withLifeTime);
    }

    internal void DelegateOnErrorEvent (string errorMsg) {
      this.OnError?.Invoke (errorMsg);
    }

    internal void DelegateOnCloseEvent (int closeCode) {
      this.OnClose?.Invoke (WebSocketTools.ParseCloseCodeEnum (closeCode));
    }

  }
}