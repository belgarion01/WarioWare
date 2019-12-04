using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class Dessin : MonoBehaviour
{
    //References
    public Tilemap tilemap;
    private TileData _tile;
    public TileTypes tileTypes;
    public MementoSavedData savedData;


    //Vérifie les tiles coloriées (ça peut être un autre script ça en vrai mais ça prend moins de place)
    int numberOfCorrectPathTiles { get { return savedData.numberOfCorrectPaths; } }
    int numberOfActivatedMainPathTiles;

    public GameObject defeatPannel;
    public GameObject victoryPannel;

    //Drawing
    public GameObject Line;
    LineRenderer currentLineRenderer;
    List<Vector3> currentLinePositions = new List<Vector3>();
    bool canDraw = true;

    //Tiles
    Dictionary<Vector3, TileData> tiles;

    //Visual
    public Transform Pen;

    private void Start()
    {
        StartCoroutine(GetTiles());
    }


    private void Update()
    {
        Pen.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Pen.position = new Vector3(Pen.position.x, Pen.position.y, -1);

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

            if (tiles.TryGetValue(worldPoint, out _tile))
            {
                if (_tile.type == tileTypes.CorrectPath&&!_tile.activated)
                {
                    _tile.tilemap.SetTileFlags(_tile.position, TileFlags.None);
                    //_tile.tilemap.SetColor(_tile.position, Color.green);
                    _tile.activated = true;
                    numberOfActivatedMainPathTiles++;
                    if (numberOfActivatedMainPathTiles == numberOfCorrectPathTiles) Victory();
                }

                else if(_tile.type == tileTypes.Building || _tile.type == tileTypes.WrongPath)
                {
                    Defeat();
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (numberOfActivatedMainPathTiles != numberOfCorrectPathTiles)
            {
                Defeat();
            }
            Debug.Log(numberOfActivatedMainPathTiles + " / " + numberOfCorrectPathTiles);
        }
    }

    #region Dessin

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

    #endregion

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

    IEnumerator GetTiles()
    {
        while(GameTiles.instance == null) yield return new WaitForEndOfFrame();
        tiles = GameTiles.instance.tiles;
    }
}


