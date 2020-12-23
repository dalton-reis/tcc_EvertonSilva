using UnityEngine;
using UnityEditor;

namespace Assets.MarkerBasedARExample.ObjectSelect
{
    public class CreateObject : MonoBehaviour
    {
        public static GameObject Create(GameObject objectSelected)
        {
            Quaternion rotation = Quaternion.identity;
            Vector3 position = Vector3.zero;


            GameObject objectCreated = Instantiate(objectSelected, position, rotation);
            
            objectCreated.name = objectSelected.name;
            objectCreated.AddComponent(typeof(MeshCollider));
            objectCreated.AddComponent(typeof(Rigidbody));
            objectCreated.GetComponent<Rigidbody>().isKinematic = true;
            objectCreated.tag = PropertiesModel.TagMoveObject;

            return objectCreated;
        }
    }
}