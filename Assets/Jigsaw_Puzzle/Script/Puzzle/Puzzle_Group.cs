using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class PuzzleGroup
{
    public string myName;
    public List<PuzzleGroupPart> puzzleGroupList = new List<PuzzleGroupPart>();

    public PuzzleGroup(string myName, List<PuzzleGroupPart> puzzleList)
    {
        this.myName = myName;
        this.puzzleGroupList = puzzleList;
    }
}
[System.Serializable]
public class PuzzleGroupPart
{
    public int myPrice;
    public int myNewPrice;
    public string myName;
    public Sprite mySprite;
    public List<PuzzleSingle> puzzleSingle = new List<PuzzleSingle>();

    public PuzzleGroupPart(int myPrice, string myName, Sprite mySprite, List<PuzzleSingle> puzzleSingle)
    {
        this.myPrice = myPrice;
        this.myName = myName;
        this.mySprite = mySprite;
        this.puzzleSingle = puzzleSingle;
    }
}
public class Puzzle_Group : MonoBehaviour
{
    private int groupOrder;
    private int groupPartOrder;
    private Image myImage;
    private TextMeshProUGUI textPuzzlePrice;
    private TextMeshProUGUI textPuzzleAmount;
    private TextMeshProUGUI textPuzzleName;
    private PuzzleGroupPart puzzleGroupPart;

    public void SetPuzzleGroup(PuzzleGroupPart groupPart, int order, int partOrder)
    {
        myImage = GetComponent<Image>();
        textPuzzleName = transform.Find("Panel-Name").Find("Text-Name").GetComponent<TextMeshProUGUI>();
        textPuzzlePrice = transform.Find("Panel-Price").Find("Text-Price").GetComponent<TextMeshProUGUI>();
        textPuzzleAmount = transform.Find("Panel-Amount").Find("Text-Amount").GetComponent<TextMeshProUGUI>();

        puzzleGroupPart = groupPart;
        groupOrder = order;
        groupPartOrder = partOrder;
        myImage.sprite = puzzleGroupPart.mySprite;
        textPuzzleAmount.text = puzzleGroupPart.puzzleSingle.ToString();
        textPuzzleName.text = puzzleGroupPart.myName;

        SetText();
    }
    public void SetText()
    {
        int puzzleFix = 0;
        int puzzleVideo = 0;
        int puzzlePrice = puzzleGroupPart.myPrice;
        int puzzlePricePart = puzzleGroupPart.myPrice / puzzleGroupPart.puzzleSingle.Count;
        for (int e = 0; e < puzzleGroupPart.puzzleSingle.Count; e++)
        {
            if (!puzzleGroupPart.puzzleSingle[e].isVideo)
            {
                puzzlePrice -= puzzlePricePart;
                puzzleVideo++;
            }
            if (puzzleGroupPart.puzzleSingle[e].isFixed)
            {
                puzzleFix++;
            }
        }
        if (puzzleVideo == puzzleGroupPart.puzzleSingle.Count)
        {
            puzzleGroupPart.myNewPrice = 0;
            textPuzzlePrice.text = "";
        }
        else
        {
            puzzleGroupPart.myNewPrice = puzzlePrice;
            string price = puzzleGroupPart.myNewPrice.ToString() + " ";
            textPuzzlePrice.text = price;
        }
        textPuzzleAmount.text = " " + puzzleFix + " / " + puzzleGroupPart.puzzleSingle.Count;
    }
    // Puzzle-Group prefabinde buttona atandı.
    public void ShowAllSinglePuzzle()
    {
        Canvas_Manager.Instance.ShowAllSinglePuzzle(puzzleGroupPart, textPuzzleAmount.text, groupOrder, groupPartOrder);
    }
}