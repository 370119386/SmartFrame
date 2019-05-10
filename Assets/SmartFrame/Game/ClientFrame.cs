using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smart.Common;
using Smart.Table;

namespace Smart.UI
{
    public class ClientFrame : IFrame
    {
        protected enum LoadType
        {
            LT_FROM_ASSET_BUNDLE = 0,
            LT_FROM_RESOURCES_FOLDER,
            LT_FROM_TABLE,
        }

        protected LoadType eLoadType = LoadType.LT_FROM_ASSET_BUNDLE;
        protected bool isopen = false;
        protected object userData;
        private int _frameId = -1;
        protected GameObject instance = null;
        protected ClientFrame parentFrame = null;
        protected List<ClientFrame> childFrames = null;
        protected FrameConfigTableItem frameConfig = null;

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

        protected GameObject LoadPrefab(GameObject root,bool worldPosStay)
        {
            if (eLoadType == LoadType.LT_FROM_TABLE)
            {
                frameConfig = UIManager.Instance().GetFrameConfig(frameId);
                if (null == frameConfig)
                {
                    Logger.LogFormat("FrameConfig Is Null FrameId = [{0}]", frameId);
                    return null;
                }

                eLoadType = (LoadType)frameConfig.LoadType;
                if(eLoadType == LoadType.LT_FROM_ASSET_BUNDLE && null != frameConfig.InitParams && frameConfig.InitParams.Length == 2)
                {
                    _bundleName = frameConfig.InitParams[0];
                    _prefabName = frameConfig.InitParams[1];
                }
                else if (eLoadType == LoadType.LT_FROM_RESOURCES_FOLDER && null != frameConfig.InitParams && frameConfig.InitParams.Length == 1)
                {
                    _prefabName = frameConfig.InitParams[0];
                }
                else
                {
                    Logger.LogFormat("FrameConfig Error frameConfig.LoadType = [{0}]", frameConfig.LoadType);
                    return null;
                }
            }

            if (eLoadType == LoadType.LT_FROM_ASSET_BUNDLE)
            {
                if (string.IsNullOrEmpty(bundleName))
                {
                    Logger.LogFormat("bundleName Has Not Setted Yet ...");
                    return null;
                }

                if (string.IsNullOrEmpty(prefabName))
                {
                    Logger.LogFormat("prefabName Has Not Setted Yet ...");
                    return null;
                }

                return AssetBundleManager.Instance().LoadGameObject(bundleName, prefabName, root, worldPosStay);
            }

            if(eLoadType == LoadType.LT_FROM_RESOURCES_FOLDER)
            {
                return AssetBundleManager.Instance().LoadGameObjectFromResourceFolder(prefabName, root, worldPosStay);
            }

            Logger.LogFormat("Frame eLoadType Error = {0}",(int)eLoadType);
            return null;
        }

        public void Open(int frameId,GameObject root,object argv)
        {
            _frameId = frameId;
            userData = argv;
            isopen = true;
            instance = LoadPrefab(root, false);
            if(null == instance)
            {
                Logger.LogFormat("Open Frame Failed type = [{0}] frameId = [{1}]", GetType().Name,_frameId);
                return;
            }

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

        protected string _bundleName = string.Empty;
        protected string bundleName
        {
            get { return _bundleName; }
        }

        protected string _prefabName = string.Empty;
        protected virtual string prefabName
        {
            get { return _prefabName; }
        }

        protected virtual void OnOpenFrame()
        {

        }

        protected virtual void OnCloseFrame()
        {

        }
    }   
}