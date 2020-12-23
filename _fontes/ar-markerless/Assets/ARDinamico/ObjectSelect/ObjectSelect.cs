using Assets.MarkerBasedARExample.ObjectSelect;
using System;
using System.IO;
using TriLibCore;
using UnityEngine;

public class ObjectSelect : MonoBehaviour
{    
    private GameObject objectList;
    private MarkerIdObject markerIdObject;
    private GameObject objectCreated;
    private GameObject objectSelected;

    void Awake()
    {
        objectList = GameObject.Find("/ListObject");
        markerIdObject = MarkerIdObject.GetInstance();

        if (PropertiesModel.ImportedExternalObject != null)
        {
            objectSelected = PropertiesModel.ImportedExternalObject.transform.GetChild(0).gameObject;
            Destroy(PropertiesModel.ImportedExternalObject);
        }
        else
        {
            if (PropertiesModel.NameObjectSelected == null)
            {
                // objectSelected = SelectObject("Gift1");
                // objectSelected = SelectObject("TreeStump");
                //objectSelected = SelectObject("Sledge");
                objectSelected = SelectObject("Gift3");
                //objectSelected = SelectObject("Cube");
            }
            else
            {
                if (Path.GetExtension(PropertiesModel.NameObjectSelected) == "")
                {
                    objectSelected = SelectObject(PropertiesModel.NameObjectSelected);
                }
                else
                {
                    string path = GetPathComplete(PropertiesModel.NameObjectSelected);
                    LoadFromPath(path);
                }

            }
        }

        ObjectSelectedCreate();
    }

    private void ObjectSelectedCreate()
    {
        if (objectSelected != null)
        {
            if (gameObject.scene.name == "ObjectSelectMarkerLessScene")
            {
                ObjectCreate(objectSelected);
            }
            else
            {
                MarkerIdControl markerIdControl = MarkerIdControl.GetInstance();
                CreateObjectWithIdMarker(objectSelected, markerIdControl.GetMarkerId());
            }
        }
    }

    private string GetPathComplete(string nameObjectSelected)
    {
        return Path.Combine(Application.persistentDataPath, PropertiesModel.Directory3D, nameObjectSelected);
    }

    private void LoadFromPath(string path)
    {
        var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        AssetLoader.LoadModelFromFile(path, OnLoad, OnMaterialsLoad, OnProgress, OnError, null, assetLoaderOptions);
    }

    private GameObject SelectObject(string nameObject)
    {
        return ImportResources.GetGameObject(nameObject);
    }

    private void ObjectCreate(GameObject objectSelected)
    {
        objectCreated = CreateObject.Create(objectSelected);
        objectCreated.transform.SetParent(objectList.transform);
    }

    private void CreateObjectWithIdMarker(GameObject objectSelected, int id)
    {
        ObjectCreate(objectSelected);
        markerIdObject.add(objectCreated.name, id);
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
            GameObject objectSel = assetLoaderContext.RootGameObject;
            objectSelected = objectSel.transform.GetChild(0).gameObject;
            ObjectSelectedCreate();

            Destroy(objectSel);
        }
    }

}
