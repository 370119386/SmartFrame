using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AssetBundleItem
{
    public string key;
    public string crc;
}

public class AssetBundleList : ScriptableObject
{
    public AssetBundleItem[] assetBundleItems = new AssetBundleItem[0];
}