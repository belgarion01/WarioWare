using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileTypes : ScriptableObject
{
    public TileType defaultTile;

    public TileType blockTile;

    public TileType bonChemin;

    public TileType mauvaisChemin;

    public TileType batiment;
}

[System.Serializable]
public class TileType
{
    public Tile tile;
    public TileBase type;
}
