using System.IO;
using TriLibCore;
using TriLibCore.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ObjectImport : MonoBehaviour
{
    public void LoadModel()
    {
        var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        var assetLoaderFilePicker = AssetLoaderFilePicker.Create();
        assetLoaderFilePicker.LoadModelFromFilePickerAsync("Select a Model file", OnLoad, OnMaterialsLoad, OnProgress, OnBeginLoad, OnError, null, assetLoaderOptions);
    }

    private void OnBeginLoad(bool filesSelected)
    {
        Debug.Log($"Loading Model. BeginLoad: {filesSelected:P}");
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
        if (PropertiesModel.ImportedExternalObject != null)
        {
            Destroy(PropertiesModel.ImportedExternalObject);
        }

        PropertiesModel.ImportedExternalObject = assetLoaderContext.RootGameObject;

        if (PropertiesModel.ImportedExternalObject != null)
        {
            CopyFileForResources(assetLoaderContext.Filename);

            DontDestroyOnLoad(PropertiesModel.ImportedExternalObject);
            SceneManager.LoadScene("ObjectSelectMarkerLessScene");
            Debug.Log("Model loaded. Loading materials.");
            //Camera.main.FitToBounds(assetLoaderContext.RootGameObject, 2f);
        }
        else
        {
            Debug.Log("Model materials could not be loaded.");
        }
    }

    private void CopyFileForResources(string pathOrigin)
    {
        if (File.Exists(pathOrigin))
        {
            string directory3D = Path.Combine(Application.persistentDataPath, PropertiesModel.Directory3D);

            if (!Directory.Exists(directory3D))
            {
                Directory.CreateDirectory(directory3D);
            }

            string pathResources = Path.Combine(directory3D, Path.GetFileName(pathOrigin));

            File.Copy(pathOrigin, pathResources, true);
            PropertiesModel.NameObjectSelected = pathOrigin;
        }
    }
}
