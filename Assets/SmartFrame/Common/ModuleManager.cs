﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smart.Common;

namespace Smart.Module
{
    public enum EnumModuleType
    {
        EMT_BASE_MODULE = 0,
    }

    public class ModuleManager : MonoBehaviour
    {
        protected static ModuleManager ms_instance;
        public static ModuleManager Instance()
        {
            if (null == ms_instance)
            {
                var gameObject = new GameObject("ModuleManager");
                ms_instance = gameObject.AddComponent<ModuleManager>();
            }
            return ms_instance;
        }

        protected void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void AwakeModules()
        {
            var iter = mGameModules.GetEnumerator();
            while(iter.MoveNext())
            {
                iter.Current.Value.Awake();
            }
        }

        protected Dictionary<EnumModuleType, IModule> mGameModules = new Dictionary<EnumModuleType, IModule>(32);
        protected void RegisterModule(EnumModuleType eModule, IModule module)
        {
            if (!mGameModules.ContainsKey(eModule))
            {
                mGameModules.Add(eModule, module);
            }
        }
        public IModule GetModule(EnumModuleType eModule)
        {
            if (mGameModules.ContainsKey(eModule))
            {
                return mGameModules[eModule];
            }
            return null;
        }

        public void RegisterModules()
        {
            RegisterModule(EnumModuleType.EMT_BASE_MODULE, new BaseModule());
        }
    }
}