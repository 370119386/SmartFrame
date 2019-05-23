using UnityEngine;
using UnityEditor;
using System.IO;
using Smart.Common;
using System.Collections.Generic;
using System.Linq;

namespace Smart.Editor
{
    public static class BuildAssetBundleScript
    {
        const string kAssetBundlesOutputPath = "AssetBundles";

        static string GetPlatformFolderForAssetBundles(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSX:
                    return "OSX";
                // Add more build targets for your own.
                // If you add more targets, don't forget to add the same platforms to GetPlatformFolderForAssetBundles(RuntimePlatform) function.
                default:
                    return string.Empty;
            }
        }

        public static string getAssetBundleBuildPath(string assetBundleOutPutPath = kAssetBundlesOutputPath)
        {
            return Path.Combine(assetBundleOutPutPath, GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget));
        }

        [MenuItem("Smart/AssetBundles/BuildPublicAssets")]
        public static void BuildPublicAssets()
        {
            try
            {
                var output = getAssetBundleBuildPath(kAssetBundlesOutputPath);
                if (!Directory.Exists(output) )
                    Directory.CreateDirectory (output);
                AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(output,0,EditorUserBuildSettings.activeBuildTarget);
                Debug.LogFormat("BuildPublicAssets Succeed ...");
                ComputeFileCRC();
            }
            catch (System.Exception e)
            {
                Debug.LogFormat("BuildPublicAssets Failed ... [{0}]",e.Message);
            }
        }

        public static void ComputeFileCRC()
        {
            var platform = GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
            var root = System.IO.Path.GetFullPath(Application.dataPath + "/../AssetBundles/" + platform + "/");
            var platformBundle = System.IO.Path.Combine(root,platform);
            var assetBundle = AssetBundle.LoadFromFile(platformBundle);
            if(null == assetBundle)
            {
                Debug.LogErrorFormat("[compute_file_crc]:failed load main assetbundle:[0] failed ...",platformBundle);
                return;
            }
            AssetBundleManifest assetBundleManifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if(null == assetBundleManifest)
            {
                Debug.LogErrorFormat("load assetbundlemanifest failed ...");
                return;
            }

            var assetBundles = assetBundleManifest.GetAllAssetBundles();
            var listItems = assetBundles.ToList();
            listItems.Add(platform);
            assetBundles = listItems.ToArray();
            List<AssetBundleItem> assetBundleItems = new List<AssetBundleItem>(assetBundles.Length);
            for(int i = 0 ; i < assetBundles.Length ; ++i)
            {
                var path = System.IO.Path.Combine(root,assetBundles[i]);
                var fileMd5 = Function.GetMD5HashFromFile(path);
                if(string.IsNullOrEmpty(fileMd5))
                {
                   continue; 
                }
                AssetBundleItem assetBundleItem = new AssetBundleItem();
                assetBundleItem.key = assetBundles[i];
                assetBundleItem.md5 = fileMd5;
                assetBundleItems.Add(assetBundleItem);
                Debug.LogFormat("[assetbundle]:{0}\t\t[md5]:[{1}]",assetBundles[i],fileMd5);
            }
            var storePath = @"Assets/Resources/Data/";;
            var bundleMd5List = Scriptablity.Create<AssetBundleList>(storePath,"AssetBundleMd5List");
            bundleMd5List.assetBundleItems = assetBundleItems.ToArray();
            bundleMd5List.baseAssetBundles = new string[]
            {
                platform,
                "table",
            };
            EditorUtility.SetDirty(bundleMd5List);
            AssetDatabase.SaveAssets();
            assetBundle.Unload(true);
        }
    }   
}
