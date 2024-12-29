using TMPro;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    private bool downloadSprite = false;
    private bool downloadTime = false;
    private bool diaryCall = false;
    private bool groupCall = false;
    private bool backgroundCall = false;
    private string driveJsonLink = "1YFz7NyFlrjhHtJBISpbS8HXJUmFzx5bR";
    private string driveStartLink = "https://drive.google.com/uc?export=download&id=";

    private void Start()
    {
        if (Game_Manager.Instance.AreWeOnline)
        {
            StartCoroutine(GetGamesData(driveStartLink + driveJsonLink));
        }
    }
    IEnumerator GetGamesData(string url)
    {
        Warning_Manager.Instance.ShowMessage("We are checking your sprite. Wait a second please...", 3);
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
                DownloadSprite();
            }
            catch
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        unityWebRequest.Dispose();
    }
    private void DownloadSprite()
    {
        if (!diaryCall)
        {
            if (!File.Exists(Application.persistentDataPath + "/Diary"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/Diary");
            }
            try
            {
                DownloadDiarySprite();
                diaryCall = true;
            }
            catch
            {
                Debug.LogError("Diary sprite arızası.");
            }
        }
        if (!groupCall)
        {
            if (!File.Exists(Application.persistentDataPath + "/AllGroup"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/AllGroup");
            }
            try
            {
                DownloadAllGroupSprite();
                groupCall = true;
            }
            catch
            {
                Debug.LogError("Group sprite arızası.");
            }
        }
        if (!backgroundCall)
        {
            if (!File.Exists(Application.persistentDataPath + "/Background"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/Background");
            }
            try
            {
                DownloadBackgroundSprite();
                backgroundCall = true;
            }
            catch
            {
                Debug.LogError("Background sprite arızası.");
            }
        }
    }
    private void AddRawImageList(ImageType imageType, RawImage rawImage, List<string> downloadInfo)
    {
        if (!downloadSprite)
        {
            Warning_Manager.Instance.ShowMessage("We start to download your sprite... (Just one time.)", 3);
            downloadSprite = true;
        }
        DownloadHolder downloadHolder = new DownloadHolder();
        downloadHolder.rawImage = rawImage;
        downloadHolder.imageType = imageType;
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
            try
            {
                Texture2D gameTexture = DownloadHandlerTexture.GetContent(unityWebRequest);
                byte[] screenShotByteArray = gameTexture.EncodeToPNG();
                File.WriteAllBytes(downloadInfos[0].downloadInfo[1], screenShotByteArray);
                Save_Load_Manager.Instance.gameData.myLastDate = Game_Manager.Instance.DateTime.DayOfYear;
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
                else if (downloadInfos[0].imageType == ImageType.Single)
                {
                    //string url, string path, int groupOrder, int groupPartOrder, int puzzleOrder, bool isVideo
                    int groupOrder = int.Parse(downloadInfos[0].downloadInfo[2]);
                    int groupPartOrder = int.Parse(downloadInfos[0].downloadInfo[3]);
                    int puzzleOrder = int.Parse(downloadInfos[0].downloadInfo[4]);
                    Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].puzzleSingle[puzzleOrder].myTexture = gameTexture;
                }
                else if (downloadInfos[0].imageType == ImageType.Background)
                {
                    order = int.Parse(downloadInfos[0].downloadInfo[2]);
                    Save_Load_Manager.Instance.gameData.puzzleBackground[order].myTexture = gameTexture;
                }
            }
            catch
            {
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
        bool showDailyWarning = false;
        // Gün atlanmış yeni resim için datada yer oluştur.
        if (Game_Manager.Instance.DateTime.DayOfYear != Save_Load_Manager.Instance.gameData.myLastDate)
        {
            showDailyWarning = true;
            // Gün atlanmış yeni resim indir.
            Save_Load_Manager.Instance.gameData.myLastDate = Game_Manager.Instance.DateTime.DayOfYear;
            Save_Load_Manager.Instance.gameData.puzzleDiary.Add(new PuzzleDaily());
            Save_Load_Manager.Instance.gameData.puzzleDiary.Add(new PuzzleDaily());
            Save_Load_Manager.Instance.gameData.puzzleDiary.Add(new PuzzleDaily());
            Save_Load_Manager.Instance.gameData.puzzleDiary.Add(new PuzzleDaily());
            Save_Load_Manager.Instance.gameData.puzzleDiary.Add(new PuzzleDaily());
        }
        // Gün atlanmışsa sıradaki 5 resmi indir.
        for (int e = Save_Load_Manager.Instance.gameData.puzzleDiary.Count - 1; e >= 0; e--)
        {
            // Diary için Gameobject üret ve rawımage al
            Puzzle_Daily puzzle = Canvas_Manager.Instance.CreatePuzzleDiarySprite();
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
                Save_Load_Manager.Instance.gameData.puzzleDiary[e].myDate = Game_Manager.Instance.DateTime.Day + "/" + Game_Manager.Instance.DateTime.Month;
                Save_Load_Manager.Instance.gameData.puzzleDiary[e].isVideo = allSpriteDatas.GameDatas.diaryDatas[e].isVideo;
                puzzle.SetDate(Game_Manager.Instance.DateTime.Day + "/" + Game_Manager.Instance.DateTime.Month);
                puzzle.SetVideo(allSpriteDatas.GameDatas.diaryDatas[e].isVideo);
            }
        }
        if (showDailyWarning)
        {
            Warning_Manager.Instance.ShowMessage("Daily sprite downloading...", 3);
        }
    }
    private void DownloadAllGroupSprite()
    {
        List<string> groupNames = new List<string>();
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
                    if (!File.Exists(Application.persistentDataPath + "/AllGroup/" + grupName))
                    {
                        Directory.CreateDirectory(Application.persistentDataPath + "/AllGroup/" + grupName);
                    }
                    puzzleContainer = Canvas_Manager.Instance.CreatePuzzleGroup();
                    puzzleContainer.GetComponentInChildren<TextMeshProUGUI>().text = grupName;
                }
            }
            if (!hasGroup)
            {
                // Grup yok bu yüzden klasörünü ve data yerini ekliyoruz.
                Save_Load_Manager.Instance.gameData.puzzleGroup.Add(new PuzzleGroup(grupName));
                Directory.CreateDirectory(Application.persistentDataPath + "/AllGroup/" + grupName);

                puzzleContainer = Canvas_Manager.Instance.CreatePuzzleGroup();
                puzzleContainer.GetComponentInChildren<TextMeshProUGUI>().text = grupName;
            }
            puzzleContainer.name = "Group : " + grupName;
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
                        if (!File.Exists(Application.persistentDataPath + "/AllGroup/" + grupName + "/" + grupPartName))
                        {
                            Directory.CreateDirectory(Application.persistentDataPath + "/AllGroup/" + grupName + "/" + grupPartName);
                        }
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
                    if (File.Exists(Application.persistentDataPath + "/AllGroup/" + grupName + "/" + grupPartName + "/" + grupPartName + "-" + c + ".kimex"))
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
                            if (!groupNames.Contains(grupName))
                            {
                                groupNames.Add(grupName);
                            }
                            rawImageGroup = puzzle.transform.GetComponent<RawImage>();
                        }
                        Save_Load_Manager.Instance.gameData.puzzleGroup[h].puzzleGroupList[e].puzzleSingle[c].isVideo = allSpriteDatas.GameDatas.groupDatas[h].allGroupPartDatas[e].groupPartDatas[c].isVideo;
                        AddRawImageList(ImageType.Single, rawImageGroup, downloadInfo);
                    }
                }
                puzzle.SetPuzzleGroup(h, e);
                puzzle.name = "Group Part : " + grupPartName;
            }
        }
        if (groupNames.Count > 0)
        {
            string downloadingGroupName = "";
            for (int e = 0; e < groupNames.Count; e++)
            {
                downloadingGroupName += ", " + groupNames[e];
            }
            downloadingGroupName = downloadingGroupName.Remove(0, 2);
            Warning_Manager.Instance.ShowMessage(downloadingGroupName + " group sprite downloading...", 3);
        }
    }
    private void DownloadBackgroundSprite()
    {
        bool showBackgroundWarning = false;
        if (Save_Load_Manager.Instance.gameData.puzzleBackground.Count < allSpriteDatas.GameDatas.gameBackground.Count)
        {
            showBackgroundWarning = true;
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
                showBackgroundWarning = true;
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
        if (showBackgroundWarning)
        {
            Warning_Manager.Instance.ShowMessage("Background sprite downloading...", 3);
        }
    }
}