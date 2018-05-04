using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

public class LevelGenerator : MonoBehaviour
{

    protected bool isSeedRandom = false;
    protected Transform root;
    protected string seed;
    protected System.Random rnd;
    public bool fromEditor;

    // PROGRESS BAR STUFF
    public bool isWorking = false;

    public void setIsRandomSeed(bool _s)
    {
        isSeedRandom = _s;
    }

    public void setSeed(System.Random _rng)
    {
        rnd = _rng;
    }

}

public class Hauberk : LevelGenerator
{
    private List<Room> _rooms; // list of placed rooms
    private List<Coord> allTileCoords;
    private List<Coord> savedTileCords; // for editor
    private int attempts;

    private bool rooms;
    public Transform tilePrefab;
    public Vector2 mapSize;

    [Range(0, 1)]
    public float outlinePercentage = 0;
    public bool useRandomSeed;

    public void GenerateLevel(Vector2 mSize, Transform map, Transform quadPrefab, float _op, bool fE, int tries)
    {
        // SETTING UP
		isWorking = true;
        fromEditor = fE;

        // SORTS OUT WHERE TO PLACE THE WALLS
        mapSize = mSize;
        tilePrefab = quadPrefab;
        outlinePercentage = _op;
        root = map;
        attempts = tries;
        allTileCoords = new List<Coord>();
    
        float step1cp = 0;
        float step1tp = mapSize.x * mapSize.y;
        for (int x = 0; x < mapSize.x; x++)
        {
            if(!isWorking){
                {
                    EditorUtility.ClearProgressBar();
                    allTileCoords = new List<Coord>();
                    break;
                }
            }

            for (int y = 0; y < mapSize.y; y++)
            {
                if(fromEditor){
                    if (EditorUtility.DisplayCancelableProgressBar("Creating Map", 
                        "Defining where quads are placed", 
                        (float)(step1cp / step1tp))
                    )
                    {
                        isWorking = false;
                    }
                }

                allTileCoords.Add(new Coord(x, y));
                step1cp += 1;
            }
        }

        if (rooms)
        {
            allTileCoords = addRooms(allTileCoords, attempts);
            savedTileCords = allTileCoords;
        }

        // DRAWS THE MAP TO THE SCENE
        Transform mapHolder = new GameObject("Generated Map").transform;
        mapHolder.parent = map;
        float step2cp = 0;
        float step2tp = mapSize.x * mapSize.y;
        for (int x = 0; x < mapSize.x; x++)
        {
            if(!isWorking){
                {
                    EditorUtility.ClearProgressBar();
                    allTileCoords = new List<Coord>();
                    break;
                }
            }

            for (int y = 0; y < mapSize.y; y++)
            {
                if(fromEditor){		
                    if (EditorUtility.DisplayCancelableProgressBar("Creating Map", 
                        "Drawing map to scence", 
                        (float)(step2cp / step2tp))
                    )
                    {
                        EditorUtility.ClearProgressBar();
                        allTileCoords = new List<Coord>();
                        break;
                    }
                }

				if (!isWorking)
				{
					allTileCoords = new List<Coord>();
					break;
				}

                if (!coordExists(allTileCoords, x, y))
                {
                    continue;
                }
                Vector3 tilePosition = coordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercentage);
                newTile.name = x + "_" + y;
                newTile.parent = mapHolder;
                step2cp += 1;
            }
        }

        EditorUtility.ClearProgressBar();
    }


    public void GenerateLevel(){ 
        // FOR EDITOR USAGE

        foreach (Transform child in root)
        {
            GameObject.DestroyImmediate(child.gameObject);
        }

        // SORTS OUT WHERE TO PLACE THE WALLS
        allTileCoords = new List<Coord>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }

        if (rooms)
        {
            allTileCoords = savedTileCords;
        }

        // DRAWS THE MAP TO THE SCENE

        Transform mapHolder = new GameObject("Generated Map").transform;
        mapHolder.parent = root;

        for (int x = 0; x < mapSize.x; x++)
        {

            for (int y = 0; y < mapSize.y; y++)
            {
                if (!coordExists(allTileCoords, x, y))
                {
                    continue;
                }
                Vector3 tilePosition = coordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercentage);
                newTile.name = x + "_" + y;
                newTile.parent = mapHolder;
            }
        }
    }

    public void addRooms(bool b)
    {
        rooms = b;
    }

    public List<Coord> addRooms(List<Coord> alTileCoords, int attempts)
    {
        _rooms = new List<Room>();
        for (var i = 0; i < attempts; i++)
        {
            Coord rndc = new Coord(rnd.Next(1, (int)mapSize.x), rnd.Next(1, (int)mapSize.y));
            Room room = new Room(rndc, rnd.Next(2, (int)mapSize.y), rnd.Next(2, (int)mapSize.x));
            var overlaps = false;
            if (_rooms.Count > 0)
            {
                foreach (Room other in _rooms)
                {
                    if (room.distanceTo(other) <= 2)
                    {
                        overlaps = true;
                    }
                }
            }

            if (overlaps)
            {
                // SKIPS ITERATION IF THERE'S AN OVERLAP
                continue;
            }

            // REMOVES COORDS SO SOME WALLS ARENT CREATED IN THE SCENE
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    if (room.coordExists(x, y))
                    {
                        Coord toRemove = room.getCoordAt(x, y);
                        alTileCoords.Remove(toRemove);
                    }
                }

            }

            _rooms.Add(room);
        }

        return alTileCoords;
    }

    public bool coordExists(List<Coord> allTileCoords, int _x, int _y)
    {
        for (int i = 0; i < allTileCoords.Count; i++)
        {
            if (allTileCoords[i].x == _x & allTileCoords[i].y == _y)
            {
                return true;
            }

        }
        return false;
    }

    Vector3 coordToPosition(int x, int y)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
    }
}

public class Cave : LevelGenerator
{
    public int width, height;
    int[,] map;

    [Range(0, 100)]
    public int randomFillPercent;

    public void GenerateLevel(Transform root, int _rfpc, int _w, int _h)
    {
        // SETTING UP PROGRESSBAR STUFF
        /*currProgress = 0;
        totalProgress = height * width;
        onePc = totalProgress / 100;/* */

        randomFillPercent = _rfpc;
        width = _w;
        height = _h;

        map = new int[width, height]; // 0 = empty, 1 occupied by wall

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = (rnd.Next(0, 100) < randomFillPercent) ? 1 : 0;
            }
        }

        // DRAWING MAP
        Transform mapHolder = new GameObject("Generated Level").transform;
        mapHolder.parent = root;

        // imcomplete

    }

    public void GenerateLevel()
    { // FOR EDITOR USAGE

        map = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = (rnd.Next(0, 100) < randomFillPercent) ? 1 : 0;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (map != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }

}

public struct Coord
{
    public int x;
    public int y;

    public Coord(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
}
