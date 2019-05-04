using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace Smart.Common
{
    public class AssetBundleManager : MonoBehaviour
    {
        public class DownLoadHandler
        {
            public string downloadUrl;
            public string storagePath;
            public string bundleName;
            public HttpDownLoad httpDownLoad;
            protected DownLoadStatus status;
            public DownLoadStatus Status
            {
                get
                {
                    return status;
                }
                set
                {
                    status = value;
                }
            }

            public void OnDownloadStatusChanged(DownLoadStatus eValue)
            {
                Status = eValue;
            }
        }

        protected Dictionary<string,DownLoadHandler> mDownLoadingHandlers = new Dictionary<string, DownLoadHandler>(32);

        public void DownLoadAssetBundles(string server,string version,string[] assetBundles)
        {
            for(int i = 0 ; i < assetBundles.Length ; ++i)
            {
                var key = string.Format(version + "_" + assetBundles[i]);
                if(mDownLoadingHandlers.ContainsKey(key))
                {
                    continue;
                }
                var handler = new DownLoadHandler();
                handler.downloadUrl = Function.getAssetBundleDownloadUrl(server,version,assetBundles[i]);
                handler.storagePath = Function.getAssetBundlePersistentPath(version,assetBundles[i],false);
                var fileName = Path.GetFileName(handler.storagePath);
                var path = Path.GetFullPath(handler.storagePath);
                handler.httpDownLoad = new HttpDownLoad();
                handler.Status = DownLoadStatus.DLS_DOWNLOADING;
                mDownLoadingHandlers.Add(key,handler);
                handler.httpDownLoad.DownLoad(handler.downloadUrl,path,fileName,handler.OnDownloadStatusChanged);
            }
        }
    }
}