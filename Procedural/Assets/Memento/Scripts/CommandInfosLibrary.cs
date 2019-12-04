using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Micro-games/Memento/CommandInfosLibrary")]
public class CommandInfosLibrary : ScriptableObject
{
    public Sprite LeftCommandSprite;
    public Sprite ForwardCommandSprite;
    public Sprite RightCommandSprite;

    public Sprite GetCommandSprite(TilemapGenerator.Command command)
    {
        switch (command)
        {
            case TilemapGenerator.Command.Gauche:
                return LeftCommandSprite;

            case TilemapGenerator.Command.Devant:
                return ForwardCommandSprite;

            case TilemapGenerator.Command.Droite:
                return RightCommandSprite;

            default:
                return null;
        }
    }

    public string[] roadsName;

    public string GetRandomRoadName()
    {
        int rand = Random.Range(0, roadsName.Length);
        return roadsName[rand];
    }
}
