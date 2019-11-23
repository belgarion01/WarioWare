using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class TilemapGenerator : MonoBehaviour
{
    //Tilemap & Tiles
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileTypes tileTypes;

    //Commands
    public enum Command { Devant, Droite, Gauche };
    [SerializeField] private int numberOfCommands = 4;
    [SerializeField] bool randomCommands = true;
    public Command[] commands;

    //Directions
    public enum Direction { Haut, Droite, Bas, Gauche};

    //Head
    private Vector3Int headPosition;
    private Direction headDirection;

    //Crossroads position list
    private List<Vector3Int> crossroadPositions = new List<Vector3Int>();

    //Number of unnecessary CorrectPath Tiles
    [HideInInspector]public int stepsToRemove;

    //Longueur maximum d'un chemin secondaire
    const int secondPathMaxLength = 8;

    //
    public MementoSavedData savedData;

    public UnityEvent OnMapGenerated;

    private void Awake()
    {
        GenerateMap();
    }

    //Génère une map
    [Button]
    void GenerateMap()
    {
        //Génère une chemin réussi
        GeneratePath();

        //Génère les chemins secondaires
        GenerateSecondPaths();

        //Remplie les cases vises par des Cross
        FillEmptyTiles();

        //Convertie les tiles pour le gameplay
        ConvertTiles();

        //Vérifie une dernière fois qu'il n'y a pas d'erreurs (des carrés)
        CheckForSquare();

        //Si tout ça est bon alors on sauvegarde tout ça
        StartCoroutine(SaveData());
    }

    public void GeneratePath()
    {
        //Reset path settings
        ClearMap();
        GenerateWall();
        crossroadPositions.Clear();
        stepsToRemove = 0;

        //Set the head
        tilemap.SetTile(headPosition, tileTypes.MainPath);
        headDirection = Direction.Droite;

        //Set des Commandes aléatoire
        if(randomCommands)commands = RandomCommands(numberOfCommands);

        //Début du long chemin (plusieurs courts chemins)
        for (int i = 0; i < commands.Length; i++)
        {
            //Detect if this loop is the last command
            bool lastCommand = i == commands.Length - 1 ? true : false;

            //If last command, count the number of steps
            //if (lastCommand) stepsToRemove++;

            //Nombre aléatoire de pas pour le court chemin
            int randomSteps;
            if (i == 0) randomSteps = 2;
            else randomSteps = Random.Range(2, 4);

            //Tourne la direction de la head en fonction de la Commande
            headDirection = RotateDirection(commands[i], headDirection);
           
            //Commence les petits pas (plusieurs points) (intermédiaire entre le début et la fin du court chemin)
            for (int j = 0; j < randomSteps; j++)
            {
                if (lastCommand) stepsToRemove++;
                //Détecte si le jeu peut être fini
                if (i == commands.Length - 1 && j == 1)
                {
                    //Si la boucle est arrivée jusque là c'est que le Generator a trouvé un chemin qui peut être gagné !
                }

                //Tente une première direction
                Vector3Int nextPosition = headPosition + ToVector3(headDirection);

                //Change la direction si elle est bloquée avec la commande donnée
                if (tilemap.GetTile(nextPosition) == tileTypes.Cross)
                {
                    //Return true si un chemin alternatif est trouvé. Vérifie aussi que c'est bien le pas initial qui va être changé
                    if (TryOtherCommands(commands[i], headPosition, i) && j == 0)
                    {
                        nextPosition = headPosition + ToVector3(headDirection);
                    }

                    //Si aucun chemin n'est trouvé ou que le chemin se mord la queue, reload la map
                    else
                    {
                        //Cul de sac ou se mord la queue --> Reload
                        GeneratePath();                       
                        return;
                    }                  
                }

                //A ce moment de la loop, la head n'a pas trouvé d'obstacle à son avancé


                //Pose une MainTile à sa prochaine position
                tilemap.SetTile(nextPosition, tileTypes.MainPath);

                //Pose les blockTiles
                for (int k = 0; k < 4; k++)
                {
                    Vector3Int positionToCheck = headPosition + ToVector3((Direction)k);
                    if(positionToCheck != nextPosition && isEmptyTile(positionToCheck))
                    {
                        tilemap.SetTile(positionToCheck, tileTypes.Cross);
                    }
                }

                //Avance la tête à la position prédit
                headPosition = nextPosition;

                //Fin d'un petit pas
            }

            //Fin d'un court chemin

            //Ajoute la position final du court chemin comme carrefour
            crossroadPositions.Add(headPosition);
            //tilemap.SetTile(headPosition, tileTypes.MainPath);
        }
        //Fin du long chemin (Path)
    }

    //Les CarrefourPath agissent comme une Head. Ils partent d'un carrefour et s'arrète au moindre obstacle. Un peu comme si on envoyait des éclaireurs sucidaires dans tous les coins. Ptin jsp moi.
    public class CarrefourPath
    {
        //Indique sa direction
        public Direction direction;

        //Indique sa position
        public Vector3Int position;

        //Indique s'il s'est stoppé
        public bool stopped;

        public CarrefourPath(Direction direction, Vector3Int position)
        {
            this.direction = direction;
            this.position = position;
            stopped = false;
        }
    }

    void GenerateSecondPaths()
    {
        List<CarrefourPath> possiblePaths = new List<CarrefourPath>();

        //Pour chaque carrefour créé...
        for (int i = 0; i < crossroadPositions.Count; i++)
        {
            //Pour chaque Direction...
            for (int j = 0; j < 4; j++)
            {
                //Vérifie quelles directions sont possibles
                Vector3Int possiblePosition = crossroadPositions[i] + ToVector3((Direction)j);
                if(tilemap.GetTile(possiblePosition)!= tileTypes.MainPath)
                {
                    //Si oui, créer un CarrefourPath dans cette direction et position
                    possiblePaths.Add(new CarrefourPath((Direction)j, crossroadPositions[i]));
                }
            }
        }

        //Créer des avenues pour X tiles...
        for (int i = 0; i < secondPathMaxLength; i++)
        {
            //Pour chaque CarrefourPath créé...
            for (int j = 0; j < possiblePaths.Count; j++)
            {
                //Si le CarrefourPath a été stoppé, ignore le
                if (possiblePaths[j].stopped) continue;

                //Get la prochaine position
                Vector3Int nextPosition = possiblePaths[j].position + ToVector3(possiblePaths[j].direction);

                //Alors là jsp lol, je le laisse au cas où j'ai oublié une intention de Elli. 
                if (tilemap.GetTile(nextPosition) == tileTypes.Crossroad)
                {
                    possiblePaths[j].stopped = true;
                    continue;
                }

                //On va check si on peut avancer un maximum sans créer de carré ni de carrefour invoulu avec les autres SecondPath

                //Check si y'a deja une tile a côté de la prochaine position verticalement
                if (possiblePaths[j].direction == Direction.Haut || possiblePaths[j].direction == Direction.Bas)
                {
                    //Get le type de tile à droite et à gauche de sa prochaine position
                    TileBase typeRight = tilemap.GetTile(nextPosition + ToVector3(Direction.Droite));
                    TileBase typeLeft = tilemap.GetTile(nextPosition + ToVector3(Direction.Gauche));

                    //S'il y a un SecondPath...
                    if (typeRight == tileTypes.SecondPath || typeLeft == tileTypes.SecondPath)
                    {
                        //Get le type de tile à sa gauche et sa droite
                        typeRight = tilemap.GetTile(possiblePaths[j].position + ToVector3(Direction.Droite));
                        typeLeft = tilemap.GetTile(possiblePaths[j].position + ToVector3(Direction.Gauche));

                        //S'il y a encore un SecondPath...
                        if (typeRight == tileTypes.SecondPath || typeLeft == tileTypes.SecondPath)
                        {
                            //Le chemin formerait un carré --> Stop le CarrefourPath
                            possiblePaths[j].stopped = true;
                            continue;
                        }

                        //S'il n'y a pas de SecondPath...
                        else
                        {
                            //Get le type à droite et à gauche de sa 2ème prochaine destination
                            typeRight = tilemap.GetTile(possiblePaths[j].position + ToVector3(possiblePaths[j].direction) * 2 + ToVector3(Direction.Droite));
                            typeLeft = tilemap.GetTile(possiblePaths[j].position + ToVector3(possiblePaths[j].direction) * 2 + ToVector3(Direction.Gauche));
                            //Si la case est vide ou Crossed...
                            if ((typeRight == null || typeRight == tileTypes.Cross) || (typeLeft == null || typeLeft == tileTypes.Cross))
                            {
                                //Se permet de créer un dernier SecondPath --> Cela va créer une ruelle
                                possiblePaths[j].stopped = true;
                            }
                            else
                            {
                                //Sinon arrète toi ou tu va créer un carré ou un carrefour invoulu
                                possiblePaths[j].stopped = true;
                                continue;
                            }                         
                        }
                    }
                }

                //Même chose que plus haut mais si cette fois-ci si la direction est horizontale
                else
                {
                    TileBase typeUp = tilemap.GetTile(nextPosition + ToVector3(Direction.Haut));
                    TileBase typeDown = tilemap.GetTile(nextPosition + ToVector3(Direction.Bas));

                    if (typeUp == tileTypes.SecondPath || typeDown == tileTypes.SecondPath)
                    {
                        typeUp = tilemap.GetTile(possiblePaths[j].position + ToVector3(Direction.Haut));
                        typeDown = tilemap.GetTile(possiblePaths[j].position + ToVector3(Direction.Bas));

                        if (typeUp == tileTypes.SecondPath || typeDown == tileTypes.SecondPath)
                        {
                            possiblePaths[j].stopped = true;
                            continue;
                        }

                        else
                        {
                            typeUp = tilemap.GetTile(possiblePaths[j].position + ToVector3(possiblePaths[j].direction) * 2 + ToVector3(Direction.Haut));
                            typeDown = tilemap.GetTile(possiblePaths[j].position + ToVector3(possiblePaths[j].direction) * 2 + ToVector3(Direction.Bas));

                            if ((typeUp == null || typeUp == tileTypes.Cross) || (typeDown == null || typeDown == tileTypes.Cross))
                            {
                                possiblePaths[j].stopped = true;
                            }
                            else
                            {
                                possiblePaths[j].stopped = true;
                                continue;
                            }
                        }
                    }
                }

                //Check si la position voulue fait partie du MainPath
                if (isTileType(nextPosition, tileTypes.MainPath))
                {
                    //Si oui, stop le CarrefourPath
                    possiblePaths[j].stopped = true;
                    continue;
                }
                else
                {
                    //Sinon place un SecondPath et avance le CarrefourPath
                    tilemap.SetTile(nextPosition, tileTypes.SecondPath);
                    possiblePaths[j].position = nextPosition;
                }
            }
        }
    }

    //Converti les tiles placées pour la génération en tiles utiles pour le Gameplay. Cela permet de diviser les 2 systèmes.
    public void ConvertTiles()
    {
        tilemap.SwapTile(tileTypes.Crossroad, tileTypes.MainPath);
        tilemap.SwapTile(tileTypes.MainPath, tileTypes.CorrectPath);
        tilemap.SwapTile(tileTypes.SecondPath, tileTypes.WrongPath);
        tilemap.SwapTile(tileTypes.Cross, tileTypes.Building);
    }

    //Remplie les cases vides de la grille par des tiles Cross
    public void FillEmptyTiles()
    {
        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (isEmptyTile(pos))
            {
                tilemap.SetTile(pos, tileTypes.Cross);
            }
        }
    }

    //Vérifie bien que aucun carré de chemins n'existe. S'il en existe Reload
    public void CheckForSquare()
    {
        //Pour chaque tile dans la grille...
        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            bool positionOk = false;
            List<TileBase> tiles = new List<TileBase>();

            //Get les tiles dans un carré 4x4 sur la position x y
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    tiles.Add(tilemap.GetTile(new Vector3Int(pos.x + i, pos.y + j, 0)));
                }
            }

            //Pour chaque tiles dans le carré...
            foreach (TileBase tile in tiles)
            {
                //Si une des cases n'est pas un chemin...
                if(tile != tileTypes.CorrectPath && tile != tileTypes.WrongPath && tile)
                {
                    //Alors il n'y a pas de carré et la position est OK
                    positionOk = true;
                }
            }

            //S'il y a une position avec un carré, Reload
            if (!positionOk)
            {
                GenerateMap();
                return;
            }
        }
    }

    //FAIRE LE PAR TYPE

    bool isTileType<T>(Vector3Int position, T tileType) where T : TileBase
    {
        if (tileType == tilemap.GetTile(position))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool isDefaultTile(Vector3Int position)
    {
        TileBase type = tilemap.GetTile(position);
        return type == tileTypes.MainPath ? true : false;
    }

    //Check si la tile est vide
    bool isEmptyTile(Vector3Int position)
    {
        TileBase type = tilemap.GetTile(position);
        return type == null ? true : false;
    }

    //Return true si il existe un chemin alternatif à la commande indiquée
    bool TryOtherCommands(Command blockedCommand, Vector3Int position, int commandToChangeIndex)
    {
        List<Command> commands = new List<Command>();

        //Remove la commande bloquée de la liste
        for (int i = 0; i < 3; i++)
        {
            if (i != (int)blockedCommand)
            {
                commands.Add((Command)i);
            }
        }

        List<Command> successfulCommands = new List<Command>();

        //Créer une copie de headDirection
        Direction testDirection = headDirection;

        //Je redirige la tête comme si elle allait avancé devant. Cela permet de vérifier toute les directions sans vérifier le chemin que l'on vient d'emprunter.
        testDirection = RotateDirection(GetInvertCommand(blockedCommand), testDirection);

        //Pour chaque commande possible...
        for (int i = 0; i < commands.Count; i++)
        {
            //Vérifie si la position potentiel est vide
            Vector3Int potentialPosition = headPosition + ToVector3(RotateDirection(commands[i], testDirection));
            if (isEmptyTile(potentialPosition))
            {
                //Si oui, l'ajoute aux possibles chemins alternatifs
                successfulCommands.Add(commands[i]);
            }
        }

        //Si on a trouvé des chemins alternatifs...
        if (successfulCommands.Count > 0)
        {
            //Rotate la head selon une commande réussie aléatoire
            int rand = Random.Range(0, successfulCommands.Count);
            headDirection = RotateDirection(successfulCommands[rand], testDirection);

            //Change la commande modifiée dans la liste pré-générée.
            this.commands[commandToChangeIndex] = successfulCommands[rand];            
            return true;
        }
        else
        {
            //Sinon return false
            return false;
        }
    }

    //Get la commande inverse (c'est pour reset la position de la head)
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

    //Génère des murs de tiles Cross
    void GenerateWall()
    {
        Vector3Int origin = new Vector3Int(-1, -5, 0);

        const int width = 17;
        const int height = 10;

        const int widthOffset = 3;
        const int heightOffset = 5;

        for (int i = origin.y; i < height+origin.y; i++)
        {
            if(i == origin.y || i == height+origin.y-1)
            {
                for (int j = origin.x; j < width+origin.x-1; j++)
                {
                    tilemap.SetTile(new Vector3Int(j, i, 0), tileTypes.Cross);
                }
            }

            else
            {
                tilemap.SetTile(new Vector3Int(origin.x, i, 0), tileTypes.Cross);
                tilemap.SetTile(new Vector3Int(width - widthOffset, i, 0), tileTypes.Cross);
            }
        }
    }

    //Get une liste de Commandes aléatoires
    Command[] RandomCommands(int numberOfCommands)
    {
        List<Command> commands = new List<Command>();
        //Ajoute la commande de base
        commands.Add(Command.Devant);

        //Devant == 0
        //Droite == 1
        //Gauche == 2
        for (int i = 0; i < numberOfCommands; i++)
        {
            //Perturbe l'aléatoire pour éviter d'avoir des spirales et que le chemin entre mieux dans le niveau horizontalement
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

    //Rotate une Direction selon une Commande de 90 degré
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

    //Get une Direction aléatoire
    Direction GetRandomDirection()
    {
        int rand = Random.Range(0, 4);
        Direction direction = (Direction)rand;
        return direction;
    }

    //Traduit une Direction comme si elle était un Vector3Int normalisé
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

    IEnumerator SaveData()
    {
        while (GameTiles.instance == null) yield return new WaitForEndOfFrame();
        var _tiles = GameTiles.instance.tiles;
        List<TileData> tilesData = new List<TileData>();
        TileData _data;
        savedData.numberOfCorrectPaths = 0;

        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            //Sauvegarde l'emplacement des tiles
            if (_tiles.TryGetValue(pos, out _data))
            {
                tilesData.Add(_data);
                if (_data.type == tileTypes.CorrectPath)
                {
                    savedData.numberOfCorrectPaths++;
                }
            }
        }

        savedData.tilesData = tilesData;
        savedData.numberOfCorrectPaths -= stepsToRemove - 1;
        savedData.commands = commands;
        yield return null;
    }

    //Reset la tilemap
    void ClearMap()
    {
        headPosition = Vector3Int.zero;
        tilemap.ClearAllTiles();
    }



    //Génère une map plusieurs fois (permet de diagnostiquer la stabilité de l'algorithme)
    void GenerateMapSeries()
    {
        for (int i = 0; i < 100; i++)
        {
            GenerateMap();
        }      
    }
}
