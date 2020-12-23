using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[Serializable]
public class InformationObject
{
    public string Name;
    public string FileNameImage;
    public string ImagePathMarkerLess;
    public int IdMarker;
    public Vector3 Position;
    public Vector3 Scale;
    public Quaternion Rotation;
}

[Serializable]
public class InformationObjectList
{
    public List<InformationObject> ListInformationObject;

    public InformationObjectList()
    {
        ListInformationObject = new List<InformationObject>();
    }
}