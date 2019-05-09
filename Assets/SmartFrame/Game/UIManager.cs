using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Smart.Client
{
    public class UIManager : MonoBehaviour
    {
        protected UIManager ms_instance = null;
        public UIManager Instance()
        {
            if(null == ms_instance)
            {
                var gameObjecgt = new GameObject();
                ms_instance = gameObjecgt.AddComponent<UIManager>();
            }
            return ms_instance;
        }

        protected Dictionary<System.Type,List<ClientFrame>> mActiveFrames = new Dictionary<System.Type, List<ClientFrame>>(32);

        public bool HasFrame<T>(int frameId = -1) where T : ClientFrame,new()
        {
            return null != FindFrame<T>(frameId);
        }

        public T FindFrame<T>(int frameId = -1) where T : ClientFrame,new()
        {
            if(!mActiveFrames.ContainsKey(typeof(T)))
            {
                return default(T);
            }
            
            var frames = mActiveFrames[typeof(T)];
            for(int i = 0 ; i < frames.Count ; ++i)
            {
                if(frames[i].frameId == frameId)
                {
                    return frames[i] as T;
                }
            }

            return null;
        }

        public void CloseFrame<T>(int frameId = -1) where T : ClientFrame,new()
        {
            if(!mActiveFrames.ContainsKey(typeof(T)))
            {
                return;
            }

            var frames = mActiveFrames[typeof(T)];
            for(int i = 0 ; i < frames.Count ; ++i)
            {
                if(frames[i].frameId == frameId)
                {
                    frames[i].Close();
                    break;
                }
            }
        }

        public T OpenFrame<T>(GameObject parent,object argv = null,int frameId = -1) where T : ClientFrame,new()
        {
            CloseFrame<T>(frameId);

            var type = typeof(T);
            if(!mActiveFrames.ContainsKey(type))
            {
                mActiveFrames.Add(type,new List<ClientFrame>(32));
            }
            var frames = mActiveFrames[type];
            
            T clientFrame = new T();
            frames.Add(clientFrame);

            clientFrame.Open(frameId,parent,argv);

            return clientFrame;
        }
    }   
}
