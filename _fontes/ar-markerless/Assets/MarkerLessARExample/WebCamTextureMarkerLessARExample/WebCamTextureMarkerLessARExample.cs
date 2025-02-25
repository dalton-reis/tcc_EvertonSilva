﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using OpenCVMarkerLessAR;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.UnityUtils.Helper;
using System.IO;
using TriLibCore;
using System.Collections.Generic;

namespace MarkerLessARExample
{
    /// <summary>
    /// WebCamTexture Markerless AR Example
    /// This code is a rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter3_MarkerlessAR using "OpenCV for Unity".
    /// </summary>
    [RequireComponent(typeof(WebCamTextureToMatHelper))]
    public class WebCamTextureMarkerLessARExample : MonoBehaviour
    {
        /// <summary>
        /// The pattern raw image.
        /// </summary>
        public RawImage patternRawImage;

        /// <summary>
        /// The AR game object.
        /// </summary>
        public GameObject ARGameObject;

        /// <summary>
        /// The AR camera.
        /// </summary>
        public Camera ARCamera;

        /// <summary>
        /// Determines if should move AR camera.
        /// </summary>
        public bool shouldMoveARCamera;

        /// <summary>
        /// Determines if displays axes.
        /// </summary>
        public bool displayAxes = false;

        /// <summary>
        /// The display axes toggle.
        /// </summary>
        public Toggle displayAxesToggle;

        /// <summary>
        /// The axes.
        /// </summary>
        public GameObject axes;

        /// <summary>
        /// Determines if displays cube.
        /// </summary>
        public bool displayCube = false;

        /// <summary>
        /// The display cube toggle.
        /// </summary>
        public Toggle displayCubeToggle;

        /// <summary>
        /// The cube.
        /// </summary>
        public GameObject cube;

        /// <summary>
        /// Determines if displays video.
        /// </summary>
        public bool displayVideo = false;

        /// <summary>
        /// The display video toggle.
        /// </summary>
        public Toggle displayVideoToggle;

        /// <summary>
        /// The video.
        /// </summary>
        public GameObject video;

        /// <summary>
        /// The pattern mat.
        /// </summary>
        Mat patternMat;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        WebCamTextureToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// The gray mat.
        /// </summary>
        Mat grayMat;

        /// <summary>
        /// The cameraparam matrix.
        /// </summary>
        Mat camMatrix;

        /// <summary>
        /// The dist coeffs.
        /// </summary>
        MatOfDouble distCoeffs;

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
        /// The pattern.
        /// </summary>
        Pattern pattern;

        /// <summary>
        /// The pattern tracking info.
        /// </summary>
        PatternTrackingInfo patternTrackingInfo;

        GameObject ARGameObjectList;

        Transform ultimoGameTransform;

        PatternDetector ultimoPatternDetector;

        Dictionary<string, InformationObject> listInformationObject;

        private void Awake()
        {
            ARGameObjectList = GameObject.Find("/ARObjectList");
            listInformationObject = new Dictionary<string, InformationObject>();
            CreatComponentMarkerLess();
        }

        // Use this for initialization
        void Start()
        {
            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();

#if UNITY_ANDROID && !UNITY_EDITOR
            // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
            webCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
#endif
            webCamTextureToMatHelper.Initialize();

            PropertiesModel.DistancePoint = 0.66f;
            PropertiesModel.AmountPointMinimum = 10;
        }

        void CreatComponentMarkerLess()
        {
            InformationObjectList informationObjectList = JsonUtility.FromJson<InformationObjectList>(PlayerPrefs.GetString(PropertiesModel.NameBDMarkerLessPlayerPrefab));

            if (informationObjectList == null)
            {
                return;
            }

            patternTrackingInfo = new PatternTrackingInfo();

            foreach (InformationObject informationObject in informationObjectList.ListInformationObject)
            {
                GameObject ARObject = ImportResources.GetGameObject(informationObject.Name);
                
                if (ARObject == null)
                {
                    LoadFromPath(informationObject);
                }
                else
                {
                    LinksObjectPattern(informationObject, ARObject);
                }
            }
        }

        private void LinksObjectPattern(InformationObject informationObject, GameObject ARObject) {
            
            patternMat = Imgcodecs.imread(informationObject.ImagePathMarkerLess);

            if (PropertiesModel.DetectarImagemOriginal)
            {
                patternMat = Imgcodecs.imread(Path.Combine(
                    Application.persistentDataPath,
                    PropertiesModel.FolderImagemDynamicOriginal,
                    informationObject.FileNameImage));
            }

            if (patternMat.total() > 0)
            {
                Imgproc.cvtColor(patternMat, patternMat, Imgproc.COLOR_BGR2GRAY);

                pattern = new Pattern();

                PatternDetector patternDetector = new PatternDetector(null, null, null, true);
                patternDetector.buildPatternFromImage(patternMat, pattern);
                patternDetector.train(pattern);

                CreateComponent(ARObject, patternDetector);
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

        void CreateComponent(GameObject ARObject, PatternDetector patternDetector)
        {
            GameObject ARGameObject = new GameObject();
            ARGameObject.name = "ARGameObject";
            ARGameObject.SetActive(false);

            DelayableSetActive delayableSetActive = ARGameObject.AddComponent<DelayableSetActive>();
            delayableSetActive.PatternDetector = patternDetector;

            GameObject objectCreated = Instantiate(ARObject);

            objectCreated.AddComponent<RectTransform>();
            objectCreated.transform.position = Vector3.zero;
            objectCreated.transform.rotation = Quaternion.identity;
            objectCreated.layer = 8;

            objectCreated.transform.SetParent(ARGameObject.transform);
            ARGameObject.transform.SetParent(ARGameObjectList.transform);
        }

        /// <summary>
        /// Raises the web cam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperInitialized");

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();
                    
            texture = new Texture2D (webCamTextureMat.width (), webCamTextureMat.height (), TextureFormat.RGBA32, false);
            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;


            grayMat = new Mat (webCamTextureMat.rows (), webCamTextureMat.cols (), CvType.CV_8UC1);
                    

            gameObject.transform.localScale = new Vector3 (webCamTextureMat.width (), webCamTextureMat.height (), 1);
            
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


            transformationM = new Matrix4x4 ();
                        
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
            
            if (grayMat != null)
                grayMat.Dispose ();
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
                
                Mat rgbaMat = webCamTextureToMatHelper.GetMat ();
                Imgproc.cvtColor (rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);

                Transform gameTransform = null;

                if (ultimoPatternDetector != null && ultimoPatternDetector.findPattern(grayMat, patternTrackingInfo))
                {
                    gameTransform = ultimoGameTransform;
                }
                else
                {
                    ultimoPatternDetector = null;

                    foreach (Transform ARObject in ARGameObjectList.transform)
                    {
                        if (ARObject.GetComponent<DelayableSetActive>().PatternDetector.findPattern(grayMat, patternTrackingInfo))
                        {
                            gameTransform = ARObject;
                            ultimoGameTransform = ARObject;
                            ultimoPatternDetector = ARObject.GetComponent<DelayableSetActive>().PatternDetector;
                        }
                    }
                }

                
//              Debug.Log ("patternFound " + patternFound);
                if (gameTransform != null) {

                    patternTrackingInfo.computePose (pattern, camMatrix, distCoeffs);

                    //Marker to Camera Coordinate System Convert Matrix
                    transformationM = patternTrackingInfo.pose3d;
                    //Debug.Log ("transformationM " + transformationM.ToString ());

                    if (shouldMoveARCamera) {
                        ARM = gameTransform.localToWorldMatrix * invertZM * transformationM.inverse * invertYM;
                        //Debug.Log ("ARM " + ARM.ToString ());
                                
                        ARUtils.SetTransformFromMatrix (ARCamera.transform, ref ARM);
                    } else {
                                
                        ARM = ARCamera.transform.localToWorldMatrix * invertYM * transformationM * invertZM;
                        //Debug.Log ("ARM " + ARM.ToString ());
                                
                        ARUtils.SetTransformFromMatrix (gameTransform, ref ARM);
                    }

                    gameTransform.GetComponent<DelayableSetActive>().SetActive (true);

                } else if(ultimoGameTransform != null) {

                    ultimoGameTransform.GetComponent<DelayableSetActive> ().SetActive (false, 0.5f);
                    ultimoGameTransform = null;

                }
                
                Utils.fastMatToTexture2D (rgbaMat, texture);
            }
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy ()
        {
            webCamTextureToMatHelper.Dispose ();

            if (patternMat != null)
                patternMat.Dispose ();
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

        /// <summary>
        /// Raises the display axes toggle value changed event.
        /// </summary>
        public void OnDisplayAxesToggleValueChanged ()
        {
            if (displayAxesToggle.isOn) {
                displayAxes = true;
            } else {
                displayAxes = false;
            }
            axes.SetActive (displayAxes);
        }

        /// <summary>
        /// Raises the display cube toggle value changed event.
        /// </summary>
        public void OnDisplayCubeToggleValueChanged ()
        {
            if (displayCubeToggle.isOn) {
                displayCube = true;
            } else {
                displayCube = false;
            }
            cube.SetActive (displayCube);
        }

        /// <summary>
        /// Raises the display video toggle value changed event.
        /// </summary>
        public void OnDisplayVideoToggleValueChanged ()
        {
            if (displayVideoToggle.isOn) {
                displayVideo = true;
            } else {
                displayVideo = false;
            }
            video.SetActive (displayVideo);
        }

        /// <summary>
        /// Raises the capture pattern button click event.
        /// </summary>
        public void OnCapturePatternButtonClick ()
        {
        //    SceneManager.LoadScene ("CapturePattern");
        }

        public void OnDetectImageOriginal()
        {
            PropertiesModel.DetectarImagemOriginal = !PropertiesModel.DetectarImagemOriginal;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnChangePointDistance(Dropdown change)
        {
            PropertiesModel.DistancePoint = float.Parse(change.captionText.text);
            Debug.Log(PropertiesModel.DistancePoint);
        }
        public void OnChangePointMinimum(Dropdown change)
        {
            PropertiesModel.AmountPointMinimum = int.Parse(change.captionText.text);
            Debug.Log(PropertiesModel.AmountPointMinimum);

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
                
                LinksObjectPattern(informationObject, objectSelected);
            }

        }
    }
}
