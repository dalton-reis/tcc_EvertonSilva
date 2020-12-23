using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(WebCamTextureToMatHelper))]
public class CreateImage : MonoBehaviour
{
    private GameObject[] listObjectSelecionado;
    private InformationObjectList informationObjectList;
    private string nameBDPlayerPrefab;
    private string fileName;

    public bool isMarker;

    public Camera myCamera;

    WebCamTextureToMatHelper webCamTextureToMatHelper;

    Texture2D texture;

    Mat rgbMat;

    Mat outputMat;

    OpenCVForUnity.CoreModule.Rect patternRect;

    private void Awake()
    {
        //DeletePlayerPrefs();

        nameBDPlayerPrefab = PropertiesModel.NameBDMarkerLessPlayerPrefab;
        PropertiesModel.isMarker = isMarker;

        if (isMarker)
        {
            nameBDPlayerPrefab = PropertiesModel.NameBDMarkerPlayerPrefab;
        }

        informationObjectList = JsonUtility.FromJson<InformationObjectList>(PlayerPrefs.GetString(nameBDPlayerPrefab));

        if (informationObjectList == null)
        {
            informationObjectList = new InformationObjectList();
        }

        webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();

#if UNITY_ANDROID && !UNITY_EDITOR
            // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
            webCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
#endif
        webCamTextureToMatHelper.Initialize();
    }

    void Start()
    {
        fileName = GetFilename();
        webCamTextureToMatHelper.Stop();
    }

    private IEnumerator TakeScreenShot()
    {
        ObjectScreenVisible();

        yield return new WaitForEndOfFrame();

        Mat patternMat = new Mat(outputMat, patternRect);
        myCamera.targetTexture = RenderTexture.GetTemporary(680, 680, 24);

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = myCamera.targetTexture;
        
        myCamera.Render();        

        Texture2D screenImageTexture = new Texture2D(myCamera.targetTexture.width, myCamera.targetTexture.height, TextureFormat.RGB24, false);
        screenImageTexture.ReadPixels(new UnityEngine.Rect(0, 0, myCamera.targetTexture.width, myCamera.targetTexture.height), 0, 0);
        screenImageTexture.Apply();
        RenderTexture.active = currentRT;

        // Converte de Texture para Mat do OpenCV
        Mat screenImageMat = new Mat(myCamera.targetTexture.height, myCamera.targetTexture.width, CvType.CV_8UC3);
        Utils.texture2DToMat(screenImageTexture, screenImageMat);

        Imgproc.cvtColor(screenImageMat, screenImageMat, Imgproc.COLOR_BGR2RGB);

        // Salva a imagem original
        string pathObjectOriginal = GetImagePath(true);
        SaveTexture(screenImageTexture, pathObjectOriginal);
             
        // Converte para tons de cinza
        Mat screenImageGrayMat = new Mat(myCamera.targetTexture.height, myCamera.targetTexture.width, CvType.CV_8UC3);
        Imgproc.cvtColor(screenImageMat, screenImageGrayMat, Imgproc.COLOR_RGBA2GRAY);

        // Usa o filtro de canny para identificar as bordas
        Mat resultCannyMat = new Mat(myCamera.targetTexture.height, myCamera.targetTexture.width, CvType.CV_8UC3);
        Imgproc.Canny(screenImageGrayMat, resultCannyMat, 200, 300, 3, true);

        int thickness = 1;
        Mat kernel_dilate = new Mat(thickness, thickness, CvType.CV_8UC1);
        Imgproc.dilate(resultCannyMat, resultCannyMat, kernel_dilate);

        //thickness = 1;
        //kernel_dilate = new Mat(thickness, thickness, CvType.CV_8UC1);

        //thickness = 1;
        //kernel_dilate = new Mat(thickness, thickness, CvType.CV_8UC1);
        //Imgproc.morphologyEx(resultCannyMat, resultCannyMat, Imgproc.MORPH_OPEN, kernel_dilate);
        //Imgproc.morphologyEx(resultCannyMat, resultCannyMat, Imgproc.MORPH_CLOSE, kernel_dilate);

        //thickness = 2;
        //kernel_dilate = new Mat(thickness, thickness, CvType.CV_8UC1);
        //Imgproc.erode(resultCannyMat, resultCannyMat, kernel_dilate);

        // Invert as cores
        Mat resultInvertMat = new Mat(patternMat.height(), patternMat.width(), CvType.CV_8UC3);
        Core.bitwise_not(resultCannyMat, resultInvertMat);

        Imgproc.cvtColor(resultInvertMat, resultInvertMat, Imgproc.COLOR_RGB2BGR);

        // Salva a imagem em bordas para ser detectado
        PropertiesModel.PathObjectDrawing = GetImagePath(false);
        SaveImage(resultInvertMat, PropertiesModel.PathObjectDrawing);
        SaveInformationObject();

        RenderTexture.ReleaseTemporary(myCamera.targetTexture);

        CaptureScreenShot();
    }

    private void DeletePlayerPrefs()
    {
        string directoryEdge = Path.Combine(Application.persistentDataPath, PropertiesModel.FolderImagemDynamicEdge);

        if (Directory.Exists(directoryEdge))
        {
            Directory.Delete(directoryEdge, true);
        }

        string directory = Path.Combine(Application.persistentDataPath, PropertiesModel.FolderImagemDynamicOriginal);

        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, true);
        }

        string directoryDrawing = Path.Combine(Application.persistentDataPath, PropertiesModel.FolderImagemDynamicDrawing);

        if (Directory.Exists(directoryDrawing))
        {
            Directory.Delete(directoryDrawing, true);
        }

        PlayerPrefs.DeleteAll();
    }

    void CaptureScreenShot()
    {
        //yield return new WaitForEndOfFrame();

        // Captura imagem da tela
        Texture2D screenImageTexture = ScreenCapture.CaptureScreenshotAsTexture(2);

        // Converte de Texture para Mat do OpenCV
        Mat screenImageMat = new Mat(screenImageTexture.height, screenImageTexture.width, CvType.CV_8UC3);
        Utils.texture2DToMat(screenImageTexture, screenImageMat);

        Imgproc.cvtColor(screenImageMat, screenImageMat, Imgproc.COLOR_RGB2BGR);

        // Salva a imagem original
        //string pathObjectOriginal = GetImagePath(true);
        //SaveImage(screenImageMat, pathObjectOriginal);

        // Converte para tons de cinza
        Mat screenImageGrayMat = new Mat(screenImageMat.rows(), screenImageMat.cols(), CvType.CV_8UC3);
        Imgproc.cvtColor(screenImageMat, screenImageGrayMat, Imgproc.COLOR_RGBA2GRAY);

        // Usa o filtro de canny para identificar as bordas
        Mat resultCannyMat = new Mat();
        Imgproc.Canny(screenImageGrayMat, resultCannyMat, 200, 300, 3, true);

        int thickness = 8;
        Mat kernel_dilate = new Mat(thickness, thickness, CvType.CV_8UC1);
        Imgproc.dilate(resultCannyMat, resultCannyMat, kernel_dilate);

        // Invert as cores
        Mat resultInvertMat = new Mat(resultCannyMat.rows(), resultCannyMat.cols(), CvType.CV_8UC3);
        Core.bitwise_not(resultCannyMat, resultInvertMat);

        Imgproc.cvtColor(resultInvertMat, resultInvertMat, Imgproc.COLOR_RGB2BGR);

        string directory = Path.Combine(Application.persistentDataPath, PropertiesModel.FolderImagemDynamicDrawing);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Salva a imagem em bordas para ser desenhado
        PropertiesModel.PathObjectDrawing = Path.Combine(directory, fileName);
        SaveImage(resultInvertMat, PropertiesModel.PathObjectDrawing);

        //Destroy(resultCannyTexture);
        Destroy(screenImageTexture);
    }

    private string GetImagePath(bool original)
    {
        string directory = Path.Combine(Application.persistentDataPath, PropertiesModel.FolderImagemDynamicEdge);

        if (original)
        {
            directory = Path.Combine(Application.persistentDataPath, PropertiesModel.FolderImagemDynamicOriginal);
        }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return Path.Combine(directory, fileName);
    }

    private string GetFilename()
    {
        string fileName = Path.GetRandomFileName().Replace(".", "").Substring(0, 8);
        return string.Concat(fileName, PropertiesModel.ImageFormatPNG);
    }

    private void SaveImage(Mat imageSave, string imagePath)
    {
        Imgcodecs.imwrite(imagePath, imageSave);
    }

    private void SaveTexture(Texture2D imageSave, string imagePath)
    {
        byte[] _bytes = imageSave.EncodeToPNG();
        File.WriteAllBytes(imagePath, _bytes);
    }

    private void SaveInformationObject()
    {
        SetInformationObject();
        string informationList = JsonUtility.ToJson(informationObjectList);

        PlayerPrefs.SetString(nameBDPlayerPrefab, informationList);
        PlayerPrefs.Save();
    }

    private void SetInformationObject()
    {
        InformationObject informationObject;
        MarkerIdObject markerIdObject = MarkerIdObject.GetInstance();

        foreach (GameObject objectSelect in listObjectSelecionado)
        {
            informationObject = new InformationObject();
            informationObject.Name = objectSelect.name;
            informationObject.FileNameImage = fileName;
            informationObject.ImagePathMarkerLess = PropertiesModel.PathObjectDrawing;
            informationObject.IdMarker = markerIdObject.getIdMarker(objectSelect.name);
            informationObject.Position = objectSelect.transform.position;
            informationObject.Rotation = objectSelect.transform.rotation;
            informationObject.Scale = objectSelect.transform.localScale;

            informationObjectList.ListInformationObject.Add(informationObject);
        }
    }

    public void OnWebCamTextureToMatHelperInitialized()
    {
        Debug.Log("OnWebCamTextureToMatHelperInitialized");


        Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

        texture = new Texture2D(webCamTextureMat.width(), webCamTextureMat.height(), TextureFormat.RGB24, false);

        rgbMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC3);

        outputMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC3);

        gameObject.transform.localScale = new Vector3(webCamTextureMat.width(), webCamTextureMat.height(), 1);

        Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);


        float width = webCamTextureMat.width();
        float height = webCamTextureMat.height();

        float widthScale = (float)Screen.width / width;
        float heightScale = (float)Screen.height / height;
        if (widthScale < heightScale)
        {
            Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
        }
        else
        {
            Camera.main.orthographicSize = height / 2;
        }

        gameObject.GetComponent<Renderer>().material.mainTexture = texture;


        //if WebCamera is frontFaceing,flip Mat.
        if (webCamTextureToMatHelper.GetWebCamDevice().isFrontFacing)
        {
            webCamTextureToMatHelper.flipHorizontal = true;
        }


        int patternWidth = (int)(Mathf.Min(webCamTextureMat.width(), webCamTextureMat.height()) * 0.8f);

        patternRect = new OpenCVForUnity.CoreModule.Rect(webCamTextureMat.width() / 2 - patternWidth / 2, webCamTextureMat.height() / 2 - patternWidth / 2, patternWidth, patternWidth);


        Mat rgbaMat = webCamTextureToMatHelper.GetMat();

        Imgproc.cvtColor(rgbaMat, rgbMat, Imgproc.COLOR_RGBA2RGB);
        Imgproc.cvtColor(rgbaMat, outputMat, Imgproc.COLOR_RGBA2RGB);


    }

    /// <summary>
    /// Raises the web cam texture to mat helper disposed event.
    /// </summary>
    public void OnWebCamTextureToMatHelperDisposed()
    {
        Debug.Log("OnWebCamTextureToMatHelperDisposed");

        if (rgbMat != null)
        {
            rgbMat.Dispose();
        }
        if (outputMat != null)
        {
            outputMat.Dispose();
        }
    }

    /// <summary>
    /// Raises the web cam texture to mat helper error occurred event.
    /// </summary>
    /// <param name="errorCode">Error code.</param>
    public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
    {
        Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
    }

    private void ObjectScreenVisible()
    {
        GameObject menu = GameObject.Find("Canvas");
        menu.SetActive(false);
    }

    public void OnCreateImageButton()
    {            
        listObjectSelecionado = GameObject.FindGameObjectsWithTag(PropertiesModel.TagMoveObject);
     
        if (isMarker)
        {
            SaveInformationObject();
            SceneManager.LoadScene("WebCamTextureMarkerBasedARExample");
        }
        else
        {
            StartCoroutine(TakeScreenShot());
            SceneManager.LoadScene("WebCamDrawingScene");
        }
    }

    public void onBackMainMenu()
    {
        PropertiesModel.TypeVisualization = "GenerateMarker";
        SceneManager.LoadScene("ObjectListScene");
    }
}