﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace Smart.Common
{
    public class AssetBundleManager : MonoBehaviour
    {
        //网络下载中的
        protected Dictionary<string, HttpDownLoadHandle> mDownLoadingHandlers = new Dictionary<string, HttpDownLoadHandle>(32);

        protected static AssetBundleManager ms_instance;
        ILogger logger = new ModuleCommonLogger("AssetBundleManager");
        protected Smart.Common.ILogger Logger
        {
            get{
                return logger;
            }
        }
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

        public IEnumerator DownLoadTextFile(string server,string platform,string fileName,UnityAction onFailed,UnityAction<string> onSucceed)
        {
            string url = System.IO.Path.Combine(server,System.IO.Path.Combine(platform,fileName));
            using(UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                if(!string.IsNullOrEmpty(request.error))
                {
                    Logger.LogFormat("[download]:[{0}] failed,url:[{1}] Error:[{2}]",fileName,url,request.error);
                    if(null != onFailed)
                    {
                        onFailed.Invoke();
                    }
                    yield break;
                }

                if(request.isHttpError)
                {
                    Logger.LogFormat("[download]:[{0}] failed,url:[{1}] HttpError",fileName,url);
                    if(null != onFailed)
                    {
                        onFailed.Invoke();
                    }
                    yield break;
                }

                if(null != onSucceed)
                {
                    onSucceed.Invoke(request.downloadHandler.text);
                }
            }
        }

        public IEnumerator DownLoadAssetBundle(string server,string version,string bundleName,UnityAction<AssetBundle> onSucceed,UnityAction onFailed)
        {
            var url = Function.getAssetBundleDownloadUrl(server, version, bundleName);
            using(UnityWebRequest request = UnityWebRequest.Get(url))
            {
                var handler = new DownloadHandlerAssetBundle(url,0);
                request.downloadHandler = handler;

                Logger.LogFormat("[download]:url=[{0}]",url);
                yield return request.SendWebRequest();

                if(!string.IsNullOrEmpty(request.error))
                {
                    Logger.LogFormat("[download]:[{0}] failed,Error:[{1}]",bundleName,request.error);
                    if(null != onFailed)
                    {
                        onFailed.Invoke();
                    }
                    yield break;
                }

                if(request.isHttpError)
                {
                    Logger.LogFormat("[download]:[{0}] failed, url=[{1}],HttpError",bundleName,url);
                    if(null != onFailed)
                    {
                        onFailed.Invoke();
                    }
                    yield break;
                }

                if(null == handler.assetBundle)
                {
                    Logger.LogFormat("[download]:[{0}] failed, assetBundle Is Null",bundleName);
                    if(null != onFailed)
                    {
                        onFailed.Invoke();
                    }
                    yield break;
                }

                if(null != onSucceed)
                {
                    onSucceed.Invoke(handler.assetBundle);
                }

                handler.assetBundle.Unload(false);
            }
        }

        public void DownLoadAssetBundles(string server,string version,string[] assetBundles,string[] chekfileMd5s)
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
                Debug.LogFormat("<color=#ff00ff>[download]</color>:url:[<color=#00ffff>{0}</color>]", url);
                var storepath = Function.getAssetBundlePersistentPath(version, string.Empty, false);
                Debug.LogFormat("<color=#ff00ff>[download]</color>:storepath:[<color=#00ffff>{0}</color>]", storepath);

                var handler = HttpDownLoadHandle.Get(url, storepath, assetBundles[i], chekfileMd5s[i],() =>
                   {
                       Debug.LogFormat("<color=#ff00ff>[download]</color>:bundleName:[<color=#00ffff>{0}</color>] succeed", bundleName);
                   },
                   () =>
                   {
                       Debug.LogFormat("<color=#ff00ff>[download]</color>:bundleName:[<color=#ff0000>{0}</color>] failed", bundleName);
                   });

                mDownLoadingHandlers.Add(key, handler);
            }
        }

        protected class AssetBundleAnsyDownLoadListener
        {
            public string[] assetBundles;
            public string[] keys;
            public UnityAction<float> actionListener;
            public bool isDone;
            public UnityAction actionDone;
        };
        protected Dictionary<string,AssetBundleAnsyDownLoadListener> ansyDownLoadOperations = new Dictionary<string,AssetBundleAnsyDownLoadListener>(16);
        protected void UpdateDownLoadProgress()
        {
            var iter = ansyDownLoadOperations.GetEnumerator();
            while(iter.MoveNext())
            {
                var ansyDownloadActionListener = iter.Current.Value;
                if(null == ansyDownloadActionListener.actionListener)
                {
                    continue;
                }
                var length = ansyDownloadActionListener.assetBundles.Length;
                float sum = 0.0f;
                float percent = 0.0f;
                bool isDone = true;
                for(int i = 0 ; i < ansyDownloadActionListener.assetBundles.Length ; ++i)
                {
                    var assetBundleName = ansyDownloadActionListener.assetBundles[i];
                    var key = ansyDownloadActionListener.keys[i];
                    if(!mDownLoadingHandlers.ContainsKey(key))
                    {
                        isDone = false;
                        continue;
                    }
                    var handler = mDownLoadingHandlers[key];
                    sum += handler.progress;
                    if(!handler.isDone)
                    {
                        isDone = false;
                    }
                }
                
                if(isDone)
                {
                    percent = 1.0f;
                }
                else
                {
                    percent = sum / length;
                }
                ansyDownloadActionListener.actionListener.Invoke(percent);

                if(isDone)
                {
                    if(null != ansyDownloadActionListener.actionDone)
                    {
                        ansyDownloadActionListener.actionDone.Invoke();
                    }
                }
            }
        }

        public void AddDownLoadActionListener(string key,string version,string[] assetBundles,UnityAction<float> actionListener,UnityAction onActionDone)
        {
            if(!ansyDownLoadOperations.ContainsKey(key))
            {
                var keys = new string[assetBundles.Length];
                for(int i = 0 ; i < assetBundles.Length; ++i)
                {
                    keys[i] = string.Format(version + "_" + assetBundles[i]);
                }
                ansyDownLoadOperations.Add(key,new AssetBundleAnsyDownLoadListener
                {
                    keys = keys,
                    assetBundles = assetBundles,
                    actionListener = actionListener,
                    actionDone = onActionDone,
                });
            }
            else
            {
                var ansyDownLoadListener = ansyDownLoadOperations[key];
                ansyDownLoadListener.actionListener = System.Delegate.Remove(ansyDownLoadListener.actionListener,actionListener) as UnityAction<float>;
                ansyDownLoadListener.actionListener = System.Delegate.Combine(ansyDownLoadListener.actionListener,actionListener) as UnityAction<float>;
                ansyDownLoadListener.actionDone = System.Delegate.Remove(ansyDownLoadListener.actionDone,onActionDone) as UnityAction;
                ansyDownLoadListener.actionDone = System.Delegate.Combine(ansyDownLoadListener.actionDone,onActionDone) as UnityAction;
            }
        }

        public void RemoveDownLoadActionListener(string key,UnityAction<float> actionListener)
        {
            if(ansyDownLoadOperations.ContainsKey(key))
            {
                var ansyDownLoadListener = ansyDownLoadOperations[key];
                ansyDownLoadListener.actionListener = System.Delegate.Remove(ansyDownLoadListener.actionListener,actionListener) as UnityAction<float>;
            }
        }

        protected class AssetBundleAnsyLoadListener
        {
            public List<UnityWebRequestAsyncOperation> items;
            public UnityAction<float> actionListener;
            public int amount;
            public int status;
            public void Invoke()
            {
                float sum = 0.0f;
                for(int i = 0 ; i < items.Count ; ++i)
                {
                    sum += items[i].progress;
                }
                float radio = sum / amount;
                actionListener.Invoke(radio);
            }
        }

        protected List<AssetBundleAnsyLoadListener> ansyOperations = new List<AssetBundleAnsyLoadListener>(16);
        protected void UpdateProcess()
        {
            for(int i = 0 ; i < ansyOperations.Count ; ++i)
            {
                ansyOperations[i].Invoke();
            }
        }

        protected IEnumerator LoadAssetBundles(string path,string[] bundleNames,
                    UnityAction<string,AssetBundle> onBundleLoadSucceed = null,
                    UnityAction onSucceed = null,
                    UnityAction onFailed = null,
                    UnityAction<float> actionListener = null)
        {
            AssetBundleAnsyLoadListener listener = null;
            if(null != actionListener)
            {
                listener = new AssetBundleAnsyLoadListener
                {
                    items = new List<UnityWebRequestAsyncOperation>(bundleNames.Length),
                    actionListener = actionListener,
                    amount = bundleNames.Length,
                };
                ansyOperations.Add(listener);

                actionListener.Invoke(0.0f);
                yield return null;
            }
            
            bool succeed = true;
            for(int i = 0 ; i < bundleNames.Length  && succeed ; ++i)
            {
                var url = System.IO.Path.Combine(path,bundleNames[i]);
                Debug.LogFormat("[download]:url:{0}",url);
                yield return LoadAssetBundle(url,onBundleLoadSucceed,()=>{succeed = false;},listener);
            }

            var action = succeed ? onSucceed : onFailed;
            if(null != action)
            {
                action.Invoke();
            }

            if(succeed)
            {
                actionListener.Invoke(1.0f);
                yield return null;
            }

            if(null != listener)
                ansyOperations.Remove(listener);
        }

        protected IEnumerator LoadAssetBundle(string url,
                    UnityAction<string,AssetBundle> onSucceed = null,
                    UnityAction onFailed = null,
                    AssetBundleAnsyLoadListener actionListener = null)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                var downloadHandler = new DownloadHandlerAssetBundle(url, 0);
                www.downloadHandler = downloadHandler;

                var webRequest = www.SendWebRequest();
                if (null != actionListener)
                {
                    actionListener.items.Add(webRequest);
                }

                yield return webRequest;

                var bundleName = System.IO.Path.GetFileNameWithoutExtension(url);

                if (!www.isDone)
                {
                    Debug.LogFormat("download:[<color=#00ffff>{0}</color>] failed ...", bundleName);
                    if (null != onFailed)
                    {
                        onFailed.Invoke();
                    }
                    yield break;
                }

                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.LogFormat("download:[<color=#00ffff>{0}</color>]:failed:[{1}]", bundleName, www.error);
                    if (null != onFailed)
                    {
                        onFailed.Invoke();
                    }
                    yield break;
                }

                if (null == downloadHandler.assetBundle)
                {
                    Debug.LogFormat("download:[<color=#00ffff>{0}</color>] failed assetbundle is null", bundleName);
                    if (null != onFailed)
                    {
                        onFailed.Invoke();
                    }
                    yield break;
                }

                Debug.LogFormat("download:[<color=#00ffff>{0}</color>] succeed", bundleName);
                if (null != onSucceed)
                {
                    onSucceed.Invoke(bundleName, downloadHandler.assetBundle);
                }
            }
        }

        AssetBundleManifest manifest = null;
        public string[] GetAllDependencyes(string[] bundleNames)
        {
            Dictionary<string,int> alreadyExist = new Dictionary<string, int>();
            for(int i = 0 ; i < bundleNames.Length ; ++i)
            {
                var bundleName = bundleNames[i];
                if(!alreadyExist.ContainsKey(bundleName))
                {
                    alreadyExist.Add(bundleName,0);
                }

                var dependes = manifest.GetAllDependencies(bundleName);
                for(int j = 0 ; j < dependes.Length ; ++j)
                {
                    if(!alreadyExist.ContainsKey(dependes[j]))
                    {
                        alreadyExist.Add(dependes[j],0);
                    }
                }
            }

            var iter = alreadyExist.GetEnumerator();
            string[] values = new string[alreadyExist.Count];
            for(int i = 0 ; i < values.Length ; ++i)
            {
                iter.MoveNext();
                values[i] = iter.Current.Key;
            }

            return values;
        }

        public void AnsyLoadAssetBundles(string path,string[] bundleNames,
                    UnityAction onSucceed = null,
                    UnityAction onFailed = null,
                    UnityAction<float> actionListener = null)
        {
            var dependes = GetAllDependencyes(bundleNames);
            StartCoroutine(LoadAssetBundles(path,bundleNames,OnAssetBundleLoaded,onSucceed,onFailed,actionListener));
        }

        public IEnumerator LoadAssetBundlesEnumerator(string path,string[] bundleNames,
                    UnityAction onSucceed = null,
                    UnityAction onFailed = null,
                    UnityAction<float> actionListener = null)
        {
            var dependes = GetAllDependencyes(bundleNames);
            yield return LoadAssetBundles(path,dependes,OnAssetBundleLoaded,onSucceed,onFailed,actionListener);
        }

        public IEnumerator LoadAssetBundleManifest(string server,string version,UnityAction onSucceed,UnityAction onFailed)
        {
            var url = Function.getAssetBundleDownloadUrl(server, version, Function.getPlatformString());
            Logger.LogFormat("LoadAssetBundleManifest:url:[{0}]", url);
            yield return LoadAssetBundle(url, (string bundleName, AssetBundle assetBundle) =>
             {
                 manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                 if(null == manifest)
                 {
                     Logger.LogFormat("LoadAssetBundleManifest:Failed ...");
                     if (null != onFailed)
                     {
                         onFailed.Invoke();
                     }
                     return;
                 }
                 Logger.LogFormat("LoadAssetBundleManifest:Succeed ...");
                 if (null != onSucceed)
                 {
                     onSucceed.Invoke();
                 }
                 assetBundle.Unload(false);
             },onFailed,null);
        }

        public string getAssetBundleStreamingAssetPath(string bundleName)
        {
            return string.Format("AssetBundles/{0}/{1}", Function.getPlatformString(),bundleName);
        }

        public IEnumerator LoadAssetBundleManifest(UnityAction onSucceed, UnityAction onFailed)
        {
            var url = getAssetBundleStreamingAssetPath(Function.getPlatformString());
            Logger.LogFormat("LoadAssetBundleManifest:url:[{0}]", url);
            yield return LoadAssetBundle(url, (string bundleName, AssetBundle assetBundle) =>
            {
                manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                if (null == manifest)
                {
                    Logger.LogFormat("LoadAssetBundleManifest:Failed ...");
                    if (null != onFailed)
                    {
                        onFailed.Invoke();
                    }
                    return;
                }
                Logger.LogFormat("LoadAssetBundleManifest:Succeed ...");
                if (null != onSucceed)
                {
                    onSucceed.Invoke();
                }
            }, onFailed, null);
        }

        Dictionary<string,AssetBundle> mAssetBundleMap = new Dictionary<string, AssetBundle>();
        protected void OnAssetBundleLoaded(string bundleName,AssetBundle assetBundle)
        {
            if(null != assetBundle)
            {
                Logger.LogFormat("Load [{0}] succeed ...",bundleName);
                mAssetBundleMap.Add(bundleName,assetBundle);
            }
        }

        protected AssetBundle GetLoadedAssetBundle(string bundleName)
        {
            if(mAssetBundleMap.ContainsKey(bundleName))
            {
                return mAssetBundleMap[bundleName];
            }
            return null;
        }

        public void UnLoadAssetBundles(string[] bundleNames)
        {
            for(int i = 0 ; i < bundleNames.Length ; ++i)
            {
                var bundleName = bundleNames[i];
                if(mAssetBundleMap.ContainsKey(bundleName))
                {
                    var assetBundle = mAssetBundleMap[bundleName];
                    mAssetBundleMap.Remove(bundleName);
                    if(null != assetBundle)
                    {
                        assetBundle.Unload(false);
                        Logger.LogFormat("UnLoad [{0}] succeed ...",bundleName);
                    }
                }
            }
        }

        public GameObject LoadGameObjectFromResourceFolder(string prefabPath,GameObject parent = null, bool worldPosStay = false)
        {
            var instance = Resources.Load<GameObject>(prefabPath);
            if (null == instance)
            {
                Logger.LogFormat("Load GameObject From [{0}] Failed", prefabPath);
                return null;
            }

            var objectHandle = Object.Instantiate(instance, parent.transform, worldPosStay);
            Logger.LogFormat("Load GameObject From [{0}] Succeed ...", prefabPath);

            return objectHandle;
        }

        public GameObject LoadGameObject(string bundleName,string prefabName,GameObject parent = null,bool worldPosStay = false)
        {
            var assetBundle = GetLoadedAssetBundle(bundleName);
            if(null == assetBundle)
            {
                Logger.LogFormat("LoadGameObject For [{0}] Failed , AssetBundle Named [{1}] Has Not Been Loaded Yet ...",prefabName,bundleName);
                return null;
            }

            var instance = assetBundle.LoadAsset<GameObject>(prefabName);
            if(null == instance)
            {
                Logger.LogFormat("Has No GameObject Named [{0}] In [{1}] AssetBundle ...",prefabName,bundleName);
                return null;
            }

            var objectHandle = Object.Instantiate(instance,parent.transform,worldPosStay);
            Logger.LogFormat("Load GameObject [{0}] From [{1}] Succeed ...",prefabName,bundleName);

            return objectHandle;
        }

        public T LoadAsset<T>(string bundleName,string assetName) where T : Object
        {
            var assetBundle = GetLoadedAssetBundle(bundleName);
            if(null == assetBundle)
            {
                Logger.LogFormat("LoadAsset For [{0}] Failed , AssetBundle Named [{1}] Has Not Been Loaded Yet ...",assetName,bundleName);
                return null;
            }

            T instance = assetBundle.LoadAsset<T>(assetName);
            if(null == instance)
            {
                Logger.LogFormat("Has No {0} Named [{1}] In AssetBundle:[{2}] ...",typeof(T).Name,assetName,bundleName);
                return null;
            }

            return instance;
        }

        protected void Update()
        {
            HttpDownLoadHandle.Update();
            UpdateProcess();
            UpdateDownLoadProgress();
        }

        protected void OnDestroy()
        {
            HttpDownLoadHandle.Abort();
        }
    }
}