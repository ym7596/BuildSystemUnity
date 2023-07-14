using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace UnityNativeMessage
{
    public enum HandlerResult
    {
        Success = 0,
        Fail = 1,
    }

    [Serializable]
    public class UnityMessage
    {
        public string name;
        public JObject data;
        //public Action<object> callBack;
        public Action<string> callBack;
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

        public T getData<T>()
        {
            return data.Value<T>();
        }

        public T getJsonToData<T>()
        {
            return JsonUtility.FromJson<T>(data.ToString());
        }

        public string getDataString()
		{
            return data != null ? data.ToString() : string.Empty;
		}

        public JToken getDataToken()
        {
            return data;
        }

        public MessageHandler(int id, string seq, string name, string prefix, JToken data)
        {
            this.id = id;
            this.seq = seq;
            this.name = name;
            this.prefix = prefix;
            this.data = data;
        }

        public string ConvertDataToString(object data = null)
		{
            JObject jsonObj = JObject.FromObject(new
            {
                id = id,
                seq = "end",
                name = name,
                prefix = prefix,
                data = data
            });

            return $"{prefix}{jsonObj}";
        }
    }
}


