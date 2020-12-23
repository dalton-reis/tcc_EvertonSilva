using System.IO;
using UnityEngine;

public class ImportResources
{
    private static string pathObject3D = "3D";
    private static string pathTextureImageOriginal = "imageDrawing/original";

    public static GameObject GetGameObject(string nameObject) 
    {
        return Resources.Load<GameObject>(Path.Combine(pathObject3D, nameObject));
    }

    public static GameObject[] GetListGameObject()
    {
        return Resources.LoadAll<GameObject>(pathObject3D);
    }

    public static Texture2D[] GetListTexture2D()
    {
        return Resources.LoadAll<Texture2D>(pathTextureImageOriginal);
    }

}