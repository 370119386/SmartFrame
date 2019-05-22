using UnityEngine;
using UnityEditor;
using System.IO;
using Smart.Common;

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
            }
            catch (System.Exception e)
            {
                Debug.LogFormat("BuildPublicAssets Failed ... [{0}]",e.Message);
            }
        }
    }   
}
