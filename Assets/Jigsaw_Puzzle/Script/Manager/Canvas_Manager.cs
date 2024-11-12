using TMPro;
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Canvas_Manager : Singletion<Canvas_Manager>
{
    [Header("Puzzle Genel")]
    [SerializeField] private Transform puzzleAllParent;
    [SerializeField] private GameObject puzzleAllContainer;
    [SerializeField] private Transform prefabPuzzleContainer;
    [SerializeField] private Transform sceneMaskImage;
    [SerializeField] private TextMeshProUGUI textGoldAmount;

    [Header("Puzzle Diary")]
    [SerializeField] private Puzzle_Daily prefabPuzzleDaily;

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
    [SerializeField] private RawImage imagePuzzleIcon;
    [SerializeField] private TextMeshProUGUI textPuzzleReward;
    [SerializeField] private Button buttonPieceCanTurn;
    [SerializeField] private Button buttonPieceCanDontTurn;
    [SerializeField] private List<Button> puzzleAmountList = new List<Button>();

    [Header("Puzzle Game")]
    [SerializeField] private GameObject objPuzzleGame;
    [SerializeField] private RawImage puzzleSpriteIcon;
    [SerializeField] private Transform puzzleBackgroundParent;
    [SerializeField] private Puzzle_Background prefabPuzzleBackground;
    [SerializeField] private TextMeshProUGUI textFinishGoldAmount;
    [SerializeField] private TextMeshProUGUI textFinishIncreaseAmount;
    [SerializeField] private TextMeshProUGUI textFinishTime;

    [Header("Puzzle Game Button")]
    [SerializeField] private GameObject objPuzzleGameButton;
    [SerializeField] private RawImage imageGameBackground;
    [SerializeField] private RawImage imageGameBackgroundHelper;
    [SerializeField] private RawImage imageGameMiniIcon;
    [SerializeField] private Transform pieceHolder;

    [Header("Puzzle Finish")]
    [SerializeField] private GameObject objPuzzleFinish;


    private bool isVideo;
    private int rewardMulti = 1;
    private int groupOrder;
    private int singleOrder;
    private int groupPartOrder;
    private int goldChangedAmount;
    private int goldChangedStartedAmount;
    private List<int> puzzleAmount = new List<int>();
    private List<Piece> edgePiece = new List<Piece>();
    private List<Puzzle_Single> puzzleGroupList = new List<Puzzle_Single>();

    #region Genel
    private void Start()
    {
        CloseMask(1, () => {
            // Puzzle Piece amount butonlarını belirle
            for (int e = 0; e < puzzleAmountList.Count; e++)
            {
                if (int.TryParse(puzzleAmountList[e].GetComponentInChildren<TextMeshProUGUI>().text, out int amount))
                {
                    puzzleAmount.Add(amount);
                    puzzleAmountList[e].onClick.AddListener(() => SetPieceAmont(amount));
                }
            }

            // Puzzle Game Piece'lerinin Turn ayarını belirle
            SetTurnPiece();

            // Puzzle Game Piece'lerinin Amount ayarını belirle
            SetPieceAmont(Save_Load_Manager.Instance.gameData.pieceAmount);

            // Canvas'da Gold ayarını belirle
            textGoldAmount.text = Save_Load_Manager.Instance.gameData.gold.ToString();
        });
    }
    private void CloseMask(float waitingTime, Action action)
    {
        // Bekle
        DOVirtual.DelayedCall(waitingTime,
            () =>
            {
                // Close Mask
                DOTween.To(value => { sceneMaskImage.localScale = Vector3.one * value; }, startValue: 2, endValue: 0, duration: 1.5f)
                    .SetEase(Ease.Linear).OnComplete(() =>
                    {
                        action?.Invoke();
                    });
            });
    }
    private void OpenMask(float waitingTime, Action action)
    {
        // Bekle
        DOVirtual.DelayedCall(waitingTime,
            () =>
            {
                action?.Invoke();
                // Open Mask
                DOTween.To(value => { sceneMaskImage.localScale = Vector3.one * value; }, startValue: 0, endValue: 2, duration: 1.5f)
                .SetEase(Ease.Linear);
            });
    }
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    // Canvas -> AreWeOnline -> Button-Reload-Scene butonuna atandı.
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    #endregion

    public Puzzle_Background SetBackgroundButton(int order)
    {
        Puzzle_Background back = Instantiate(prefabPuzzleBackground, puzzleBackgroundParent);
        return back;
    }
    public bool BuyBackground(int order)
    {
        if (Save_Load_Manager.Instance.gameData.gold >= Save_Load_Manager.Instance.gameData.puzzleBackground[order].isPrice)
        {
            Save_Load_Manager.Instance.gameData.gold -= Save_Load_Manager.Instance.gameData.puzzleBackground[order].isPrice;
            Save_Load_Manager.Instance.gameData.puzzleBackground[order].isOpen = true;
            SetBackground(order);
            Save_Load_Manager.Instance.SaveGame();
            return true;
        }
        else
        {
            Warning_Manager.Instance.ShowMessage("You dont have enough Gold.", 2);
            return false;
        }
    }
    public void SetBackground(int order)
    {
        if (Save_Load_Manager.Instance.gameData.puzzleBackground[order].myTexture == null)
        {
            float oran = 105.0f/ 255;
            imageGameBackground.color = new Color(oran, oran, oran);
        }
        else
        {
            imageGameBackground.color = new Color(1, 1, 1);
            imageGameBackground.texture = Save_Load_Manager.Instance.gameData.puzzleBackground[order].myTexture;
        }
        Save_Load_Manager.Instance.gameData.backgroundOrder = order;
        puzzleBackgroundParent.parent.gameObject.SetActive(false);
    }
    public Puzzle_Daily CreatePuzzleDiarySprite()
    {
        Puzzle_Daily puzzle = Instantiate(prefabPuzzleDaily, puzzleAllParent.GetChild(0).GetChild(1));
        return puzzle;
    }
    public Transform CreatePuzzleGroup()
    {
        // Tüm grpları oluştur
        Transform puzzleContainer = Instantiate(prefabPuzzleContainer, puzzleAllParent);
        return puzzleContainer;
    }
    public Puzzle_Group CreatePuzzleGroupPart(Transform puzzleParent)
    {
        // Tüm grpları oluştur
        Puzzle_Group puzzle = Instantiate(prefabPuzzle_Group, puzzleParent.GetChild(1));
        return puzzle;
    }
    public void ShowAllSinglePuzzle(PuzzleGroupPart puzzleGroupPart, string puzzleFix, int order, int partOrder)
    {
        objGroupList.SetActive(true);
        puzzleAllContainer.SetActive(false);

        // Resim parentindeki tüm resim butonlarını kapat
        for (int e = 0; e < puzzleGroupList.Count; e++)
        {
            puzzleGroupList[e].gameObject.SetActive(true);
        }
        // Grupda objelerin konulduğu parenttan daha fazla resim varsa bunlar için resim butonu oluştur ve listeye ekle
        if (puzzleGroupPart.puzzleSingle.Count > puzzleGroupListParent.childCount)
        {
            int newPuzzleAmount = puzzleGroupPart.puzzleSingle.Count - puzzleGroupListParent.childCount;
            for (int i = 0; i < newPuzzleAmount; i++)
            {
                Puzzle_Single puzzle = Instantiate(prefabPuzzleSingle, puzzleGroupListParent);
                puzzleGroupList.Add(puzzle);
            }
        }
        // Resim butonlarını aktif et ve setle
        for (int i = 0; i < puzzleGroupPart.puzzleSingle.Count; i++)
        {
            puzzleGroupList[i].gameObject.SetActive(true);
            puzzleGroupList[i].SetPuzzleSingle(puzzleGroupPart.puzzleSingle[i], order, partOrder, i);
        }
        groupOrder = order;
        groupPartOrder = partOrder;

        textPuzzleGroupName.text = puzzleGroupPart.groupPartName;

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
        if (Save_Load_Manager.Instance.gameData.gold < Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].myNewPrice)
        {
            Warning_Manager.Instance.ShowMessage("You dont have enough Gold.", 2);
            return;
        }
        Save_Load_Manager.Instance.gameData.gold -= Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].myNewPrice;
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
        Save_Load_Manager.Instance.SaveGame();
    }
    public void ShowPuzzleSetting(Texture2D myTexture2D, bool isVideo, int order = -1, int partOrder = -1, int single = -1)
    {
        Audio_Manager.Instance.PlayPuzzleStartPanelOpened();
        objStartPuzzle.SetActive(true);

        objIsVideo.SetActive(isVideo);
        imagePuzzleIcon.texture = myTexture2D;
        imageGameMiniIcon.texture = myTexture2D;
        imageGameBackgroundHelper.texture = myTexture2D;
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
        SetRewardText();
    }
    public void SetTurnPiece()
    {
        rewardMulti = Save_Load_Manager.Instance.gameData.canTurnPiece ? 2 : 1;
        buttonPieceCanTurn.interactable = !Save_Load_Manager.Instance.gameData.canTurnPiece;
        buttonPieceCanDontTurn.interactable = Save_Load_Manager.Instance.gameData.canTurnPiece;
        SetRewardText();
    }
    // Canvas -> Show-Puzzle-Setting -> Piece-Parent -> Button-Piece-Amount butonlarına eklendi
    private void SetPieceAmont(int pieceAmont)
    {
        Save_Load_Manager.Instance.gameData.pieceAmount = pieceAmont;
        SetRewardText();

        for (int e = 0; e < puzzleAmountList.Count; e++)
        {
            puzzleAmountList[e].interactable = puzzleAmount[e] != Save_Load_Manager.Instance.gameData.pieceAmount;
        }
    }
    private void SetRewardText()
    {
        int reward = rewardMulti * Save_Load_Manager.Instance.gameData.pieceAmount;
        textPuzzleReward.text = reward.ToString();
    }
    // Canvas -> Show-Puzzle-Setting -> Button-Start butonuna eklendi
    public void StartPuzzle()
    {
        if (isVideo)
        {
            // Reklam izle ve OpenPuzzle fonksiyonunu çalıştır. Reklam sistemi olmadığı için biz direk çalıştırıyoruz.
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
                finded = Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].puzzleSingle[e].isVideo;
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
        Audio_Manager.Instance.GameSound();
        objPuzzleGame.SetActive(true);
        objPuzzleGameButton.SetActive(true);
        objGroupList.SetActive(false);
        objStartPuzzle.SetActive(false);
        puzzleAllContainer.SetActive(false);
        SetBackground(Save_Load_Manager.Instance.gameData.backgroundOrder);

        Texture2D texture = null;
        if (singleOrder == -1)
        {
            texture = Save_Load_Manager.Instance.gameData.puzzleDiary[groupOrder].myTexture;
        }
        else
        {
            texture = Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].puzzleSingle[singleOrder].myTexture;
        }
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        Game_Manager.Instance.StartPuzzle(sprite);
    }
    public void PuzzleFinish()
    {
        objPuzzleFinish.SetActive(true);
        textFinishGoldAmount.text = "Gold : " + Save_Load_Manager.Instance.gameData.gold.ToString();
        textFinishIncreaseAmount.text = "Reward : " + (Save_Load_Manager.Instance.gameData.pieceAmount * rewardMulti);
        textFinishTime.text = Game_Manager.Instance.GameTime;

        if (singleOrder == -1)
        {
            Save_Load_Manager.Instance.gameData.puzzleDiary[groupOrder].isFixed = true;
            puzzleSpriteIcon.texture = Save_Load_Manager.Instance.gameData.puzzleDiary[groupOrder].myTexture;
            bool finded = false;
            Transform diaryParent = puzzleAllParent.GetChild(0).GetChild(1);
            for (int e = 0; e < diaryParent.childCount && !finded; e++)
            {
                if (diaryParent.GetChild(e).TryGetComponent(out Puzzle_Daily daily))
                {
                    if (daily.CheckDiaryOrder(groupOrder))
                    {
                        finded = true;
                        daily.SetFixed();
                    }
                }
            }
        }
        else
        {
            puzzleSpriteIcon.texture = Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].puzzleSingle[singleOrder].myTexture;
            Save_Load_Manager.Instance.gameData.puzzleGroup[groupOrder].puzzleGroupList[groupPartOrder].puzzleSingle[singleOrder].isFixed = true;
            // Group puzzle ise group partın price düşülsün ve amountu düşsün
            SetPuzzleGroupText();
        }
        IncreaseGold(Save_Load_Manager.Instance.gameData.pieceAmount * rewardMulti);
    }
    private void SetPuzzleGroupText()
    {
        Transform puzzleGroup = puzzleAllParent.GetChild(groupOrder + 1).GetChild(1).GetChild(groupPartOrder);
        Puzzle_Group puzzle_Group = puzzleGroup.GetComponentInChildren<Puzzle_Group>(true);
        puzzle_Group.SetAmountText();
        puzzle_Group.SetPriceText();
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
        Audio_Manager.Instance.PlayGoldChance();
        if (goldChangedAmount == 0)
        {
            goldChangedStartedAmount = Save_Load_Manager.Instance.gameData.gold;
        }
        goldChangedAmount += amount;
        Save_Load_Manager.Instance.gameData.gold += amount;
        DOTween.To(value => {
            // goldu arttır
            textGoldAmount.text = (goldChangedStartedAmount + (int)value).ToString();
        }, startValue: 0, endValue: goldChangedAmount, duration: 1.5f).SetEase(Ease.Linear).OnComplete(() =>
        {
            goldChangedAmount = 0;
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
            piece.transform.parent.SetParent(imageGameBackground.transform);
            return true;
        }
        return false;
    }
}