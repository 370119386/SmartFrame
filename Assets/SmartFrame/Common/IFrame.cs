using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Smart.Common
{
    public interface IFrame
    {
        void Open(int frameId,GameObject root,object argv);
        void Close();
        int frameId {get;}
    }
}