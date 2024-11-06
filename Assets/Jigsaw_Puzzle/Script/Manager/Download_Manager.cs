using System;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Globalization;
using UnityEngine.Networking;
using System.Collections.Generic;
using TMPro;

public enum ImageType
{
    Diary,
    Single,
    Background
}
[Serializable]
public class DownloadHolder
{
    public RawImage rawImage;
    public ImageType imageType;
    public List<string> downloadInfo = new List<string>();
}
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
    [SerializeField] private List<DownloadHolder> downloadInfos = new List<DownloadHolder>();
    // RawImage+indirme linki + indirme yeri + indirme ismi

    private bool downloadTime = false;
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
                    DownloadBackgroundSprite();
                    DownloadAllGroupSprite();
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
    private void AddRawImageList(ImageType imageType, RawImage rawImage, List<string> downloadInfo)
    {
        DownloadHolder downloadHolder = new DownloadHolder();
        downloadHolder.imageType = imageType;
        downloadHolder.rawImage = rawImage;
        for (int e = 0; e < downloadInfo.Count; e++)
        {
            downloadHolder.downloadInfo.Add(downloadInfo[e]);
        }
        downloadInfos.Add(downloadHolder);
        if (!downloadTime)
        {
            downloadTime = true;
            // Dosya yok yani resim indirilmemiş.
            StartCoroutine(GetSpriteData());
        }
    }
    IEnumerator GetSpriteData()
    {
        UnityWebRequest unityWebRequest = UnityWebRequest.Get(downloadInfos[0].downloadInfo[0]);
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
            File.WriteAllBytes(downloadInfos[0].downloadInfo[1], screenShotByteArray);
            Save_Load_Manager.Instance.gameData.myLastDate = dateTime.DayOfYear;
            if (downloadInfos[0].rawImage != null)
            {
                downloadInfos[0].rawImage.texture = gameTexture;
            }
            int order = -1;
            if (downloadInfos[0].imageType == ImageType.Diary)
            {
                order = int.Parse(downloadInfos[0].downloadInfo[2]);
                Save_Load_Manager.Instance.gameData.puzzleDiary[order].myTexture = gameTexture;
            }
            if (downloadInfos[0].imageType == ImageType.Single)
            {
                //string url, string path, int groupOrder, int groupPartOrder, int puzzleOrder, bool isVideo
                int groupOrder = int.Parse(downloadInfos[0].downloadInfo[2]);
                int groupPartOrder = int.Parse(downloadInfos[0].downloadInfo[3]);
                int puzzleOrder = int.Parse(downloadInfos[0].downloadInfo[4]);
                Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].puzzleSingle[puzzleOrder].myTexture = gameTexture;
            }
            if (downloadInfos[0].imageType == ImageType.Background)
            {
                order = int.Parse(downloadInfos[0].downloadInfo[2]);
                Save_Load_Manager.Instance.gameData.puzzleBackground[order].myTexture = gameTexture;
            }
        }
        unityWebRequest.Dispose();
        downloadInfos.RemoveAt(0);
        if (downloadInfos.Count > 0)
        {
            // Dosya yok yani resim indirilmemiş.
            StartCoroutine(GetSpriteData());
        }
        else
        {
            downloadTime = false;
        }
    }
    private void DownloadDiarySprite()
    {
        // Gün atlanmış yeni resim indir.
        if (dateTime.DayOfYear != Save_Load_Manager.Instance.gameData.myLastDate)
        {
            // Gün atlanmış yeni resim indir.
            Save_Load_Manager.Instance.gameData.puzzleDiary.Add(new PuzzleDiary(true, null, ""));
            Save_Load_Manager.Instance.gameData.myLastDate = dateTime.DayOfYear;
        }
        // Gün atlanmışsa sıradaki resmi ver.
        for (int e = Save_Load_Manager.Instance.gameData.puzzleDiary.Count - 1; e >= 0; e--)
        {
            // Diary için Gameobject üret ve rawımage al
            Puzzle_Diary puzzle = Canvas_Manager.Instance.CreatePuzzleDiarySprite();
            if (File.Exists(Application.persistentDataPath + "/Diary/Diary-" + e + ".kimex"))
            {
                byte[] allByte;
                // Dosya var yani resim indirilmiş.
                allByte = File.ReadAllBytes(Application.persistentDataPath + "/Diary/Diary-" + e + ".kimex");
                Texture2D gameTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                gameTexture.LoadImage(allByte);
                Save_Load_Manager.Instance.gameData.puzzleDiary[e].myTexture = gameTexture;
                puzzle.SetPuzzleFullDiary(e);
            }
            else
            {
                // Dosya yok yani resim indirilmemiş.
                List<string> downloadInfo = new List<string>
                {
                    driveStartLink + allSpriteDatas.GameDatas.diaryDatas[e].diaryDatas,
                    Application.persistentDataPath + "/Diary/Diary-" + e + ".kimex",
                    e.ToString(),
                };
                AddRawImageList(ImageType.Diary, puzzle.GetComponent<RawImage>(), downloadInfo);
                puzzle.SetPuzzleSimpleDiary(e);
                Save_Load_Manager.Instance.gameData.puzzleDiary[e].myDate = dateTime.Day + "/" + dateTime.Month;
                Save_Load_Manager.Instance.gameData.puzzleDiary[e].isVideo = allSpriteDatas.GameDatas.diaryDatas[e].isVideo;
                puzzle.SetDate(dateTime.Day + "/" + dateTime.Month);
                puzzle.SetVideo(allSpriteDatas.GameDatas.diaryDatas[e].isVideo);
            }
        }
    }
    private void DownloadAllGroupSprite()
    {
        for (int h = 0; h < allSpriteDatas.GameDatas.groupDatas.Count; h++)
        {
            bool hasGroup = false;
            Transform puzzleContainer = null;
            string grupName = allSpriteDatas.GameDatas.groupDatas[h].groupName;
            for (int e = 0; e < Save_Load_Manager.Instance.gameData.puzzleGroup.Count && !hasGroup; e++)
            {
                if (Save_Load_Manager.Instance.gameData.puzzleGroup[e].groupName == grupName)
                {
                    // Group var
                    hasGroup = true;
                    puzzleContainer = Canvas_Manager.Instance.CreatePuzzleGroup1();
                    puzzleContainer.GetComponentInChildren<TextMeshProUGUI>().text = grupName;
                }
            }
            if (!hasGroup)
            {
                // Grup yok bu yüzden klasörünü ve data yerini ekliyoruz.
                Save_Load_Manager.Instance.gameData.puzzleGroup.Add(new PuzzleGroup(grupName));
                Directory.CreateDirectory(Application.persistentDataPath + "/AllGroup/" + grupName);

                puzzleContainer = Canvas_Manager.Instance.CreatePuzzleGroup1();
                puzzleContainer.GetComponentInChildren<TextMeshProUGUI>().text = grupName;
            }
            for (int e = 0; e < allSpriteDatas.GameDatas.groupDatas[h].allGroupPartDatas.Count; e++)
            {
                bool hasGroupPart = false;
                string grupPartName = allSpriteDatas.GameDatas.groupDatas[h].allGroupPartDatas[e].groupPartName;
                Puzzle_Group puzzle = Canvas_Manager.Instance.CreatePuzzleGroupPart(puzzleContainer);
                for (int c = 0; c < Save_Load_Manager.Instance.gameData.puzzleGroup[h].puzzleGroupList.Count && !hasGroupPart; c++)
                {
                    // Grubun alt parçası var
                    if (Save_Load_Manager.Instance.gameData.puzzleGroup[h].puzzleGroupList[c].groupPartName == grupPartName)
                    {
                        // GroupPart var
                        hasGroupPart = true;
                    }
                }
                if (!hasGroupPart)
                {
                    // Grubun alt parçası yok bu yüzden klasörünü ve datasını ekliyoruz.
                    PuzzleGroupPart puzzleGroupPart = new PuzzleGroupPart(allSpriteDatas.GameDatas.groupDatas[h].allGroupPartDatas[e].groupPartPrice, grupPartName);
                    Save_Load_Manager.Instance.gameData.puzzleGroup[h].puzzleGroupList.Add(puzzleGroupPart);
                    Directory.CreateDirectory(Application.persistentDataPath + "/AllGroup/" + grupName + "/" + grupPartName);

                    for (int c = 0; c < allSpriteDatas.GameDatas.groupDatas[h].allGroupPartDatas[e].groupPartDatas.Count; c++)
                    {
                        puzzleGroupPart.puzzleSingle.Add(new PuzzleSingle());
                    }
                }
                for (int c = 0; c < allSpriteDatas.GameDatas.groupDatas[h].allGroupPartDatas[e].groupPartDatas.Count; c++)
                {
                    // Diary için Gameobject üret ve rawımage al
                    RawImage rawImageGroup = null;
                    if (File.Exists(Application.persistentDataPath + "/AllGroup/" + grupName + "/" + grupPartName + "/" + grupPartName + "-" + e + ".kimex"))
                    {
                        byte[] allByte;
                        // Dosya var yani resim indirilmiş.
                        allByte = File.ReadAllBytes(Application.persistentDataPath + "/AllGroup/" + grupName + "/" + grupPartName + "/" + grupPartName + "-" + c + ".kimex");
                        Texture2D gameTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                        gameTexture.LoadImage(allByte);

                        Save_Load_Manager.Instance.gameData.puzzleGroup[h].puzzleGroupList[e].puzzleSingle[c].myTexture = gameTexture;
                        if (c == 0)
                        {
                            rawImageGroup = puzzle.transform.GetComponent<RawImage>();
                            rawImageGroup.texture = gameTexture;
                        }
                    }
                    else
                    {
                        // Dosya yok yani resim indirilmemiş.
                        List<string> downloadInfo = new List<string>
                        {
                            driveStartLink + allSpriteDatas.GameDatas.groupDatas[h].allGroupPartDatas[e].groupPartDatas[c].diaryDatas,
                            Application.persistentDataPath + "/AllGroup/" + grupName + "/" + grupPartName + "/" + grupPartName + "-" + c + ".kimex",
                            h.ToString(), e.ToString(), c.ToString(),
                        };
                        if (c == 0)
                        {
                            rawImageGroup = puzzle.transform.GetComponent<RawImage>();
                        }
                        Save_Load_Manager.Instance.gameData.puzzleGroup[h].puzzleGroupList[e].puzzleSingle[c].isVideo = allSpriteDatas.GameDatas.groupDatas[h].allGroupPartDatas[e].groupPartDatas[c].isVideo;
                        AddRawImageList(ImageType.Single, rawImageGroup, downloadInfo);
                    }
                }
                puzzle.SetPuzzleGroup(h, e);
            }
        }
    }
    private void DownloadBackgroundSprite()
    {
        if (Save_Load_Manager.Instance.gameData.puzzleBackground.Count < allSpriteDatas.GameDatas.gameBackground.Count)
        {
            for (int e = Save_Load_Manager.Instance.gameData.puzzleBackground.Count; e < allSpriteDatas.GameDatas.gameBackground.Count; e++)
            {
                Save_Load_Manager.Instance.gameData.puzzleBackground.Add(new PuzzleBackground(allSpriteDatas.GameDatas.gameBackground[e].myPrice));
            }
        }
        Save_Load_Manager.Instance.gameData.puzzleBackground[0].isOpen = true;
        for (int e = 0; e < allSpriteDatas.GameDatas.gameBackground.Count; e++)
        {
            Puzzle_Background puzzleBackground = Canvas_Manager.Instance.SetBackgroundButton(e);
            puzzleBackground.SetPuzzleBackground(e);
            if (File.Exists(Application.persistentDataPath + "/Background/Background-" + e + ".kimex"))
            {
                byte[] allByte;
                // Dosya var yani resim indirilmiş. 130 99 030
                allByte = File.ReadAllBytes(Application.persistentDataPath + "/Background/Background-" + e + ".kimex");
                Texture2D gameTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                gameTexture.LoadImage(allByte);
                Save_Load_Manager.Instance.gameData.puzzleBackground[e].myTexture = gameTexture;
                puzzleBackground.GetComponent<RawImage>().texture = gameTexture;
            }
            else
            {
                // Dosya yok yani resim indirilmemiş.
                List<string> downloadInfo = new List<string>
                {
                    driveStartLink + allSpriteDatas.GameDatas.gameBackground[e].backgroundData,
                    Application.persistentDataPath + "/Background/Background-" + e + ".kimex",
                    e.ToString(),
                };
                AddRawImageList(ImageType.Background, puzzleBackground.GetComponent<RawImage>(), downloadInfo);
            }
        }
    }
}