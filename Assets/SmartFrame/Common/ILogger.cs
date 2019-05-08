using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Smart.Common
{
    public interface ILogger
    {
        void LogFormat(string fmt, params object[] argv);
    }
}