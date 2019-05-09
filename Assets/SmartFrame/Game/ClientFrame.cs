using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smart.Common;

namespace Smart.Client
{
    public class ClientFrame : IFrame
    {
        protected bool isopen = false;
        protected object userData;
        private int _frameId = -1;
        protected GameObject instance = null;
        protected ClientFrame parentFrame = null;
        protected List<ClientFrame> childFrames = null;

        protected static Smart.Common.ILogger Logger = new ModuleCommonLogger("ClientFrame");

        public int frameId 
        {
            get
            {
                return _frameId;
            }
        }

        protected void OnChildFrameClosed(ClientFrame child)
        {
            if(null != childFrames)
                childFrames.Remove(child);
        }

        protected void AddChildFrame(IFrame child)
        {
            var frame = child as ClientFrame;
            if(null == childFrames)
            {
                childFrames = new List<ClientFrame>(2);
            }
            frame.parentFrame = this;
            childFrames.Add(frame);
        }

        public void Open(int frameId,GameObject root,object argv)
        {
            _frameId = frameId;
            userData = argv;
            isopen = true;

            if(string.IsNullOrEmpty(bundleName))
            {
                Logger.LogFormat("bundleName Has Not Setted Yet ...");
                return;
            }

            if(string.IsNullOrEmpty(prefabName))
            {
                Logger.LogFormat("prefabName Has Not Setted Yet ...");
                return;
            }

            AssetBundleManager.Instance().LoadGameObject(bundleName,prefabName);

            OnOpenFrame();
        }

        public void Close()
        {
            if(!isopen)
            {
                return;
            }
            isopen = false;

            if(null != childFrames)
            {
                List<ClientFrame> handles = new List<ClientFrame>(childFrames.Count);
                handles.AddRange(childFrames);
                for(int i = 0 ; i < handles.Count ; ++i)
                {
                    handles[i].Close();
                }
                handles.Clear();
                childFrames.Clear();
            }

            OnCloseFrame();

            if(null != parentFrame)
            {
                parentFrame.OnChildFrameClosed(this);
                parentFrame = null;
            }

            if(null != instance)
            {
                Object.Destroy(instance);
                instance = null;
            }

            Logger.LogFormat("[{0}] HasBeenClosed ID = {1}",GetType().Name,frameId);
        }

        protected virtual string bundleName
        {
            get { return string.Empty; }
        }

        protected virtual string prefabName
        {
            get { return string.Empty; }
        }

        protected virtual void OnOpenFrame()
        {

        }

        protected virtual void OnCloseFrame()
        {

        }
    }   
}