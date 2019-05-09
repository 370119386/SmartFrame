using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Smart
{
    public class MatchItem
    {
        public double radio = 1.0f;
        public string modelName = string.Empty;
        public float matchRadio = 0.0f;
    }

    public class UIAdapter : MonoBehaviour
    {
        static MatchItem[] ms_match_list = new MatchItem[]
        {
            new MatchItem
            {
                radio = 2436 * 1.0 / 1125,
                modelName = "iphoneX",
                matchRadio = 1.0f,
            },
            new MatchItem
            {
                radio = 1792 * 1.0 / 828,//2.164251
                modelName = "IPhoneXR,iphoneXSMax",
                matchRadio = 1.0f,
            },
            new MatchItem
            {
                radio = 1334 * 1.0 / 750,//1.778
                modelName = "iphone6",
                matchRadio = 1.0f,
            },
            new MatchItem
            {
                radio = 1920 * 1.0 / 1080,//1.77777778
                modelName = "iphone6Plus",
                matchRadio = 1.0f,
            },
            new MatchItem
            {
                radio = 2160 * 1.0 / 1080,//2
                modelName = "小米MIX 2S",
                matchRadio = 1.0f,
            },
            new MatchItem
            {
                radio = 2732 * 1.0 / 2048,//1.3339843
                modelName = "IpadPro_12_9",
                matchRadio = 0.0f,
            },
            new MatchItem
            {
                radio = 2732 * 1.0 / 2048,//1.431654676258993
                modelName = "IpadPro_11",
                matchRadio = 0.0f,
            },
            new MatchItem
            {
                radio = 2244 * 1.0 / 1668,//1.345323741007194
                modelName = "IpadPro_10_5",
                matchRadio = 0.0f,
            },
            new MatchItem
            {
                radio = 2048 * 1.0 / 1536,//1.333333
                modelName = "IpadPro_9_7,IpadMini4_7_9",
                matchRadio = 0.0f,
            },
        };

        public static MatchItem getMatchItem()
        {
            MatchItem ret = ms_match_list[0];
            double currentRadio = Screen.width / Screen.height;
            Debug.LogFormat("[screen]=[{0} X {1}]=[{2}]", Screen.width, Screen.height, currentRadio);
            double radioMin = ms_match_list[0].radio - currentRadio;
            if (radioMin < 0)
            {
                radioMin = -radioMin;
            }

            for (int i = 1; i < ms_match_list.Length; ++i)
            {
                var currentDelta = ms_match_list[i].radio - currentRadio;
                if (currentDelta < 0)
                {
                    currentDelta = -currentDelta;
                }

                if (currentDelta < radioMin)
                {
                    ret = ms_match_list[i];
                    radioMin = currentDelta;
                }
            }
            return ret;
        }

        // Use this for initialization
        void Start()
        {
            var matchItem = getMatchItem();
            if (null != matchItem)
            {
                Debug.LogFormat("Current Screen Resolution is {0}", matchItem.modelName);
                CanvasScaler scaler = GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.matchWidthOrHeight = matchItem.matchRadio;
                }
            }
        }

    }
}