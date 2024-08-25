using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PuzzleSingle
{
    public bool isVideo;
    public bool isFixed;
    public Sprite mySprite;

    public PuzzleSingle(bool isVideo, bool isFixed, Sprite mySprite)
    {
        this.isVideo = isVideo;
        this.isFixed = isFixed;
        this.mySprite = mySprite;
    }
}
public class Puzzle_Single : MonoBehaviour
{
    private int groupOrder;
    private int groupPartOrder;
    private int singleOrder;
    private Image myImage;
    private GameObject puzzleFixed;
    private GameObject puzzleVideo;
    private PuzzleSingle puzzleSingle;

    public void SetPuzzleSingle(PuzzleSingle puzzSingle, int order, int partOrder, int single)
    {
        if (puzzleFixed is null)
        {
            myImage = GetComponent<Image>();
            puzzleFixed = transform.Find("Image-Fixed").gameObject;
            puzzleVideo = transform.Find("Image-Video").gameObject;
        }

        puzzleSingle = puzzSingle;
        groupOrder = order;
        groupPartOrder = partOrder;
        singleOrder = single;
        myImage.sprite = puzzleSingle.mySprite;
        puzzleVideo.SetActive(puzzleSingle.isVideo);
        puzzleFixed.SetActive(puzzleSingle.isFixed);
    }
    public void SetVideo()
    {
        puzzleVideo.SetActive(puzzleSingle.isVideo);
    }
    // Puzzle-Single prefabinde buttona atandı.
    public void StartPuzzle()
    {
        Canvas_Manager.Instance.ShowPuzzleSetting(myImage.sprite, puzzleSingle.isVideo, groupOrder, groupPartOrder, singleOrder);
    }
}