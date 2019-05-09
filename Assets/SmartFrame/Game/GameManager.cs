using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smart.Common;
using Smart.Module;

namespace Smart
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("游戏主要相机")]
        public Camera mainCamera;

        [SerializeField]
        protected GameConfig gameConfig;
        public GameConfig GameConfig
        {
            get
            {
                return gameConfig;
            }
        }

        protected static GameManager ms_instance;
        public static GameManager Instance()
        {
            return ms_instance;
        }

        protected void Awake()
        {
            ModuleManager.Instance().RegisterModules();
            ModuleManager.Instance().AwakeModules();
        }

        void Start()
        {
            ms_instance = this;
            DontDestroyOnLoad(gameObject);

            var baseModule = ModuleManager.Instance().GetModule(EnumModuleType.EMT_BASE_MODULE);
            baseModule.Create(this);

            GameLoading.Loading(baseModule.AnsyEnter(),()=>{return true;},null,null);
        }

        protected void OnDestroy()
        {
            var baseModule = ModuleManager.Instance().GetModule(EnumModuleType.EMT_BASE_MODULE);
            baseModule.Exit();
        }
    }
}