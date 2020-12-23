using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotateObject : MonoBehaviour
{
    RaycastHit hit;
    Rigidbody rbTemp;
    GameObject tempObject;
    Camera mainCamera;
    float rotXTemp;
    float rotYTemp;

    Slider slider;


    void Awake()
    {
        mainCamera = Camera.main;
        slider = FindObjectOfType<Slider>();
    }

    void Update()
    {
        //Quando usar click esquerdo do mouse
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            //converte a posição do click para um ray
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //Se o ray acertar (hit) o Collider (não 2DCollider)
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag(PropertiesModel.TagMoveObject))
                {
                    tempObject = hit.transform.gameObject;
                }
            }
        }

        if (tempObject)
        {
            rbTemp = tempObject.GetComponent<Rigidbody>();
        }

        if (tempObject && mainCamera)
        {
            rotXTemp = 0;
            rotYTemp = 0;
            
            if (Input.touchCount > 0)
            {
                MoveOnFinger();
            }
            else
            {
                MoveOnMouse();
            }
        }
    }

    private void MoveOnFinger()
    {
        float speed = 0.1f;
        rotXTemp = Input.touches[0].deltaPosition.x;
        rotYTemp = Input.touches[0].deltaPosition.y;

        if (Input.touchCount == 2)
        {
            Vector3 newPosiction = new Vector3(rotXTemp, 0, rotYTemp);
            newPosiction = newPosiction.normalized * Time.deltaTime;
            rbTemp.MovePosition(tempObject.transform.position + newPosiction);
        }

        if (Input.touchCount == 1)
        {
            tempObject.transform.Rotate(mainCamera.transform.up, -(rotXTemp * speed), Space.World);
            tempObject.transform.Rotate(mainCamera.transform.right, (rotYTemp * speed), Space.World);
        }
    }

    private void MoveOnMouse()
    {
        rotXTemp = Input.GetAxis("Mouse X");
        rotYTemp = Input.GetAxis("Mouse Y");

        if (Input.GetMouseButton(0))
        {            
            tempObject.transform.Rotate(mainCamera.transform.up, -rotXTemp, Space.World);
            tempObject.transform.Rotate(mainCamera.transform.right, rotYTemp, Space.World);
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 newPosiction = new Vector3(rotXTemp, 0, rotYTemp);
            newPosiction = newPosiction.normalized * 2 * Time.deltaTime;
            rbTemp.MovePosition(tempObject.transform.position + newPosiction);
        }
    }

    void OnEnable()
    {
        slider.onValueChanged.AddListener(delegate { OnChangeSizeImage(slider.value); });
    }

    public void OnChangeSizeImage(float valueSlider)
    {
        var gameListObject = GameObject.Find("ListObject");
        gameListObject.transform.position = new Vector3(gameListObject.transform.position.x, valueSlider, gameListObject.transform.position.z);
    }
}
