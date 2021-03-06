﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using NaughtyAttributes;

public class GameTiles : MonoBehaviour
{
    public static GameTiles instance;
    public Tilemap Tilemap;

    public Dictionary<Vector3, TileData> tiles;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        GetWorldTiles();
    }

    // Use this for initialization
    private void GetWorldTiles()
    {
        tiles = new Dictionary<Vector3, TileData>();
        foreach (Vector3Int pos in Tilemap.cellBounds.allPositionsWithin)
        {
            var localPlace = new Vector3Int(pos.x, pos.y, pos.z);

            if (!Tilemap.HasTile(localPlace)) continue;
            var tile = new TileData
            {
                position = localPlace,
                worldPosition = Tilemap.CellToWorld(localPlace),
                activated = false,
                tilemap = Tilemap,
                name = localPlace.x + "," + localPlace.y,
                type = Tilemap.GetTile(localPlace),
            };
            tiles.Add(tile.worldPosition, tile);
        }
    }
}
