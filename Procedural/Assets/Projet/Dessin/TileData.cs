using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileData
{
    public string name;

    public Vector3Int position;

    public Vector3 worldPosition;

    public Tilemap tilemap;

    public bool activated;

    public TileBase type;

    public TileData() { }
}
