using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smart.Common;

namespace Smart
{
    public class BaseModule : ModuleTemplate<BaseModule>
    {
        protected void InitCertificateValidation()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                System.Security.Cryptography.X509Certificates.X509Chain chain,
                System.Net.Security.SslPolicyErrors sslPolicyErrors)
                {
                    return true; // **** Always accept
                };
        }

        protected override void OnInitialize()
        {
            InitCertificateValidation();

            AssetBundleManager.Instance();

            string[] assetBundles = new string[] { "blood_train" };
            string[] fileMd5s = new string[] { "0xdddddd" };
            AssetBundleManager.Instance().DownLoadAssetBundles("https://resourcekids.66uu.cn/kids/", Application.version, assetBundles,fileMd5s);
        }
    }
}