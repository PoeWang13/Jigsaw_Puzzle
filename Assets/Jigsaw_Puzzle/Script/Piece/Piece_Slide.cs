using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class Piece_Slide : MonoBehaviour, IPointerClickHandler
{
    public Vector2Int myCoor;
    public Vector2Int myOrjCoor;
    private RectTransform myRect;

    public RectTransform MyRect { get { return myRect; } }
    public Vector2Int MyCoor { get { return myCoor; } }
    public Vector2Int MyOrjCoor { get { return myOrjCoor; } }
    public bool IsStuck { get { return myCoor == myOrjCoor; } }

    private void Start()
    {
        myRect = GetComponent<RectTransform>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase > TouchPhase.Ended)
            {
                ClickPiece();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ClickPiece();
        }
    }
    private void ClickPiece()
    {
        Slide_Manager.Instance.ClickPiece(this);
    }
    public void SetCoor(Vector2Int pos)
    {
        myCoor = pos;
        myOrjCoor = pos;
    }
    public void SetNewCoor(Vector2Int pos)
    {
        myCoor = pos;
    }
    public void SlidePiece(Vector2 newPos, Vector2Int newCoor)
    {
        myCoor = newCoor;
        myRect.DOAnchorPos(newPos, 1).OnComplete(() =>
        {
            Slide_Manager.Instance.CheckPuzzle();
        });
    }
}