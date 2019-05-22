using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smart.Table;
using Smart.Common;

namespace Smart.UI
{
    public class UIManager : Singleton<UIManager>
    {
        protected FrameConfigTable frameConfigTable;

        public void Initialize(FrameConfigTable frameConfigs)
        {
            frameConfigTable = frameConfigs;
        }

        protected Dictionary<System.Type,List<ClientFrame>> mActiveFrames = new Dictionary<System.Type, List<ClientFrame>>(32);

        public FrameConfigTableItem GetFrameConfig(int iId)
        {
            return frameConfigTable.GetTableItem(iId);
        }

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

        public T OpenFrame<T>(object argv = null, int layer = 1,int frameId = -1) where T : ClientFrame,new()
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

            GameObject parent = null;
            if(null != GameManager.Instance().Layers)
            {
                if (layer >= 0 && layer < GameManager.Instance().Layers.Length)
                {
                    parent = GameManager.Instance().Layers[layer];
                }
            }
            clientFrame.Open(frameId,parent,argv);

            return clientFrame;
        }
    }   
}
