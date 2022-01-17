using System;
using UnityEngine;
using UnityEngine.UI;
using WebGL.WebSockets.Tests;

namespace Test
{
    public class Binder : MonoBehaviour
    {
        public Button Connect;
        public Button Disconnect;
        public InputField Uri;

        public WebSocketTest Model;
        private void Start()
        {
            Connect.onClick.AddListener(Model.Connect);
            Uri.onValueChanged.AddListener(Changed);
            Changed(Uri.text);
            Disconnect.onClick.AddListener(Model.Disconnect);
        }

        private void Changed(string arg0)
        {
            Model.ConnectionUri = arg0;
        }

        private void OnDestroy()
        {
            Connect.onClick.RemoveAllListeners();
            Uri.onValueChanged.RemoveAllListeners();
        }
    }
}