using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Puzzle_Daily : MonoBehaviour
{
    private int diaryOrder;
    private RawImage myImage;
    private GameObject puzzleFixed;
    private GameObject puzzleVideo;
    private PuzzleDaily puzzleDiary;
    private TextMeshProUGUI textPuzzleDate;

    private void SetPuzzle(int order)
    {
        myImage = GetComponent<RawImage>();
        puzzleFixed = transform.Find("Image-Fixed").gameObject;
        puzzleVideo = transform.Find("Image-Video").gameObject;
        textPuzzleDate = transform.Find("Panel-Date").Find("Text-Date").GetComponent<TextMeshProUGUI>();

        puzzleDiary = Save_Load_Manager.Instance.gameData.puzzleDiary[order];
        diaryOrder = order;
    }
    public void SetPuzzleFullDiary(int order)
    {
        SetPuzzle(order);
        myImage.texture = puzzleDiary.myTexture;
        string myDate = " " + puzzleDiary.myDate + " ";
        textPuzzleDate.text = myDate;
        puzzleVideo.SetActive(puzzleDiary.isVideo);
        puzzleFixed.SetActive(puzzleDiary.isFixed);
    }
    public bool CheckDiaryOrder(int order)
    {
        return diaryOrder == order;
    }
    public void SetPuzzleSimpleDiary(int order)
    {
        SetPuzzle(order);
        puzzleFixed.SetActive(false);
    }
    public void SetDate(string myDate)
    {
        textPuzzleDate.text = myDate + " ";
    }
    public void SetVideo(bool video)
    {
        puzzleVideo.SetActive(video);
    }
    public void SetFixed()
    {
        puzzleFixed.SetActive(true);
    }
    // Puzzle-Daily prefabinde buttona atandı.
    public void StartPuzzle()
    {
        if (myImage.texture == null)
        {
            Warning_Manager.Instance.ShowMessage("This puzzle not ready...", 2);
        }
        else
        {
            Canvas_Manager.Instance.ShowPuzzleSetting(puzzleDiary.myTexture, puzzleDiary.isVideo, diaryOrder);
        }
    }
}