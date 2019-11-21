using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class Dessin : MonoBehaviour
{
    //References
    public TilemapGenerator generator;
    public Tilemap tilemap;
    private TileData _tile;
    public TileTypes tileTypes;


    //Vérifie les tiles coloriées (ça peut être un autre script ça en vrai mais ça prend moins de place)
    int numberOfMainPathTiles;
    int numberOfActivatedMainPathTiles;

    public GameObject defeatPannel;
    public GameObject victoryPannel;

    //Drawing
    public GameObject Line;
    LineRenderer currentLineRenderer;
    List<Vector3> currentLinePositions = new List<Vector3>();
    bool canDraw = true;


    private void Start()
    {
        numberOfMainPathTiles = 0;
        numberOfActivatedMainPathTiles = 0;
        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            var localPlace = new Vector3Int(pos.x, pos.y, pos.z);

            if (!tilemap.HasTile(localPlace)) continue;
            if (tilemap.GetTile(localPlace) == tileTypes.CorrectPath) numberOfMainPathTiles++;
        }
        numberOfMainPathTiles -= generator.stepsToRemove+1;
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
            if(Vector2.Distance(tempLinePos, currentLinePositions[currentLinePositions.Count - 1]) > 0.1f)
            {
                AddLinePoint(tempLinePos);
            }

            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var worldPoint = new Vector3Int(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), 0);

            var tiles = GameTiles.instance.tiles; // This is our Dictionary of tiles

            if (tiles.TryGetValue(worldPoint, out _tile))
            {
                if (_tile.type == tileTypes.CorrectPath&&!_tile.activated)
                {
                    //print("Tile " + _tile.name);
                    _tile.tilemap.SetTileFlags(_tile.position, TileFlags.None);
                    //_tile.tilemap.SetColor(_tile.position, Color.green);
                    _tile.activated = true;
                    numberOfActivatedMainPathTiles++;
                    if (numberOfActivatedMainPathTiles == numberOfMainPathTiles) Victory();
                }

                else if(_tile.type == tileTypes.Building || _tile.type == tileTypes.WrongPath)
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
            if (numberOfActivatedMainPathTiles != numberOfMainPathTiles)
            {
                Defeat();
            }
            Debug.Log(numberOfActivatedMainPathTiles + " / " + numberOfMainPathTiles);
        }
    }

    void CreateLine()
    {
        GameObject _Line = Instantiate(Line, transform.position, Quaternion.identity);
        currentLineRenderer = _Line.GetComponent<LineRenderer>();
        currentLinePositions.Clear();
        currentLinePositions.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        currentLinePositions.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        currentLineRenderer.SetPosition(0, currentLinePositions[0]);
        currentLineRenderer.SetPosition(1, currentLinePositions[1]);
    }

    void AddLinePoint(Vector3 position)
    {
        currentLinePositions.Add(position);
        currentLineRenderer.positionCount++;
        currentLineRenderer.SetPosition(currentLineRenderer.positionCount - 1, position);
    }

    void Defeat()
    {
        canDraw = false;
        defeatPannel.SetActive(true);
    }

    void Victory()
    {
        canDraw = false;
        victoryPannel.SetActive(true);
    }
}


