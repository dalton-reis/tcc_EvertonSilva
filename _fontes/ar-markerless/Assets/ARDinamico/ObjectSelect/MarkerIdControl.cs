using System;
using UnityEngine;

public class MarkerIdControl
{
	private InformationObjectList informationObjectList;
    private static MarkerIdControl Instance;
    private int markerId = 0;

    private MarkerIdControl()
	{
		informationObjectList = JsonUtility.FromJson<InformationObjectList>(PlayerPrefs.GetString(PropertiesModel.NameBDMarkerPlayerPrefab));

        if (informationObjectList != null)
        {
            markerId = informationObjectList.ListInformationObject.Count;
        }
    }

    public static MarkerIdControl GetInstance()
    {
        if (Instance == null)
        {
            Instance = new MarkerIdControl();
        }

        return Instance;
    }

    public int GetMarkerId()
    {
        int idMarker = markerId;
        markerId++;
        return idMarker;
    }

}
