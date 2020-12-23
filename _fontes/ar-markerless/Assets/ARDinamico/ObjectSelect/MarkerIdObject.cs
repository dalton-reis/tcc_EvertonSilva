using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class MarkerIdObject
{
    private Dictionary<string, int> ListObjectSelected;
    private static MarkerIdObject Instance;

    private MarkerIdObject()
    {
        ListObjectSelected = new Dictionary<string, int>();
    }

    public static MarkerIdObject GetInstance()
    {
        if (Instance == null)
        {
            Instance = new MarkerIdObject();
        }

        return Instance;
    }

    public void add(string key, int markerId)
    {
        ListObjectSelected.Add(key, markerId);
    }

    public int getIdMarker(string key)
    {
        if(ListObjectSelected.Count == 0)
        {
            return -1;
        }

        return ListObjectSelected[key];
    }

    public Dictionary<string, int> getList()
    {
        return ListObjectSelected;
    }
}