using System;
using System.IO;
using System.Net;
using UnityEngine;
using System.Collections;
using System.Globalization;
using UnityEngine.Networking;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;

[Serializable]
public class GameBackground
{
    public int myPrice;
    public string backgroundData;
}
[Serializable]
public class GameDiary
{
    public bool isVideo;
    public string diaryDatas;
}
[Serializable]
public class GameGroupPart
{
    public int groupPartPrice;
    public string groupPartName;
    public List<GameDiary> groupPartDatas = new List<GameDiary>();
}
[Serializable]
public class GameGroupDatas
{
    public string groupName;
    public List<GameGroupPart> allGroupPartDatas = new List<GameGroupPart>();
}
[Serializable]
public class GameDatas
{
    public List<GameDiary> diaryDatas = new List<GameDiary>();
    public List<GameGroupDatas> groupDatas = new List<GameGroupDatas>();
    public List<GameBackground> gameBackground = new List<GameBackground>();
}
[Serializable]
public class SpriteDataContainer
{
    public GameDatas GameDatas;
}
public class Download_Manager : Singletion<Download_Manager>
{
    [SerializeField] private SpriteDataContainer allSpriteDatas = new SpriteDataContainer();

    private int gameDataLearningDeneme = 10;
    private string driveJsonLink = "1YFz7NyFlrjhHtJBISpbS8HXJUmFzx5bR";
    private string driveStartLink = "https://drive.google.com/uc?export=download&id=";
    private DateTime dateTime;
    private void Start()
    {
        if (Game_Manager.Instance.AreWeOnline)
        {
            DayTime();
            StartCoroutine(GetGamesData(driveStartLink + driveJsonLink));
        }
    }
    [ContextMenu("Day Time")]
    private void DayTime()
    {
        // Yerel saati veriyor.
        try
        {
            using (var response = WebRequest.Create("http://www.google.com").GetResponse())
            {
                dateTime = DateTime.ParseExact(response.Headers["date"], "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal);
                //Debug.Log(dateTime.DayOfYear);
                //Debug.Log(dateTime.Month);
                //Debug.Log(dateTime.Day);
            }
        }
        catch (WebException)
        {
            Debug.Log(DateTime.Now);
        }
    }
    [ContextMenu("Day Time1")]
    private void DayTime1()
    {
        if (!File.Exists(Application.persistentDataPath + "/Diary"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Diary");
        }
        if (!File.Exists(Application.persistentDataPath + "/AllGroup"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/AllGroup");
        }
    }
    IEnumerator GetGamesData(string url)
    {
        gameDataLearningDeneme--;
        if (gameDataLearningDeneme >= 0)
        {
            Debug.Log("Sprite Data almayı " + (10 - gameDataLearningDeneme) + " defadır deniyoruz.");
            bool hataVar = false;
            UnityWebRequest unityWebRequest = UnityWebRequest.Get(url);

            yield return unityWebRequest.SendWebRequest();
            UnityWebRequest.Result result = unityWebRequest.result;
            if (result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning("Bilgi gelmedi. Hata : " + unityWebRequest.error);
            }
            else
            {
                try
                {
                    allSpriteDatas = JsonUtility.FromJson<SpriteDataContainer>(unityWebRequest.downloadHandler.text);
                    if (!File.Exists(Application.persistentDataPath + "/Diary"))
                    {
                        Directory.CreateDirectory(Application.persistentDataPath + "/Diary");
                    }
                    if (!File.Exists(Application.persistentDataPath + "/AllGroup"))
                    {
                        Directory.CreateDirectory(Application.persistentDataPath + "/AllGroup");
                    }
                    if (!File.Exists(Application.persistentDataPath + "/Background"))
                    {
                        Directory.CreateDirectory(Application.persistentDataPath + "/Background");
                    }
                    DownloadDiarySprite();
                    DownloadAllGroupSprite();
                    DownloadBackgroundSprite();
                }
                catch
                {
                    hataVar = true;
                }
            }
            if (hataVar)
            {
                Debug.LogError("Bilgi uyumsuz geldi. Hata : " + unityWebRequest.error);
                StartCoroutine(GetGamesData(driveStartLink + driveJsonLink));
            }
            unityWebRequest.Dispose();
        }
    }
    private void DownloadBackgroundSprite()
    {
        if (Save_Load_Manager.Instance.gameData.puzzleBackground.Count < allSpriteDatas.GameDatas.gameBackground.Count)
        {
            for (int e = Save_Load_Manager.Instance.gameData.puzzleBackground.Count; e < allSpriteDatas.GameDatas.gameBackground.Count; e++)
            {
                Save_Load_Manager.Instance.gameData.puzzleBackground.Add(new PuzzleBackground());
            }
        }
        for (int e = 0; e < allSpriteDatas.GameDatas.gameBackground.Count; e++)
        {
            if (File.Exists(Application.persistentDataPath + "/Background/Background-" + e + ".kimex"))
            {
                byte[] allByte;
                // Dosya var yani resim indirilmiş.
                allByte = File.ReadAllBytes(Application.persistentDataPath + "/Background/Background-" + e + ".kimex");
                Texture2D gameTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                gameTexture.LoadImage(allByte);
                Save_Load_Manager.Instance.gameData.puzzleBackground[e].myTexture = gameTexture;
            }
            else
            {
                Save_Load_Manager.Instance.gameData.puzzleBackground[e].isOpen = false;
                Save_Load_Manager.Instance.gameData.puzzleBackground[e].isPrice = allSpriteDatas.GameDatas.gameBackground[e].myPrice;
                // Dosya yok yani resim indirilmemiş.
                StartCoroutine(GetBackgroundSpriteData(driveStartLink + allSpriteDatas.GameDatas.gameBackground[e].backgroundData, 
                    Application.persistentDataPath +"/Background/Background-" + e + ".kimex", e));
            }
        }
    }
    IEnumerator GetBackgroundSpriteData(string url, string path, int order)
    {
        UnityWebRequest unityWebRequest = UnityWebRequest.Get(url);
        unityWebRequest.downloadHandler = new DownloadHandlerTexture();

        yield return unityWebRequest.SendWebRequest();
        UnityWebRequest.Result result = unityWebRequest.result;
        if (result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning("Bilgi gelmedi. Hata : " + unityWebRequest.error);
        }
        else
        {
            Texture2D gameTexture = DownloadHandlerTexture.GetContent(unityWebRequest);
            byte[] screenShotByteArray = gameTexture.EncodeToPNG();
            File.WriteAllBytes(path, screenShotByteArray);
            Save_Load_Manager.Instance.gameData.puzzleBackground[order].myTexture = gameTexture;
        }
        unityWebRequest.Dispose();
    }
    private void DownloadDiarySprite()
    {
        for (int e = 0; e < Save_Load_Manager.Instance.gameData.puzzleDiary.Count; e++)
        {
            if (File.Exists(Application.persistentDataPath + "/Diary/Diary-" + e + ".kimex"))
            {
                byte[] allByte;
                // Dosya var yani resim indirilmiş.
                allByte = File.ReadAllBytes(Application.persistentDataPath + "/Diary/Diary-" + e + ".kimex");
                Texture2D gameTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                gameTexture.LoadImage(allByte);
                Save_Load_Manager.Instance.gameData.puzzleDiary[e].myTexture = gameTexture;
            }
            else
            {
                // Dosya yok yani resim indirilmemiş.
                StartCoroutine(GetDiarySpriteData(driveStartLink + allSpriteDatas.GameDatas.diaryDatas[e].diaryDatas, Application.persistentDataPath + "/Diary/Diary-" + Save_Load_Manager.Instance.gameData.puzzleDiary.Count + ".kimex",
                    allSpriteDatas.GameDatas.diaryDatas[Save_Load_Manager.Instance.gameData.puzzleDiary.Count].isVideo, dateTime.Day + "/" + dateTime.Month));
            }
        }
        if (dateTime.DayOfYear != Save_Load_Manager.Instance.gameData.myLastDate)
        {
            // Gün atlanmış yeni resim indir.
            StartCoroutine(GetDiarySpriteData(driveStartLink + allSpriteDatas.GameDatas.diaryDatas[Save_Load_Manager.Instance.gameData.puzzleDiary.Count].diaryDatas,
                Application.persistentDataPath + "/Diary/Diary-" + Save_Load_Manager.Instance.gameData.puzzleDiary.Count + ".kimex",
                allSpriteDatas.GameDatas.diaryDatas[Save_Load_Manager.Instance.gameData.puzzleDiary.Count].isVideo, dateTime.Day + "/" + dateTime.Month));
        }
    }
    IEnumerator GetDiarySpriteData(string url, string path, bool isVideo, string date)
    {
        UnityWebRequest unityWebRequest = UnityWebRequest.Get(url);
        unityWebRequest.downloadHandler = new DownloadHandlerTexture();

        yield return unityWebRequest.SendWebRequest();
        UnityWebRequest.Result result = unityWebRequest.result;
        if (result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning("Bilgi gelmedi. Hata : " + unityWebRequest.error);
        }
        else
        {
            Texture2D gameTexture = DownloadHandlerTexture.GetContent(unityWebRequest);
            byte[] screenShotByteArray = gameTexture.EncodeToPNG();
            File.WriteAllBytes(path, screenShotByteArray);
            Save_Load_Manager.Instance.gameData.puzzleDiary.Add(new PuzzleDiary(isVideo, gameTexture, date));
            Save_Load_Manager.Instance.gameData.myLastDate = dateTime.DayOfYear;
        }
        unityWebRequest.Dispose();
    }
    private void DownloadAllGroupSprite()
    {
        for (int h = 0; h < allSpriteDatas.GameDatas.groupDatas.Count; h++)
        {
            bool hasGroup = false;
            string grupName = allSpriteDatas.GameDatas.groupDatas[h].groupName;
            for (int e = 0; e < Save_Load_Manager.Instance.gameData.puzzleGroup.Count && !hasGroup; e++)
            {
                if (Save_Load_Manager.Instance.gameData.puzzleGroup[e].groupName == grupName)
                {
                    // Group var
                    hasGroup = true;
                }
            }
            if (!hasGroup)
            {
                //Debug.Log(grupName + " eklendi.");
                Save_Load_Manager.Instance.gameData.puzzleGroup.Add(new PuzzleGroup(grupName));
                Directory.CreateDirectory(Application.persistentDataPath + "/AllGroup/" + grupName);
            }
            for (int e = 0; e < allSpriteDatas.GameDatas.groupDatas[h].allGroupPartDatas.Count; e++)
            {
                bool hasGroupPart = false;
                string grupPartName = allSpriteDatas.GameDatas.groupDatas[h].allGroupPartDatas[e].groupPartName;
                for (int c = 0; c < Save_Load_Manager.Instance.gameData.puzzleGroup[h].puzzleGroupList.Count && !hasGroupPart; c++)
                {
                    //Debug.Log(Save_Load_Manager.Instance.gameData.puzzleGroup[h].puzzleGroupList[c].groupPartName);
                    if (Save_Load_Manager.Instance.gameData.puzzleGroup[h].puzzleGroupList[c].groupPartName == grupPartName)
                    {
                        // GroupPart var
                        hasGroupPart = true;
                    }
                }
                if (!hasGroupPart)
                {
                    //Debug.Log(grupName + " grubunda " + grupPartName + " eklendi.");
                    PuzzleGroupPart puzzleGroupPart = new PuzzleGroupPart(allSpriteDatas.GameDatas.groupDatas[h].allGroupPartDatas[e].groupPartPrice, grupPartName);
                    Save_Load_Manager.Instance.gameData.puzzleGroup[h].puzzleGroupList.
                        Add(puzzleGroupPart);
                    Directory.CreateDirectory(Application.persistentDataPath + "/AllGroup/" + grupName + "/" + grupPartName);

                    for (int c = 0; c < allSpriteDatas.GameDatas.groupDatas[h].allGroupPartDatas[e].groupPartDatas.Count; c++)
                    {
                        //Debug.Log(grupName + " grubunda " + grupPartName + " bölümünde " + c + " resmi için yer açıldı.");
                        puzzleGroupPart.puzzleSingle.Add(new PuzzleSingle());
                    }
                }
                for (int c = 0; c < allSpriteDatas.GameDatas.groupDatas[h].allGroupPartDatas[e].groupPartDatas.Count; c++)
                {
                    if (File.Exists(Application.persistentDataPath + "/AllGroup/" + grupName + "/" + grupPartName + "/" + grupPartName + "-" + e + ".kimex"))
                    {
                        byte[] allByte;
                        // Dosya var yani resim indirilmiş.
                        allByte = File.ReadAllBytes(Application.persistentDataPath + "/AllGroup/" + grupName + "/" + grupPartName + "/" + grupPartName + "-" + c + ".kimex");
                        Texture2D gameTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                        gameTexture.LoadImage(allByte);

                        Save_Load_Manager.Instance.gameData.puzzleGroup[h].puzzleGroupList[e].puzzleSingle[c].myTexture = gameTexture;
                    }
                    else
                    {
                        // Dosya yok yani resim indirilmemiş.
                        StartCoroutine(GetGroupSpriteData(driveStartLink + allSpriteDatas.GameDatas.groupDatas[h].allGroupPartDatas[e].groupPartDatas[c].diaryDatas,
                            Application.persistentDataPath + "/AllGroup/" + grupName + "/" + grupPartName + "/" + grupPartName + "-" + c + ".kimex",
                            h, e, c, allSpriteDatas.GameDatas.groupDatas[h].allGroupPartDatas[e].groupPartDatas[c].isVideo));
                    }
                }
            }
        }
    }
    IEnumerator GetGroupSpriteData(string url, string path, int groupOrder, int groupPartOrder, int puzzleOrder, bool isVideo)
    {
        UnityWebRequest unityWebRequest = UnityWebRequest.Get(url);
        unityWebRequest.downloadHandler = new DownloadHandlerTexture();

        yield return unityWebRequest.SendWebRequest();
        UnityWebRequest.Result result = unityWebRequest.result;
        if (result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning("Bilgi gelmedi. Hata : " + unityWebRequest.error);
        }
        else
        {
            Texture2D gameTexture = DownloadHandlerTexture.GetContent(unityWebRequest);
            byte[] screenShotByteArray = gameTexture.EncodeToPNG();
            File.WriteAllBytes(path, screenShotByteArray);

            Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].puzzleSingle[puzzleOrder] = new PuzzleSingle(isVideo, gameTexture);
        }
        unityWebRequest.Dispose();
    }
}