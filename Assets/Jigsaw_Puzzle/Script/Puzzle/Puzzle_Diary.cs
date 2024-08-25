using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PuzzleDiary : PuzzleSingle
{
    public string myDate;

    public PuzzleDiary (bool isVideo, bool isFixed, Sprite mySprite, string myDate) : base (isVideo, isFixed, mySprite)
    {
        this.myDate = myDate;
    }
}
public class Puzzle_Diary : MonoBehaviour
{
    private Image myImage;
    private int diaryOrder;
    private GameObject puzzleFixed;
    private GameObject puzzleVideo;
    private TextMeshProUGUI textPuzzleDate;
    private PuzzleDiary puzzleDiary;

    public void SetPuzzleDiary(PuzzleDiary diary, int order)
    {
        myImage = GetComponent<Image>();
        puzzleFixed = transform.Find("Image-Fixed").gameObject;
        puzzleVideo = transform.Find("Image-Video").gameObject;
        textPuzzleDate = transform.Find("Panel-Date").Find("Text-Date").GetComponent<TextMeshProUGUI>();

        puzzleDiary = diary;
        diaryOrder = order;
        myImage.sprite = puzzleDiary.mySprite;
        string price = puzzleDiary.myDate + " ";
        textPuzzleDate.text = price;
        puzzleVideo.SetActive(puzzleDiary.isVideo);
        puzzleFixed.SetActive(puzzleDiary.isFixed);
    }
    // Puzzle-Diary prefabinde buttona atandı.
    public void StartPuzzle()
    {
        Canvas_Manager.Instance.ShowPuzzleSetting(myImage.sprite, puzzleDiary.isVideo, diaryOrder);
    }
}