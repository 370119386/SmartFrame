using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smart.Common;

namespace Smart.Event
{
    public delegate void Action(object argv);
    public class EventManager : Singleton<EventManager>
    {
        protected Dictionary<Event,Action> eventMap = new Dictionary<Event, Action>(32);

        public void RegisterEvent(Event e,Action action)
        {
            if(eventMap.ContainsKey(e))
                eventMap[e] = System.Delegate.Combine(eventMap[e],action) as Action;
            else
                eventMap.Add(e,action);
        }

        public void UnRegisterEvent(Event e,Action action)
        {
            if(eventMap.ContainsKey(e))
                eventMap[e] = System.Delegate.Remove(eventMap[e],action) as Action;
        }

        public void SendEvent(Event e,object argv = null)
        {
            if(eventMap.ContainsKey(e))
            {
                var action = eventMap[e];
                if(null != action)
                {
                    action.Invoke(argv);
                }
            }
        }
    }
}