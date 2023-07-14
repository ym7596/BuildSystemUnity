using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using UnityEngine;
//추가
using UnityNativeMessage;

namespace FlutterUnityIntegration
{
    #region UnityNativeMessage(Move)
    /*public enum HandlerResult
    {
        Success = 0,
        Fail = 1,
    }

    [Serializable]
    public class UnityMessage
    {
        public string name;
        public JObject data;
        public Action<object> callBack;
    }

    [Serializable]
    public class MessageHandler
    {
        public int id;
        public string seq;
        public string prefix;
        public string name;

        private JToken data;

        public static MessageHandler Deserialize(string message, string prefix)
        {
            JObject m = JObject.Parse(message);
            MessageHandler handler = new MessageHandler(
                m.GetValue("id").Value<int>(),
                m.GetValue("seq").Value<string>(),
                m.GetValue("name").Value<string>(),
                prefix,
                m.GetValue("data")
            );
            return handler;
        }

        public MessageHandler(int id, string seq, string name, string prefix, JToken data)
        {
            this.id = id;
            this.seq = seq;
            this.name = name;
            this.prefix = prefix;
            this.data = data;
        }

        public JToken GetDataToken()
        {
            return data;
        }

        public T getData<T>()
        {
            return data.Value<T>();
        }

        public T getJsonToData<T>()
        {
            return JsonUtility.FromJson<T>(data.ToString());
        }

        public string getDataString(object data = null)
        {
            var o = JObject.FromObject(new
            {
                id = id,
                seq = "end",
                name = name,
                prefix = prefix,
                data = data
            });

            return $"{prefix}{o.ToString()}";
        }
    }*/
    #endregion

    //SingletonMonoBehaviour<UnityMessageManager> => MonoBehaviour
    public class UnityMessageManager : MonoBehaviour, INativeMessage
    {
        //MessagePrefix => (UnityMessagePrefix, NativeMessagePrefix)
        public const string UnityMessagePrefix = "@UnityMessage@";
        public const string NativeMessagePrefix = "@NativeMessage@";

        //추가
        private const string _checkSeq = "end";
        private static int ID = 0;

        private static int generateId()
        {
            ID = ID + 1;
            return ID;
        }

        public delegate void MessageDelegate(string message);
        public event MessageDelegate OnMessage;

        //public delegate void MessageHandlerDelegate(MessageHandler handler);
        //public event MessageHandlerDelegate OnFlutterMessage;
        public event Action<MessageHandler> OnFlutterMessage;

        private Dictionary<int, UnityMessage> waitCallbackMessageMap = new Dictionary<int, UnityMessage>();

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            NativeAPI.OnSceneLoaded(scene, mode);
        }

        public void ShowHostMainWindow()
        {
            NativeAPI.ShowHostMainWindow();
        }

        public void UnloadMainWindow()
        {
            NativeAPI.UnloadMainWindow();
        }

        public void QuitUnityWindow()
        {
            NativeAPI.QuitUnityWindow();
        }

        public void SendMessageToFlutter(string message)
        {
            NativeAPI.SendMessageToFlutter(message);
        }

        public void SendMessageToFlutter(UnityMessage message)
        {
            //추가 코드
            if (message == null)
                return;

            int id = generateId();

            //추가 코드
            var useCallback = message.callBack != null;
            //추가 코드
            string seq = "start";

            //추가 코드
            if (useCallback == true)
			{
                waitCallbackMessageMap.Add(id, message);
                seq = message.callBack != null ? "start" : "";
            }
                
            JObject o = JObject.FromObject(new
            {
                id = id,
                seq = seq,
                name = message.name,
                data = message.data
            });

            string unityMessage = UnityMessagePrefix + o.ToString(Newtonsoft.Json.Formatting.None);

            //변경 UnityMessageManager.Instance.SendMessageToFlutter => SendMessageToFlutter
            SendMessageToFlutter(unityMessage);
        }

        void onMessage(string message)
        {
            OnMessage?.Invoke(message);
        }

        //추가된 기능
        protected void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            OnMessage = null;
            OnFlutterMessage = null;
        }

        protected void Awake()
        {
            gameObject.name = nameof(UnityMessageManager);
        }

        //Unity => Flutter
        public void SendMessageToNative(string message)
        {
            SendMessageToFlutter(message);
        }

        public void SendMessageToNative(UnityMessage message)
        {
            SendMessageToFlutter(message);
        }

        public void SendMessageToNative<T>(string name, T data, Action<object> callback = null)
        {
            if (data == null)
                return;

            JObject jObject = JObject.FromObject(data);

            UnityMessage message = new UnityMessage()
            {
                name = name,
                data = jObject,
                callBack = callback
            };

            SendMessageToFlutter(message);
        }

        public void SendMessageToNative(MessageHandler handler, HandlerResult result)
        {
            if (handler == null)
                return;

            SendMessageToFlutter(handler.ConvertDataToString((int)result));
        }

        //Flutter => Unity
        private void onFlutterMessage(string message)
        {
            var isNative = true;
            string prefix = NativeMessagePrefix;

            if (message.StartsWith(NativeMessagePrefix) == true)
            {
                message = message.Replace(NativeMessagePrefix, "");
            }
            else if (message.StartsWith(UnityMessagePrefix) == true)
            {
                isNative = false;
                prefix = UnityMessagePrefix;
                message = message.Replace(UnityMessagePrefix, "");
            }
            else
                return;
            
            //변경
            MessageHandler handler = MessageHandler.Deserialize(message, prefix);

            // handle callback message
            if (isNative == false && _checkSeq.Equals(handler.seq) && waitCallbackMessageMap.TryGetValue(handler.id, out var m))
			{
                waitCallbackMessageMap.Remove(handler.id);
                m.callBack?.Invoke(handler.getDataString());
                //m.callBack?.Invoke(handler.getData<object>());
            }
            else
			{
                OnFlutterMessage?.Invoke(handler);
            }
        }

        public void RegisterReceiveEvent(Action<MessageHandler> callback)
		{
            OnFlutterMessage += callback;
		}

        public void UnRegisterReceiveEvent(Action<MessageHandler> callback)
		{
            OnFlutterMessage -= callback;
		}
    }

	#region Org Code
	/*
    public class MessageHandler
	{
		public int id;
		public string seq;

		public String name;
		private readonly JToken data;

		public static MessageHandler Deserialize(string message)
		{
			var m = JObject.Parse(message);
			var handler = new MessageHandler(
				m.GetValue("id").Value<int>(),
				m.GetValue("seq").Value<string>(),
				m.GetValue("name").Value<string>(),
				m.GetValue("data")
			);
			return handler;
		}

		public T getData<T>()
		{
			return data.Value<T>();
		}

		public MessageHandler(int id, string seq, string name, JToken data)
		{
			this.id = id;
			this.seq = seq;
			this.name = name;
			this.data = data;
		}

		public void send(object data)
		{
			var o = JObject.FromObject(new
			{
				id = id,
				seq = "end",
				name = name,
				data = data
			});
			UnityMessageManager.Instance.SendMessageToFlutter(UnityMessageManager.MessagePrefix + o.ToString());
		}
	}

	public class UnityMessage
	{
		public String name;
		public JObject data;
		public Action<object> callBack;
	}

	public class UnityMessageManager : SingletonMonoBehaviour<UnityMessageManager>
    {

        public const string MessagePrefix = "@UnityMessage@";
        private static int ID = 0;

        private static int generateId()
        {
            ID = ID + 1;
            return ID;
        }

        public delegate void MessageDelegate(string message);
        public event MessageDelegate OnMessage;

        public delegate void MessageHandlerDelegate(MessageHandler handler);
        public event MessageHandlerDelegate OnFlutterMessage;

        private readonly Dictionary<int, UnityMessage> waitCallbackMessageMap = new Dictionary<int, UnityMessage>();

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            NativeAPI.OnSceneLoaded(scene, mode);

        }

        public void ShowHostMainWindow()
        {
            NativeAPI.ShowHostMainWindow();
        }

        public void UnloadMainWindow()
        {
            NativeAPI.UnloadMainWindow();
        }


        public void QuitUnityWindow()
        {
            NativeAPI.QuitUnityWindow();
        }


        public void SendMessageToFlutter(string message)
        {
            NativeAPI.SendMessageToFlutter(message);
        }

        public void SendMessageToFlutter(UnityMessage message)
        {
            var id = generateId();
            if (message.callBack != null)
            {
                waitCallbackMessageMap.Add(id, message);
            }

            var o = JObject.FromObject(new
            {
                id = id,
                seq = message.callBack != null ? "start" : "",
                name = message.name,
                data = message.data
            });
            UnityMessageManager.Instance.SendMessageToFlutter(MessagePrefix + o.ToString());
        }

        void onMessage(string message)
        {
            OnMessage?.Invoke(message);
        }

        void onFlutterMessage(string message)
        {
            if (message.StartsWith(MessagePrefix))
            {
                message = message.Replace(MessagePrefix, "");
            }
            else
            {
                return;
            }

            var handler = MessageHandler.Deserialize(message);
            if ("end".Equals(handler.seq))
            {
                // handle callback message
                if (!waitCallbackMessageMap.TryGetValue(handler.id, out var m)) return;
                waitCallbackMessageMap.Remove(handler.id);
                m.callBack?.Invoke(handler.getData<object>()); // todo
                return;
            }

            OnFlutterMessage?.Invoke(handler);
        }
    }
    */
	#endregion
}
