using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Net;

public enum PieceType
{
    Corner, Edge, Middle
}
public class Board
{
    public Piece piece;
    public int spriteNumber;
    public Vector2Int myCoor;
    public PieceType pieceType;
    public Piece pieceLeft;
    public Piece pieceRight;
    public Piece pieceTop;
    public Piece pieceBottom;

    public Board(Piece pi, int e, int i)
    {
        piece = pi;
        myCoor = new Vector2Int(e, i);
    }
}
public class Game_Manager : Singletion<Game_Manager>
{
    [ContextMenu("Puzzle Finish")]
    public void PuzzleFinish()
    {
        Canvas_Manager.Instance.PuzzleFinish();
    }
    [SerializeField] private Puzzle_Pieces puzzlePieces;
    [SerializeField] private GridLayoutGroup layoutGroup;
    [SerializeField] private Image imagePuzzle;
    [SerializeField] private float waitingDelay;
    [SerializeField] private GameObject onlineController;

    private float closeDistance;
    private Vector2 myParentEdgeScale;

    private Canvas toolTipCanvas;
    private Vector3 newPos;
    private RectTransform movingPiece;
    private int second;
    private int minute;
    private int hour;
    private float gameTimeNext;
    private bool gameStart;
    private bool areWeOnline;
    private Board[,] board;
    private List<Piece> allPieces = new List<Piece>();
    public bool AreWeOnline { get { return areWeOnline; } }
    public string GameTime { get { return hour + " : " + minute + " : " + second; } }
    public float CloseDistance { get { return closeDistance; } }
    public Vector2 MyParentEdgeScale { get { return myParentEdgeScale; } }

    public override void OnAwake()
    {
        toolTipCanvas = GetComponentInChildren<Canvas>();
        DayTime();
    }
    [ContextMenu("Day Time")]
    private void DayTime()
    {
        // Yerel saati veriyor.
        try
        {
            using (var response = WebRequest.Create("http://www.google.com").GetResponse())
            {
                SetOnlineController(true);
            }
        }
        catch (WebException)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    private void SetOnlineController(bool online)
    {
        areWeOnline = online;
        onlineController.SetActive(!areWeOnline);
        if (!areWeOnline)
        {
            Debug.Log("Nete bağlı değiliz. Her saniye kontrol et. Bağlanınca sayfayı yeniden yükle.");
            StartCoroutine(ControlConnection());
        }
    }
    IEnumerator ControlConnection()
    {
        yield return new WaitForSeconds(1);
        DayTime();
    }
    private void Update()
    {
        if (gameStart)
        {
            gameTimeNext += Time.deltaTime;
            if (gameTimeNext > 1)
            {
                gameTimeNext--;
                if (!CheckAreWeOnline())
                {
                    return;
                }
                second++;
                if (second == 60)
                {
                    second = 0;
                    minute++;
                    if (minute == 60)
                    {
                        minute = 0;
                        hour++;
                    }
                }
            }
            if (movingPiece is null)
            {
                return;
            }
            if (Input.GetMouseButtonUp(0))
            {
                SetMovingPiece(null);
                return;
            }
            newPos = Input.mousePosition;
            newPos.z = 0f;
            float rightEdgeToScreenEdgeDistance = Screen.width - (newPos.x + movingPiece.rect.width * toolTipCanvas.scaleFactor / 2);
            if (rightEdgeToScreenEdgeDistance < 0)
            {
                newPos.x += rightEdgeToScreenEdgeDistance;
            }
            float leftEdgeToScreenEdgeDistance = 0 - (newPos.x - movingPiece.rect.width * toolTipCanvas.scaleFactor / 2);
            if (leftEdgeToScreenEdgeDistance > 0)
            {
                newPos.x += leftEdgeToScreenEdgeDistance;
            }
            float topEdgeToScreenEdgeDistance = Screen.height - (newPos.y + movingPiece.rect.height * toolTipCanvas.scaleFactor / 2);
            if (topEdgeToScreenEdgeDistance < 0)
            {
                newPos.y += topEdgeToScreenEdgeDistance;
            }
            movingPiece.transform.position = newPos;
        }
    }
    public void SetMovingPiece(RectTransform piece)
    {
        movingPiece = piece;
    }
    public void CheckPuzzle()
    {
        bool isFinished = true;
        for (int e = 0; e < allPieces.Count && isFinished; e++)
        {
            isFinished = allPieces[e].IsStuck;
        }
        if (isFinished)
        {
            gameStart = false;
            Canvas_Manager.Instance.PuzzleFinish();
            Audio_Manager.Instance.PlayPuzzleFixed();
        }
    }
    public void StartPuzzle(Sprite puzzleSprite)
    {
        gameTimeNext = 0;
        gameStart = true;

        // Amounta göre Satır ve Sütün sayısını öğren
        (int, int) row_Col = LearnRowAndColumn();
        int x = row_Col.Item1; // Sütün
        int y = row_Col.Item2; // Satır
        board = new Board[x, y];

        layoutGroup.constraintCount = x;
        RectTransform rect = layoutGroup.GetComponent<RectTransform>();
        float XSize = rect.rect.size.x / x;
        float YSize = rect.rect.size.y / y;

        Vector2 cellSize = new Vector2(XSize, YSize);
        layoutGroup.cellSize = cellSize;

        List<Transform> backs = new List<Transform>();
        List<Piece> pieces = new List<Piece>();
        allPieces.Clear();
        DOVirtual.DelayedCall(waitingDelay, () =>
        {
            for (int i = layoutGroup.transform.childCount - 1; i > 0; i--)
            {
                Destroy(layoutGroup.transform.GetChild(i).gameObject);
            }
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

                for (int e = 0; e < x; e++)
                {
                    for (int i = 0; i < y; i++)
                    {
                        Transform piece = Instantiate(layoutGroup.transform.GetChild(0), layoutGroup.transform).transform;
                        piece.gameObject.SetActive(true);
                        piece.name = "Piece : " + e + " - " + i;
                        piece.GetComponent<RectTransform>().sizeDelta = cellSize;
                        piece.GetChild(0).GetComponent<RectTransform>().sizeDelta = myOrjSize;

                        // Piece üret, pos ayarla, viewi ekle, listeye ekle
                        Piece pi = piece.GetChild(0).GetComponent<Piece>();
                        allPieces.Add(pi);
                        pieces.Add(pi);

                        // Piece tipini ayarla
                        board[e, i] = new Board(pi, e, i);

                        SetPiece(e, i, x, y);

                    }
                }
                DOVirtual.DelayedCall(waitingDelay, () =>
                {
                    for (int e = 0; e < allPieces.Count; e++)
                    {
                        imagePuzzle.sprite = puzzleSprite;
                        Transform backImage = Instantiate(imagePuzzle, imagePuzzle.transform.parent).transform;
                        backImage.gameObject.SetActive(true);
                        backs.Add(backImage);
                    }
                    DOVirtual.DelayedCall(waitingDelay, () =>
                    {
                        for (int e = 0; e < backs.Count; e++)
                        {
                            backs[e].SetParent(allPieces[e].transform);
                        }
                        DOVirtual.DelayedCall(waitingDelay, () =>
                        {
                            for (int e = 0; e < x; e++)
                            {
                                for (int h = 0; h < y; h++)
                                {
                                    // Sol taraf
                                    if (e != 0)
                                    {
                                        board[e, h].pieceLeft = board[e - 1, h].piece;
                                    }
                                    // Alt taraf
                                    if (h != 0)
                                    {
                                        board[e, h].pieceBottom = board[e, h - 1].piece;
                                    }
                                    // Sağ taraf
                                    if (e != x - 1)
                                    {
                                        board[e, h].pieceRight = board[e + 1, h].piece;
                                    }
                                    // Üst taraf
                                    if (h != y - 1)
                                    {
                                        board[e, h].pieceTop = board[e, h + 1].piece;
                                    }
                                    board[e, h].piece.SetPos();
                                }
                            }
                            // Komşulukları ayarla
                            DOVirtual.DelayedCall(waitingDelay, () =>
                            {
                                layoutGroup.enabled = false;
                                DOVirtual.DelayedCall(waitingDelay, () =>
                                {
                                    // Resimleri karıştır ve holdera ekle
                                    while (pieces.Count > 0)
                                    {
                                        int rndPiece = Random.Range(0, pieces.Count);
                                        Canvas_Manager.Instance.AddPuzzlePieceToHolder(pieces[rndPiece]);
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
            board[e, h].pieceType = PieceType.Corner;
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
        board[e, h].piece.SetPiece(view, isEdge, new Vector2Int(e, h));
    }
    private Sprite Middle(int e, int h)
    {
        // Solu kontrol et
        bool isSolCukur = false;
        if (board[e - 1, h].pieceType == PieceType.Edge)
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
        else if (board[e - 1, h].pieceType == PieceType.Middle)
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
        if (board[e, h - 1].pieceType == PieceType.Edge)
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
        else if (board[e, h - 1].pieceType == PieceType.Middle)
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
        board[e, h].pieceType = PieceType.Middle;
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
        board[e, h].pieceType = PieceType.Corner;
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
        board[e, h].pieceType = PieceType.Corner;
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
        board[e, h].pieceType = PieceType.Corner;
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
        board[e, h].pieceType = PieceType.Edge;
        return puzzlePieces.edgePieces_0[randomSprite[rnd]];
    }
    private Sprite AltEdge(int e, int h)
    {
        List<int> randomSprite = new List<int>();

        // Solu kontrol et
        if (board[e - 1, h].pieceType == PieceType.Corner)
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
        else if (board[e - 1, h].pieceType == PieceType.Edge)
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
        board[e, h].pieceType = PieceType.Edge;
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
        if (board[e, h - 1].pieceType == PieceType.Corner)
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
        else if (board[e, h - 1].pieceType == PieceType.Edge)
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
        board[e, h].pieceType = PieceType.Edge;
        return puzzlePieces.edgePieces_180[randomSprite[rnd]];
    }
    private Sprite TopEdge(int e, int h)
    {
        List<int> randomSprite = new List<int>();
        // Solu kontrol et
        bool isSolCukur = false;
        if (board[e - 1, h].pieceType == PieceType.Corner)
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
        else if (board[e - 1, h].pieceType == PieceType.Edge)
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
        board[e, h].pieceType = PieceType.Edge;
        return puzzlePieces.edgePieces_270[randomSprite[rnd]];
    }
    private (int, int) LearnRowAndColumn()
    {
        // 1720 - 1080 -> 2,5
        //  172 - 108  -> 2
        //  66  - 54   -> 2
        //  33  - 27   -> 3
        //  11  - 9    -> 9
        //  11  - 1    -> 11
        // 11*2 - 2*2*5
        int x = 0; // Sütün
        int y = 0; // Satır
        if (Save_Load_Manager.Instance.gameData.pieceAmount == 60)
        {
            x = 10;
            y = 6;
        }
        else if (Save_Load_Manager.Instance.gameData.pieceAmount == 90)
        {
            x = 10;
            y = 9;
        }
        else if (Save_Load_Manager.Instance.gameData.pieceAmount == 165)
        {
            x = 15;
            y = 11;
        }
        else if (Save_Load_Manager.Instance.gameData.pieceAmount == 198)
        {
            x = 18;
            y = 11;
        }
        else if (Save_Load_Manager.Instance.gameData.pieceAmount == 330)
        {
            x = 22;
            y = 15;
        }
        else if (Save_Load_Manager.Instance.gameData.pieceAmount == 440)
        {
            x = 22;
            y = 20;
        }
        else if (Save_Load_Manager.Instance.gameData.pieceAmount == 594)
        {
            x = 27;
            y = 22;
        }
        else if (Save_Load_Manager.Instance.gameData.pieceAmount == 660)
        {
            x = 33;
            y = 20;
        }
        return (x, y);
    }
    public void IsPieceConnectNeighbor(Piece piece, Vector2Int coor)
    {
        // TODO : Yerinde olmadığı halde komsularına denk gelen bir piece olursa
        //          boştaki piece tekilse ve komsusunun kendi komsularıyla birlikteyse onun birlikteliğine gir, komsu birlikte değilse 2 komsuyu birlikte yap
        //          boştaki piece birlikteyse komsu birlikte olsada olmasada bu birlikteliğe al
        // Komsularına yakınlığına bak
        RectTransform pieceRect = piece.LearnParentRect();
        // Sol komsu bos değilse ve yakınsa -> yaklastir
        if (board[coor.x, coor.y].pieceLeft is not null)
        {
            RectTransform pieceLeftRect = board[coor.x, coor.y].pieceLeft.LearnParentRect();
            if (Vector2.SqrMagnitude(pieceRect.anchoredPosition - pieceLeftRect.anchoredPosition) < 1000.0f)
            {
                if (pieceLeftRect.anchoredPosition.x > pieceRect.anchoredPosition.x) // Y kontrolüde yapılmalı
                {
                    pieceRect.DOAnchorPos(pieceLeftRect.anchoredPosition - new Vector2(pieceRect.sizeDelta.x, 0), 0.25f);
                    if (board[coor.x, coor.y].pieceLeft.LearnParent() is null)
                    {
                        if (piece.LearnParent() is null)
                        {
                            // diğer piece tekil ve bu piece tekilmiş ortak olsunlar
                        }
                        else
                        {
                            // diğer piece tek değilmiş, bu piece ve grubuna katılsın
                        }
                    }
                    else
                    {
                        if (piece.LearnParent() is null)
                        {
                            // bu piece tekilmiş diğer piecein grubuna katılsın
                        }
                        else
                        {
                            // diğer piece ve bu piecein grubu varmış, bu piecein grubu diğer grubuna katılsın
                        }
                    }
                }
            }
        }
        // Sag komsu bos değilse ve yakınsa -> yaklastir
        if (board[coor.x, coor.y].pieceRight is not null)
        {
            RectTransform pieceRightRect = board[coor.x, coor.y].pieceRight.LearnParentRect();
            if (Vector2.SqrMagnitude(pieceRect.anchoredPosition - pieceRightRect.anchoredPosition) < 1000.0f)
            {
                if (pieceRightRect.anchoredPosition.x < pieceRect.anchoredPosition.x)
                {
                    pieceRect.DOAnchorPos(pieceRightRect.anchoredPosition + new Vector2(pieceRect.sizeDelta.x, 0), 0.25f);
                }
            }
        }
        // Top komsu bos değilse ve yakınsa -> yaklastir
        if (board[coor.x, coor.y].pieceTop is not null)
        {
            RectTransform pieceTopRect = board[coor.x, coor.y].pieceTop.LearnParentRect();
            if (Vector2.SqrMagnitude(pieceRect.anchoredPosition - pieceTopRect.anchoredPosition) < 1000.0f)
            {
                if (pieceTopRect.anchoredPosition.y > pieceRect.anchoredPosition.y)
                {
                    pieceRect.DOAnchorPos(pieceTopRect.anchoredPosition - new Vector2(0, pieceRect.sizeDelta.y), 0.25f);
                }
            }
        }
        // Alt komsu bos değilse ve yakınsa -> yaklastir
        if (board[coor.x, coor.y].pieceBottom is not null)
        {
            RectTransform pieceBottomRect = board[coor.x, coor.y].pieceBottom.LearnParentRect();
            if (Vector2.SqrMagnitude(pieceRect.anchoredPosition - pieceBottomRect.anchoredPosition) < 1000.0f)
            {
                if (pieceBottomRect.anchoredPosition.y > pieceRect.anchoredPosition.y)
                {
                    pieceRect.DOAnchorPos(pieceBottomRect.anchoredPosition + new Vector2(0, pieceRect.sizeDelta.y), 0.25f);
                }
            }
        }
    }
}