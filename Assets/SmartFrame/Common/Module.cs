using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Smart.Common
{
    public class ModuleData
    {
        public string moduleName;
        public object argv;
    }

    public class Module<T> : IModule where T : Module<T>,new()
    {
        protected ModuleData userData;
        protected ILogger logger;
        protected List<IEnumerator> enumerators = new List<IEnumerator>();
        protected const int INVOKE_ON_CREATE = 0;
        protected const int INVOKE_ON_ENTER = 1;
        protected const int INVOKE_ON_EXIT = 2;
        protected const int INVOKE_ON_DESTROY = 3;
        protected const int INVOKE_COUNT = 4;
        protected delegate void InvokeAction();
        protected InvokeAction[] actionSlots = new InvokeAction[INVOKE_COUNT];

        public ILogger Logger
        {
            get
            {
                return logger;
            }
        }

        private void InvokeFunction(int functionId)
        {
            if(functionId >= 0 && functionId < actionSlots.Length)
            {
                if(null != actionSlots[functionId])
                {
                    actionSlots[functionId].Invoke();
                }
            }
        }

        protected void RegisterFunction(int functionId, InvokeAction action)
        {
            if (functionId >= 0 && functionId < actionSlots.Length)
            {
                actionSlots[functionId] = action;
            }
        }

        public virtual void Awake()
        {

        }

        public void Create(object argv)
        {
            logger = new ModuleCommonLogger();
            (logger as ModuleCommonLogger).SetModuleName(typeof(T).Name);

            userData = new ModuleData
            {
                argv = argv,
                moduleName = typeof(T).Name,
            };

            logger.LogFormat("OnCreate");
            InvokeFunction(INVOKE_ON_CREATE);
        }

        public void Enter()
        {
            InvokeFunction(INVOKE_ON_ENTER);
        }

        public IEnumerator AnsyEnter()
        {
            InvokeFunction(INVOKE_ON_ENTER);

            for(int i = 0; i < enumerators.Count; ++i)
            {
                yield return enumerators[i];
            }
        }

        public void Exit()
        {
            InvokeFunction(INVOKE_ON_EXIT);
        }

        public void Destroy()
        {
            InvokeFunction(INVOKE_ON_DESTROY);
        }

        protected void AddAnsyTask(IEnumerator iter)
        {
            enumerators.Add(iter);
        }

        protected IEnumerator FadeEnter()
        {
            for (int i = 0; i < enumerators.Count; ++i)
            {
                logger.LogFormat("enumerator_task_{0}",i + 1);
                yield return enumerators[0];
            }
            enumerators.Clear();
        }

        public string Name
        {
            get
            {
                if (null != userData)
                    return userData.moduleName;
                return @"[ModuleEmptyed]";
            }
        }
    }
}