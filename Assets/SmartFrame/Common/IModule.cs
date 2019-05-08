using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Smart.Common
{
    public interface IModule
    {
        void Awake();
        void Create(object argv);
        void Enter();
        IEnumerator AnsyEnter();
        void Exit();
        void Destroy();
        string Name { get; }
    }
}