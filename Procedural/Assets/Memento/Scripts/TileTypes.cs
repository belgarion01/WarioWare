using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Micro-games/Memento/TileTypes")]
public class TileTypes : ScriptableObject
{
    public TileBase MainPath;

    public TileBase SecondPath;

    public TileBase Crossroad;

    public TileBase Cross;

    public TileBase Debug;

    public TileBase CorrectPath;

    public TileBase WrongPath;

    public TileBase Building;
}