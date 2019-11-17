using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGenerator : MonoBehaviour
{
    public int mapHeight;
    public int mapWidth;

    public int numberOfCommands;

    public Tilemap tilemap;
    public Tile defaultTile;
    public Tile blockTile;
    public Tile roundTile;
    public Tile debugTile;
    public Tile stageTile;
    public Tile stageBlockTile;

    public enum Direction { Haut, Droite, Bas, Gauche};

    public Vector3Int headPosition;
    public Vector3Int lastPosition;
    Direction headDirection;

    public enum Command { Devant, Droite, Gauche};
    public Command[] commands;

    public TileBase defaultType;
    public TileBase blockType;
    public TileBase carrefourType;
    public TileBase stageType;

    public bool success;

    public List<Vector3Int> carrefourPositions = new List<Vector3Int>();

    public void GeneratePath()
    {
        ClearMap();
        GenerateWall();
        carrefourPositions.Clear();

        tilemap.SetTile(headPosition, defaultTile);
        lastPosition = headPosition;

        headDirection = Direction.Droite;

        success = false;

        //Set des Commandes aléatoire
        commands = RandomCommands(numberOfCommands);

        //Début du long chemin (plusieurs courts chemins)
        for (int i = 0; i < commands.Length; i++)
        {
            //Nombre aléatoire de pas pour le court chemin
            int randomSteps = Random.Range(2, 4);
            if (i == 0) randomSteps = 2;

            //Tourne en fonction de la Commande
            headDirection = RotateDirection(commands[i], headDirection);

            //Prédit la fin normale du court chemin
            Vector3Int finalDestination = headPosition + ToVector3(headDirection) * randomSteps;
           
            //Commence les petits pas (plusieurs points) (intermédiaire entre le début et la fin du court chemin)
            for (int j = 0; j < randomSteps; j++)
            {
                //Détecte si le jeu peut être fini
                if (i == commands.Length - 1 && j == 1)
                {
                    success = true;
                    Debug.Log("Success!");
                }

                //Tente une première direction
                Vector3Int nextPosition = headPosition + ToVector3(headDirection);

                //Change la direction si elle est bloquée avec la commande donnée
                if (tilemap.GetTile(nextPosition) == blockType/*&&j == 0*/)
                {
                    if (TryAnotherCommand(commands[i], headPosition, i)&&j == 0)
                    {
                        nextPosition = headPosition + ToVector3(headDirection);
                    }
                    //Si aucun chemin n'est trouvé, reload la map
                    else
                    {
                        GeneratePath();
                        Debug.Log("BLOCKED");
                        return;
                    }                  
                }

                //Pose la Tile chemin à la prochaine position
                tilemap.SetTile(nextPosition, defaultTile);

                //Pose les blockTiles
                for (int k = 0; k < 4; k++)
                {
                    Vector3Int positionToCheck = headPosition + ToVector3((Direction)k);
                    if(positionToCheck != nextPosition && isEmptyTile(positionToCheck))
                    {
                        tilemap.SetTile(positionToCheck, blockTile);
                    }
                }

                //Avance la tête à la position prédit
                headPosition = nextPosition;
                //Fin du court chemin
            }

            //Place une balise à la destination prédite qu'elle soit correcte ou non
            if (isEmptyTile(finalDestination))
            {
                tilemap.SetTile(finalDestination, debugTile);
            }

            //Place une balise au carrefour
            tilemap.SetTile(headPosition, roundTile);
            carrefourPositions.Add(headPosition);

            //WIP STOCKER LA POSITION DES CARREFOURS POUR GENERATE OTHER ROUTES LA
            //COMMENT AJOUTER D'AUTRES CARREFOURS ?
        }
    }

    [Button]
    void GenerateCarrefour()
    {
        List<CarrefourPath> possiblePaths = new List<CarrefourPath>();

        for (int i = 0; i < carrefourPositions.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                //Check if there's other way possible in blockTile
                Vector3Int possiblePosition = carrefourPositions[i] + ToVector3((Direction)j);
                if(tilemap.GetTile(possiblePosition)!= defaultType /*&& tilemap.GetTile(possiblePosition) != defaultType*/)
                {
                    possiblePaths.Add(new CarrefourPath((Direction)j, carrefourPositions[i]));
                }
            }
        }
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < possiblePaths.Count; j++)
            {
                if (possiblePaths[j].stopped) continue;
                //Check si la prochain position est un defaultType si oui il stop
                Vector3Int nextPosition = possiblePaths[j].position + ToVector3(possiblePaths[j].direction);
                //Check si y'a deja une tile a côté
                if (possiblePaths[j].direction == Direction.Haut || possiblePaths[j].direction == Direction.Bas)
                {
                    TileBase typeRight = tilemap.GetTile(nextPosition + ToVector3(Direction.Droite));
                    TileBase typeLeft = tilemap.GetTile(nextPosition + ToVector3(Direction.Gauche));

                    if (typeRight == stageType || typeLeft == stageType)
                    {
                        //Check y'en avait déjà un
                        typeRight = tilemap.GetTile(possiblePaths[j].position + ToVector3(Direction.Droite));
                        typeLeft = tilemap.GetTile(possiblePaths[j].position + ToVector3(Direction.Gauche));

                        if (typeRight == stageType || typeLeft == stageType)
                        {
                            possiblePaths[j].stopped = true;
                            continue;
                        }
                        else
                        {
                            possiblePaths[j].stopped = true;
                        }
                    }
                }
                else
                {
                    TileBase typeUp = tilemap.GetTile(nextPosition + ToVector3(Direction.Haut));
                    TileBase typeDown = tilemap.GetTile(nextPosition + ToVector3(Direction.Bas));

                    if (typeUp == stageType || typeDown == stageType)
                    {
                        //Check y'en avait déjà un
                        typeUp = tilemap.GetTile(possiblePaths[j].position + ToVector3(Direction.Haut));
                        typeDown = tilemap.GetTile(possiblePaths[j].position + ToVector3(Direction.Bas));

                        if (typeUp == stageType || typeDown == stageType)
                        {
                            possiblePaths[j].stopped = true;
                            continue;
                        }
                        else
                        {
                            possiblePaths[j].stopped = true;
                        }
                    }
                }

                if (isDefaultTile(nextPosition)/*||tilemap.GetTile(nextPosition) == stageBlockType*/)
                {
                    possiblePaths[j].stopped = true;
                    continue;
                }
                else
                {
                    //Commence à set une tile
                    tilemap.SetTile(nextPosition, stageTile);
                    //Pose stageBlockTile
                    //Vector3Int nextNextPosition = nextPosition + ToVector3(possiblePaths[j].direction);
                    //for (int k = 0; k < 4; k++)
                    //{
                    //    Vector3Int tempPosition = nextPosition + ToVector3((Direction)k);
                    //    if(tempPosition != nextNextPosition && tempPosition != possiblePaths[j].position)
                    //    {
                    //        tilemap.SetTile(tempPosition, stageBlockType);
                    //    }
                    //}

                    possiblePaths[j].position = nextPosition;
                }
            }
        }
    }

    public class CarrefourPath
    {
        public Direction direction;
        public Vector3Int position;
        public bool stopped;

        public CarrefourPath(Direction direction, Vector3Int position)
        {
            this.direction = direction;
            this.position = position;
            stopped = false;
        }
    }

    public void ConvertTiles()
    {

    }

    bool isDefaultTile(Vector3Int position)
    {
        TileBase type = tilemap.GetTile(position);
        return type == defaultType ? true : false;
    }

    bool isEmptyTile(Vector3Int position)
    {
        TileBase type = tilemap.GetTile(position);
        return type == null ? true : false;
    }

    bool TryAnotherCommand(Command blockedCommand, Vector3Int position, int commandToChangeIndex)
    {
        List<Command> commands = new List<Command>();
        for (int i = 0; i < 3; i++)
        {
            if (i != (int)blockedCommand)
            {
                commands.Add((Command)i);
            }
        }

        List<Command> successfulCommands = new List<Command>();
        Direction testDirection = headDirection;
        testDirection = RotateDirection(GetInvertCommand(blockedCommand), testDirection);
        for (int i = 0; i < commands.Count; i++)
        {
            Vector3Int potentialPosition = headPosition + ToVector3(RotateDirection(commands[i], testDirection));
            if (isEmptyTile(potentialPosition))
            {
                successfulCommands.Add(commands[i]);
            }
        }
        if (successfulCommands.Count > 0)
        {
            headDirection = RotateDirection(successfulCommands[0], testDirection);
            this.commands[commandToChangeIndex] = successfulCommands[0];            
            return true;
        }
        else
        {
            return false;
        }
    }

    Command GetInvertCommand(Command command)
    {
        switch (command)
        {
            case Command.Devant:
                return Command.Devant;
            case Command.Droite:
                return Command.Gauche;
            case Command.Gauche:
                return Command.Droite;
            default:
                return Command.Devant;
        }
    }

    void GenerateWall()
    {
        Vector3Int origin = new Vector3Int(-1, -5, 0);

        const int width = 17;
        const int height = 10;

        const int widthOffset = 1;
        const int heightOffset = 5;

        for (int i = origin.y; i < height+origin.y; i++)
        {
            if(i == origin.y || i == height+origin.y-1)
            {
                for (int j = origin.x; j < width+origin.x-1; j++)
                {
                    tilemap.SetTile(new Vector3Int(j /*- widthOffset*/, i, 0), blockTile);
                }
            }

            else
            {
                tilemap.SetTile(new Vector3Int(origin.x /*- widthOffset*/, i, 0), blockTile);
                tilemap.SetTile(new Vector3Int(width-3 /*- widthOffset*/, i, 0), blockTile);
            }
        }
        //tilemap.SetTile(origin, defaultTile);
    }

    Command[] RandomCommands(int commandsNumber)
    {
        List<Command> commands = new List<Command>();
        commands.Add(Command.Devant);
        //Devant == 0
        //Droite == 1
        //Gauche == 2
        for (int i = 0; i < commandsNumber; i++)
        {
            int rand = Random.Range(0, 3);
            if(commands[commands.Count-1] == Command.Droite && (Command)rand == Command.Droite)
            {
                rand = 2;
            }
            else if(commands[commands.Count-1] == Command.Gauche && (Command)rand == Command.Gauche)
            {
                rand = 1;
            }
            commands.Add((Command)rand);
        }
        return commands.ToArray();
    }

    Direction RotateDirection(Command command, Direction direction)
    {
        Direction currentDirection = direction;
        if(command == Command.Droite)
        {
            switch (currentDirection)
            {
                case Direction.Haut:
                    return Direction.Droite;
                case Direction.Droite:
                    return Direction.Bas;
                case Direction.Bas:
                    return Direction.Gauche;
                case Direction.Gauche:
                    return Direction.Haut;
                default:
                    return 0;
            }
        }

        else if(command == Command.Gauche)
        {
            switch (currentDirection)
            {
                case Direction.Haut:
                    return Direction.Gauche;
                case Direction.Droite:
                    return Direction.Haut;
                case Direction.Bas:
                    return Direction.Droite;
                case Direction.Gauche:
                    return Direction.Bas;
                default:
                    return 0;
            }
        }

        else
        {
            return currentDirection;
        }
    }

    Direction GetRandomDirection()
    {
        int rand = Random.Range(0, 4);
        Direction direction = (Direction)rand;
        return direction;
    }

    Vector3Int ToVector3(Direction direction)
    {
        switch (direction)
        {
            case Direction.Haut:
                return new Vector3Int(0, 1, 0);

            case Direction.Droite:
                return new Vector3Int(1, 0, 0);

            case Direction.Bas:
                return new Vector3Int(0, -1, 0);

            case Direction.Gauche:
                return new Vector3Int(-1, 0, 0);

            default:
                return Vector3Int.zero;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(headPosition+new Vector3(0.5f, 0.5f), 0.3f);
    }

    [Button] 
    void ClearMap()
    {
        headPosition = Vector3Int.zero;
        tilemap.ClearAllTiles();
    }

    [Button]
    void Launch()
    {
        GeneratePath();
        GenerateCarrefour();
    }

    [Button]
    void LaunchSeries()
    {
        for (int i = 0; i < 100; i++)
        {
            GeneratePath();
        }      
    }
}
