using UnityEngine;
using OpenCVForUnity.ArucoModule;
using System.IO;

public class PropertiesModel : MonoBehaviour
{
    public static string TagMoveObject = "moveObject";
    public static int DictionaryId = Aruco.DICT_ARUCO_ORIGINAL;
    public static string FolderImagemDynamic = "patternImg";
    public static string FolderImagemDynamicOriginal = "patternImg/original";
    public static string FolderImagemDynamicEdge = "patternImg/edge";
    public static string FolderImagemDynamicDrawing = "patternImg/drawing";
    public static string Directory3D = "3D";
    public static string NameBDMarkerPlayerPrefab = "informationObjectWithMarker";
    public static string NameBDMarkerLessPlayerPrefab = "informationObjectMarkerLess";
    public static string ImageFormatPNG = ".png";
    public static string NameObjectSelected;
    public static string PathObjectDrawing;
    public static string TypeVisualization;
    public static bool isMarker = false;
    public static GameObject ImportedExternalObject;
    public static bool DetectarImagemOriginal = false;

    public static int AmountPointMinimum = 6;
    public static float DistancePoint = 0.70f;

}