using System;
using System.Net;
using UnityEngine;
using System.Collections;
using System.Globalization;
using UnityEngine.Rendering;

public enum PuzzleType
{
    Jigsaw,
    Slide,
    Swap
}
public class Game_Manager : Singletion<Game_Manager>
{
    [ContextMenu("Puzzle Finish")]
    public void PuzzleFinish()
    {
        Canvas_Manager.Instance.PuzzleFinish();
    }
    [SerializeField] private GameObject onlineController;

    private int hour;
    private int minute;
    private int second;
    
    private float gameTimeNext;

    private bool gameStart;
    private bool areWeOnline;

    private Vector3 newPos;
    private DateTime dateTime;
    private Canvas toolTipCanvas;
    private PuzzleType puzzleType;
    private RectTransform movingPiece;

    public bool AreWeOnline { get { return areWeOnline; } }
    public DateTime DateTime { get { return dateTime; } }
    public string GameTime { get { return hour + " : " + minute + " : " + second; } }

    public override void OnAwake()
    {
        toolTipCanvas = GetComponentInChildren<Canvas>();
        StartCoroutine(ControlConnection());
    }
    IEnumerator ControlConnection()
    {
        while (true)
        {
            // Yerel saati veriyor.
            try
            {
                using (var response = WebRequest.Create("http://www.google.com").GetResponse())
                {
                    areWeOnline = true;
                    onlineController.SetActive(false);
                    dateTime = DateTime.ParseExact(response.Headers["date"], "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal);

                }
            }
            catch (WebException)
            {
                areWeOnline = false;
                onlineController.SetActive(true);
            }
            yield return new WaitForSeconds(5);
        }
    }
    private void Update()
    {
        if (gameStart)
        {
            gameTimeNext += Time.deltaTime;
            if (gameTimeNext > 1)
            {
                gameTimeNext--;
                if (!areWeOnline)
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
    public void SetGame(bool isStart)
    {
        gameStart = isStart;
    }
    public void SetPuzzleType(PuzzleType pType)
    {
        puzzleType = pType;
    }
    public void StartPuzzle(Texture2D texture)
    {
        gameTimeNext = 0;
        gameStart = true;
        if (puzzleType == PuzzleType.Jigsaw)
        {
            Canvas_Manager.Instance.OpenJigsawPuzzleObjects();
            Sprite puzzleSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            Jigsaw_Manager.Instance.StartPuzzleJigsaw(puzzleSprite);
        }
        else if (puzzleType == PuzzleType.Slide)
        {
            Canvas_Manager.Instance.OpenSlidePuzzleObjects();
            Slide_Manager.Instance.StartPuzzleSlide(texture);
        }
        else if (puzzleType == PuzzleType.Swap)
        {
            Canvas_Manager.Instance.OpenSwapPuzzleObjects();
            Swap_Manager.Instance.StartPuzzleSwap(texture);
        }
    }
    public (int, int) LearnRowAndColumn()
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
            x = 6;
            y = 4;
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
}