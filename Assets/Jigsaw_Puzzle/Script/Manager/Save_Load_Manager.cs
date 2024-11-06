﻿using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class PuzzleBackground
{
    public int isPrice;
    public bool isOpen;
    public Texture2D myTexture;

    public PuzzleBackground(int isPrice)
    {
        this.isPrice = isPrice;
    }
}
[Serializable]
public class PuzzleGroup
{
    public string groupName;
    public List<PuzzleGroupPart> puzzleGroupList = new List<PuzzleGroupPart>();

    public PuzzleGroup(string myName)
    {
        this.groupName = myName;
    }
}
[Serializable]
public class PuzzleGroupPart
{
    public int myOrjPrice;
    public int myNewPrice;
    public string groupPartName;
    public List<PuzzleSingle> puzzleSingle = new List<PuzzleSingle>();

    public PuzzleGroupPart(int myOrjPrice, string myName)
    {
        this.myOrjPrice = myOrjPrice;
        this.myNewPrice = myOrjPrice;
        this.groupPartName = myName;
    }
}
[Serializable]
public class PuzzleSingle
{
    public bool isVideo;
    public bool isFixed;
    public Texture2D myTexture;

    public PuzzleSingle()
    {
    }
    public PuzzleSingle(Texture2D myTexture)
    {
        this.myTexture = myTexture;
    }
    public PuzzleSingle(bool isVideo, Texture2D myTexture)
    {
        this.isVideo = isVideo;
        this.myTexture = myTexture;
    }
}
[Serializable]
public class PuzzleDiary : PuzzleSingle
{
    public string myDate;
    public PuzzleDiary(bool isVideo, Texture2D myTexture, string myDate) : base(isVideo, myTexture)
    {
        this.myDate = myDate;
        this.isVideo = isVideo;
        this.myTexture = myTexture;
    }
}
[Serializable]
public class GameData
{
    public int gold;
    public int myLastDate = 0;
    public int pieceAmount = 35;
    public int backgroundOrder = 0;
    public bool canTurnPiece;
    public List<PuzzleDiary> puzzleDiary = new List<PuzzleDiary>();
    public List<PuzzleGroup> puzzleGroup = new List<PuzzleGroup>();
    public List<PuzzleBackground> puzzleBackground = new List<PuzzleBackground>();
    public GameData()
    {
        canTurnPiece = false;
        pieceAmount = 35;
    }
}
public class Save_Load_Manager : Singletion<Save_Load_Manager>
{
    [Header("Save-Load")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useSifre;
    public GameData gameData;
    private Save_Load_File_Data_Handler save_Load_File_Data_Handler;
    public override void OnAwake()
    {
        DontDestroyOnLoad(gameObject);
        if (string.IsNullOrEmpty(fileName))
        {
            #if UNITY_2022_1_OR_NEWER
            Debug.LogError("Save_Load scriptinde fileName boş olamaz.");
            UnityEditor.EditorApplication.isPaused = true;
            #endif
            return;
        }
        save_Load_File_Data_Handler =
                    new Save_Load_File_Data_Handler(Application.persistentDataPath, fileName, useSifre);
        LoadGame();
    }

    #region Save-Load Game Fonksiyon
    private void LoadGame()
    {
        gameData = save_Load_File_Data_Handler.LoadGame();
        if (gameData == null)
        {
            gameData = new GameData();
        }
    }
    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        save_Load_File_Data_Handler.SaveGame(gameData);
    }
    private void OnApplicationQuit()
    {
        //int Year = DateTime.UtcNow.Year;
        //int Month = DateTime.UtcNow.Month;
        //int Day = DateTime.UtcNow.Day;
        ////int Hour = DateTime.UtcNow.Hour + hoursFark;
        //int Hour = DateTime.UtcNow.Hour;
        //int Minute = DateTime.UtcNow.Minute;
        //int Second = DateTime.UtcNow.Second;

        //if (Hour > 24)
        //{
        //    Hour -= 24;
        //    Day += 1;
        //}
        //if (Month == 1 || Month == 3 || Month == 5 || Month == 7 || Month == 8 || Month == 10 || Month == 12)
        //{
        //    if (Day > 31)
        //    {
        //        Day -= 31;
        //        Month += 1;
        //    }
        //}
        //if (Month == 4 || Month == 6 || Month == 9 || Month == 11)
        //{
        //    if (Day > 30)
        //    {
        //        Day -= 30;
        //        Month += 1;
        //    }
        //}
        //if (Month == 2)
        //{
        //    if (Day > 28)
        //    {
        //        Day -= 28;
        //        Month += 1;
        //    }
        //}
        //if (Month > 12)
        //{
        //    Month -= 12;
        //    Year += 1;
        //}
        //DateTime dateTime = new DateTime(Year, Month, Day, Hour, Minute, Second);
        ////gameData.lastOnlineDayTime = new DayTime(dateTime);

        SaveGame();
    }
    #endregion
}
public class Save_Load_File_Data_Handler
{
    // Nereye ve nasıl kayıt yapıalacağını belirleyen class
    private string directoryPath;
    private string fileName;
    private bool useSifre;
    private readonly string sifreName = "HuseyinEmreCAN";
    public Save_Load_File_Data_Handler(string directoryPath, string fileName, bool useSifre)
    {
        this.directoryPath = directoryPath;
        this.fileName = fileName;
        this.useSifre = useSifre;
    }
    public GameData LoadGame()
    {
        string fullDataPath = Path.Combine(directoryPath, fileName + ".kimex");
        GameData loadedData = null;
        if (File.Exists(fullDataPath))
        {
            try
            {
                string jsonData = "";
                using (FileStream stream = new FileStream(fullDataPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        jsonData = reader.ReadToEnd();
                    }
                }
                if (useSifre)
                {
                    jsonData = SifrelemeYap(jsonData);
                }
                loadedData = JsonUtility.FromJson<GameData>(jsonData);
            }
            catch (Exception e)
            {

                Debug.LogError("Error happining when we try to load in " + fullDataPath + "\n" + "Error is " + e);
                throw;
            }
        }
        return loadedData;
    }
    public void SaveGame(GameData gameData)
    {
        string fullDataPath = Path.Combine(directoryPath, fileName + ".kimex");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullDataPath));
            string jsonData = JsonUtility.ToJson(gameData, true);
            if (useSifre)
            {
                jsonData = SifrelemeYap(jsonData);
            }
            using (FileStream stream = new FileStream(fullDataPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(jsonData);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error happining when we try to save in " + fullDataPath + "\n" + "Error is " + e);
        }
    }
    public string SifrelemeYap(string gameData)
    {
        string sifreliData = "";
        for (int e = 0; e < gameData.Length; e++)
        {
            sifreliData += (char)(gameData[e] ^ sifreName[e % sifreName.Length]);
        }
        return sifreliData;
    }
}