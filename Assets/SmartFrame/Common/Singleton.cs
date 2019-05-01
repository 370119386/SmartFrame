using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Smart.Common
{
    public class Singleton<T> where T : Singleton<T>,new()
    {
        static T ms_handle;
        public static T Instance()
        {
            if(null == ms_handle)
            {
                ms_handle = new T();
            }
            return ms_handle;
        }
    }
}