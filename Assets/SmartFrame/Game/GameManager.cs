using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smart.Common;
using Smart.Module;
using Smart.UI;
using Smart.Table;

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

            // AssetBundleManager.Instance().DownLoadAssetBundle(gameConfig.gameResourcesServer,Application.version,@"fileMd5",
            // (AssetBundle bundle)=>
            // {
            //     AssetBundleManifest manifest;
            // }
            // ,null);

            //AssetBundleManager.Instance().DownLoadAssetBundles(gameConfig.gameResourcesServer,Application.version,)

            yield return null;
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