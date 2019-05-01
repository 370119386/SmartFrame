using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smart.Common;

namespace Smart
{
    public enum EnumModuleType
    {
        EMT_BASE_MODULE = 0,
    }

    public class GameManager : MonoBehaviour
    {
        protected Dictionary<EnumModuleType, IModule> mGameModules = new Dictionary<EnumModuleType, IModule>(32);

        protected void RegisterModule(EnumModuleType eModule,IModule module)
        {
            if(!mGameModules.ContainsKey(eModule))
            {
                mGameModules.Add(eModule, module);
            }
        }

        protected void InitModules()
        {
            RegisterModule(EnumModuleType.EMT_BASE_MODULE, new BaseModule());
        }

        public IModule GetModule(EnumModuleType eModule)
        {
            if(mGameModules.ContainsKey(eModule))
            {
                return mGameModules[eModule];
            }
            return null;
        }

        [SerializeField][Tooltip("游戏主要相机")]
        protected Camera mainCamera;

        IEnumerator Start()
        {
            InitModules();

            IModule baseModule = GetModule(EnumModuleType.EMT_BASE_MODULE);
            baseModule.Initialize(this);

            yield return baseModule.Startup();
        }
    }
}