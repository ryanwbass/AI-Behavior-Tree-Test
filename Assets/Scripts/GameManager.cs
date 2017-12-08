using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour {

    public TileBase[] tiles;
    public GameObject grid;
    public GameObject pedestrianPrefab;
    private GameObject pedestrian;
    public GameObject actorPrefab;
    private GameObject actor;

    public bool actorIsAlive = false;
    public int numOfPedestrians;
    public List<GameObject> pedestrians;
    public List<Vector2Int> shops;

    private Tilemap collisionLayer;
    private Tilemap floorLayer;
    private Tilemap detailsLayer;
    
    // create the tiles map
    int width = 51;
    int height = 21;
    public bool[,] walkableMap;
    




    public static GameManager instance = null;

    void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        InitGame();
    }

    private void InitGame()
    {
        grid = GameObject.Find("Grid");
        floorLayer = grid.transform.Find("Floor").GetComponent<Tilemap>();
        collisionLayer = grid.transform.Find("Collisions").GetComponent<Tilemap>();
        detailsLayer = grid.transform.Find("Details").GetComponent<Tilemap>();

        walkableMap = new bool[width, height];
        shops = new List<Vector2Int>();
        GenerateMap();

        pedestrians = new List<GameObject>();
    }

    private void Update()
    {
        if (!actorIsAlive)
        {
            SpawnActor();
        }

        if (pedestrians.Count < numOfPedestrians)
        {
            int pedestrainSpawnProb = Random.Range(0, 50);
            if(pedestrainSpawnProb == 0)
            {
                SpawnPedestrian();
            }
        }
    }


    private void SpawnActor()
    {
        int spawnShop = Random.Range(0, shops.Count);
        actor = (GameObject)Instantiate(actorPrefab, new Vector2(shops[spawnShop].x, shops[spawnShop].y), Quaternion.identity);
        actorIsAlive = true;
    }

    private void SpawnPedestrian()
    {
        int randLane = Random.Range(0, 4);
        int randPosInLane = Random.Range(0, 2);
        int startY = 5 + randLane * 3 + randPosInLane;
        GameObject newPed = (GameObject)Instantiate(pedestrianPrefab, new Vector2(0, startY), Quaternion.identity);
        newPed.GetComponent<PedestrianController>().Init(randLane, randPosInLane);
        pedestrians.Add(newPed);
    }

    private void GenerateMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                walkableMap[x, y] = true;
                
                floorLayer.SetTile(new Vector3Int(x, y, 0), tiles[0]);
                
                int distFromCenterY = Mathf.Abs(y - height / 2);
                int distFromCenterX = Mathf.Abs(x - width / 2);

                if (distFromCenterY == 0 || distFromCenterY == 3)
                {
                    walkableMap[x, y] = true;
                    detailsLayer.SetTile(new Vector3Int(x, y, 0), tiles[6]);
                }

                if (distFromCenterX % 6 < 3 && x < 23)
                {
                    if (distFromCenterY >= 6 && distFromCenterY <= 10)
                    {
                        walkableMap[x, y] = false;
                        collisionLayer.SetTile(new Vector3Int(x, y, 0), tiles[7]);
                    }
                }
                else if (x >= 23)
                {
                    if (distFromCenterY >= 6 && distFromCenterY <= 10)
                    {
                        walkableMap[x, y] = false;
                        collisionLayer.SetTile(new Vector3Int(x, y, 0), tiles[7]);
                    }
                }
                if (distFromCenterY == 10)
                {
                    walkableMap[x, y] = false;
                    collisionLayer.SetTile(new Vector3Int(x, y, 0), tiles[7]);
                }

                if (x == 0 && distFromCenterY < 6)
                {
                    walkableMap[x, y] = true;
                    detailsLayer.SetTile(new Vector3Int(x, y, 0), tiles[1]);
                }

                if (x == width - 1 && distFromCenterY < 6)
                {
                    walkableMap[x, y] = true;
                    detailsLayer.SetTile(new Vector3Int(x, y, 0), tiles[2]);
                }

                if (distFromCenterY == 9 && (x == 3 || x == 9 || x == 15 || x == 21))
                {
                    walkableMap[x, y] = true;
                    detailsLayer.SetTile(new Vector3Int(x, y, 0), tiles[3]);
                    shops.Add(new Vector2Int(x, y));
                }
            }
        }
    }
}
