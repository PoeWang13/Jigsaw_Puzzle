using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Puzzle_Group : MonoBehaviour
{
    private int groupOrder;
    private int groupPartOrder;
    private RawImage myImage;
    private TextMeshProUGUI textPuzzlePrice;
    private TextMeshProUGUI textPuzzleAmount;
    private TextMeshProUGUI textPuzzleName;
    private PuzzleGroupPart puzzleGroupPart;

    public void SetPuzzleGroup(PuzzleGroupPart groupPart, int order, int partOrder)
    {
        myImage = GetComponent<RawImage>();
        textPuzzleName = transform.Find("Panel-Name").Find("Text-Name").GetComponent<TextMeshProUGUI>();
        textPuzzleAmount = transform.Find("Panel-Amount").Find("Text-Amount").GetComponent<TextMeshProUGUI>();
        Transform price = transform.Find("Panel-Price");
        puzzleGroupPart = groupPart;
        groupOrder = order;
        groupPartOrder = partOrder;
        textPuzzleName.text = puzzleGroupPart.groupPartName;
        if (groupPart.myNewPrice == 0)
        {
            price.gameObject.SetActive(false);
        }
        else
        {
            textPuzzlePrice = price.Find("Text-Price").GetComponent<TextMeshProUGUI>();
            SetPriceText();
        }
        SetAmountText();
    }
    private void Update()
    {
        if (myImage.texture != null)
        {
            return;
        }
        if (puzzleGroupPart.puzzleSingle[0].myTexture == null)
        {
            return;
        }
        myImage.texture = puzzleGroupPart.puzzleSingle[0].myTexture;
    }
    public void SetPriceText()
    {
        int puzzleVideo = 0;
        int puzzlePrice = 0;
        for (int e = 0; e < puzzleGroupPart.puzzleSingle.Count; e++)
        {
            if (puzzleGroupPart.puzzleSingle[e].isVideo)
            {
                puzzleVideo++;
            }
        }
        puzzlePrice = (puzzleGroupPart.myOrjPrice / puzzleGroupPart.puzzleSingle.Count) * puzzleVideo;
        puzzleGroupPart.myNewPrice = puzzlePrice;
        textPuzzlePrice.text = puzzlePrice.ToString();
    }
    public void SetAmountText()
    {
        int puzzleFixed = 0;
        for (int e = 0; e < puzzleGroupPart.puzzleSingle.Count; e++)
        {
            if (puzzleGroupPart.puzzleSingle[e].isFixed)
            {
                puzzleFixed++;
            }
        }
        textPuzzleAmount.text = " " + puzzleFixed + " / " + puzzleGroupPart.puzzleSingle.Count;
    }
    // Puzzle-Group prefabinde buttona atandı.
    public void ShowAllSinglePuzzle()
    {
        Canvas_Manager.Instance.ShowAllSinglePuzzle(puzzleGroupPart, textPuzzleAmount.text, groupOrder, groupPartOrder);
    }
}