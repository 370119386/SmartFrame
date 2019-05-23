using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smart.Common;
using Smart.Module;
using Smart.UI;
using Smart.Table;
using Smart.Event;

namespace Smart
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("游戏主要相机")]
        public Camera UIMainCamera;

        [SerializeField]
        [Tooltip("游戏层级")]
        public GameObject[] Layers = new GameObject[0];

        [SerializeField]
        [Tooltip("游戏界面配置")]
        protected FrameConfigTable frameConfigs;


        [SerializeField]
        protected GameConfig gameConfig;
        public GameConfig GameConfig
        {
            get
            {
                return gameConfig;
            }
        }

        [SerializeField]
        protected AssetBundleList _bundleList;
        public AssetBundleList AssetBundleList
        {
            get
            {
                return _bundleList;
            }
            set
            {
                _bundleList = value;
            }
        }

        protected Dictionary<string,string> configs = new Dictionary<string,string>();

        protected string Version
        {
            get;set;
        }

        protected static GameManager ms_instance;
        public static GameManager Instance()
        {
            return ms_instance;
        }

        protected void Awake()
        {
            ms_instance = this;
        }

        IEnumerator Start()
        {
            Debug.LogFormat("[GameManager]:Start()");
            DontDestroyOnLoad(gameObject);

            Version = Application.version;
            UIManager.Instance().Initialize(frameConfigs);

            UIManager.Instance().OpenFrame<LoadingFrame>(null,7);

            //加载线上版本
            bool succeed = true;
            yield return AssetBundleManager.Instance().DownLoadTextFile(gameConfig.gameResourcesServer,Function.getPlatformString(),"gameConfig.txt",
            ()=>{succeed = false;},onLoadGameConfigSucceed);
            if(configs.ContainsKey(@"version"))
            {
                Version = configs[@"version"];
            }
            Debug.LogFormat("[version]:{0}",Version);

            AssetBundleList remoteAssetBundleList = null;
            yield return AssetBundleManager.Instance().DownLoadAssetBundle(gameConfig.gameResourcesServer,Version,"filemd5",(AssetBundle bundle)=>
            {
                remoteAssetBundleList = bundle.LoadAsset<AssetBundleList>("AssetBundleMd5List.asset");
                if(null != remoteAssetBundleList)
                {
                    AssetBundleList = remoteAssetBundleList;
                    Debug.LogFormat("download assetbundle succeed ...");
                }
                else
                {
                    Debug.LogFormat("download assetbundle failed ...");
                }
            },
            ()=>
            {
                Debug.LogFormat("download assetbundle failed ...");
            });

            if(null == AssetBundleList)
            {
                Debug.LogErrorFormat("load AssetBundleList failed ...");
                yield break;
            }
            AssetBundleList.Make();

            bool isDone = false;
            AssetBundleManager.Instance().AddDownLoadActionListener("baseBundle",Version,AssetBundleList.baseAssetBundles,OnDownLoadProcess,
            ()=>
            {
                AssetBundleManager.Instance().RemoveDownLoadActionListener("baseBundle",OnDownLoadProcess);
                isDone = true;
                Debug.LogFormat("DownLoad Finish ...");
            });
            AssetBundleManager.Instance().DownLoadAssetBundles(gameConfig.gameResourcesServer,Version,AssetBundleList.baseAssetBundles,AssetBundleList.baseKeys);

            while(!isDone)
                yield return null;

            yield return AssetBundleManager.Instance().LoadAssetBundleManifest(gameConfig.gameResourcesServer,Version,null,()=>
            {
                succeed = false;
            });
            if(!succeed)
            {
                Debug.LogErrorFormat("Load AssetBundleManifest Failed ...");
                yield break;
            }
            
            var storepath = Function.getAssetBundlePersistentPath(Version, string.Empty, true);

            yield return AssetBundleManager.Instance().LoadAssetBundlesEnumerator(storepath,AssetBundleList.baseAssetBundles,null,null,(float value)=>
            {
                EventManager.Instance().SendEvent(Event.Event.EventLoadingProgress,value * 0.50f + 0.50f);
            });

            EventManager.Instance().SendEvent(Event.Event.EventEndLoading);
        }

        protected static void OnDownLoadProcess(float value)
        {
            EventManager.Instance().SendEvent(Event.Event.EventLoadingProgress,value * 0.50f);
        }

        protected void onLoadGameConfigSucceed(string content)
        {
            Debug.LogFormat("[gameconfig]:{0}",content);
            configs.Clear();
            var lines = content.Split('\r','\n');
            for(int i = 0 ; i < lines.Length; ++i)
            {
                var tokens = lines[i].Split(':');
                if(tokens.Length != 2)
                {
                    continue;
                }
                if(!configs.ContainsKey(tokens[0]))
                {
                    configs.Add(tokens[0],tokens[1]);
                }
            }
        }

        protected void OnDestroy()
        {

        }
    }
}