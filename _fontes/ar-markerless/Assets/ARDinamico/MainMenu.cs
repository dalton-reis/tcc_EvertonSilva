using TriLibCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MainMenu
{
    public class MainMenu : MonoBehaviour
    {

        public void OnObjectCreateWithMarkerButtonClick()
        {
            PropertiesModel.isMarker = true;
            PropertiesModel.TypeVisualization = "GenerateMarker";
            SceneManager.LoadScene("ObjectListScene");
        }

        public void OnObjectCreateMarkerLessButtonClick()
        {
            PropertiesModel.isMarker = false;
            PropertiesModel.TypeVisualization = "GenerateMarker";
            SceneManager.LoadScene("ObjectListScene");
        }

        public void OnObjectsImportButtonClick()
        {
            //SceneManager.LoadScene("ObjectSelectMarkerLessScene");
            SceneManager.LoadScene("ImporterScene");
        }

        public void OnObjectAugmentedRealityWithMarkerClick()
        {
            SceneManager.LoadScene("WebCamTextureMarkerBasedARExample");
        }

        public void OnObjectAugmentedRealityMarkerLessClick()
        {
            SceneManager.LoadScene("WebCamTextureMarkerLessARExample");
        }

        public void OnDrawAgain()
        {
            PropertiesModel.TypeVisualization = "DrawAgain";
            SceneManager.LoadScene("ObjectListScene");
        }

    }
}