﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using OpenCVForUnity.CoreModule;

namespace MarkerBasedARExample
{
    /// <summary>
    /// MarkerBasedAR Example
    /// </summary>
    public class MarkerBasedARExample : MonoBehaviour
    {
        public Text exampleTitle;
        public Text versionInfo;
        public ScrollRect scrollRect;
        static float verticalNormalizedPosition = 1f;

        // Use this for initialization
        void Start ()
        {
            exampleTitle.text = "MarkerBasedAR Example " + Application.version;

            versionInfo.text = Core.NATIVE_LIBRARY_NAME + " " + OpenCVForUnity.UnityUtils.Utils.getVersion () + " (" + Core.VERSION + ")";
            versionInfo.text += " / UnityEditor " + Application.unityVersion;
            versionInfo.text += " / ";

            #if UNITY_EDITOR
            versionInfo.text += "Editor";
            #elif UNITY_STANDALONE_WIN
            versionInfo.text += "Windows";
            #elif UNITY_STANDALONE_OSX
            versionInfo.text += "Mac OSX";
            #elif UNITY_STANDALONE_LINUX
            versionInfo.text += "Linux";
            #elif UNITY_ANDROID
            versionInfo.text += "Android";
            #elif UNITY_IOS
            versionInfo.text += "iOS";
            #elif UNITY_WSA
            versionInfo.text += "WSA";
            #elif UNITY_WEBGL
            versionInfo.text += "WebGL";
            #endif
            versionInfo.text += " ";
            #if ENABLE_MONO
            versionInfo.text += "Mono";
            #elif ENABLE_IL2CPP
            versionInfo.text += "IL2CPP";
            #elif ENABLE_DOTNET
            versionInfo.text += ".NET";
            #endif

            scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;
        }
        
        // Update is called once per frame
        void Update ()
        {
            
        }

        public void OnScrollRectValueChanged ()
        {
            verticalNormalizedPosition = scrollRect.verticalNormalizedPosition;
        }


        public void OnShowLicenseButtonClick ()
        {
            SceneManager.LoadScene ("ShowLicense");
        }

        public void OnShowARMarkerButtonClick ()
        {
            SceneManager.LoadScene ("ShowARMarker");
        }

        public void OnTexture2DMarkerBasedARExampleButtonClick ()
        {
            SceneManager.LoadScene ("Texture2DMarkerBasedARExample");
        }

        public void OnWebCamTextureMarkerBasedARExampleButtonClick ()
        {
            SceneManager.LoadScene ("WebCamTextureMarkerBasedARExample");
        }

        public void OnGyroSensorMarkerBasedARExampleButtonClick ()
        {
            SceneManager.LoadScene ("GyroSensorMarkerBasedARExample");
        }
    }
}