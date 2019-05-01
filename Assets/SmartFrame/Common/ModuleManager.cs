using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Smart.Common
{
    public interface ILogger
    {
        void SetModuleName(string name);
        void LogFormat(string fmt, params object[] argv);
    }

    public interface IModule
    {
        void Initialize(object argv);
        IEnumerator Startup();
        void Finalized();
        string Name();
        ILogger Logger { get; }
    }

    public class ModuleData
    {
        public string moduleName;
        public object argv;
    }

    public class LoggerManager : ILogger
    {
        protected string moduleName;
        public void SetModuleName(string name)
        {
            moduleName = name;
        }

        public void LogFormat(string fmt, params object[] argv)
        {
            try
            {
                var log = string.Format("[<color=#ff00ff>{0}</color>]:[<color=#00ffff>{1}</color>]", moduleName, string.Format(fmt, argv));
                Debug.LogFormat(log);
            }
            catch(System.Exception e)
            {
                Debug.LogFormat("[<color=#ff00ff>{0}</color>]:<color=#ff0000>[Log Exception]:{1}</color>",moduleName,e.Message);
            }
        }
    }

    public class ModuleTemplate<T> : /*Singleton<T>,*/IModule where T : ModuleTemplate<T>,new()
    {
        protected ModuleData userData;
        protected ILogger logger;
        protected List<IEnumerator> enumerators = new List<IEnumerator>();

        public ILogger Logger
        {
            get
            {
                return logger;
            }
        }

        public void Initialize(object argv)
        {
            logger = new LoggerManager();
            logger.SetModuleName(typeof(T).Name);

            logger.LogFormat("Enter Initialize");
            userData = new ModuleData
            {
                argv = argv,
                moduleName = typeof(T).Name,
            };

            OnInitialize();
            logger.LogFormat("Exit Initialize");
        }

        public void Finalized()
        {
            logger.LogFormat("Enter Finalize");
            OnFinalized();
            logger.LogFormat("Exit Finalize");
        }

        public IEnumerator Startup()
        {
            logger.LogFormat("Enter Startup");

            logger.LogFormat("BeginLoading");
            BeginLoading();

            for(int i = 0; i < enumerators.Count; ++i)
            {
                logger.LogFormat("enumerator_task_{0}",i + 1);
                yield return enumerators[0];
            }

            logger.LogFormat("EndLoading");
            EndLoading();

            logger.LogFormat("Exit Startup");
        }

        public string Name()
        {
            if(null != userData)
                return userData.moduleName;
            return @"[ModuleEmptyed]";
        }

        protected virtual void OnInitialize()
        {

        }

        protected virtual void OnFinalized()
        {

        }

        protected virtual void BeginLoading()
        {

        }

        protected virtual void EndLoading()
        {

        }
    }
}