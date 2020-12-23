using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVMarkerBasedAR;
using System;
using OpenCVForUnity.ArucoModule;
using OpenCVForUnity.ImgprocModule;
using UnityEngine.UI;
using TriLibCore;
using System.IO;

namespace MarkerBasedARExample
{
    /// <summary>
    /// WebcamTexture Marker Based AR Example
    /// This code is a rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter2_iPhoneAR using "OpenCV for Unity".
    /// </summary>
    [RequireComponent (typeof(WebCamTextureToMatHelper))]
    public class WebCamTextureMarkerBasedARExample : MonoBehaviour
    {
        /// <summary>
        /// The AR camera.
        /// </summary>
        public Camera ARCamera;

        /// <summary>
        /// The marker settings.
        /// </summary>
        public MarkerSettings[] markerSettings;
        
        /// <summary>
        /// Determines if should move AR camera.
        /// </summary>
        [Tooltip ("If true, only the first element of markerSettings will be processed.")]
        public bool shouldMoveARCamera;

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
        /// The transformation matrix for AR.
        /// </summary>
        Matrix4x4 ARM;

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        WebCamTextureToMatHelper webCamTextureToMatHelper;

        MarkerSettings markerSettingsMarkerActual;

        MarkerSettings[] markerSettingsList;

        GameObject markerList;

        Mat rgbMat;

        Mat idsAruco;
        
        List<Mat> cornersAruco;
        
        Dictionary dictionaryAruco;

        PoseData oldPoseData;

        public float positionLowPass = 0.005f;

        public float rotationLowPass = 2f;

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

        Dictionary<string, InformationObject> listInformationObject;

        private void Awake()
        {
            markerList = GameObject.Find("/MarkerList");
            CreatComponentMarker();
        }

        private void CreatComponentMarker()
        {
            InformationObjectList informationObjectList = JsonUtility.FromJson<InformationObjectList>(PlayerPrefs.GetString(PropertiesModel.NameBDMarkerPlayerPrefab));

            if (informationObjectList == null)
            {
                return;
            }

            foreach (InformationObject informationObject in informationObjectList.ListInformationObject)
            {
                GameObject ARObject = ImportResources.GetGameObject(informationObject.Name);

                if (ARObject == null)
                {
                    LoadFromPath(informationObject);
                }
                else
                {
                    CreateComponent(informationObject, ARObject);
                }
            }
        }

        private void LoadFromPath(InformationObject informationObject)
        {
            listInformationObject.Add(informationObject.Name, informationObject);

            var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
            string path = GetPathComplete(informationObject.Name);

            AssetLoader.LoadModelFromFile(path, OnLoad, OnMaterialsLoad, OnProgress, OnError, null, assetLoaderOptions);
        }

        private string GetPathComplete(string nameObjectSelected)
        {
            return Path.Combine(Application.persistentDataPath, PropertiesModel.Directory3D, nameObjectSelected.Replace(" ", "") + ".fbx");
        }

        private void CreateComponent(InformationObject informationObject, GameObject ARObject)
        {
            GameObject ARObjects = new GameObject();
            ARObjects.name = "ARObjects";
            ARObjects.SetActive(false);

            GameObject OBJMarkerSettings = new GameObject();
            OBJMarkerSettings.name = "MarkerSettings";

            MarkerDesign markerDesign = new MarkerDesign();
            markerDesign.id = informationObject.IdMarker;

            MarkerSettings markerSettings = OBJMarkerSettings.AddComponent<MarkerSettings>();
            markerSettings.markerDesign = markerDesign;
            markerSettings.ARGameObject = ARObjects;

            GameObject objectCreated = Instantiate(ARObject);

            objectCreated.AddComponent<RectTransform>();
            objectCreated.transform.position = Vector3.zero;
            objectCreated.transform.rotation = Quaternion.identity;
            objectCreated.layer = 8;

            objectCreated.transform.SetParent(ARObjects.transform);
            ARObjects.transform.SetParent(OBJMarkerSettings.transform);
            OBJMarkerSettings.transform.SetParent(markerList.transform);
        }

        // Use this for initialization
        void Start ()
        {
            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper> ();

            markerSettingsMarkerActual = null;
            markerSettingsList = markerList.transform.GetComponentsInChildren<MarkerSettings>();

            dictionaryAruco = Aruco.getPredefinedDictionary(PropertiesModel.DictionaryId);
            cornersAruco = new List<Mat>();
            idsAruco = new Mat();

            //if (markerSettingsList.Length == 0)
            //{
            //    //GameObject.Find("Canvas/Menu/TextSemObjetoDetectar").SetActive(true);
            //}

#if UNITY_ANDROID && !UNITY_EDITOR
            // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
            webCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
#endif
            webCamTextureToMatHelper.Initialize ();
        }

        /// <summary>
        /// Raises the web cam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperInitialized");
            
            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();

            texture = new Texture2D (webCamTextureMat.cols (), webCamTextureMat.rows (), TextureFormat.RGBA32, false);
            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;


            gameObject.transform.localScale = new Vector3 (webCamTextureMat.cols (), webCamTextureMat.rows (), 1);
            
            Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);


            float width = webCamTextureMat.width ();
            float height = webCamTextureMat.height ();
            
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
            int max_d = (int)Mathf.Max (width, height);
            double fx = max_d;
            double fy = max_d;
            double cx = width / 2.0f;
            double cy = height / 2.0f;
            camMatrix = new Mat (3, 3, CvType.CV_64FC1);
            camMatrix.put (0, 0, fx);
            camMatrix.put (0, 1, 0);
            camMatrix.put (0, 2, cx);
            camMatrix.put (1, 0, 0);
            camMatrix.put (1, 1, fy);
            camMatrix.put (1, 2, cy);
            camMatrix.put (2, 0, 0);
            camMatrix.put (2, 1, 0);
            camMatrix.put (2, 2, 1.0f);
            Debug.Log ("camMatrix " + camMatrix.dump ());
            
            distCoeffs = new MatOfDouble (0, 0, 0, 0);
            Debug.Log ("distCoeffs " + distCoeffs.dump ());
            
            //calibration camera
            Size imageSize = new Size (width * imageSizeScale, height * imageSizeScale);
            double apertureWidth = 0;
            double apertureHeight = 0;
            double[] fovx = new double[1];
            double[] fovy = new double[1];
            double[] focalLength = new double[1];
            Point principalPoint = new Point (0, 0);
            double[] aspectratio = new double[1];
            
            
            Calib3d.calibrationMatrixValues (camMatrix, imageSize, apertureWidth, apertureHeight, fovx, fovy, focalLength, principalPoint, aspectratio);
            
            Debug.Log ("imageSize " + imageSize.ToString ());
            Debug.Log ("apertureWidth " + apertureWidth);
            Debug.Log ("apertureHeight " + apertureHeight);
            Debug.Log ("fovx " + fovx [0]);
            Debug.Log ("fovy " + fovy [0]);
            Debug.Log ("focalLength " + focalLength [0]);
            Debug.Log ("principalPoint " + principalPoint.ToString ());
            Debug.Log ("aspectratio " + aspectratio [0]);


            //To convert the difference of the FOV value of the OpenCV and Unity. 
            double fovXScale = (2.0 * Mathf.Atan ((float)(imageSize.width / (2.0 * fx)))) / (Mathf.Atan2 ((float)cx, (float)fx) + Mathf.Atan2 ((float)(imageSize.width - cx), (float)fx));
            double fovYScale = (2.0 * Mathf.Atan ((float)(imageSize.height / (2.0 * fy)))) / (Mathf.Atan2 ((float)cy, (float)fy) + Mathf.Atan2 ((float)(imageSize.height - cy), (float)fy));
            
            Debug.Log ("fovXScale " + fovXScale);
            Debug.Log ("fovYScale " + fovYScale);
            
            
            //Adjust Unity Camera FOV https://github.com/opencv/opencv/commit/8ed1945ccd52501f5ab22bdec6aa1f91f1e2cfd4
            if (widthScale < heightScale) {
                ARCamera.fieldOfView = (float)(fovx [0] * fovXScale);
            } else {
                ARCamera.fieldOfView = (float)(fovy [0] * fovYScale);
            }

            
            MarkerDesign[] markerDesigns = new MarkerDesign[markerSettingsList.Length];
            for (int i = 0; i < markerDesigns.Length; i++) {
                markerDesigns [i] = markerSettingsList[i].markerDesign;
            }

            markerDetector = new MarkerDetector (camMatrix, distCoeffs, markerDesigns);

            rvecs = new Mat();
            tvecs = new Mat();

            rgbMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC3);
            transformationM = new Matrix4x4();


            invertYM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, -1, 1));
            Debug.Log ("invertYM " + invertYM.ToString ());
            
            invertZM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, -1));
            Debug.Log ("invertZM " + invertZM.ToString ());


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
        public void OnWebCamTextureToMatHelperErrorOccurred (WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // Update is called once per frame
        void Update ()
        {
            if (webCamTextureToMatHelper.IsPlaying () && webCamTextureToMatHelper.DidUpdateThisFrame ()) {

                Mat rgbaMat = webCamTextureToMatHelper.GetMat();
                
                
                    Imgproc.cvtColor(rgbaMat, rgbMat, Imgproc.COLOR_RGBA2RGB);

                    markerDetector.processFrame(rgbaMat, 1);

                    foreach (MarkerSettings settings in markerSettingsList)
                    {
                        settings.setAllARGameObjectsDisable();
                    }

                    Aruco.detectMarkers(rgbMat, dictionaryAruco, cornersAruco, idsAruco);

                    if (idsAruco.total() > 0)
                    {
                        for (int i = 0; i < idsAruco.cols(); i++)
                        {
                            int idMarker = (int)idsAruco.get(0, i)[0];
                            Debug.Log(idMarker);

                            if (markerSettingsMarkerActual != null && markerSettingsMarkerActual.getId() == idMarker)
                            {
                                ShowGameObjectMarker();
                            }
                            else
                            {
                                markerSettingsMarkerActual = null;

                                foreach (MarkerSettings markerSettings in markerSettingsList)
                                {
                                    if (idMarker != -1 && idMarker == markerSettings.getId())
                                    {
                                        markerSettingsMarkerActual = markerSettings;
                                        ShowGameObjectMarker();
                                    }
                                }
                            }
                        }
                    }

                Utils.fastMatToTexture2D (rgbaMat, texture);
            }
        }

        private void ShowGameObjectMarker()
        {
            GameObject ARGameObjectQrCode = markerSettingsMarkerActual.getARGameObject();

            if (ARGameObjectQrCode != null)
            {
                EstimatePoseMarker(ARGameObjectQrCode);
            }
        }

        private void EstimatePoseMarker(GameObject ARGameObject)
        {
            Aruco.estimatePoseSingleMarkers(cornersAruco, markerLength, camMatrix, distCoeffs, rvecs, tvecs);

            for (int i = 0; i < idsAruco.total(); i++)
            {
                using (Mat rvec = new Mat(rvecs, new OpenCVForUnity.CoreModule.Rect(0, i, 1, 1)))
                using (Mat tvec = new Mat(tvecs, new OpenCVForUnity.CoreModule.Rect(0, i, 1, 1)))
                {
                    if (i == 0)
                    {
                        UpdateARObjectTransform(rvec, tvec, ARGameObject);
                    }
                }
            }
        }

        private void UpdateARObjectTransform(Mat rvec, Mat tvec, GameObject ARGameObject)
        {
            // Convert to unity pose data.
            double[] rvecArr = new double[3];
            rvec.get(0, 0, rvecArr);
            double[] tvecArr = new double[3];
            tvec.get(0, 0, tvecArr);
            PoseData poseData = ARUtils.ConvertRvecTvecToPoseData(rvecArr, tvecArr);

            ARUtils.LowpassPoseData(ref oldPoseData, ref poseData, positionLowPass, rotationLowPass);
            oldPoseData = poseData;

            Matrix4x4 matrix = Matrix4x4.TRS(poseData.pos, poseData.rot, new Vector3(0.15f, 0.15f, 0.15f));
            Matrix4x4 ARM = ARCamera.transform.localToWorldMatrix * invertYM * matrix * invertYM;

            ARGameObject.SetActive(true);

            if (shouldMoveARCamera)
            {
                ARUtils.SetTransformFromMatrix(ARCamera.transform, ref ARM);
            }
            else
            {
                ARUtils.SetTransformFromMatrix(ARGameObject.transform, ref ARM);
            }
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy ()
        {
            webCamTextureToMatHelper.Dispose ();
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick ()
        {
            SceneManager.LoadScene ("MainMenuScene");
        }

        /// <summary>
        /// Raises the play button click event.
        /// </summary>
        public void OnPlayButtonClick ()
        {
            webCamTextureToMatHelper.Play ();
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick ()
        {
            webCamTextureToMatHelper.Pause ();
        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void OnStopButtonClick ()
        {
            webCamTextureToMatHelper.Stop ();
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick ()
        {
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.IsFrontFacing ();
        }

        private void OnError(IContextualizedError obj)
        {
            Debug.LogError($"An error ocurred while loading your Model: {obj.GetInnerException()}");
        }

        private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
        {
            Debug.Log($"Loading Model. Progress: {progress:P}");
        }

        private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
        {
            if (assetLoaderContext.RootGameObject != null)
            {
                Debug.Log("Materials loaded. Model fully loaded.");
            }
            else
            {
                Debug.Log("Model could not be loaded.");
            }
        }

        private void OnLoad(AssetLoaderContext assetLoaderContext)
        {
            if (assetLoaderContext.RootGameObject != null)
            {
                InformationObject informationObject;

                GameObject objectSel = assetLoaderContext.RootGameObject;
                GameObject objectSelected = objectSel.transform.GetChild(0).gameObject;
                Destroy(objectSel);

                listInformationObject.TryGetValue(objectSelected.name, out informationObject);

                CreateComponent(informationObject, objectSelected);
            }
        }
    }
}
