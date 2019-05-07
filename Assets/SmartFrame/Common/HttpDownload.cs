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
        protected UnityAction<string> onFailed;

        protected const int threadsCount = 2;
        protected static Thread[] threads = new Thread[threadsCount];
        protected static Dictionary<string,HttpDownLoadHandle> downloadList = new Dictionary<string, HttpDownLoadHandle>(32);
        protected static List<HttpDownLoadHandle> unstartedList = new List<HttpDownLoadHandle>(32);
        protected static object lock_obj = new object();
        protected static List<UnityAction> downloadActions = new List<UnityAction>(32);
        protected static List<UnityAction> actions = new List<UnityAction>(32);

        protected const int ReadWriteTimeOut = 2 * 1000;//超时等待时间
        protected const int TimeOutWait = 5 * 1000;//超时等待时间

        protected static long HttpGetFileLength(string url)
        {
            //UnityEngine.Debug.Log(url);
            HttpWebRequest requet = HttpWebRequest.Create(url) as HttpWebRequest;
            requet.Method = "HEAD";
            HttpWebResponse response = requet.GetResponse() as HttpWebResponse;
            return response.ContentLength;
        }

        protected static void Run()
        {
            if(unstartedList.Count <= 0)
            {
                Thread.Sleep(150);
            }
            else
            {
                HttpDownLoadHandle handler = null;
                lock (lock_obj)
                {
                    if(unstartedList.Count > 0)
                    {
                        handler = unstartedList[0];
                        unstartedList.RemoveAt(0);
                    }
                }

                if(null != handler)
                {
                    if (!Directory.Exists(handler.storepath))
                    {
                        Directory.CreateDirectory(handler.storepath);
                    }

                    var filePath = System.IO.Path.Combine(handler.storepath, handler.fileName);
                    using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        long fileLength = fs.Length;
                        long totalLength = 0L;// HttpGetFileLength(handler.url);

                        if (true || fileLength < totalLength)
                        {
                            fs.Seek(fileLength, SeekOrigin.Begin);

                            HttpWebRequest request = HttpWebRequest.Create(handler.url) as HttpWebRequest;
                            //request.ReadWriteTimeout = ReadWriteTimeOut;
                            //request.Timeout = TimeOutWait;
                            if(fileLength > 0)
                                request.AddRange((int)fileLength);

                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                            try
                            {
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
                                    lock (lock_obj)
                                    {
                                        downloadActions.Add(handler.onSucceed);
                                        handler.onSucceed = null;
                                    }
                                }
                            }
                            catch (System.Exception e)
                            {
                                var errMsg = string.Format("download:[{0}] failed errMsg:[{1}]", handler.fileName, e.Message);
                                var actionFailed = handler.onFailed;
                                handler.onFailed = null;

                                lock (lock_obj)
                                {
                                    downloadActions.Add(() =>
                                    {
                                        if (null != actionFailed)
                                        {
                                            actionFailed.Invoke(errMsg);
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        public static HttpDownLoadHandle Get(string url,string storepath,string fileName, UnityAction onSucceed,UnityAction onFailed)
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
            downLoadHandler.onFailed = (string errMsg) =>
            {
                Debug.LogErrorFormat("download [{0}] failed ... reason:{1}", fileName, errMsg);
                if(null != onFailed)
                {
                    onFailed.Invoke();
                }
            };

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