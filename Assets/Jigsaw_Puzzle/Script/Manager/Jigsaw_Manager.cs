using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum JigsawPieceType
{
    Corner, Edge, Middle
}
public class Board_Jigsaw
{
    public Piece_Jigsaw pieceJigsaw;
    public int spriteNumber;
    public Vector2Int myCoor;
    public JigsawPieceType pieceType;
    public Piece_Jigsaw pieceJigsawLeft;
    public Piece_Jigsaw pieceJigsawRight;
    public Piece_Jigsaw pieceJigsawTop;
    public Piece_Jigsaw pieceJigsawBottom;

    public Board_Jigsaw(Piece_Jigsaw pi, int e, int i)
    {
        pieceJigsaw = pi;
        myCoor = new Vector2Int(e, i);
    }
}
public class Jigsaw_Manager : Singletion<Jigsaw_Manager>
{
    [SerializeField] private Image imagePuzzleBackground;
    [SerializeField] private Transform pieceJigsaw;
    [SerializeField] private float waitingDelay;
    [SerializeField] private Puzzle_Pieces puzzlePieces;
    [SerializeField] private GridLayoutGroup gameAreaLayoutGroup;

    private Board_Jigsaw[,] board;
    private float closeDistance;
    private Vector2 myParentEdgeScale;
    private List<Piece_Jigsaw> allPieces = new List<Piece_Jigsaw>();

    public float CloseDistance { get { return closeDistance; } }
    public Vector2 MyParentEdgeScale { get { return myParentEdgeScale; } }

    public void CheckPuzzle()
    {
        bool isFinished = true;
        for (int e = 0; e < allPieces.Count && isFinished; e++)
        {
            isFinished = allPieces[e].IsStuck;
        }
        if (isFinished)
        {
            Game_Manager.Instance.SetGame(false);
            Canvas_Manager.Instance.PuzzleFinish();
            Audio_Manager.Instance.PlayPuzzleFixed();
        }
    }
    public void StartPuzzleJigsaw(Sprite puzzleSprite)
    {
        gameAreaLayoutGroup.enabled = true;
        // Amounta göre Satır ve Sütün sayısını öğren
        (int, int) row_Col = Game_Manager.Instance.LearnRowAndColumn();
        int x = row_Col.Item1; // Sütün
        int y = row_Col.Item2; // Satır
        board = new Board_Jigsaw[x, y];

        gameAreaLayoutGroup.constraintCount = x;
        RectTransform rect = gameAreaLayoutGroup.GetComponent<RectTransform>();
        float XSize = rect.rect.size.x / x;
        float YSize = rect.rect.size.y / y;

        Vector2 cellSize = new Vector2(XSize, YSize);
        gameAreaLayoutGroup.cellSize = cellSize;

        List<Transform> backs = new List<Transform>();
        List<Piece_Jigsaw> pieces = new List<Piece_Jigsaw>();
        allPieces.Clear();
        DOVirtual.DelayedCall(waitingDelay, () =>
        {
            // Eski pieceleri yok et.
            for (int i = gameAreaLayoutGroup.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(gameAreaLayoutGroup.transform.GetChild(i).gameObject);
            }
            // Yeni piece ler için bout belirle
            DOVirtual.DelayedCall(waitingDelay, () =>
            {
                // Puzzle parçasının orj genişliği 760 * 760 genişliğinde ama merkezi 500 * 500 genişliğinde
                // 500 * 500 lük kısım 172 * 180 olursa 760 * 760 kaç olur
                // 500 -> 172 olursa 760 kaç olur
                // 500 -> 180 olursa 760 kaç olur
                float xSide = cellSize.x * 1.52f; // cellSize.x * 760 / 500; -> 760 / 500 = 1.52f
                float ySide = cellSize.y * 1.52f;

                // xSide -> 90 olursa cellSize.x kaç olur
                // ySide -> 90 olursa cellSize.y kaç olur
                float xParentEdgeScale = cellSize.x * 90 / xSide;
                float yParentEdgeScale = cellSize.y * 90 / ySide;
                myParentEdgeScale = new Vector2(xParentEdgeScale, xParentEdgeScale);

                // xSide -> 1 ise xParentEdgeScale kaçtır
                // ySide -> 1 ise yParentEdgeScale kaçtır
                xParentEdgeScale = xParentEdgeScale / cellSize.x;
                yParentEdgeScale = yParentEdgeScale / cellSize.y;

                Vector2 myOrjSize = new Vector2(xSide, ySide);
                myParentEdgeScale = new Vector2(xParentEdgeScale, xParentEdgeScale);

                float xCloseDistance = cellSize.x * 0.26f; // cellSize.x * 130 / 500; -> 130 / 500 = 0.26f
                float yCloseDistance = cellSize.y * 0.26f;
                closeDistance = Mathf.Max(xCloseDistance * xCloseDistance, yCloseDistance * yCloseDistance);

                // Piece leri üret ve boarda yerleştir
                for (int e = 0; e < x; e++)
                {
                    for (int i = 0; i < y; i++)
                    {
                        Transform piece = Instantiate(pieceJigsaw, gameAreaLayoutGroup.transform);
                        piece.gameObject.SetActive(true);
                        piece.name = "Piece Jigsaw : " + e + " - " + i;
                        piece.GetComponent<RectTransform>().sizeDelta = cellSize;
                        piece.GetChild(0).GetComponent<RectTransform>().sizeDelta = myOrjSize;

                        // Piece üret, pos ayarla, viewi ekle, listeye ekle
                        Piece_Jigsaw pi = piece.GetChild(0).GetComponent<Piece_Jigsaw>();
                        allPieces.Add(pi);
                        pieces.Add(pi);

                        // Piece tipini ayarla
                        board[e, i] = new Board_Jigsaw(pi, e, i);

                        SetPiece(e, i, x, y);

                    }
                }
                DOVirtual.DelayedCall(waitingDelay, () =>
                {
                    imagePuzzleBackground.sprite = puzzleSprite;
                    // Piece lerin backgroundunu yerleştir
                    for (int e = 0; e < allPieces.Count; e++)
                    {
                        Transform backImage = Instantiate(imagePuzzleBackground, imagePuzzleBackground.transform.parent).transform;
                        backImage.gameObject.SetActive(true);
                        backs.Add(backImage);
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
                            // Komşulukları ayarla
                            for (int e = 0; e < x; e++)
                            {
                                for (int h = 0; h < y; h++)
                                {
                                    // Sol taraf
                                    if (e != 0)
                                    {
                                        board[e, h].pieceJigsawLeft = board[e - 1, h].pieceJigsaw;
                                    }
                                    // Alt taraf
                                    if (h != 0)
                                    {
                                        board[e, h].pieceJigsawBottom = board[e, h - 1].pieceJigsaw;
                                    }
                                    // Sağ taraf
                                    if (e != x - 1)
                                    {
                                        board[e, h].pieceJigsawRight = board[e + 1, h].pieceJigsaw;
                                    }
                                    // Üst taraf
                                    if (h != y - 1)
                                    {
                                        board[e, h].pieceJigsawTop = board[e, h + 1].pieceJigsaw;
                                    }
                                    board[e, h].pieceJigsaw.SetPos();
                                }
                            }
                            DOVirtual.DelayedCall(waitingDelay, () =>
                            {
                                gameAreaLayoutGroup.enabled = false;
                                DOVirtual.DelayedCall(waitingDelay, () =>
                                {
                                    // Resimleri karıştır ve holdera ekle
                                    while (pieces.Count > 0)
                                    {
                                        int rndPiece = Random.Range(0, pieces.Count);
                                        Canvas_Manager.Instance.AddJigsawPuzzlePieceToHolder(pieces[rndPiece]);
                                        if (Save_Load_Manager.Instance.gameData.canTurnPiece)
                                        {
                                            int rndAngle = Random.Range(0, 4);
                                            pieces[rndPiece].transform.Rotate(Vector3.back * 90 * rndAngle);
                                        }
                                        pieces.RemoveAt(rndPiece);
                                    }
                                    Audio_Manager.Instance.PlayPuzzlePieceMixed();
                                });
                            });
                        });
                    });
                });
            });
        });
    }
    private void SetPiece(int e, int h, int x, int y)
    {
        // Alt-Sol köşe
        Sprite view = null;
        bool isEdge = true;
        if (e == 0 && h == 0)
        {
            int rnd = Random.Range(0, puzzlePieces.cornerPieces_0.Count);
            board[e, h].spriteNumber = rnd;
            board[e, h].pieceType = JigsawPieceType.Corner;
            view = puzzlePieces.cornerPieces_0[rnd];
        }
        // Top-Sol köşe
        else if (e == 0 && h == y - 1)
        {
            view = SolTopCorner(e, h);
        }
        // Alt-Sağ köşe
        else if (e == x - 1 && h == 0)
        {
            view = SagAltCorner(e, h);
        }
        // Top-Sağ taraf
        else if (e == x - 1 && h == y - 1)
        {
            view = SagTopCorner(e, h);
        }
        // Sol taraf
        else if (e == 0)
        {
            view = SolEdge(e, h);
        }
        // Alt taraf
        else if (h == 0)
        {
            view = AltEdge(e, h);
        }
        // Sağ taraf
        else if (e == x - 1)
        {
            view = SagEdge(e, h);
        }
        // Üst taraf
        else if (h == y - 1)
        {
            view = TopEdge(e, h);
        }
        // Orta taraf
        else
        {
            view = Middle(e, h);
            isEdge = false;
        }
        board[e, h].pieceJigsaw.SetPiece(view, isEdge);
    }
    private Sprite Middle(int e, int h)
    {
        // Solu kontrol et
        bool isSolCukur = false;
        if (board[e - 1, h].pieceType == JigsawPieceType.Edge)
        {
            if (board[e - 1, h].spriteNumber == 0 || board[e - 1, h].spriteNumber == 1 || board[e - 1, h].spriteNumber == 4 || board[e - 1, h].spriteNumber == 5)
            {
                isSolCukur = true;
            }
            else if (board[e - 1, h].spriteNumber == 2 || board[e - 1, h].spriteNumber == 3 || board[e - 1, h].spriteNumber == 6 || board[e - 1, h].spriteNumber == 7)
            {
                isSolCukur = false;
            }
        }
        else if (board[e - 1, h].pieceType == JigsawPieceType.Middle)
        {
            if (board[e - 1, h].spriteNumber == 0 || board[e - 1, h].spriteNumber == 1 || board[e - 1, h].spriteNumber == 4 || board[e - 1, h].spriteNumber == 5 ||
                board[e - 1, h].spriteNumber == 8 || board[e - 1, h].spriteNumber == 9 || board[e - 1, h].spriteNumber == 12 || board[e - 1, h].spriteNumber == 13)
            {
                isSolCukur = true;
            }
            else if (board[e - 1, h].spriteNumber == 2 || board[e - 1, h].spriteNumber == 3 || board[e - 1, h].spriteNumber == 6 || board[e - 1, h].spriteNumber == 7 ||
                board[e - 1, h].spriteNumber == 10 || board[e - 1, h].spriteNumber == 11 || board[e - 1, h].spriteNumber == 14 || board[e - 1, h].spriteNumber == 15)
            {
                isSolCukur = false;
            }
        }

        // Asagiyi kontrol et
        bool isAsagiCukur = false;
        if (board[e, h - 1].pieceType == JigsawPieceType.Edge)
        {
            if (board[e, h - 1].spriteNumber == 0 || board[e, h - 1].spriteNumber == 1 || board[e, h - 1].spriteNumber == 4 || board[e, h - 1].spriteNumber == 5)
            {
                isAsagiCukur = true;
            }
            else if (board[e, h - 1].spriteNumber == 2 || board[e, h - 1].spriteNumber == 3 || board[e, h - 1].spriteNumber == 6 || board[e, h - 1].spriteNumber == 7)
            {
                isAsagiCukur = false;
            }
        }
        else if (board[e, h - 1].pieceType == JigsawPieceType.Middle)
        {
            if (board[e, h - 1].spriteNumber == 0 || board[e, h - 1].spriteNumber == 2 || board[e, h - 1].spriteNumber == 4 || board[e, h - 1].spriteNumber == 6 ||
                board[e, h - 1].spriteNumber == 8 || board[e, h - 1].spriteNumber == 10 || board[e, h - 1].spriteNumber == 12 || board[e, h - 1].spriteNumber == 14)
            {
                isAsagiCukur = true;
            }
            else if (board[e, h - 1].spriteNumber == 1 || board[e, h - 1].spriteNumber == 3 || board[e, h - 1].spriteNumber == 5 || board[e, h - 1].spriteNumber == 7 ||
                board[e, h - 1].spriteNumber == 9 || board[e, h - 1].spriteNumber == 11 || board[e, h - 1].spriteNumber == 13 || board[e, h - 1].spriteNumber == 15)
            {
                isAsagiCukur = false;
            }
        }
        List<int> randomSprite = new List<int>();
        if (isAsagiCukur && isSolCukur)
        {
            randomSprite.Add(12);
            randomSprite.Add(13);
            randomSprite.Add(14);
            randomSprite.Add(15);
        }
        else if (isAsagiCukur && !isSolCukur)
        {
            randomSprite.Add(4);
            randomSprite.Add(5);
            randomSprite.Add(6);
            randomSprite.Add(7);
        }
        else if (!isAsagiCukur && isSolCukur)
        {
            randomSprite.Add(8);
            randomSprite.Add(9);
            randomSprite.Add(10);
            randomSprite.Add(11);
        }
        else if (!isAsagiCukur && !isSolCukur)
        {
            randomSprite.Add(0);
            randomSprite.Add(1);
            randomSprite.Add(2);
            randomSprite.Add(3);
        }

        int rnd = Random.Range(0, randomSprite.Count);
        board[e, h].spriteNumber = randomSprite[rnd];
        board[e, h].pieceType = JigsawPieceType.Middle;
        return puzzlePieces.middlePieces[randomSprite[rnd]];
    }
    private Sprite SagAltCorner(int e, int h)
    {
        List<int> randomSprite = new List<int>();
        // Solu kontrol et
        if (board[e - 1, h].spriteNumber == 0 || board[e - 1, h].spriteNumber == 1 || board[e - 1, h].spriteNumber == 2 || board[e - 1, h].spriteNumber == 3)
        {
            randomSprite.Add(1);
            randomSprite.Add(3);
        }
        else if (board[e - 1, h].spriteNumber == 4 || board[e - 1, h].spriteNumber == 5 || board[e - 1, h].spriteNumber == 6 || board[e - 1, h].spriteNumber == 7)
        {
            randomSprite.Add(0);
            randomSprite.Add(2);
        }

        int rnd = Random.Range(0, randomSprite.Count);
        board[e, h].spriteNumber = randomSprite[rnd];
        board[e, h].pieceType = JigsawPieceType.Corner;
        return puzzlePieces.cornerPieces_90[randomSprite[rnd]];
    }
    private Sprite SagTopCorner(int e, int h)
    {
        // Solu kontrol et
        bool isSolCukur = false;
        if (board[e - 1, h].spriteNumber == 0 || board[e - 1, h].spriteNumber == 2 || board[e - 1, h].spriteNumber == 4 || board[e - 1, h].spriteNumber == 6)
        {
            isSolCukur = true;
        }
        else if (board[e - 1, h].spriteNumber == 1 || board[e - 1, h].spriteNumber == 3 || board[e - 1, h].spriteNumber == 5 || board[e - 1, h].spriteNumber == 7)
        {
            isSolCukur = false;
        }

        // Asagiyi kontrol et
        bool isAsagiCukur = false;
        if (board[e, h - 1].spriteNumber == 0 || board[e, h - 1].spriteNumber == 1 || board[e, h - 1].spriteNumber == 2 || board[e, h - 1].spriteNumber == 3)
        {
            isAsagiCukur = true;
        }
        else if (board[e, h - 1].spriteNumber == 4 || board[e, h - 1].spriteNumber == 5 || board[e, h - 1].spriteNumber == 6 || board[e, h - 1].spriteNumber == 7)
        {
            isAsagiCukur = false;
        }

        List<int> randomSprite = new List<int>();
        if (isAsagiCukur && isSolCukur)
        {
            randomSprite.Add(3);
        }
        else if (isAsagiCukur && !isSolCukur)
        {
            randomSprite.Add(1);
        }
        else if (!isAsagiCukur && isSolCukur)
        {
            randomSprite.Add(2);
        }
        else if (!isAsagiCukur && !isSolCukur)
        {
            randomSprite.Add(0);
        }

        int rnd = Random.Range(0, randomSprite.Count);
        board[e, h].spriteNumber = randomSprite[rnd];
        board[e, h].pieceType = JigsawPieceType.Corner;
        return puzzlePieces.cornerPieces_180[randomSprite[rnd]];
    }
    private Sprite SolTopCorner(int e, int h)
    {
        List<int> randomSprite = new List<int>();
        // Asagiyi kontrol et
        if (board[e, h - 1].spriteNumber == 0 || board[e, h - 1].spriteNumber == 2 ||
            board[e, h - 1].spriteNumber == 4 || board[e, h - 1].spriteNumber == 6)
        {
            randomSprite.Add(2);
            randomSprite.Add(3);
        }
        else if (board[e, h - 1].spriteNumber == 1 || board[e, h - 1].spriteNumber == 3 ||
            board[e, h - 1].spriteNumber == 5 || board[e, h - 1].spriteNumber == 7)
        {
            randomSprite.Add(0);
            randomSprite.Add(1);
        }

        int rnd = Random.Range(0, randomSprite.Count);
        board[e, h].spriteNumber = randomSprite[rnd];
        board[e, h].pieceType = JigsawPieceType.Corner;
        return puzzlePieces.cornerPieces_270[randomSprite[rnd]];
    }
    private Sprite SolEdge(int e, int h)
    {
        // Asagiyi kontrol et
        List<int> randomSprite = new List<int>();
        if (board[e, h - 1].spriteNumber == 0 || board[e, h - 1].spriteNumber == 2 || board[e, h - 1].spriteNumber == 4 || board[e, h - 1].spriteNumber == 6)
        {
            randomSprite.Add(4);
            randomSprite.Add(5);
            randomSprite.Add(6);
            randomSprite.Add(7);
        }
        else if (board[e, h - 1].spriteNumber == 1 || board[e, h - 1].spriteNumber == 3 || board[e, h - 1].spriteNumber == 5 || board[e, h - 1].spriteNumber == 7)
        {
            randomSprite.Add(0);
            randomSprite.Add(1);
            randomSprite.Add(2);
            randomSprite.Add(3);
        }

        int rnd = Random.Range(0, randomSprite.Count);
        board[e, h].spriteNumber = randomSprite[rnd];
        board[e, h].pieceType = JigsawPieceType.Edge;
        return puzzlePieces.edgePieces_0[randomSprite[rnd]];
    }
    private Sprite AltEdge(int e, int h)
    {
        List<int> randomSprite = new List<int>();

        // Solu kontrol et
        if (board[e - 1, h].pieceType == JigsawPieceType.Corner)
        {
            if (board[e - 1, h].spriteNumber == 0 || board[e - 1, h].spriteNumber == 1)
            {
                randomSprite.Add(1);
                randomSprite.Add(3);
                randomSprite.Add(5);
                randomSprite.Add(7);
            }
            else if (board[e - 1, h].spriteNumber == 2 || board[e - 1, h].spriteNumber == 3)
            {
                randomSprite.Add(0);
                randomSprite.Add(2);
                randomSprite.Add(4);
                randomSprite.Add(6);
            }
        }
        else if (board[e - 1, h].pieceType == JigsawPieceType.Edge)
        {
            if (board[e - 1, h].spriteNumber == 0 || board[e - 1, h].spriteNumber == 1 || board[e - 1, h].spriteNumber == 2 || board[e - 1, h].spriteNumber == 3)
            {
                randomSprite.Add(1);
                randomSprite.Add(3);
                randomSprite.Add(5);
                randomSprite.Add(7);
            }
            else if (board[e - 1, h].spriteNumber == 4 || board[e - 1, h].spriteNumber == 5 || board[e - 1, h].spriteNumber == 6 || board[e - 1, h].spriteNumber == 7)
            {
                randomSprite.Add(0);
                randomSprite.Add(2);
                randomSprite.Add(4);
                randomSprite.Add(6);
            }
        }

        int rnd = Random.Range(0, randomSprite.Count);
        board[e, h].spriteNumber = randomSprite[rnd];
        board[e, h].pieceType = JigsawPieceType.Edge;
        return puzzlePieces.edgePieces_90[randomSprite[rnd]];
    }
    private Sprite SagEdge(int e, int h)
    {
        List<int> randomSprite = new List<int>();
        bool isSolCukur = false;
        // Solu kontrol et
        if (board[e - 1, h].spriteNumber == 0 || board[e - 1, h].spriteNumber == 1 || board[e - 1, h].spriteNumber == 4 || board[e - 1, h].spriteNumber == 5 ||
            board[e - 1, h].spriteNumber == 8 || board[e - 1, h].spriteNumber == 9 || board[e - 1, h].spriteNumber == 12 || board[e - 1, h].spriteNumber == 13)
        {
            isSolCukur = true;
        }
        else if (board[e - 1, h].spriteNumber == 2 || board[e - 1, h].spriteNumber == 3 || board[e - 1, h].spriteNumber == 6 || board[e - 1, h].spriteNumber == 7 ||
            board[e - 1, h].spriteNumber == 10 || board[e - 1, h].spriteNumber == 11 || board[e - 1, h].spriteNumber == 14 || board[e - 1, h].spriteNumber == 15)
        {
            isSolCukur = false;
        }

        // Asagiyi kontrol et
        bool isAsagiCukur = false;
        if (board[e, h - 1].pieceType == JigsawPieceType.Corner)
        {
            if (board[e, h - 1].spriteNumber == 0 || board[e, h - 1].spriteNumber == 1)
            {
                isAsagiCukur = true;
            }
            else if (board[e, h - 1].spriteNumber == 2 || board[e, h - 1].spriteNumber == 3)
            {
                isAsagiCukur = false;
            }
        }
        else if (board[e, h - 1].pieceType == JigsawPieceType.Edge)
        {
            if (board[e, h - 1].spriteNumber == 0 || board[e, h - 1].spriteNumber == 1 || board[e, h - 1].spriteNumber == 2 || board[e, h - 1].spriteNumber == 3)
            {
                isAsagiCukur = true;
            }
            else if (board[e, h - 1].spriteNumber == 4 || board[e, h - 1].spriteNumber == 5 || board[e, h - 1].spriteNumber == 6 || board[e, h - 1].spriteNumber == 7)
            {
                isAsagiCukur = false;
            }
        }
        if (isAsagiCukur && isSolCukur)
        {
            randomSprite.Add(3);
            randomSprite.Add(7);
        }
        else if (isAsagiCukur && !isSolCukur)
        {
            randomSprite.Add(1);
            randomSprite.Add(5);
        }
        else if (!isAsagiCukur && isSolCukur)
        {
            randomSprite.Add(2);
            randomSprite.Add(6);
        }
        else if (!isAsagiCukur && !isSolCukur)
        {
            randomSprite.Add(0);
            randomSprite.Add(4);
        }

        int rnd = Random.Range(0, randomSprite.Count);
        board[e, h].spriteNumber = randomSprite[rnd];
        board[e, h].pieceType = JigsawPieceType.Edge;
        return puzzlePieces.edgePieces_180[randomSprite[rnd]];
    }
    private Sprite TopEdge(int e, int h)
    {
        List<int> randomSprite = new List<int>();
        // Solu kontrol et
        bool isSolCukur = false;
        if (board[e - 1, h].pieceType == JigsawPieceType.Corner)
        {
            if (board[e - 1, h].spriteNumber == 0 || board[e - 1, h].spriteNumber == 2)
            {
                isSolCukur = true;
            }
            else if (board[e - 1, h].spriteNumber == 1 || board[e - 1, h].spriteNumber == 3)
            {
                isSolCukur = false;
            }
        }
        else if (board[e - 1, h].pieceType == JigsawPieceType.Edge)
        {
            if (board[e - 1, h].spriteNumber == 0 || board[e - 1, h].spriteNumber == 2 || board[e - 1, h].spriteNumber == 4 || board[e - 1, h].spriteNumber == 6)
            {
                isSolCukur = true;
            }
            else if (board[e - 1, h].spriteNumber == 1 || board[e - 1, h].spriteNumber == 3 || board[e - 1, h].spriteNumber == 5 || board[e - 1, h].spriteNumber == 7)
            {
                isSolCukur = false;
            }
        }
        // Asagiyi kontrol et
        bool isAsagiCukur = false;
        if (board[e, h - 1].spriteNumber == 0 || board[e, h - 1].spriteNumber == 2 || board[e, h - 1].spriteNumber == 4 || board[e, h - 1].spriteNumber == 6 ||
            board[e, h - 1].spriteNumber == 8 || board[e, h - 1].spriteNumber == 10 || board[e, h - 1].spriteNumber == 12 || board[e, h - 1].spriteNumber == 14)
        {
            isAsagiCukur = true;
        }
        else if (board[e, h - 1].spriteNumber == 1 || board[e, h - 1].spriteNumber == 3 || board[e, h - 1].spriteNumber == 5 || board[e, h - 1].spriteNumber == 7 ||
            board[e, h - 1].spriteNumber == 9 || board[e, h - 1].spriteNumber == 11 || board[e, h - 1].spriteNumber == 13 || board[e, h - 1].spriteNumber == 15)
        {
            isAsagiCukur = false;
        }
        if (isAsagiCukur && isSolCukur)
        {
            randomSprite.Add(6);
            randomSprite.Add(7);
        }
        else if (isAsagiCukur && !isSolCukur)
        {
            randomSprite.Add(2);
            randomSprite.Add(3);
        }
        else if (!isAsagiCukur && isSolCukur)
        {
            randomSprite.Add(4);
            randomSprite.Add(5);
        }
        else if (!isAsagiCukur && !isSolCukur)
        {
            randomSprite.Add(0);
            randomSprite.Add(1);
        }

        int rnd = Random.Range(0, randomSprite.Count);
        board[e, h].spriteNumber = randomSprite[rnd];
        board[e, h].pieceType = JigsawPieceType.Edge;
        return puzzlePieces.edgePieces_270[randomSprite[rnd]];
    }
}