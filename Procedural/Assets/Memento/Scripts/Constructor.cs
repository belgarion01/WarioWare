using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Constructor : MonoBehaviour
{
    public MementoSavedData savedData;
    public Tilemap tilemap;

    private void Awake()
    {
        Construct();
    }

    
    void Construct()
    {
        foreach (TileData tile in savedData.tilesData)
        {
            tilemap.SetTile(tile.position, tile.type);
        }
    }
}
