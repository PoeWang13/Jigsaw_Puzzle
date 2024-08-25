using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Canvas_Manager : Singletion<Canvas_Manager>
{
    [Header("Puzzle Genel")]
    [SerializeField] private Transform puzzleAllParent;
    [SerializeField] private GameObject puzzleAllContainer;
    [SerializeField] private Transform prefabPuzzleContainer;
    [SerializeField] private TextMeshProUGUI textGoldAmount;

    [Header("Puzzle Diary")]
    [SerializeField] private Puzzle_Diary prefabPuzzle_Diary;

    [Header("Puzzle Group")]
    [SerializeField] private Puzzle_Group prefabPuzzle_Group;
    [SerializeField] private GameObject objPuzzleGroupBuy;

    [Header("Puzzle Single")]
    [SerializeField] private GameObject objGroupList;
    [SerializeField] private Transform puzzleGroupListParent;
    [SerializeField] private Puzzle_Single prefabPuzzleSingle;
    [SerializeField] private TextMeshProUGUI textPuzzleGroupName;
    [SerializeField] private TextMeshProUGUI textPuzzleGroupPrice;
    [SerializeField] private TextMeshProUGUI textPuzzleGroupAmount;

    [Header("Puzzle Setting")]
    [SerializeField] private GameObject objStartPuzzle;
    [SerializeField] private GameObject objIsVideo;
    [SerializeField] private Image imagePuzzleIcon;
    [SerializeField] private TextMeshProUGUI textPuzzleReward;
    [SerializeField] private Button buttonPieceCanTurn;
    [SerializeField] private Button buttonPieceCanDontTurn;
    [SerializeField] private List<Button> puzzleAmountList = new List<Button>();

    [Header("Puzzle Game")]
    [SerializeField] private GameObject objPuzzleGame;
    [SerializeField] private Image puzzleSprite;
    [SerializeField] private Transform puzzleBackgroundParent;
    [SerializeField] private TextMeshProUGUI textFinishGoldAmount;
    [SerializeField] private TextMeshProUGUI textFinishIncreaseAmount;
    [SerializeField] private TextMeshProUGUI textFinishTime;

    [Header("Puzzle Game Button")]
    [SerializeField] private GameObject objPuzzleGameButton;
    [SerializeField] private Image imageGameBackground;
    [SerializeField] private Image imageGameMiniIcon;
    [SerializeField] private Transform pieceHolder;
    [SerializeField] private Transform puzzleGameArea;

    [Header("Puzzle Finish")]
    [SerializeField] private GameObject objPuzzleFinish;


    private bool isVideo;
    private int rewardMulti = 1;
    private int groupOrder;
    private int groupPartOrder;
    private int singleOrder;
    private List<int> puzzleAmount = new List<int>();
    private List<Piece> edgePiece = new List<Piece>();
    private List<Puzzle_Single> puzzleGroupList = new List<Puzzle_Single>();

    private void Start()
    {
        // Puzzle Piece amount butonlarını belirle
        for (int e = 0; e < puzzleAmountList.Count; e++)
        {
            if (int.TryParse(puzzleAmountList[e].GetComponentInChildren<TextMeshProUGUI>().text, out int amount))
            {
                puzzleAmount.Add(amount);
                puzzleAmountList[e].onClick.AddListener(() => SetPieceAmont(amount));
            }
        }

        // Background butonları ayarla
        SetBackgroundButton();

        // Puzzle Diary'leri ayarla
        CreatePuzzleDiary(Save_Load_Manager.Instance.gameData.puzzleDiary);

        // Puzzle Group'larını ayarla
        CreatePuzzleGroup(Save_Load_Manager.Instance.gameData.puzzleGroup);

        // Puzzle Game Piece'lerinin Turn ayarını belirle
        SetTurnPiece(Save_Load_Manager.Instance.gameData.canTurnPiece);

        // Puzzle Game Piece'lerinin Amount ayarını belirle
        SetPieceAmont(Save_Load_Manager.Instance.gameData.pieceAmount);

        // Canvas'da Gold ayarını belirle
        textGoldAmount.text = Save_Load_Manager.Instance.gameData.gold.ToString();
    }
    private void SetBackgroundButton()
    {
        puzzleBackgroundParent.GetChild(0).gameObject.SetActive(true);
        Image image = puzzleBackgroundParent.GetChild(0).GetComponent<Image>();
        image.sprite = Save_Load_Manager.Instance.gameData.puzzleBackground[0].puzzleBackground;
        Button button = puzzleBackgroundParent.GetChild(0).GetComponent<Button>();
        button.onClick.AddListener(() => SetBackground(0));
        // Video gösterimi
        GameObject isVideo = puzzleBackgroundParent.GetChild(0).GetChild(0).gameObject;
        isVideo.SetActive(Save_Load_Manager.Instance.gameData.puzzleBackground[0].isVideo);

        // Price gösterimi
        GameObject isPrice = puzzleBackgroundParent.GetChild(0).Find("Panel-Price").gameObject;
        if (Save_Load_Manager.Instance.gameData.puzzleBackground[0].isPrice == 0)
        {
            isPrice.SetActive(false);
        }
        else
        {
            isPrice.SetActive(true);
            isPrice.GetComponentInChildren<TextMeshProUGUI>().text = Save_Load_Manager.Instance.gameData.puzzleBackground[0].isPrice.ToString();
        }

        for (int e = 1; e < Save_Load_Manager.Instance.gameData.puzzleBackground.Count; e++)
        {
            Transform back = Instantiate(puzzleBackgroundParent.GetChild(0), puzzleBackgroundParent);
            Image backImage = back.GetComponent<Image>();
            backImage.sprite = Save_Load_Manager.Instance.gameData.puzzleBackground[e].puzzleBackground;
            Button backButton = back.GetComponent<Button>();
            int order = e;
            backButton.onClick.AddListener(() => SetBackground(order));
            // Video gösterimi
            GameObject backVideo = back.GetChild(0).gameObject;
            backVideo.SetActive(Save_Load_Manager.Instance.gameData.puzzleBackground[e].isVideo);
            // Price gösterimi
            GameObject backPrice = back.GetChild(1).gameObject;
            backPrice.SetActive(Save_Load_Manager.Instance.gameData.puzzleBackground[e].isPrice != 0);
            backPrice.GetComponentInChildren<TextMeshProUGUI>().text = Save_Load_Manager.Instance.gameData.puzzleBackground[e].isPrice.ToString();
        }

        SetBackground(0);
    }
    private void SetBackground(int order)
    {
        imageGameBackground.sprite = Save_Load_Manager.Instance.gameData.puzzleBackground[order].puzzleBackground;
        puzzleBackgroundParent.parent.gameObject.SetActive(false);
    }
    private void CreatePuzzleDiary(List<PuzzleDiary> puzzleDiaryList)
    {
        Transform puzzleContainer = Instantiate(prefabPuzzleContainer, puzzleAllParent);
        puzzleContainer.GetComponentInChildren<TextMeshProUGUI>().text = "Diary Puzzle";
        RectTransform rectContainer = puzzleContainer.GetComponent<RectTransform>();
        rectContainer.sizeDelta = new Vector2(rectContainer.sizeDelta.x, 260);

        Transform puzzleParent = puzzleContainer.GetChild(1);
        RectTransform rectParent = puzzleParent.GetComponent<RectTransform>();
        rectParent.sizeDelta = new Vector2(rectParent.sizeDelta.x, 230);

        for (int e = 0; e < puzzleDiaryList.Count; e++)
        {
            Puzzle_Diary puzzle = Instantiate(prefabPuzzle_Diary, puzzleParent);
            puzzle.SetPuzzleDiary(puzzleDiaryList[e], e);
        }
    }
    private void CreatePuzzleGroup(List<PuzzleGroup> puzzleGroup)
    {
        // Grupların içindeki btonları oluştur
        for (int e = 0; e < puzzleGroup.Count; e++)
        {
            // Tüm grpları oluştur
            Transform puzzleContainer = Instantiate(prefabPuzzleContainer, puzzleAllParent);
            puzzleContainer.GetComponentInChildren<TextMeshProUGUI>().text = puzzleGroup[e].myName;
            Transform puzzleParent = puzzleContainer.GetChild(1);

            for (int h = 0; h < puzzleGroup[e].puzzleGroupList.Count; h++)
            {
                Puzzle_Group puzzle = Instantiate(prefabPuzzle_Group, puzzleParent);
                puzzle.SetPuzzleGroup(puzzleGroup[e].puzzleGroupList[h], e, h);
            }
        }
    }
    public void ShowAllSinglePuzzle(PuzzleGroupPart puzzleGroupPart, string puzzleFix, int order, int partOrder)
    {
        objGroupList.SetActive(true);
        puzzleAllContainer.SetActive(false);

        for (int e = 0; e < puzzleGroupListParent.childCount; e++)
        {
            puzzleGroupListParent.GetChild(e).gameObject.SetActive(false);
        }
        if (puzzleGroupPart.puzzleSingle.Count > puzzleGroupListParent.childCount)
        {
            int newPuzzleAmount = puzzleGroupPart.puzzleSingle.Count - puzzleGroupListParent.childCount;
            for (int i = 0; i < newPuzzleAmount; i++)
            {
                Puzzle_Single puzzle = Instantiate(prefabPuzzleSingle, puzzleGroupListParent);
                puzzleGroupList.Add(puzzle);
            }
        }
        for (int i = 0; i < puzzleGroupPart.puzzleSingle.Count; i++)
        {
            puzzleGroupList[i].gameObject.SetActive(true);
            puzzleGroupList[i].SetPuzzleSingle(puzzleGroupPart.puzzleSingle[i], order, partOrder, i);
        }

        textPuzzleGroupName.text = puzzleGroupPart.myName;
        if (puzzleGroupPart.myNewPrice == 0)
        {
            textPuzzleGroupPrice.text = "Free";
        }
        else
        {
            textPuzzleGroupPrice.text = puzzleGroupPart.myNewPrice.ToString();
        }
        textPuzzleGroupAmount.text = puzzleFix.ToString();

        objPuzzleGroupBuy.SetActive(puzzleGroupPart.myNewPrice > 0);
    }
    // Canvas -> Puzzle-Group-List -> Panel-Title -> Button-Buy butonuna eklendi
    public void BuyGroupPart()
    {
        int price = int.Parse(textPuzzleGroupPrice.text);
        if (Save_Load_Manager.Instance.gameData.gold < price)
        {
            return;
        }
        Save_Load_Manager.Instance.gameData.gold -= price;
        textGoldAmount.text = Save_Load_Manager.Instance.gameData.gold.ToString();

        for (int e = 0; e < Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].puzzleSingle.Count; e++)
        {
            Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].puzzleSingle[e].isVideo = false;
        }
        Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].myNewPrice = 0;
        textPuzzleGroupPrice.text = "Free";
        SetPuzzleGroupText();

        for (int e = 0; e < puzzleGroupListParent.childCount; e++)
        {
            puzzleGroupListParent.GetChild(e).GetComponent<Puzzle_Single>().SetVideo();
        }

        objPuzzleGroupBuy.SetActive(false);
    }
    public void ShowPuzzleSetting(Sprite mySprite, bool isVideo, int order = -1, int partOrder = -1, int single = -1)
    {
        objStartPuzzle.SetActive(true);

        objIsVideo.SetActive(isVideo);
        imagePuzzleIcon.sprite = mySprite;
        imageGameMiniIcon.sprite = mySprite;
        puzzleSprite.sprite = mySprite;
        this.isVideo = isVideo;

        textPuzzleReward.text = (Save_Load_Manager.Instance.gameData.pieceAmount * rewardMulti).ToString();
        groupOrder = order;
        groupPartOrder = partOrder;
        singleOrder = single;
    }
    // Canvas -> Show-Puzzle-Setting -> Turn-Parent -> Button-Turn ve Button-Dont-Turn butonlarına eklendi
    public void SetTurnPiece(bool canTurn)
    {
        Save_Load_Manager.Instance.gameData.canTurnPiece = canTurn;
        rewardMulti = Save_Load_Manager.Instance.gameData.canTurnPiece ? 2 : 1;
        buttonPieceCanTurn.interactable = !Save_Load_Manager.Instance.gameData.canTurnPiece;
        buttonPieceCanDontTurn.interactable = Save_Load_Manager.Instance.gameData.canTurnPiece;
    }
    // Canvas -> Show-Puzzle-Setting -> Piece-Parent -> Button-Piece-Amount butonlarına eklendi
    public void SetPieceAmont(int pieceAmont)
    {
        Save_Load_Manager.Instance.gameData.pieceAmount = pieceAmont;
        textPuzzleReward.text = pieceAmont.ToString();

        for (int e = 0; e < puzzleAmountList.Count; e++)
        {
            puzzleAmountList[e].interactable = puzzleAmount[e] != Save_Load_Manager.Instance.gameData.pieceAmount;
        }
    }
    // Canvas -> Show-Puzzle-Setting -> Button-Start butonuna eklendi
    public void StartPuzzle()
    {
        if (isVideo)
        {
            // Reklam izle
            OpenPuzzle();
            return;
        }
        StartedPuzzle();
    }
    public void OpenPuzzle()
    {
        if (singleOrder == -1)
        {
            puzzleAllParent.GetChild(0).GetChild(1).GetChild(groupOrder).Find("Image-Video").gameObject.SetActive(false);
            Save_Load_Manager.Instance.gameData.puzzleDiary[groupOrder].isVideo = false;
        }
        else
        {
            Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].puzzleSingle[singleOrder].isVideo = false;

            bool finded = false;
            for (int e = 0; e < Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].puzzleSingle.Count && !finded; e++)
            {
                finded = Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].puzzleSingle[singleOrder].isVideo;
            }
            if (!finded)
            {
                objPuzzleGroupBuy.SetActive(false);
                Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].myNewPrice = 0;
                textPuzzleGroupPrice.text = "Free";
            }
            SetPuzzleGroupText();
        }
        StartedPuzzle();
    }
    private void StartedPuzzle()
    {
        objPuzzleGame.SetActive(true);
        objPuzzleGameButton.SetActive(true);
        objGroupList.SetActive(false);
        objStartPuzzle.SetActive(false);
        puzzleAllContainer.SetActive(false);

        if (singleOrder == -1)
        {
            Game_Manager.Instance.StartPuzzle(Save_Load_Manager.Instance.gameData.puzzleDiary[groupOrder].mySprite);
        }
        else
        {
            Game_Manager.Instance.StartPuzzle(Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].puzzleSingle[singleOrder].mySprite);
        }

    }
    public void PuzzleFinish()
    {
        objPuzzleFinish.SetActive(true);
        textFinishGoldAmount.text = "Gold : " + Save_Load_Manager.Instance.gameData.gold.ToString();
        textFinishIncreaseAmount.text = 25.ToString();
        textFinishTime.text = Game_Manager.Instance.GameTime;

        if (singleOrder == -1)
        {
            Save_Load_Manager.Instance.gameData.puzzleDiary[groupOrder].isFixed = true;
            puzzleAllParent.GetChild(0).GetChild(1).Find("Image-Video").gameObject.SetActive(false);
        }
        else
        {
            Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].puzzleSingle[singleOrder].isFixed = true;
        }

        IncreaseGold(Save_Load_Manager.Instance.gameData.pieceAmount * rewardMulti);

        // Group puzzle ise group partın price düşülsün ve amountu düşsün
        SetPuzzleGroupText();
    }
    private void SetPuzzleGroupText()
    {
        Transform puzzleGroup = puzzleAllParent.GetChild(groupOrder + 1).GetChild(1).GetChild(groupPartOrder);
        Puzzle_Group puzzle_Group = puzzleGroup.GetComponentInChildren<Puzzle_Group>(true);
        puzzle_Group.SetText();
    }
    // Canvas -> Puzzle-Game-Parent -> Menu-Parent -> Menu-Setting-Parent -> Button-Edges butonuna eklendi
    public void PuzzleSetFirstEdge()
    {
        for (int e = 0; e < edgePiece.Count; e++)
        {
            if (!edgePiece[e].IsEdge)
            {
                edgePiece[e].transform.parent.SetAsLastSibling();
            }
        }
    }
    public void IncreaseGold(int amount)
    {
        // 1 saniye bekle
        DOVirtual.DelayedCall(1.0f, () =>
        {
            int start = Save_Load_Manager.Instance.gameData.gold;
            int end = start + amount;

            DOTween.To(value => {
                // goldu arttır
                Save_Load_Manager.Instance.gameData.gold = (int)value;
                textFinishGoldAmount.text = "Gold : " + Save_Load_Manager.Instance.gameData.gold.ToString();
                textGoldAmount.text = Save_Load_Manager.Instance.gameData.gold.ToString();
            }, startValue: start, endValue: end, duration: 0.5f).SetEase(Ease.Linear);
        });
    }
    // Canvas -> Game-Finish -> Button-Gold-Video butonuna eklendi
    public void PuzzleFinishRewardIncrease()
    {
        IncreaseGold(25);
    }
    public void AddPuzzlePieceToHolder(Piece piece)
    {
        edgePiece.Add(piece);
        piece.SetPieceSizeForEdge(true);
        // yan menuden çıkarıp, game panele aktar
        piece.transform.parent.SetParent(pieceHolder);
    }
    public bool RemovePuzzlePieceFromHolder(Piece piece)
    {
        if (edgePiece.Contains(piece))
        {
            edgePiece.Remove(piece);
            // yan menuden çıkarıp, game panele aktar
            piece.transform.parent.SetParent(puzzleGameArea);
            return true;
        }
        return false;
    }
}