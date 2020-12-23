using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sample : MonoBehaviour
{

    public void OnAruco()
    {
        SceneManager.LoadScene("WebCamTextureMarkerBasedARExample");
    }

    public void OnMarkerLess()
    {
        SceneManager.LoadScene("WebCamTextureMarkerLessARExample");
    }

}
