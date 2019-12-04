using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triche : MonoBehaviour
{
    public MementoSavedData savedData;
    public bool on = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            On(!on);
        }
    }

    private void OnGUI()
    {
        if(savedData.commands.Length > 0&&on)
        {
            for (int i = 0; i < savedData.commands.Length; i++)
            {
                GUI.Label(new Rect(new Vector2(0, (i - 1) * 20f), new Vector2(100f, 20f)), savedData.commands[i].ToString());
            }
        }
    }

    public void On(bool on)
    {
        this.on = on;
    }
}
