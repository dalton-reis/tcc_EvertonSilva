using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveObject : MonoBehaviour
{
    RaycastHit hit;
    Rigidbody rbTemp;
    GameObject tempObject;
    Camera mainCamera;
    float rotXTemp;
    float rotYTemp;

    RawImage rawImage;
    Slider slider;
    bool canMoveObject;

    void Awake()
    {
        mainCamera = Camera.main;
        rawImage = FindObjectOfType<RawImage>();
        slider = FindObjectOfType<Slider>();
    }

    void Update()
    {
        //Quando usar click esquerdo do mouse
        if (Input.GetMouseButtonDown(0))
        {
            //converte a posição do click para um ray
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            tempObject = rawImage.GetComponent<Rigidbody>().transform.gameObject;

            //Se o ray acertar (hit) o Collider (não 2DCollider)
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag(PropertiesModel.TagMoveObject))
                {
                    rawImage.GetComponent<Rigidbody>().useGravity = true;
                    canMoveObject = true;
                }             
            }
        }


        if (canMoveObject)
        {
            rbTemp = tempObject.GetComponent<Rigidbody>();
        }

        if (canMoveObject && mainCamera)
        {            
            // Se estiver com o mouse clicado
            if (Input.GetMouseButton(0)) {
                rotXTemp = Input.GetAxis("Mouse X");
                rotYTemp = Input.GetAxis("Mouse Y");
                Vector3 newPosiction = new Vector3(rotXTemp, rotYTemp, 0);
                newPosiction = newPosiction.normalized * 200 * Time.deltaTime;
                rbTemp.MovePosition(tempObject.transform.position + newPosiction);
            } 
        }

        //Quando solta o click do mouse
        if (Input.GetMouseButtonUp(0))
        {
            tempObject = null;
            rbTemp = null;
            canMoveObject = false;
        }

    }

    void OnEnable()
    {
        slider.onValueChanged.AddListener(delegate { OnChangeSizeImage(slider.value); });
    }

    public void OnChangeSizeImage(float valueSlider)
    {
        tempObject.transform.localScale = new Vector3(valueSlider, valueSlider, valueSlider);
    }
}
