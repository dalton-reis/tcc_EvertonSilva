using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ObjectListControl : MonoBehaviour
{
    private List<PlayerItem> playerItems;
    private Texture2D[] imageTextures;
    private List<string> nameButton;
    private bool isDraw;

    [SerializeField]
    private GameObject buttonTemplate;

    [SerializeField]
    private GridLayoutGroup gridLayoutGroup;

    [SerializeField]
    private GameObject viewPort;

    private void Awake()
    {
        isDraw = PropertiesModel.TypeVisualization == "DrawAgain";

        if (isDraw)
        {
            DirectoryInfo info = new DirectoryInfo(Path.Combine(Application.persistentDataPath, PropertiesModel.FolderImagemDynamicOriginal));
            FileInfo[] fileInfo = info.GetFiles("*" + PropertiesModel.ImageFormatPNG);

            imageTextures = new Texture2D[fileInfo.Length];

            for (int i = 0; i < fileInfo.Length; i++) {
                imageTextures[i] = GetTexture2D(fileInfo[i]);
            }
        }
        else
        {
            GameObject[] game = ImportResources.GetListGameObject();
            nameButton = new List<string>();

            for (int i = 0; i < game.Length; i++)
            {
                nameButton.Add(game[i].name);
            }

            DirectoryInfo info = new DirectoryInfo(Path.Combine(Application.persistentDataPath, PropertiesModel.Directory3D));

            if (info.Exists)
            {
                FileInfo[] fileInfo = info.GetFiles("*");

                for (int i = 0; i < fileInfo.Length; i++)
                {
                    nameButton.Add(fileInfo[i].Name);
                }
            }
        }
    }

    private Texture2D GetTexture2D(FileInfo fileInfo)
    {
        MemoryStream dest = new MemoryStream();

        //Read from each Image File
        using (Stream source = fileInfo.OpenRead())
        {
            byte[] buffer = new byte[2048];
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                dest.Write(buffer, 0, bytesRead);
            }
        }

        byte[] imageBytes = dest.ToArray();

        Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        texture2D.LoadImage(imageBytes);
        texture2D.name = fileInfo.Name;

        return texture2D;
    }

    private void Start()
    {
        playerItems = new List<PlayerItem>();

        if (isDraw)
        {
            GenerateItemTextures();
        }
        else
        {
            GenerateItemTexts();
        }        

        GenerationListButton();
    }

    private void GenerateItemTexts()
    {
        foreach (string name in nameButton)
        {
            PlayerItem playerItem = new PlayerItem();
            playerItem.textButton = name;

            playerItems.Add(playerItem);
        }
    }

    private void GenerateItemTextures()
    {
        for (int i = 0; i < imageTextures.Length; i++)
        {
            PlayerItem playerItem = new PlayerItem();
            playerItem.textureButton = imageTextures[i];

            playerItems.Add(playerItem);
        }
    }

    void GenerationListButton()
    {
        var mediaScreen = viewPort.GetComponent<RectTransform>().rect.width / (gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x);
        gridLayoutGroup.constraintCount = (int) Math.Floor(mediaScreen);

        foreach (PlayerItem item in playerItems)
        {
            GameObject newButtom = Instantiate(buttonTemplate);
            newButtom.SetActive(true);

            if(isDraw)
            {
                newButtom.GetComponent<ObjectListButton>().SetImage(item.textureButton);
            }
            else
            {
                newButtom.GetComponent<ObjectListButton>().SetText(item.textButton);
            }

            newButtom.transform.SetParent(buttonTemplate.transform.parent, false);
        }
    }

    public class PlayerItem
    {
        public Texture2D textureButton;
        public string textButton;
    }
}
