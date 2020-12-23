using System;
using UnityEngine;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVMarkerBasedAR;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ArucoModule;
using OpenCVMarkerLessAR;
using OpenCVForUnity.ImgcodecsModule;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Drawing
{
    /// <summary>
    /// WebcamTexture Marker Based AR Example
    /// This code is a rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter2_iPhoneAR using "OpenCV for Unity".
    /// </summary>
    [RequireComponent (typeof(WebCamTextureToMatHelper))]
    public class WebCamDrawing : MonoBehaviour
    {
        /// <summary>
        /// The AR camera.
        /// </summary>
        Camera ARCamera;

        /// <summary>
        /// Gameobject armazenas os objetos 3D
        /// </summary>
        GameObject markerList;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The cameraparam matrix.
        /// </summary>
        Mat camMatrix;

        /// <summary>
        /// The dist coeffs.
        /// </summary>
        MatOfDouble distCoeffs;

        /// <summary>
        /// The dist coeffs.
        /// </summary>
        MatOfDouble distCoeffsMarkerLess;

        /// <summary>
        /// The marker detector.
        /// </summary>
        MarkerDetector markerDetector;
            
        /// <summary>
        /// The matrix that inverts the Y axis.
        /// </summary>
        Matrix4x4 invertYM;

        /// <summary>
        /// The matrix that inverts the Z axis.
        /// </summary>
        Matrix4x4 invertZM;
        
        /// <summary>
        /// The transformation matrix.
        /// </summary>
        Matrix4x4 transformationM;

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        WebCamTextureToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// Lista de objetos 3D
        /// </summary>
        MarkerSettings[] markerSettingsList;

        Mat rgbMat;


        Mat idsAruco;
        List<Mat> cornersAruco;
        Dictionary dictionaryAruco;
        PoseData oldPoseData;


        /// <summary>
        /// The rvecs.
        /// </summary>
        Mat rvecs;

        /// <summary>
        /// The tvecs.
        /// </summary>
        Mat tvecs;

        /// <summary>
        /// The length of the markers' side. Normally, unit is meters.
        /// </summary>
        public float markerLength = 0.1f;

        /// <summary>
        /// The position low pass. (Value in meters)
        /// </summary>
        public float positionLowPass = 0.005f;

        /// <summary>
        /// The rotation low pass. (Value in degrees)
        /// </summary>
        public float rotationLowPass = 2f;

        private int screenWidth;
        private int screenHeight;

        // Use this for initialization
        void Start ()
        {
            GameObject cameraAR = GameObject.Find("ARCamera");
            ARCamera = cameraAR.GetComponent<Camera>();

            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();

#if UNITY_ANDROID && !UNITY_EDITOR
            // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
            webCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
#endif
            webCamTextureToMatHelper.Initialize();

            Texture2D originalTexture = LoadTexture2D();
            Texture2D transparentTexture = GetTransparentTexture(originalTexture);
            
            RawImage imageTransparent = FindObjectOfType<RawImage>();
            imageTransparent.texture = transparentTexture;
            
            Canvas canvas = FindObjectOfType<Canvas>();
            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
            //+imageTransparent.GetComponent<RectTransform>().rect.width
            imageTransparent.rectTransform.sizeDelta = new Vector2(
                canvasRectTransform.rect.width, 
                canvasRectTransform.rect.height);
        }

        private Texture2D GetTransparentTexture(Texture2D texture)
        {
            Color transparentColor = new Color(1.0f, 1.0f, 1.0f, 0f);
            
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    if (!Color.black.Equals(texture.GetPixel(x, y)))
                    {
                        texture.SetPixel(x, y, transparentColor);
                    }
                }
            }

            texture.Apply();
            return texture;
        }

        private Texture2D LoadTexture2D()
        {
            Texture2D Texture = null;
            byte[] fileData;

            
            if (File.Exists(PropertiesModel.PathObjectDrawing))
            {
                fileData = File.ReadAllBytes(PropertiesModel.PathObjectDrawing);
                Texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                Texture.LoadImage(fileData);
            }

            return Texture;
        }
        
        void Update()
        {
            if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
            {
                Mat rgbaMat = webCamTextureToMatHelper.GetMat();
                Utils.fastMatToTexture2D(rgbaMat, texture);
            }
        }

        /// <summary>
        /// Raises the web cam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized() {
            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;
            gameObject.transform.localScale = new Vector3(webCamTextureMat.cols(), webCamTextureMat.rows(), 1);
            
            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();
            
            float imageSizeScale = 1.0f;
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
                imageSizeScale = (float)Screen.height / (float)Screen.width;
            } else {
                Camera.main.orthographicSize = height / 2;
            }
            
            //set cameraparam
            int max_d = (int)Mathf.Max(width, height);
            double fx = max_d;
            double fy = max_d;
            double cx = width / 2.0f;
            double cy = height / 2.0f;
            camMatrix = new Mat (3, 3, CvType.CV_64FC1);
            camMatrix.put(0, 0, fx);
            camMatrix.put(0, 1, 0);
            camMatrix.put(0, 2, cx);
            camMatrix.put(1, 0, 0);
            camMatrix.put(1, 1, fy);
            camMatrix.put(1, 2, cy);
            camMatrix.put(2, 0, 0);
            camMatrix.put(2, 1, 0);
            camMatrix.put(2, 2, 1.0f);
            
            distCoeffs = new MatOfDouble(0, 0, 0, 0);
            distCoeffsMarkerLess = new MatOfDouble(0, 0, 0, 0);

            //calibration camera
            Size imageSize = new Size(width * imageSizeScale, height * imageSizeScale);
            double apertureWidth = 0;
            double apertureHeight = 0;
            double[] fovx = new double[1];
            double[] fovy = new double[1];
            double[] focalLength = new double[1];
            Point principalPoint = new Point(0, 0);
            double[] aspectratio = new double[1];
                      
            Calib3d.calibrationMatrixValues(camMatrix, imageSize, apertureWidth, apertureHeight, fovx, fovy, focalLength, principalPoint, aspectratio);

            //To convert the difference of the FOV value of the OpenCV and Unity. 
            double fovXScale = (2.0 * Mathf.Atan((float)(imageSize.width / (2.0 * fx)))) / (Mathf.Atan2 ((float)cx, (float)fx) + Mathf.Atan2((float)(imageSize.width - cx), (float)fx));
            double fovYScale = (2.0 * Mathf.Atan((float)(imageSize.height / (2.0 * fy)))) / (Mathf.Atan2 ((float)cy, (float)fy) + Mathf.Atan2((float)(imageSize.height - cy), (float)fy));  
            
            //Adjust Unity Camera FOV https://github.com/opencv/opencv/commit/8ed1945ccd52501f5ab22bdec6aa1f91f1e2cfd4
            if (widthScale < heightScale) {
                ARCamera.fieldOfView = (float)(fovx [0] * fovXScale);
            } else {
                ARCamera.fieldOfView = (float)(fovy [0] * fovYScale);
            }
          
            rvecs = new Mat();
            tvecs = new Mat();

            rgbMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC3);
            transformationM = new Matrix4x4();

            invertYM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, -1, 1));
            invertZM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, -1));

            //if WebCamera is frontFaceing,flip Mat.
            webCamTextureToMatHelper.flipHorizontal = webCamTextureToMatHelper.GetWebCamDevice ().isFrontFacing;
        }

        /// <summary>
        /// Raises the web cam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperDisposed");
        }

        /// <summary>
        /// Raises the web cam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred (WebCamTextureToMatHelper.ErrorCode errorCode) {
            Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy () {
            webCamTextureToMatHelper.Dispose ();
        }
        
        public void OnBackMainMenu()
        {
            SceneManager.LoadScene("MainMenuScene");
        }

        public void OnDetectMarker()
        {
            SceneManager.LoadScene("WebCamTextureMarkerLessARExample");
        }
    }
}
