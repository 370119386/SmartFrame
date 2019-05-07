using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;

namespace Smart.Common
{
    public class AssetBundleManager : MonoBehaviour
    {
        protected Dictionary<string, HttpDownLoadHandle> mDownLoadingHandlers = new Dictionary<string, HttpDownLoadHandle>(32);

        protected static AssetBundleManager ms_instance;
        public static AssetBundleManager Instance()
        {
            if(null == ms_instance)
            {
                var gameObject = new GameObject("AssetBundleManager");
                ms_instance = gameObject.AddComponent<AssetBundleManager>();
            }
            return ms_instance;
        }

        protected void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void DownLoadAssetBundles(string server,string version,params string[] assetBundles)
        {
            for(int i = 0 ; i < assetBundles.Length ; ++i)
            {
                var key = string.Format(version + "_" + assetBundles[i]);
                if(mDownLoadingHandlers.ContainsKey(key))
                {
                    continue;
                }

                var bundleName = assetBundles[i];
                var url = Function.getAssetBundleDownloadUrl(server, version, bundleName);
                Debug.LogFormat("[download]:url:[<color=#00ffff>{0}</color>]", url);
                var storepath = Function.getAssetBundlePersistentPath(version, string.Empty, false);
                Debug.LogFormat("[download]:storepath:[<color=#00ffff>{0}</color>]", storepath);

                //StartCoroutine(download(url));
                var handler = HttpDownLoadHandle.Get(url, storepath, assetBundles[i], () =>
                   {
                       Debug.LogFormat("[download]:bundleName:[<color=#00ffff>{0}</color>] succeed", bundleName);
                   },
                   () =>
                   {
                       Debug.LogFormat("[download]:bundleName:[<color=#ff0000>{0}</color>] failed", bundleName);
                   });

                mDownLoadingHandlers.Add(key, handler);
            }
        }

        protected IEnumerator download(string url)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            var downloadHandler = new DownloadHandlerAssetBundle(url,0);
            www.downloadHandler = downloadHandler;
            yield return www.SendWebRequest();

            var bundleName = System.IO.Path.GetFileNameWithoutExtension(url);

            if(!www.isDone)
            {
                Debug.LogFormat("download:[<color=#00ffff>{0}</color>] failed ...", bundleName);
                yield break;
            }

            if(!string.IsNullOrEmpty(www.error))
            {
                Debug.LogFormat("download:[<color=#00ffff>{0}</color>]:failed:[{1}]", bundleName, www.error);
                yield break;
            }

            if (null == downloadHandler.assetBundle)
            {
                Debug.LogFormat("download:[<color=#00ffff>{0}</color>] failed assetbundle is null", bundleName);
                yield break;
            }

            Debug.LogFormat("download:[<color=#00ffff>{0}</color>] succeed", bundleName);
        }

        protected void Update()
        {
            HttpDownLoadHandle.Update();
        }
    }
}