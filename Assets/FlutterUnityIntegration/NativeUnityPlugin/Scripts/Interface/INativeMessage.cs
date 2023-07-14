using System;

namespace UnityNativeMessage
{
    public interface INativeMessage
    {
        void ShowHostMainWindow();
        void UnloadMainWindow();
        void QuitUnityWindow();
        void SendMessageToNative(string message);
        void SendMessageToNative(UnityMessage message);
        void SendMessageToNative<T>(string name, T data, Action<object> callback = null);
        void SendMessageToNative(MessageHandler handler, HandlerResult result);
        void RegisterReceiveEvent(Action<MessageHandler> callback);
        void UnRegisterReceiveEvent(Action<MessageHandler> callback);
    }
}


