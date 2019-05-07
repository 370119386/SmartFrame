using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Smart.Common
{
    [CreateAssetMenu]
    public class GameConfig : ScriptableObject
    {
        [SerializeField]
        [Tooltip("游戏资源服务器目录")]
        public string gameResourcesServer = @"https://resourcekids.66uu.cn/kids/TestAds/";
    }
}