using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  Smart
{
    public class CameraAdapter : MonoBehaviour
    {
        [SerializeField][Tooltip("设计宽度")]
        protected float designedWidth;
        [SerializeField][Tooltip("设计高度")]
        protected float designedHeight;
        [SerializeField][Tooltip("是否横屏")]
        protected bool isLandScape;
        [SerializeField][Tooltip("目标相机")]
        protected Camera gCamera;
        [SerializeField][Tooltip("设计正交视口尺寸")]
        protected float designedOrthographicSize;

        // Start is called before the first frame update
        void Start()
        {
            updateOrthographicSize();
        }

        protected void updateOrthographicSize()
        {
            if(null == gCamera)
            {
                Debug.LogErrorFormat("<color=#ff00ff>[CameraAdapter]</color>: gCamera is null");
                return;
            }

            if(!gCamera.orthographic)
            {
                Debug.LogErrorFormat("<color=#ff00ff>[CameraAdapter]</color>: gCamera is not orthographic camera");
                return;
            }

            float screenHeight = Screen.height;
            float screenWidth = Screen.width;

            if(isLandScape && screenWidth < screenHeight)
            {
                float temp = screenWidth;
                screenWidth = screenHeight;
                screenHeight = temp;
            }

            float aspectRadio = screenWidth * 1.0f / screenHeight;
            float designedAspectRadio = designedWidth / designedHeight;
            float orthographicSize = designedOrthographicSize * designedAspectRadio / aspectRadio;

            if(Mathf.Abs(gCamera.orthographicSize - orthographicSize) > 0.001f)
            {
                gCamera.orthographicSize = orthographicSize;
            }
            
            Debug.LogFormat("<color=#ff00ff>[CameraAdapter]:</color> orth size = <color=#00ffff>[{0:F2}]</color>",gCamera.orthographicSize);
        }
    }
}