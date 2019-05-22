using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            mLoadingText = mScriptBinder.GetObject("LoadingText") as UnityEngine.UI.Text;}
        }
}