using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smart.Table;

namespace Smart.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField][Tooltip("界面配置数据")]
        protected FrameConfigTable frameConfigTable;

        protected static UIManager ms_instance = null;
        public static UIManager Instance()
        {
            if(null == ms_instance)
            {
                var gameObjecgt = new GameObject();
                ms_instance = gameObjecgt.AddComponent<UIManager>();
                ms_instance.frameConfigTable.MakeTable();
            }
            return ms_instance;
        }

        protected void Start()
        {
            if(null == ms_instance)
            {
                ms_instance = this;
                frameConfigTable.MakeTable();
            }
            
            DontDestroyOnLoad(gameObject);
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
