using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Piece : MonoBehaviour, IBeginDragHandler, IDropHandler, IDragHandler
{
    [SerializeField] private Image myImage;
    [SerializeField] private RectTransform myRect;
    [SerializeField] private RectTransform myParentRect;

    private float closeDistance;
    private float clickTimeNext;
    private bool isStuck;
    private bool isEdge;
    private bool inMenu;
    private Vector2 myParentEdgeScale;

    private Vector2 myPos;
    private Vector2Int myCoor;
    private Transform myParent;

    public bool IsStuck { get { return isStuck; } }
    public bool IsEdge { get { return isEdge; } }

    private void Update()
    {
        if (isStuck)
        {
            // Piece yerinde
            return;
        }
        if (inMenu)
        {
            // Piece kenarda
            return;
        }
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase > TouchPhase.Ended)
            {
                ClickTimePassed();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ClickTimePassed();
        }
        clickTimeNext += Time.deltaTime;
    }
    private void ClickTimePassed()
    {
        if (clickTimeNext < 0.5f)
        {
            // Double click yapıldı.
            if (Save_Load_Manager.Instance.gameData.canTurnPiece)
            {
                transform.Rotate(Vector3.back * 90);
            }
        }
        clickTimeNext = 0;
    }
    public void SetPiece(Sprite sprite, bool edge, Vector2Int coor)
    {
        isEdge = edge;
        inMenu = true;
        myCoor = coor;
        myImage.sprite = sprite;

        myParentEdgeScale = Game_Manager.Instance.MyParentEdgeScale;
        closeDistance = Game_Manager.Instance.CloseDistance;
    }
    public void SetPos()
    {
        myPos = myParentRect.anchoredPosition;
    }
    public RectTransform LearnParentRect()
    {
        return myParentRect;
    }
    public RectTransform LearnMyRect()
    {
        return myRect;
    }
    public void SetParent(Transform parent)
    {
        myParent = parent;
    }
    public Transform LearnParent()
    {
        return myParent;
    }
    public void SetPieceSizeForEdge(bool isEnterEdge)
    {
        if (isEnterEdge)
        {
            myParentRect.localScale = new Vector2(myParentRect.localScale.x * myParentEdgeScale.x, myParentRect.localScale.y * myParentEdgeScale.y);
        }
        else
        {
            myParentRect.localScale = Vector2.one;
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (isStuck)
                {
                    return;
                }
                if (myParent is not null)
                {
                    return;
                }
                // Yan menuden çıkart
                if (Canvas_Manager.Instance.RemovePuzzlePieceFromHolder(this))
                {
                    inMenu = false;
                    // Orj size yap
                    SetPieceSizeForEdge(false);
                }
                transform.parent.SetAsLastSibling();
                Game_Manager.Instance.SetMovingPiece(myParentRect);
            }
        }
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // X kontrolü yap
            if (myParentRect.anchoredPosition.x < 0)
            {
                // Holdera koy
                inMenu = true;
                Canvas_Manager.Instance.AddPuzzlePieceToHolder(this);
                Game_Manager.Instance.SetMovingPiece(null);
                return;
            }
            // Puzzle Pos kontrol yap
            CheckPos();
            Game_Manager.Instance.SetMovingPiece(null);
        }
    }
    public void CheckPos(bool checkNeighbor = false)
    {
        if (Vector2.SqrMagnitude(myPos - myParentRect.anchoredPosition) < closeDistance)
        {
            if (transform.eulerAngles.z == 0)
            {
                Audio_Manager.Instance.PlayPuzzlePieceRightPlace();
                myParentRect.DOAnchorPos(myPos, 0.25f);
                isStuck = true;
                inMenu = true;
                myImage.raycastTarget = false;
                transform.parent.SetAsFirstSibling();
                Game_Manager.Instance.CheckPuzzle();
            }
        }
        else
        {
            if (checkNeighbor)
            {
                // Komsularla yakınlık kontrolu yap.
                Game_Manager.Instance.IsPieceConnectNeighbor(this, myCoor);
            }
        }
    }
    // OnBeginDrag çalışmıyor bu olmayınca
    public void OnDrag(PointerEventData eventData)
    {
    }
}