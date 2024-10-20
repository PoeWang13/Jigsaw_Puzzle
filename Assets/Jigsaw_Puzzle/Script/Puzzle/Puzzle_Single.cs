using UnityEngine;
using UnityEngine.UI;

public class Puzzle_Single : MonoBehaviour
{
    private int groupOrder;
    private int groupPartOrder;
    private int singleOrder;
    private RawImage myImage;
    private GameObject puzzleFixed;
    private GameObject puzzleVideo;
    private PuzzleSingle puzzleSingle;

    public void SetPuzzleSingle(PuzzleSingle puzzSingle, int order, int partOrder, int single)
    {
        if (puzzleFixed is null)
        {
            myImage = GetComponent<RawImage>();
            puzzleFixed = transform.Find("Image-Fixed").gameObject;
            puzzleVideo = transform.Find("Image-Video").gameObject;
        }

        puzzleSingle = puzzSingle;
        groupOrder = order;
        groupPartOrder = partOrder;
        singleOrder = single;
        myImage.texture = null;
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
        if (myImage.texture == null)
        {
            Warning_Manager.Instance.ShowMessage("This puzzle not ready...", 2);
        }
        else
        {
            Canvas_Manager.Instance.ShowPuzzleSetting(puzzleSingle.myTexture, puzzleSingle.isVideo, groupOrder, groupPartOrder, singleOrder);
        }
    }
    private void Update()
    {
        if (myImage.texture == null)
        {
            myImage.texture = puzzleSingle.myTexture;
        }
    }
}