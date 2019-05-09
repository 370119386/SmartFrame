using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Spine.Unity;

namespace Smart
{
    public delegate bool CbLoadingResult();
    public delegate void CbLoadingFailed();
    public class GameLoading : MonoBehaviour
    {
        [SerializeField]
        protected CameraAdapter gameCamera;
        [SerializeField]
        protected SkeletonAnimation loadingAnimation;
        [SerializeField]
        protected Text loadingText;
        [SerializeField]
        protected AudioClip loadingSoundEffect;
        [SerializeField]
        protected string[] loadingStartAnimation = new string[] { @"loading-1", @"loading-2", @"loading-3" };
        [SerializeField]
        protected string[] loadingContents = new string[]
        {
            @"加载中",@"加载中.",@"加载中..",@"加载中...",
        };

        protected enum LoadingStage
        {
            LS_START = 0,
            LS_RUNNING,
            LS_END,
            LS_TRIGGER_FINISH,
        }
        protected LoadingStage eLoadingStage = LoadingStage.LS_START;

        protected static IEnumerator gEnumeratorRunning;
        protected static CbLoadingResult gLoadingResult;
        protected static IEnumerator gActionEnd;
        protected static CbLoadingFailed gActionFailed;

        protected IEnumerator enumeratorRunning;
        protected CbLoadingResult actionLoadingResult;
        protected IEnumerator enumeratorEnd;
        protected CbLoadingFailed actionFailed;

        protected float lastTime = -1.0f;
        protected int idx = -1;
        protected void NextLoadingText()
        {
            if(lastTime < 0.0f)
            {
                lastTime = Time.time;
            }
            else
            {
                if(Time.time < lastTime + 0.50f)
                {
                    return;
                }
                lastTime = Time.time;
            }

            if(loadingContents.Length > 0)
            {
                idx = (idx + 1) % loadingContents.Length;
                if (null != loadingText)
                {
                    loadingText.text = loadingContents[idx];
                }
            }
        }

        void Start()
        {
            SetLoadingActions(gEnumeratorRunning,gLoadingResult, gActionEnd,gActionFailed);
            gEnumeratorRunning = null;
            gLoadingResult = null;
            gActionEnd = null;
            gActionFailed = null;
            NextLoadingText();
            StartCoroutine(LoadingRunning());
        }

        private void Update()
        {
            NextLoadingText();
        }

        IEnumerator LoadingRunning()
        {
            eLoadingStage = LoadingStage.LS_START;
            Debug.LogFormat("[gameLoading]:{0}", eLoadingStage);

            var begin = Time.time;
            if (loadingAnimation != null)
            {
                //AudioManager.Instance.PlayMusic(loadingSoundEffect,1.0f,false,false);

                loadingAnimation.AnimationState.SetAnimation(0, loadingStartAnimation[(int)eLoadingStage], false);

                loadingAnimation.AnimationState.Complete += (Spine.TrackEntry trackEntry) =>
                {
                    if (null != trackEntry)
                    {
                        string animationName = trackEntry.Animation.Name;

                        if (animationName == loadingStartAnimation[(int)LoadingStage.LS_START])
                        {
                            eLoadingStage = LoadingStage.LS_RUNNING;
                            loadingAnimation.AnimationState.SetAnimation(0, loadingStartAnimation[(int)eLoadingStage], true);
                        }
                        //else if(animationName == loadingStartAnimation[(int)LoadingStage.LS_END])
                        //{
                            //eLoadingStage = LoadingStage.LS_TRIGGER_FINISH;
                        //}
                    }
                };
            }

            while (eLoadingStage == LoadingStage.LS_START)
            {
                yield return null;
            }

            Debug.LogFormat("[gameLoading]:{0}", eLoadingStage);
            yield return enumeratorRunning;

            var delta = Time.time - begin;
            if(delta < loadingSoundEffect.length)
            {
                yield return new WaitForSeconds(loadingSoundEffect.length - delta);
            }

            if (!actionLoadingResult())
            {
                if(null != actionFailed)
                {
                    actionFailed.Invoke();
                }
                //yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("LoadingScene");
                yield break;
            }

            eLoadingStage = LoadingStage.LS_END;
            Debug.LogFormat("[gameLoading]:{0}", eLoadingStage);
            loadingAnimation.AnimationState.SetAnimation(0, loadingStartAnimation[(int)eLoadingStage], false);

            yield return null;
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            yield return new WaitForSeconds(0.30f);
            eLoadingStage = LoadingStage.LS_TRIGGER_FINISH;
            Debug.LogFormat("[gameLoading]:{0}", eLoadingStage);

            yield return enumeratorEnd;

            if (!actionLoadingResult())
            {
                //yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("LoadingScene");
                if(null != actionFailed)
                {
                    actionFailed.Invoke();
                }
            }
        }

        protected void SetLoadingActions(IEnumerator enumeratorRunning, CbLoadingResult actionLoadingResult, IEnumerator enumeratorEnd,CbLoadingFailed actionFailed)
        {
            this.enumeratorRunning = enumeratorRunning;
            this.actionLoadingResult = actionLoadingResult;
            this.enumeratorEnd = enumeratorEnd;
            this.actionFailed = actionFailed;
        }

        public static void Loading(IEnumerator enumeratorRunning, CbLoadingResult actionLoadingResult, IEnumerator enumeratorEnd,CbLoadingFailed actionFailed)
        {
            gEnumeratorRunning = enumeratorRunning;
            gActionEnd = enumeratorEnd;
            gLoadingResult = actionLoadingResult;
            gActionFailed = actionFailed;

            UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScene");
        }
    }
}