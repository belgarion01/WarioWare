using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Dessin : MonoBehaviour
{
    private WorldTile _tile;
    public TileTypes tileSettings;
    public bool canDraw = true;

    public Tilemap tilemap;
    public GameObject GameOver;
    public GameObject Victoire;

    int numberOfBonChemin;
    int numberOfCheckedBonChemin;

    public TilemapGenerator generator;

    public GameObject Line;
    LineRenderer lRenderer;
    List<Vector3> linePositions = new List<Vector3>();

    private void Start()
    {
        numberOfBonChemin = 0;
        numberOfCheckedBonChemin = 0;
        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            var localPlace = new Vector3Int(pos.x, pos.y, pos.z);

            if (!tilemap.HasTile(localPlace)) continue;
            if (tilemap.GetTile(localPlace) == tileSettings.bonChemin.type) numberOfBonChemin++;
        }
        numberOfBonChemin -= generator.stepsToRemove+1;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CreateLine();
        }
        if (Input.GetMouseButton(0)&&canDraw)
        {
            Vector2 tempLinePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if(Vector2.Distance(tempLinePos, linePositions[linePositions.Count - 1]) > 0.1f)
            {
                AddLinePoint(tempLinePos);
            }

            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var worldPoint = new Vector3Int(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), 0);

            var tiles = GameTiles.instance.tiles; // This is our Dictionary of tiles

            if (tiles.TryGetValue(worldPoint, out _tile))
            {
                if (_tile.type == tileSettings.bonChemin.type&&!_tile.activated)
                {
                    //print("Tile " + _tile.name);
                    _tile.tilemap.SetTileFlags(_tile.position, TileFlags.None);
                    //_tile.tilemap.SetColor(_tile.position, Color.green);
                    _tile.activated = true;
                    numberOfCheckedBonChemin++;
                    if (numberOfCheckedBonChemin == numberOfBonChemin) Victory();
                }

                else if(_tile.type == tileSettings.batiment.type || _tile.type == tileSettings.mauvaisChemin.type)
                {
                    //canDraw = false;
                    //Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), _tile.position + (new Vector3(0.5f, 0.5f, 0f)), Quaternion.identity);
                    Defeat();
                    Debug.Log("GAME OVER : " + _tile.type);
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (numberOfCheckedBonChemin != numberOfBonChemin)
            {
                Defeat();
            }
            Debug.Log(numberOfCheckedBonChemin + " / " + numberOfBonChemin);
        }
    }

    void CreateLine()
    {
        GameObject _Line = Instantiate(Line, transform.position, Quaternion.identity);
        lRenderer = _Line.GetComponent<LineRenderer>();
        linePositions.Clear();
        linePositions.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        linePositions.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        lRenderer.SetPosition(0, linePositions[0]);
        lRenderer.SetPosition(1, linePositions[1]);
    }

    void AddLinePoint(Vector3 position)
    {
        linePositions.Add(position);
        lRenderer.positionCount++;
        lRenderer.SetPosition(lRenderer.positionCount - 1, position);
    }

    void Defeat()
    {
        canDraw = false;
        GameOver.SetActive(true);
    }

    void Victory()
    {
        canDraw = false;
        Victoire.SetActive(true);
    }
}


