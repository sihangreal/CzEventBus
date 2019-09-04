using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CzEventBus
{
    public class EventBus
    {
        private class ListenerInfo
        {
            public object listener;
            public MethodInfo methodInfo;
        }

        private class InvokeParam
        {
            public ListenerInfo listener;
            public object[] param;
        }

        //存放事件的容器
        private static Dictionary<string, List<ListenerInfo>> listenerMap = new Dictionary<string, List<ListenerInfo>>();

        ///// <summary>
        ///// 注册事件 最懒的方式不推荐使用
        ///// </summary>
        ///// <param name="assembly">程序集</param>
        //public static void Register(Assembly assembly, bool isFormOrControl = false)
        //{
        //    var types = assembly.ExportedTypes;
        //    foreach (var type in types)
        //    {
        //        if (isFormOrControl)
        //        {
        //            if (typeof(Form).IsAssignableFrom(type) || typeof(Control).IsAssignableFrom(type))
        //            {
        //                object instance = Activator.CreateInstance(type);
        //                RegisterEvent(instance);
        //            }
        //        }
        //        else
        //        {
        //            object instance = Activator.CreateInstance(type);
        //            RegisterEvent(instance);
        //        }
        //    }
        //}

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="listener">监听者</param>
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

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="eventType">事件名</param>
        /// <param name="listener">监听者</param>
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

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="eventType">事件名</param>
        /// <param name="method">方法</param>
        /// <param name="listener">监听者</param>
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

        /// <summary>
        ///是否注册事件
        /// </summary>
        /// <param name="eventType">事件名</param>
        /// <param name="method">方法</param>
        /// <param name="listener">监听者</param>
        /// <returns></returns>
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

        /// <summary>
        /// 注销事件
        /// </summary>
        /// <param name="listener"></param>
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

        /// <summary>
        /// 注销事件
        /// </summary>
        /// <param name="eventType">事件名</param>
        /// <param name="listener">监听者</param>
        public static void UnRegisterEvent(string eventType, object listener)
        {
            UnRegisterEvent(eventType, null, listener);
        }

        /// <summary>
        /// 注销事件
        /// </summary>
        /// <param name="eventType">事件名</param>
        /// <param name="methodInfo">方法</param>
        /// <param name="listener">监听者</param>
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

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="eventType">事件名</param>
        /// <param name="isAsync">是否支持异步</param>
        public static void PublishEvent(string eventType, bool isAsync = false)
        {
            PublishEvent(eventType, null, null, isAsync);
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="eventType">事件名</param>
        /// <param name="eventArg">事件参数</param>
        /// <param name="isAsync">是否支持异步</param>
        public static void PublishEvent(string eventType, object eventArg, bool isAsync = false)
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
                if (isAsync)
                {
                    InvokeParam invokeParam = new InvokeParam() { listener = listenerInfo, param = objs };
                    ThreadPool.QueueUserWorkItem(Invoke, invokeParam);
                }
                else
                {
                    listenerInfo.methodInfo.Invoke(listenerInfo.listener, objs);
                }
            }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="eventType">事件名</param>
        /// <param name="sender">发送者</param>
        /// <param name="eventArg">事件参数</param>
        /// <param name="isAsync">是否支持异步</param>
        public static void PublishEvent(string eventType, object sender, object eventArg, bool isAsync = false)
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
                if (isAsync)
                {
                    InvokeParam invokeParam = new InvokeParam() { listener = listenerInfo, param = objs };
                    ThreadPool.QueueUserWorkItem(Invoke, invokeParam);
                }
                else
                {
                    listenerInfo.methodInfo.Invoke(listenerInfo.listener, objs);
                }
            }
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="eventType">事件名</param>
        /// <param name="eventArgs">事件参数</param>
        /// <param name="isAsync">是否支持异步</param>
        public static void PublishEvent(string eventType, object[] eventArgs, bool isAsync = false)
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
                if (isAsync)
                {
                    InvokeParam invokeParam = new InvokeParam() { listener = listenerInfo, param = objs };
                    ThreadPool.QueueUserWorkItem(Invoke, invokeParam);
                }
                else
                {
                    listenerInfo.methodInfo.Invoke(listenerInfo.listener, objs);
                }
            }
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="eventType">事件名</param>
        /// <param name="sender">发送者</param>
        /// <param name="eventArgs">事件参数</param>
        /// <param name="isAsync">是否支持异步</param>
        public static void PublishEvent(string eventType, object sender, object[] eventArgs, bool isAsync = false)
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
                if (isAsync)
                {
                    InvokeParam invokeParam = new InvokeParam() { listener = listenerInfo, param = objs };
                    ThreadPool.QueueUserWorkItem(Invoke, invokeParam);
                }
                else
                {
                    listenerInfo.methodInfo.Invoke(listenerInfo.listener, objs);
                }
            }
        }

        /// <summary>
        /// 利用线程池执行方法
        /// </summary>
        /// <param name="arg"></param>
        private static void Invoke(object arg)
        {
            InvokeParam invokeParam = (InvokeParam)arg;
            invokeParam.listener.methodInfo.Invoke(invokeParam.listener.listener, invokeParam.param);
        }
    }
}

