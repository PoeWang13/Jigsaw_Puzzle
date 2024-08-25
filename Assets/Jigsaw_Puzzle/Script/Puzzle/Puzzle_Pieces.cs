using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Genel/Pieces")]
public class Puzzle_Pieces : ScriptableObject
{
    //public bool onValid;
    //public void OnValidate()
    //{
    //    if (onValid)
    //    {
    //        onValid = false;
    //        middlePiec3es.Add(-1);
    //    }
    //}

    [Header("Middle - Puzzle Piece")]
    public List<Sprite> middlePieces = new List<Sprite>();

    [Header("0 - Puzzle Piece")]
    public List<Sprite> cornerPieces_0 = new List<Sprite>();
    public List<Sprite> edgePieces_0 = new List<Sprite>();

    [Header("90 - Puzzle Finish")]
    public List<Sprite> cornerPieces_90 = new List<Sprite>();
    public List<Sprite> edgePieces_90 = new List<Sprite>();

    [Header("180 - Puzzle Finish")]
    public List<Sprite> cornerPieces_180 = new List<Sprite>();
    public List<Sprite> edgePieces_180 = new List<Sprite>();

    [Header("270 - Puzzle Finish")]
    public List<Sprite> cornerPieces_270 = new List<Sprite>();
    public List<Sprite> edgePieces_270 = new List<Sprite>();
}