using UnityEngine;
using Smart.Event;

namespace Smart.UI
{
    public class LoadingFrame : ClientFrame
    {
        public LoadingFrame()
        {
            eLoadType = LoadType.LT_FROM_RESOURCES_FOLDER;
            _prefabName = @"LoadingFrame/LoadingFrame";
        }

        UnityEngine.UI.Image mProgress;
        UnityEngine.UI.Text mLoadingText;
        
        protected override void _InitScriptBinder()
        {
            mProgress = mScriptBinder.GetObject("Progress") as UnityEngine.UI.Image;
            mLoadingText = mScriptBinder.GetObject("LoadingText") as UnityEngine.UI.Text;
        }

        protected void SetProcess(float value)
        {
            if(null != mLoadingText)
            {
                mLoadingText.text = string.Format("{0}%",value * 100);
            }

            if(null != mProgress)
            {
                mProgress.fillAmount = value;
            }
        }

        protected void OnLoadingProcessChanged(object argv)
        {
            float value = (float)argv;
            SetProcess(value);
        }

        protected void OnLoadingEnd(object argv)
        {
            Close();
        }

        protected override void OnOpenFrame()
        {
            SetProcess(0);
            EventManager.Instance().RegisterEvent(Event.Event.EventLoadingProgress,OnLoadingProcessChanged);
            EventManager.Instance().RegisterEvent(Event.Event.EventEndLoading,OnLoadingEnd);
        }

        protected override void OnCloseFrame()
        {
            EventManager.Instance().UnRegisterEvent(Event.Event.EventLoadingProgress,OnLoadingProcessChanged);
            EventManager.Instance().UnRegisterEvent(Event.Event.EventEndLoading,OnLoadingEnd);
        }
    }
}