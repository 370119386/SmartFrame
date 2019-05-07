using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

namespace Smart.Common
{
    public static class Function
    {
        public static void CustomActive(this GameObject gameObject, bool active)
        {
            if (null != gameObject)
            {
                if (gameObject.activeSelf != active)
                {
                    gameObject.SetActive(active);
                }
            }
        }

        public static void CustomActive(this Component component, bool active)
        {
            if (null != component && null != component.gameObject)
            {
                if (component.gameObject.activeSelf != active)
                {
                    component.gameObject.SetActive(active);
                }
            }
        }

        public static void Shuffle(int[] intArray)
        {
            for (int i = 0; i < intArray.Length; i++)
            {
                int temp = intArray[i];
                int randomIndex = Random.Range(0, intArray.Length);
                intArray[i] = intArray[randomIndex];
                intArray[randomIndex] = temp;
            }
        }

        public static GameObject FindCanvasRoot(string layername)
        {
            var layer = GameObject.Find(layername);
            return layer;
        }

        static System.DateTime ms_utc_start = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
        public static long GetUtcStamp()
        {
            System.TimeSpan ts = System.DateTime.UtcNow - ms_utc_start;
            long stamp = System.Convert.ToInt64(ts.TotalSeconds * 1000);
            return stamp;
        }

        public static string getPlatformString()
        {
        #if UNITY_IOS
            return "iOS";
        #elif UNITY_ANDROID
            return "Android";
        #else
            return string.Empty;
        #endif
        }

        public static string getAssetBundlePersistentPath(string version,string bundleName,bool useWWW)
        {
            if (useWWW)
            {
            #if UNITY_IOS
                return "file://" + Application.persistentDataPath + "/AssetBundles/" + version + "/" + bundleName;
            #elif UNITY_ANDROID
                return "file:///" + Application.persistentDataPath + "/AssetBundles/" + version + "/" + bundleName;
            #else
                return Application.persistentDataPath + "/AssetBundles/" + version + "/" + bundleName;
            #endif
            }
            else
            {
                return Application.persistentDataPath + "/AssetBundles/" + version + "/" + bundleName;
            }
        }

        public static string getAssetBundleStreamingPath(string version,string bundleName, bool useWWW)
        {
                if (useWWW)
                {

                    #if UNITY_EDITOR
                    return "file://" + Application.streamingAssetsPath + "/AssetBundles/" + bundleName;
                    #elif UNITY_IOS
                    return "file://" + Application.streamingAssetsPath + "/AssetBundles/" + bundleName;
                    #else
                    return Application.streamingAssetsPath + "/AssetBundles/" + bundleName;
                    #endif
                }
                else
                {
                    return Application.streamingAssetsPath + "/AssetBundles/" + bundleName;
                }
        }

        public static string getStreamingAssetsPath(string path)
        {
            #if UNITY_IOS
            var url = @"file://" + System.IO.Path.Combine(Application.streamingAssetsPath, path);
            #else
            var url = System.IO.Path.Combine(Application.streamingAssetsPath, path);
            #endif
            return url;
        }

        public static string getAssetBundleDownloadUrl(string server,string version,string bundleName)
        {
            server = server.TrimEnd('/');
            var url = string.Format("{0}/AssetBundles/V{1}/{2}/{3}",server,version,getPlatformString(),bundleName);
            return url;
        }

        public static string getAssetBundleManifestDownloadUrl(string server,string version)
        {
            server = server.TrimEnd('/');
            var url = string.Format("{0}/AssetBundles/V{1}/{2}/{2}.manifest", server,version, getPlatformString());
            return url;
        }

   
        static StringBuilder S_StringBuidler = new StringBuilder(256);
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    return string.Empty;
                }

                byte[] retVal = new byte[0];
                using (FileStream file = new FileStream(fileName, FileMode.Open))
                {
                    System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    retVal = md5.ComputeHash(file);
                    file.Close();
                }

                S_StringBuidler.Clear();
                for (int i = 0; i < retVal.Length; i++)
                {
                    S_StringBuidler.Append(retVal[i].ToString("x2"));
                }
                return S_StringBuidler.ToString();
            }
            catch (System.Exception ex)
            {
                //Debug.LogErrorFormat("GetMD5HashFromFile fail,error:" + ex.Message);
                return string.Empty;
            }
        }
    }
}