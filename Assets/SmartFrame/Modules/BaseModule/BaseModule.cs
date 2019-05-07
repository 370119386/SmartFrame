using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smart.Common;

namespace Smart
{
    public class BaseModule : ModuleTemplate<BaseModule>
    {
        protected override void OnInitialize()
        {
            AssetBundleManager.Instance();

            AssetBundleManager.Instance().DownLoadAssetBundles("https://resourcekids.66uu.cn/kids/", Application.version,"ad","blood_train");
        }
    }
}