using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smart.Common;

namespace Smart
{
    public class BaseModule : Module<BaseModule>
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

        public override void Awake()
        {
            base.Awake();

            RegisterFunction(INVOKE_ON_CREATE, OnCreate);
            RegisterFunction(INVOKE_ON_ENTER, OnEnter);

            AssetBundleManager.Instance();
            InitCertificateValidation();
        }

        protected void OnEnter()
        {

        }

        protected void OnCreate()
        {
            string[] assetBundles = new string[] { "blood_train" };
            string[] fileMd5s = new string[] { "0xdddddd" };
            AssetBundleManager.Instance().DownLoadAssetBundles(GameManager.Instance().GameConfig.gameResourcesServer, Application.version, assetBundles,fileMd5s);
        }
    }
}