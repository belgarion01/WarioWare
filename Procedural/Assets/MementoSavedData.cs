using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MementoSavedData : ScriptableObject
{
    public List<TileData> tilesData;
    public int numberOfCorrectPaths;
    public TilemapGenerator.Command[] commands;
}
