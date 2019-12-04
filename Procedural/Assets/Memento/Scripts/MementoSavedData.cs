using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Micro-games/Memento/MementoSavedData")]
public class MementoSavedData : ScriptableObject
{
    public List<TileData> tilesData;
    public int numberOfCorrectPaths;
    public TilemapGenerator.Command[] commands;
    public ChooseDifficulty.Difficulty difficulty;
}
