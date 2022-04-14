using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private int width = 20;
    [SerializeField] private int depth = 20;
    [SerializeField] private int numberOfObstacles = 15;
    [SerializeField] private GameObject obstablePrefab;
    [SerializeField] private GameObject player;
    [SerializeField] private HashSet<Vector3> obstacles;

    public HashSet<Vector3> walkableCells;

    private bool shouldPlayerMove = false;
    private List<Vector3> playerPath;
    private GameObject playerInstance;
    private int pathIndex = 0;
    private TerrainGeneration TR;

    [HideInInspector]
    public Vector3 startPosition;
    public GameObject destination;
    public Vector3 endPosition;
    public Transform ground;

    [Header("Visualized Path")]
    public Transform pathCells;
    public GameObject pathPrefab;


    private void Start()
    {
        TR = GetComponent<TerrainGeneration>();
        obstacles = new HashSet<Vector3>();
        walkableCells = new HashSet<Vector3>();
        width = TR.width;
        depth = TR.depth;

        GenerateGrid();
    }

    private void Update()
    {
        if (shouldPlayerMove)
        {
            var nextCellToVisit = playerPath[pathIndex];
            //Debug.Log("Player move in Update: " + nextCellToVisit);


            playerInstance.transform.position = Vector3.MoveTowards(playerInstance.transform.position, nextCellToVisit, 10 * Time.deltaTime);
            playerInstance.transform.LookAt(nextCellToVisit);

            if (playerInstance.transform.position == nextCellToVisit)
                pathIndex--;

            if (pathIndex < 0)
            {
                shouldPlayerMove = false;
                playerPath.Clear();
            }
        }
    }

    public void GenerateGrid()
    {
        ClearData();
        ClearPath();

        ground.position = new Vector3(width / 2f, 0, depth / 2f);
        ground.localScale = new Vector3(width / 10f, 1, depth / 10f);
        Camera.main.transform.position = new Vector3(ground.position.x, 5f * (width / 10f) + (width / 10f), ground.position.z - depth / 2f - depth / 4f - (depth / 10f));

        PlaceObstacles();
        startPosition = PlaceObject(player);
        endPosition = PlaceObject(destination);

        LocateWalkableCells();
    }

    public void ClearData()
    {
        DeleteChildren(transform);
        obstacles.Clear();
        walkableCells.Clear();
    }
    private void ClearPath()
    {
        foreach (Transform pathCell in pathCells)
        {
            Destroy(pathCells.gameObject);
        }
    }

    private void DeleteChildren(Transform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }

    private void PlaceObstacles()
    {
        int obstaclesToGenerate = numberOfObstacles;

        while (obstaclesToGenerate > 0)
        {
            int positionX = UnityEngine.Random.Range(1, width);
            int positionZ = UnityEngine.Random.Range(1, depth);

            Vector3 cellPosition = new Vector3(positionX, 0, positionZ);

            if (!IsCellOccupied(cellPosition))
            {
                obstacles.Add(cellPosition);
                Vector3 objectPosition = cellPosition;
                objectPosition.y = obstablePrefab.transform.position.y;

                Instantiate(obstablePrefab, objectPosition, Quaternion.identity, transform);
                obstaclesToGenerate--;
            }
        }
    }

    private Vector3 PlaceObject(GameObject gameObjectToPlace)
    {
        while (true)
        {
            int positionX = UnityEngine.Random.Range(1, width);
            int positionZ = UnityEngine.Random.Range(1, depth);

            Vector3 cellPosition = new Vector3(positionX, 0, positionZ);

            if (!IsCellOccupied(cellPosition))
            {
                Vector3 objectPosition = cellPosition;
                objectPosition.y = gameObjectToPlace.transform.position.y;

                if (gameObjectToPlace.name == "Player")
                    playerInstance = Instantiate(gameObjectToPlace, objectPosition, Quaternion.identity, transform);
                else
                    Instantiate(gameObjectToPlace, objectPosition, Quaternion.identity, transform);

                return cellPosition;
            }
        }
    }

    private bool IsCellOccupied(Vector3 position)
    {
        if (obstacles.Contains(position))
            return true;

        return false;
    }

    private void LocateWalkableCells()
    {
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!IsCellOccupied(new Vector3(x, 0, z)))
                    walkableCells.Add(new Vector3(x, 0, z));
            }
        }
    }

    public List<Vector3> GetNeighbours(Vector3 currentCell)
    {
        List<Vector3> neighbours = new List<Vector3>() ///
        {
            new Vector3(currentCell.x - 1, 0, currentCell.z),
            new Vector3(currentCell.x + 1, 0, currentCell.z),
            new Vector3(currentCell.x, 0, currentCell.z - 1),
            new Vector3(currentCell.x, 0, currentCell.z + 1),
        };

        List<Vector3> walkableNeighbours = new List<Vector3>();
        foreach (Vector3 neighbour in neighbours)
        {
            if (!IsCellOccupied(neighbour) && IsInLevelBounds(neighbour))
                walkableNeighbours.Add(neighbour);
        }

        return walkableNeighbours;
    }

    private bool IsInLevelBounds(Vector3 neighbour)
    {
        if (neighbour.x > 0 && neighbour.x <= width - 1 && neighbour.z > 0 && neighbour.z <= depth - 1)
            return true;

        return false;
    }

    public void VisualizePath(Dictionary<Vector3, Vector3> cellParents)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3 current = cellParents[endPosition];

        path.Add(endPosition);

        while(current != startPosition)
        {
            path.Add(current);
            current = cellParents[current];
        }

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 pathCellPosition = path[i];
            pathCellPosition.y = pathPrefab.transform.position.y;
            Instantiate(pathPrefab, pathCellPosition, Quaternion.identity, pathCells);
        }

        MovePlayer(path);
    }

    public void MovePlayer(List<Vector3> path)
    {
        shouldPlayerMove = true;
        playerPath = path;
        pathIndex = playerPath.Count - 1;
    }
}

