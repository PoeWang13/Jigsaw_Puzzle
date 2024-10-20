using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Puzzle_Diary : MonoBehaviour
{
    private RawImage myImage;
    private int diaryOrder;
    private GameObject puzzleFixed;
    private GameObject puzzleVideo;
    private TextMeshProUGUI textPuzzleDate;
    private PuzzleDiary puzzleDiary;

    public void SetPuzzleDiary(int order)
    {
        myImage = GetComponent<RawImage>();
        puzzleFixed = transform.Find("Image-Fixed").gameObject;
        puzzleVideo = transform.Find("Image-Video").gameObject;
        textPuzzleDate = transform.Find("Panel-Date").Find("Text-Date").GetComponent<TextMeshProUGUI>();

        puzzleDiary = Save_Load_Manager.Instance.gameData.puzzleDiary[order];
        diaryOrder = order;
        myImage.texture = null;
        string myDate = " " + puzzleDiary.myDate + " ";
        textPuzzleDate.text = myDate;
        puzzleVideo.SetActive(puzzleDiary.isVideo);
        puzzleFixed.SetActive(puzzleDiary.isFixed);
    }
    // Puzzle-Diary prefabinde buttona atandı.
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
    private void Update()
    {
        if (myImage.texture == null)
        {
            myImage.texture = puzzleDiary.myTexture;
        }
    }
}