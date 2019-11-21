using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triche : MonoBehaviour
{
    public TilemapGenerator tilemapGenerator;
    private TilemapGenerator.Command[] commands { get { return tilemapGenerator.commands; } }

    private void OnGUI()
    {
        if (tilemapGenerator)
        {
            for (int i = 1; i < commands.Length; i++)
            {
                GUI.Label(new Rect(new Vector2(0, (i-1) * 20f), new Vector2(100f, 20f)), commands[i].ToString());
            }
        }
    }
}
