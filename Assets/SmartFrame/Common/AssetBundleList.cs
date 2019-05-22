using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AssetBundleItem
{
    public string key;
    public string md5;
}

[CreateAssetMenu]
public class AssetBundleList : ScriptableObject
{
    public AssetBundleItem[] assetBundleItems = new AssetBundleItem[0];

    protected Dictionary<string,AssetBundleItem> itemDic;
    public void Make()
    {
        if(null == itemDic)
        {
            itemDic = new Dictionary<string, AssetBundleItem>(assetBundleItems.Length);
        }

        for(int i = 0 ; i < assetBundleItems.Length ; ++i)
        {
            var item = assetBundleItems[i];
            if(null == item)
                continue;
            if(!itemDic.ContainsKey(item.key))
            {
                itemDic.Add(item.key,item);
            }
        }
    }
    public string getFileMd5(string key)
    {
        if(null != itemDic && itemDic.ContainsKey(key))
        {
            return itemDic[key].md5;
        }
        return string.Empty;
    }
}