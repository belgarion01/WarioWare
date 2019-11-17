using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class MapGeneration : MonoBehaviour
{
    public int mapHeight;
    public int mapWidth;

    GameObject Map;

    public GameObject prefab;

    private void Start()
    {
        //GenerateMap(mapWidth, mapHeight);
    }

    public int[,] GenerateMap(int width, int height)
    {
        int[,] map = new int[width, height];

        for (int x = 0; x < map.GetUpperBound(0); x++)
        {
            for (int y = 0; y < map.GetUpperBound(1); y++)
            {
                int rand = Random.Range(0, 2);
                map[x, y] = rand;
            }
        }
        return map;
    }

    public void RenderMap(int[,] map)
    {
        ClearMap();
        Map = new GameObject("Map");

        for (int x = 0; x < map.GetUpperBound(0); x++)
        {
            for (int y = 0; y < map.GetUpperBound(1); y++)
            {
                if(map[x, y] == 1)
                {
                    GameObject obj = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity, Map.transform);
                }
            }
        }
    }

    public void ClearMap()
    {
        DestroyImmediate(Map);
    }

    [Button]
    public void Launch()
    {
        RenderMap(GenerateMap(mapWidth, mapHeight));
    }
}
