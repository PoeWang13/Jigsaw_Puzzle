using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Board_Slide
{
    public Piece_Slide myPiece;
    public Vector2Int myCoor;

    public Board_Slide(Piece_Slide myPieceSlide, int e, int i)
    {
        myCoor = new Vector2Int(e, i);
        myPiece = myPieceSlide;
    }
}
public class Slide_Manager : Singletion<Slide_Manager>
{
    [SerializeField] private float waitingDelay;
    [SerializeField] private Transform pieceSlide;
    [SerializeField] private RawImage imagePuzzleBackground;
    [SerializeField] private GridLayoutGroup gameAreaLayoutGroup;

    private bool canClick = true;
    private Board_Slide[,] board;
    private Piece_Slide piSlide = null;
    private Vector2Int piecePos = Vector2Int.zero;
    private Vector2Int pieceNewCoor = Vector2Int.zero;
    private Vector2 pieceAnchoredCoor = Vector2.zero;
    private Vector2 pieceAnchoredNewPos = Vector2.zero;
    private List<Piece_Slide> allPieces = new List<Piece_Slide>();

    public void CheckPuzzle()
    {
        canClick = true;
        bool isFinished = true;
        for (int e = 0; e < allPieces.Count && isFinished; e++)
        {
            isFinished = allPieces[e].IsStuck;
        }
        if (isFinished)
        {
            canClick = false;
            Game_Manager.Instance.SetGame(false);
            Canvas_Manager.Instance.PuzzleFinish();
            Audio_Manager.Instance.PlayPuzzleFixed();
        }
    }
    public void StartPuzzleSlide(Texture2D puzzleTexture)
    {
        (int, int) row_Col = Game_Manager.Instance.LearnRowAndColumn();
        int x = row_Col.Item1; // Sütün
        int y = row_Col.Item2; // Satır
        board = new Board_Slide[x, y];

        gameAreaLayoutGroup.constraintCount = x;
        RectTransform rect = gameAreaLayoutGroup.GetComponent<RectTransform>();
        int XSize = Mathf.RoundToInt(rect.rect.size.x / x);
        int YSize = Mathf.RoundToInt(rect.rect.size.y / y);

        Vector2 cellSize = new Vector2(XSize, YSize);
        gameAreaLayoutGroup.cellSize = cellSize;

        List<Transform> backs = new List<Transform>();
        allPieces.Clear();
        DOVirtual.DelayedCall(waitingDelay, () =>
        {
            // Eski pieceleri yok et.
            for (int i = gameAreaLayoutGroup.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(gameAreaLayoutGroup.transform.GetChild(i).gameObject);
            }
            // Boş parçayı belirle
            int rndSide = Random.Range(0, 4);
            int xSide = 0;
            int ySide = 0;
            if (rndSide == 0)
            {
                // Alt taraf
                ySide = 0;
                xSide = Random.Range(0, x);
            }
            else if (rndSide == 1)
            {
                // Sol taraf
                xSide = 0;
                ySide = Random.Range(0, y);
            }
            else if(rndSide == 2)
            {
                // Üst taraf
                ySide = y - 1;
                xSide = Random.Range(0, x);
            }
            else if(rndSide == 3)
            {
                // Sağ taraf
                xSide = x - 1;
                ySide = Random.Range(0, y);
            }
            DOVirtual.DelayedCall(waitingDelay, () =>
            {
                imagePuzzleBackground.texture = puzzleTexture;
                // Piece leri üret ve boarda yerleştir
                for (int e = 0; e < x; e++)
                {
                    for (int i = 0; i < y; i++)
                    {
                        Transform backImage = Instantiate(imagePuzzleBackground, imagePuzzleBackground.transform.parent).transform;
                        backImage.gameObject.SetActive(true);
                        backs.Add(backImage);

                        Transform piece = Instantiate(pieceSlide, gameAreaLayoutGroup.transform);
                        piece.gameObject.SetActive(true);
                        piece.name = "Piece Slide : " + e + " - " + i;
                        piece.GetComponent<RectTransform>().sizeDelta = cellSize;

                        // Piece üret, pos ayarla, viewi ekle, listeye ekle
                        Piece_Slide pi = piece.GetComponent<Piece_Slide>();
                        pi.SetCoor(new Vector2Int(e, i));
                        allPieces.Add(pi);

                        // Piece tipini ayarla
                        board[e, i] = new Board_Slide(pi, e, i);

                        if (e == xSide && i == ySide)
                        {
                            piSlide = pi;
                            pi.GetComponent<RawImage>().raycastTarget = false;
                        }
                    }
                }
                DOVirtual.DelayedCall(waitingDelay, () =>
                {
                    // Backgroundları sahiplerine chield olarak ekle
                    for (int e = 0; e < backs.Count; e++)
                    {
                        backs[e].SetParent(allPieces[e].transform);
                    }
                    DOVirtual.DelayedCall(waitingDelay, () =>
                    {
                        gameAreaLayoutGroup.enabled = false;
                        allPieces.Remove(piSlide);
                        piSlide.gameObject.SetActive(false);
                        // Boş parçayı 5000 kere yer değiştir.
                        int amount = 5000;
                        while (amount != 0)
                        {
                            pieceAnchoredCoor = Vector2.zero;
                            pieceAnchoredNewPos = Vector2.zero;
                            piecePos = Vector2Int.zero;
                            pieceNewCoor = Vector2Int.zero;
                            rndSide = Random.Range(0, 4);
                            if (rndSide == 0)
                            {
                                // Alt taraf null mu
                                if (ySide != 0)
                                {
                                    amount--;
                                    SetPiece(xSide, ySide - 1);
                                    ySide = ySide - 1;
                                }
                            }
                            else if (rndSide == 1)
                            {
                                // Sol taraf null mu
                                if (xSide != 0)
                                {
                                    amount--;
                                    SetPiece(xSide - 1, ySide);
                                    xSide = xSide - 1;
                                }
                            }
                            else if (rndSide == 2)
                            {
                                // Üst taraf null mu
                                if (ySide + 1 != y)
                                {
                                    amount--;
                                    SetPiece(xSide, ySide + 1);
                                    ySide = ySide + 1;
                                }
                            }
                            else if (rndSide == 3)
                            {
                                // Sağ taraf null mu
                                if (xSide + 1 != x)
                                {
                                    amount--;
                                    SetPiece(xSide + 1, ySide);
                                    xSide = xSide + 1;
                                }
                            }
                        }
                        Audio_Manager.Instance.PlayPuzzlePieceMixed();
                    });
                });
            });
        });
    }
    public void ClickPiece(Piece_Slide pieceSlide)
    {
        if (canClick)
        {
            if ((piSlide.MyCoor - pieceSlide.MyCoor).sqrMagnitude == 1)
            {
                canClick = false;
                // Parçaların koordinatlarını öğren
                pieceAnchoredCoor = piSlide.MyRect.anchoredPosition;
                pieceAnchoredNewPos = pieceSlide.MyRect.anchoredPosition;

                // Parçaların coordinatlarını öğren
                piecePos = piSlide.MyCoor;
                pieceNewCoor = pieceSlide.MyCoor;

                piSlide.SlidePiece(pieceAnchoredNewPos, pieceNewCoor);
                pieceSlide.SlidePiece(pieceAnchoredCoor, piecePos);
            }
        }
    }
    private void SetPiece(int xSide, int ySide)
    {
        // Parçaların koordinatlarını öğren
        pieceAnchoredCoor = piSlide.MyRect.anchoredPosition;
        pieceAnchoredNewPos = board[xSide, ySide].myPiece.MyRect.anchoredPosition;

        // Parçaların coordinatlarını öğren
        piecePos = piSlide.MyCoor;
        pieceNewCoor = board[xSide, ySide].myPiece.MyCoor;

        // Parçaları yeni koordinatlara gönder
        piSlide.MyRect.anchoredPosition = pieceAnchoredNewPos;
        board[xSide, ySide].myPiece.MyRect.anchoredPosition = pieceAnchoredCoor;

        // Parçaların coordinatlarını setle
        piSlide.SetNewCoor(pieceNewCoor);
        board[xSide, ySide].myPiece.SetNewCoor(piecePos);
    }
}