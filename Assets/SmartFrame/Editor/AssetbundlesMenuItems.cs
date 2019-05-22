// using UnityEngine;
// using UnityEditor;
// using System.Collections.Generic;
// using System.IO;

// public class AssetbundlesMenuItems
// {
//     // public static void MakeHFData()
//     // {
//     //     //BuildPublicAssets();
//     //     //创建版本Asset文件
//     //     string storePath = @"Assets/Resources/Data/";
//     //     if (!Directory.Exists(storePath))
//     //     {
//     //         Directory.CreateDirectory(storePath);
//     //     }
//     //     var fileMd5List = NI.Scriptablity.Create<FileMd5List>(storePath, "FileMd5List");
//     //     if (null == fileMd5List)
//     //     {
//     //         Debug.LogErrorFormat("MakeFileMd5List Failed ...");
//     //         return;
//     //     }

//     //     var platform = BuildScript.GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
//     //     var bundleRootPath = Application.dataPath + "/../AssetBundles/" + platform + "/";
//     //     bundleRootPath = System.IO.Path.GetFullPath(bundleRootPath);
//     //     var platformBundlePath = System.IO.Path.Combine(bundleRootPath, platform);

//     //     var assetBundle = AssetBundle.LoadFromFile(platformBundlePath);
//     //     if (null != assetBundle)
//     //     {
//     //         var abManifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
//     //         if (null != abManifest)
//     //         {
//     //             //Debug.LogFormat("<color=#00ff00>load manifest succeed ...</color>");
//     //             var assetBundles = abManifest.GetAllAssetBundles();
//     //             Debug.LogFormat("<color=#00ff00>[bundles]:count=[{0}]</color>",assetBundles.Length);
//     //             var excludeFileName = @"filemd5";
//     //             List<Md5Pair> pairList = new List<Md5Pair>();
//     //             for (int i = 0; i < assetBundles.Length; ++i)
//     //             {
//     //                 if(assetBundles[i].Equals(excludeFileName))
//     //                 {
//     //                     continue;
//     //                 }
//     //                 var filePath = System.IO.Path.Combine(bundleRootPath, assetBundles[i]);
//     //                 var md5 = CommonFunction.GetMD5HashFromFile(filePath);
//     //                 if(!string.IsNullOrEmpty(md5))
//     //                 {
//     //                     pairList.Add(new Md5Pair
//     //                     {
//     //                         key = assetBundles[i],
//     //                         md5 = md5,
//     //                         crc = computeCRC(bundleRootPath, assetBundles[i]),
//     //                     });
//     //                     Debug.LogFormat("<color=#00ff00>[{0}]:[{1}]</color>",assetBundles[i],md5);
//     //                 }
//     //             }
//     //             fileMd5List.md5pairs = pairList.ToArray();
//     //             var baseBundles = getPlatformBaseBundles();
//     //             List<BaseAssetBundleInfo> baseAssetBundlesInfo = new List<BaseAssetBundleInfo>(baseBundles.Length);
//     //             for(int i = 0; i < baseBundles.Length; ++i)
//     //             {
//     //                 baseAssetBundlesInfo.Add(new BaseAssetBundleInfo
//     //                 {
//     //                     name = baseBundles[i],
//     //                     url = string.Empty,
//     //                 });
//     //             }
//     //             fileMd5List.baseBundles = baseAssetBundlesInfo.ToArray();
//     //             EditorUtility.SetDirty(fileMd5List);
//     //             AssetDatabase.SaveAssets();
//     //             Debug.LogFormat("<color=#00ff00>bundle md5 compute succeed ...</color>");
//     //         }
//     //         assetBundle.Unload(true);
//     //     }
//     // }

//     [MenuItem("AssetBundles/BuildPublicAssets")]
//     static public void BuildFinalAssets()
//     {
//         MoveTable2ResourcesFolder();
//         BuildPublicAssets();
//         MakeHFData();
//         BuildPublicAssets();
//     }

//     [MenuItem("GameClient/MoveTable2ResourcesFolder")]
//     static public void MoveTable2ResourcesFolder()
//     {
//         string[] srcFiles = new string[]
//         {
//             //typeof(ProtoTable.ModuleTable).Name,
//             //typeof(ProtoTable.GamePurchaseTable).Name,
//         };
//         var srcPath = System.IO.Path.GetFullPath(Application.dataPath + "/UGame/LeSiMath/Data/Table/");
//         var dstPath = System.IO.Path.GetFullPath(Application.dataPath + "/Resources/Data/Table/");
//         for(int i = 0; i < srcFiles.Length; ++i)
//         {
//             try
//             {
//                 var src = srcPath + srcFiles[i] + ".asset";
//                 var dst = dstPath + srcFiles[i] + ".asset";
//                 if (System.IO.File.Exists(dst))
//                 {
//                     System.IO.File.Delete(dst);
//                 }
//                 System.IO.File.Copy(src, dst);
//                 Debug.LogFormat("<color=#00ff00>Copy [{0}] To [{1}] Succeed .</color>", src, dst);
//             }
//             catch (System.Exception ex)
//             {
//                 Debug.LogError(ex.Message);
//             }
//         }
//     }

//     static string[] getPlatformBaseBundles()
//     {
//         var platformBundle = GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
//         string[] files = new string[]
//         {
//             platformBundle,
//             @"lifesinadropscenes",
//             @"commonscenes",
//             @"cartooncinema",
//             @"usercenter",
//             @"lifesinadrop",
//             @"sharedresources",
//             @"science_game_shared",
//             @"usercenterscenes",
//             @"cartooncinemascene",
//             @"startup",
//             @"categories",
//         };
//         return files;
//     }

//     static public void BuildAssetBundles()
//     {
//         BuildPipeline.BuildAssetBundles (outputPath, 0, EditorUserBuildSettings.activeBuildTarget);
//     }

//     //[MenuItem("AssetBundles/BuildPublicAssets")]
//     static public void BuildPublicAssets()
//     {
//         BuildAssetBundles();

//         var platformBundle = GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);

//         var outputPath = Application.dataPath + "/StreamingAssets/AssetBundles/" + platformBundle;
//         if (Directory.Exists(outputPath))
//         {
//             Directory.Delete(outputPath,true);
//         }
//         Directory.CreateDirectory(outputPath);

//         var srcBundlePath = Application.dataPath + "/../AssetBundles/";
//         srcBundlePath = System.IO.Path.GetFullPath(srcBundlePath);
//         srcBundlePath = BuildScript.getBundlePath(srcBundlePath);
//         MoveAssetBundles(srcBundlePath, outputPath, getPlatformBaseBundles());
//     }

//     [MenuItem("AssetBundles/MoveAssetBundle2PersistentPath(UnityEditor)")]
//     static public void MoveAssetBundle2PersistentPath()
//     {
//         BuildAssetBundles();
//         var outputPath = Application.persistentDataPath;
//         var srcBundlePath = Application.dataPath + "/../AssetBundles/";
//         srcBundlePath = System.IO.Path.GetFullPath(srcBundlePath);
//         srcBundlePath = BuildScript.getBundlePath(srcBundlePath);
//         var files = System.IO.Directory.GetFiles(srcBundlePath, "*.manifest");
//         for(int i = 0; i < files.Length; ++i)
//         {
//             files[i] = Path.GetFileNameWithoutExtension(files[i]);
//         }
//         MoveAssetBundles(srcBundlePath, outputPath, files);
//         Debug.LogFormat("<color=#00ff00ff>{0}</color>", outputPath);
//     }

//     static public void MoveAssetBundles(string srcPath,string dstPath,string[] files)
//     {
//         if(!Directory.Exists(srcPath))
//         {
//             Debug.LogErrorFormat("MoveAssetBundles Failed ...");
//             return;
//         }

//         for (int i = 0; i < files.Length; ++i)
//         {
//             if(!CopyAssetBundle(srcPath,dstPath,files[i]))
//             {
//                 return;
//             }
//         }

//         Debug.LogFormat("<color=#00ff00>Move Packaged AssetBundles Succeed ...</color>");
//     }

//     static public bool CopyAssetBundle(string src,string dst,string bundleName)
//     {
//         var bundlePath = System.IO.Path.Combine(src, bundleName);
//         if (!File.Exists(bundlePath))
//         {
//             Debug.LogErrorFormat("SrcFileNotExist {0} ...", bundlePath);
//             return false;
//         }

//         var bundleTargetPath = System.IO.Path.Combine(dst, bundleName);

//         try
//         {
//             if(!Directory.Exists(dst))
//             {
//                 Directory.CreateDirectory(dst);
//             }
//             File.Copy(bundlePath, bundleTargetPath,true);
//             File.Copy(bundlePath + ".manifest", bundleTargetPath + ".manifest",true);
//             //Debug.LogFormat("<color=#00ff00>Copy {0} Succeed ...</color>", bundleName);
//             return true;
//         }
//         catch (System.Exception e)
//         {
//             Debug.LogErrorFormat("failed:{0}", e.Message);
//             Debug.LogErrorFormat("CopyBundle {0} Failed ...", bundleName);
//             return false;
//         }
//     }
// }
