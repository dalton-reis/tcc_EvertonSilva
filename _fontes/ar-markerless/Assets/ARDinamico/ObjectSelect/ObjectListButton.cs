using Assets.MarkerBasedARExample.ObjectSelect;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ObjectListButton : MonoBehaviour
{
    [SerializeField]
    private RawImage imageButton;
    private string myNameImage;

    [SerializeField]
    private Text textButton;

    public void SetImage(Texture2D texture2d)
    {
        imageButton.texture = texture2d;
        myNameImage = texture2d.name;
    }

    public void SetText(string nameButton)
    {
        textButton.text = nameButton;
        myNameImage = nameButton;
    }

    public void OnClick()
    {
        Debug.Log(myNameImage);

        if (PropertiesModel.TypeVisualization == "DrawAgain")
        {
            string directory = Path.Combine(Application.persistentDataPath, PropertiesModel.FolderImagemDynamicDrawing);
            PropertiesModel.PathObjectDrawing = Path.Combine(directory, myNameImage);

            SceneManager.LoadScene("WebCamDrawingScene");
        }
        else
        {
            PropertiesModel.NameObjectSelected = myNameImage;

            if (PropertiesModel.isMarker)
            {
                SceneManager.LoadScene("ObjectSelectMarkerScene");
            } 
            else
            {
                SceneManager.LoadScene("ObjectSelectMarkerLessScene");
            }
        }
    }

    public void onBackMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
