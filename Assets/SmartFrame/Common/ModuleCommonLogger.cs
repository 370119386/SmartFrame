using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Smart.Common
{
    public class ModuleCommonLogger : ILogger
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
            catch (System.Exception e)
            {
                Debug.LogFormat("[<color=#ff00ff>{0}</color>]:<color=#ff0000>[Log Exception]:{1}</color>", moduleName, e.Message);
            }
        }
    }
}