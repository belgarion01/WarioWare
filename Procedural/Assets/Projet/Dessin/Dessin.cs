using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Dessin : MonoBehaviour
{
    private WorldTile _tile;
    public TileTypes tileSettings;
    public bool canDraw = true;

    private void Update()
    {
        if (Input.GetMouseButton(0)&&canDraw)
        {
            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var worldPoint = new Vector3Int(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), 0);

            var tiles = GameTiles.instance.tiles; // This is our Dictionary of tiles

            if (tiles.TryGetValue(worldPoint, out _tile))
            {
                if (_tile.type == tileSettings.defaultTile.type)
                {
                    print("Tile " + _tile.name);
                    _tile.tilemap.SetTileFlags(_tile.position, TileFlags.None);
                    _tile.tilemap.SetColor(_tile.position, Color.green);
                    _tile.activated = true;
                }

                else if(_tile.type == tileSettings.blockTile.type)
                {
                    canDraw = false;
                }
            }
        }
    }
}
