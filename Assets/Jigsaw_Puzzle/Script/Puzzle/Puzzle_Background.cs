using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Puzzle_Background : MonoBehaviour
{
    private RawImage myImage;
    private int myOrder;
    private Transform panelPrice;

    public void SetPuzzleBackground(int order)
    {
        myImage = GetComponent<RawImage>();
        myOrder = order;
        panelPrice = transform.Find("Panel-Price");
        if (Save_Load_Manager.Instance.gameData.puzzleBackground[order].isOpen)
        {
            panelPrice.gameObject.SetActive(false);
        }
        else
        {
            panelPrice.GetComponentInChildren<TextMeshProUGUI>().text = Save_Load_Manager.Instance.gameData.puzzleBackground[order].isPrice.ToString();
        }
    }
    // Buttona atandı.
    public void SetPuzzleBackground()
    {
        if (myImage.texture == null)
        {
            Warning_Manager.Instance.ShowMessage("This background not ready...", 2);
        }
        else
        {
            if (Save_Load_Manager.Instance.gameData.puzzleBackground[myOrder].isOpen)
            {
                Canvas_Manager.Instance.SetBackground(myOrder);
            }
            else
            {
                // Açık değil satın al.
                if (Canvas_Manager.Instance.BuyBackground(myOrder))
                {
                    panelPrice.gameObject.SetActive(false);
                }
            }
        }
    }
    private void Update()
    {
        if (myImage.texture == null)
        {
            myImage.texture = Save_Load_Manager.Instance.gameData.puzzleBackground[myOrder].myTexture;
        }
    }
}