using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BFS : MonoBehaviour
{
    [SerializeField] private GridGenerator gridData;
    [SerializeField] TerrainGeneration TerrainData;
    private Queue<Vector3> queue;
    private HashSet<Vector3> visited;
    private Dictionary<Vector3, Vector3> cellParents;

    private void Start()
    {
        queue = new Queue<Vector3>();
        visited = new HashSet<Vector3>();
        cellParents = new Dictionary<Vector3, Vector3>();
    }

    public void Search()
    {
        ClearData();

        queue.Enqueue(gridData.startPosition);
        visited.Add(gridData.startPosition);

        while(queue.Count > 0)
        {
            Vector3 currentCell = queue.Dequeue();

            if (currentCell == gridData.endPosition)
            {
                gridData.VisualizePath(cellParents);
                return;
            }

            List<Vector3> neighbours = gridData.GetNeighbours(currentCell);
            foreach (Vector3 neighbour in neighbours)
            {
                if (!visited.Contains(neighbour))
                {
                    queue.Enqueue(neighbour);
                    visited.Add(neighbour);
                    cellParents[neighbour] = currentCell;
                }
            }
        }
    }

    private void ClearData()
    {
        queue.Clear();
        visited.Clear();
        cellParents.Clear();
    }

}
