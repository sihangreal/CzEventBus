using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CzEventBus
{
    public class EventBus
    {
        private class ListenerInfo
        {
            public object listener;
            public MethodInfo methodInfo;
        }
        private static Dictionary<string, List<ListenerInfo>> listenerMap = new Dictionary<string, List<ListenerInfo>>();
        public static void RegisterEvent(object listener)
        {
            Type type = listener.GetType();
            MethodInfo[] methodInfos = type.GetMethods();
            foreach (MethodInfo methodInfo in methodInfos)
            {
                EventAttr eventAttr = (EventAttr)methodInfo.GetCustomAttribute(typeof(EventAttr), false);
                if (eventAttr != null)
                {
                    string eventType = eventAttr.EventType;
                    if (string.IsNullOrEmpty(eventType))
                    {
                        eventType = methodInfo.Name;
                    }
                    RegisterEvent(eventType, methodInfo, listener);
                }
            }
        }
        public static void RegisterEvent(string eventType, object listener)
        {
            Type type = listener.GetType();
            MethodInfo[] methodInfos = type.GetMethods();
            foreach (MethodInfo methodInfo in methodInfos)
            {
                EventAttr eventAttr = (EventAttr)methodInfo.GetCustomAttribute(typeof(EventAttr), false);
                if (eventAttr == null || eventAttr.EventType != eventType)
                {
                    continue;
                }
                RegisterEvent(eventType, methodInfo, listener);
            }
        }
        public static void RegisterEvent(string eventType, MethodInfo method, object listener)
        {
            if (IsRegisterEvent(eventType, method, listener))
            {
                return;
            }
            List<ListenerInfo> listenInfos = null;
            if (listenerMap.ContainsKey(eventType))
            {
                listenInfos = listenerMap[eventType];
            }
            else
            {
                listenInfos = new List<ListenerInfo>();
                listenerMap.Add(eventType, listenInfos);
            }
            ListenerInfo listenerInfo = new ListenerInfo();
            listenerInfo.methodInfo = method;
            listenerInfo.listener = listener;
            listenInfos.Add(listenerInfo);
        }
        public static bool IsRegisterEvent(string eventType, MethodInfo method, object listener)
        {
            if (!listenerMap.ContainsKey(eventType))
            {
                return false;
            }
            List<ListenerInfo> listenInfos = listenerMap[eventType];
            foreach (ListenerInfo listenInfo in listenInfos)
            {
                if (listenInfo.listener != null && listenInfo.methodInfo == method)
                {
                    return true;
                }
            }
            return false;
        }
        public static void UnRegisterEvent(object listener)
        {
            Type type = listener.GetType();
            MethodInfo[] methodInfos = type.GetMethods();
            foreach (MethodInfo methodInfo in methodInfos)
            {
                EventAttr eventAttr = (EventAttr)methodInfo.GetCustomAttribute(typeof(EventAttr), false);
                if (eventAttr == null)
                {
                    continue;
                }
                UnRegisterEvent(eventAttr.EventType, methodInfo, listener);
            }
        }
        public static void UnRegisterEvent(string eventType, object listener)
        {
            UnRegisterEvent(eventType, null, listener);
        }
        public static void UnRegisterEvent(string eventType, MethodInfo methodInfo, object listener)
        {
            if (!listenerMap.ContainsKey(eventType))
            {
                return;
            }
            List<ListenerInfo> listenInfos = listenerMap[eventType];
            int count = listenInfos.Count;
            for (int i = 0; i < count; ++i)
            {
                ListenerInfo listenerInfo = listenInfos[i];
                if (methodInfo == null)
                {
                    if (listenerInfo.listener == listener)
                    {
                        listenInfos.Remove(listenerInfo);
                        break;
                    }
                }
                else
                {
                    if (listenerInfo.listener == listener && listenerInfo.methodInfo == methodInfo)
                    {
                        listenInfos.Remove(listenerInfo);
                        break;
                    }
                }
            }
        }
        public static void PublishEvent(string eventType)
        {
            PublishEvent(eventType, null, null);
        }
        public static void PublishEvent(string eventType, object eventArg)
        {
            if (!listenerMap.ContainsKey(eventType))
            {
                return;
            }
            List<ListenerInfo> listenInfos = listenerMap[eventType];
            foreach (ListenerInfo listenerInfo in listenInfos)
            {
                object[] objs = null;
                if (listenerInfo.methodInfo.GetParameters().Length != 0)
                {
                    objs = new object[] { eventArg };
                }
                listenerInfo.methodInfo.Invoke(listenerInfo.listener, objs);
            }
        }
        public static void PublishEvent(string eventType, object sender, object eventArg)
        {
            if (!listenerMap.ContainsKey(eventType))
            {
                return;
            }
            List<ListenerInfo> listenInfos = listenerMap[eventType];
            foreach (ListenerInfo listenerInfo in listenInfos)
            {
                object[] objs = null;
                if (listenerInfo.methodInfo.GetParameters().Length != 0)
                {
                    objs = new object[] { sender, eventArg };
                }
                listenerInfo.methodInfo.Invoke(listenerInfo.listener, objs);
            }
        }

        public static void PublishEvent(string eventType,object[] eventArgs)
        {
            if (!listenerMap.ContainsKey(eventType))
            {
                return;
            }
            List<ListenerInfo> listenInfos = listenerMap[eventType];
            foreach (ListenerInfo listenerInfo in listenInfos)
            {
                object[] objs = null;
                if (listenerInfo.methodInfo.GetParameters().Length != 0)
                {
                    List<object> objList = new List<object>();
                    objList.AddRange(eventArgs);
                    objs = objList.ToArray();
                }
                listenerInfo.methodInfo.Invoke(listenerInfo.listener, objs);
            }
        }

        public static void PublishEvent(string eventType,object sender,object[] eventArgs)
        {
            if (!listenerMap.ContainsKey(eventType))
            {
                return;
            }
            List<ListenerInfo> listenInfos = listenerMap[eventType];
            foreach (ListenerInfo listenerInfo in listenInfos)
            {
                object[] objs = null;
                if (listenerInfo.methodInfo.GetParameters().Length != 0)
                {
                    List<object> objList = new List<object>();
                    objList.Add(sender);
                    objList.AddRange(eventArgs);
                    objs = objList.ToArray();
                }
                listenerInfo.methodInfo.Invoke(listenerInfo.listener, objs);
            }
        }
    }
}

