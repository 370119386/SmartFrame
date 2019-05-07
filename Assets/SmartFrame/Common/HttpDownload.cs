using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Net;
using System;
using UnityEngine.Events;

namespace Smart.Common
{
    public class HttpDownLoadHandle
    {
        public float progress { get; private set; }
        public bool isDone { get; private set; }

        protected string url = string.Empty;
        protected string storepath = string.Empty;
        protected string fileName = string.Empty;
        protected UnityAction onSucceed;
        protected UnityAction onFailed;
        protected string checkMd5 = string.Empty;
        protected int downloadCnt = maxDownLoadCnt;

        protected const int threadsCount = 2;
        protected const int maxDownLoadCnt = 5;
        protected static Thread[] threads = new Thread[threadsCount];
        protected static Dictionary<string,HttpDownLoadHandle> downloadList = new Dictionary<string, HttpDownLoadHandle>(32);
        protected static List<HttpDownLoadHandle> unstartedList = new List<HttpDownLoadHandle>(32);
        protected static object lock_obj = new object();
        protected static List<UnityAction> downloadActions = new List<UnityAction>(32);
        protected static List<UnityAction> actions = new List<UnityAction>(32);
        protected static List<LogData> logDatas = new List<LogData>(32);

        protected class LogData
        {
            public string log;

            public static void LogFormat(string fmt,params object[] argv)
            {
                var logData = new LogData();
                var content = string.Format(fmt, argv);
                logData.log = string.Format("<color=#ff00ff>[ERROR]</color>:<color=#00ffff>{0}</color>", content);
                lock(lock_obj)
                {
                    logDatas.Add(logData);
                }
            }

            public static void LogErrorFormat(string fmt, params object[] argv)
            {
                var logData = new LogData();
                var content = string.Format(fmt, argv);
                logData.log = string.Format("<color=#ff00ff>[ERROR]</color>:<color=#ff0000>{0}</color>", content);
                lock (lock_obj)
                {
                    logDatas.Add(logData);
                }
            }
        }

        protected const int ReadWriteTimeOut = 2 * 1000;//超时等待时间
        protected const int TimeOutWait = 5 * 1000;//超时等待时间

        protected static long HttpGetFileLength(string url)
        {
            HttpWebRequest requet = HttpWebRequest.Create(url) as HttpWebRequest;
            requet.Method = "HEAD";
            HttpWebResponse response = requet.GetResponse() as HttpWebResponse;
            return response.ContentLength;
        }

        protected static bool VerifyFileMd5(string filePath, string checkMd5)
        {
            var fileMd5 = Function.GetMD5HashFromFile(filePath);
            if(string.IsNullOrEmpty(fileMd5))
            {
                return false;
            }

            if (!string.Equals(fileMd5, checkMd5))
            {
                return false;
            }

            return true;
        }

        protected static void Run()
        {
            while(true)
            {
                if (unstartedList.Count <= 0)
                {
                    Thread.Sleep(150);
                    return;
                }

                HttpDownLoadHandle handler = null;
                lock (lock_obj)
                {
                    if (unstartedList.Count > 0)
                    {
                        handler = unstartedList[0];
                        unstartedList.RemoveAt(0);
                    }
                }

                if(null == handler)
                {
                    return;
                }

                if (!Directory.Exists(handler.storepath))
                {
                    Directory.CreateDirectory(handler.storepath);
                }

                var filePath = System.IO.Path.Combine(handler.storepath, handler.fileName);
                LogData.LogFormat("[文件下载]:[{0}]:第[{1}]次", handler.fileName, maxDownLoadCnt - handler.downloadCnt + 1);
                LogData.LogFormat("[文件下载]:[{0}]:[{1}]",handler.fileName, filePath);

                try
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        long fileLength = fs.Length;
                        long totalLength = HttpGetFileLength(handler.url);
                        LogData.LogFormat("[文件下载]:[{0}]:远端文件长度[{1}]:本地文件长度[{2}]", handler.fileName, totalLength,fileLength);

                        bool needReCheck = true;
                        if (fileLength == totalLength)
                        {
                            LogData.LogFormat("[文件下载]:[{0}]:文件长度相同", handler.fileName);
                            if (!VerifyFileMd5(handler.fileName, handler.checkMd5))
                            {
                                fs.SetLength(0);
                                fileLength = fs.Length;
                                needReCheck = false;
                                LogData.LogFormat("[文件下载]:[{0}]:校验文件MD5码失败,需要重新下载", handler.fileName);
                            }
                            else
                            {
                                LogData.LogFormat("[文件下载]:[{0}]:校验文件MD5码成功，不需要重新下载", handler.fileName);
                                string fileName = handler.fileName;
                                var actionSucceed = handler.onSucceed;
                                needReCheck = false;
                                lock (lock_obj)
                                {
                                    downloadActions.Add(() =>
                                    {
                                        if (null != actionSucceed)
                                        {
                                            actionSucceed.Invoke();
                                        }
                                    });
                                }
                                return;
                            }
                        }
                        else if (fileLength > totalLength)
                        {
                            LogData.LogFormat("[文件下载]:[{0}]:校验文件长度不对,需要重新下载", handler.fileName);
                            fs.SetLength(0);
                            fileLength = fs.Length;
                            needReCheck = false;
                        }

                        fs.Seek(fileLength, SeekOrigin.Begin);

                        HttpWebRequest request = HttpWebRequest.Create(handler.url) as HttpWebRequest;
                        request.ReadWriteTimeout = ReadWriteTimeOut;
                        request.Timeout = TimeOutWait;

                        if (fileLength > 0)
                            request.AddRange((int)fileLength);

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        bool checkOk = false;
                        using (Stream stream = request.GetResponse().GetResponseStream())
                        {
                            byte[] buffer = new byte[1024];
                            int length = stream.Read(buffer, 0, buffer.Length);
                            while (length > 0)
                            {
                                //如果Unity客户端关闭，停止下载
                                //if (isStop) break;
                                //将内容再写入本地文件中
                                fs.Write(buffer, 0, length);
                                //计算进度
                                fileLength += length;
                                handler.progress = (float)fileLength / (float)totalLength;
                                //类似尾递归
                                length = stream.Read(buffer, 0, buffer.Length);
                            }

                            stream.Close();

                            if (needReCheck)
                            {
                                LogData.LogFormat("[文件下载]:[{0}]:需要重新校验", handler.fileName);
                                checkOk = VerifyFileMd5(handler.fileName, handler.checkMd5);
                            }
                            else
                            {
                                LogData.LogFormat("[文件下载]:[{0}]:不需要重新校验", handler.fileName);
                                checkOk = true;
                            }

                            if (checkOk)
                            {
                                LogData.LogFormat("[文件下载]:[{0}]:文件下载成功", handler.fileName);
                                lock (lock_obj)
                                {
                                    downloadActions.Add(handler.onSucceed);
                                    handler.onSucceed = null;
                                }
                            }
                            else
                            {
                                LogData.LogFormat("[文件下载]:[{0}]:重新校验文件失败,扔回队列等待重下", handler.fileName);
                                fs.SetLength(0);
                                lock (lock_obj)
                                {
                                    unstartedList.Add(handler);
                                }
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    LogData.LogErrorFormat("[文件下载]:[{0}]:下载失败:[{1}]", handler.fileName, e.Message);
                    handler.downloadCnt -= 1;

                    if(handler.downloadCnt <= 0)
                    {
                        LogData.LogErrorFormat("[文件下载]:[{0}]:重下次数已经用完", handler.fileName);
                        var actionFailed = handler.onFailed;
                        handler.onFailed = null;
                        lock (lock_obj)
                        {
                            downloadActions.Add(actionFailed);
                        }
                    }
                    else
                    {
                        LogData.LogErrorFormat("[文件下载]:[{0}]:扔回队列等待重下", handler.fileName);
                        lock (lock_obj)
                        {
                            unstartedList.Add(handler);
                        }
                    }
                }
            }
        }

        public static HttpDownLoadHandle Get(string url,string storepath,string fileName,string checkMd5,UnityAction onSucceed,UnityAction onFailed)
        {
            var key = System.IO.Path.Combine(url, fileName);

            if(null == threads[0])
            {
                for(int i = 0; i < threads.Length; ++i)
                {
                    threads[i] = new Thread(Run);
                    var thread = threads[i];
                    thread.IsBackground = true;
                    thread.Priority = System.Threading.ThreadPriority.BelowNormal;
                    threads[i].Start();
                }
            }

            lock(lock_obj)
            {
                if (downloadList.ContainsKey(key))
                {
                    return downloadList[key];
                }
            }

            HttpDownLoadHandle downLoadHandler = new HttpDownLoadHandle();
            downLoadHandler.progress = 0.0f;
            downLoadHandler.isDone = false;
            downLoadHandler.url = url;
            downLoadHandler.storepath = storepath;
            downLoadHandler.onSucceed = onSucceed;
            downLoadHandler.fileName = fileName;
            downLoadHandler.checkMd5 = checkMd5;
            downLoadHandler.onFailed = onFailed;
            downLoadHandler.downloadCnt = maxDownLoadCnt;

            lock (lock_obj)
            {
                downloadList.Add(key, downLoadHandler);
                unstartedList.Add(downLoadHandler);
            }

            return downLoadHandler;
        }
        public static void Update()
        {
            lock(lock_obj)
            {
                actions.AddRange(downloadActions);
                downloadActions.Clear();
            }

            for(int i = 0; i < actions.Count; ++i)
            {
                if(null != actions[i])
                {
                    actions[i].Invoke();
                }
            }
            actions.Clear();
        }
    }
}