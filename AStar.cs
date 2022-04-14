using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStar : MonoBehaviour
{
    [SerializeField] private GridGenerator gridData;
    private SimplePriorityQueue<Vector3, int> priorityQueue;
    private HashSet<Vector3> visited;
    private Dictionary<Vector3, Vector3> cellParents;

    private void Start()
    {
        priorityQueue = new SimplePriorityQueue<Vector3, int>();
        visited = new HashSet<Vector3>();
        cellParents = new Dictionary<Vector3, Vector3>();
    }

    public void Search()
    {
        ClearData();

       Dictionary<Vector3, int> distance = gridData.walkableCells.ToDictionary(Matrix4x4 => Matrix4x4, Matrix4x4 => int.MaxValue);
       distance[gridData.startPosition] = 0;

       priorityQueue.Enqueue(gridData.startPosition, 0);
        visited.Add(gridData.startPosition);

        while (priorityQueue.Count > 0)
        {
            Vector3 currentCell = priorityQueue.Dequeue();

            if (currentCell == gridData.endPosition)
            {
                Debug.Log("Destination reached!");
                gridData.VisualizePath(cellParents);
                return;
            }

            List<Vector3> neighbours = gridData.GetNeighbours(currentCell);
            foreach (Vector3 neighbour in neighbours)
            {
                if (!visited.Contains(neighbour))
                {
                    int dist = distance[currentCell] + 1;

                    if (dist < distance[neighbour])
                    {
                        distance[neighbour] = dist;
                        int fScore = distance[neighbour] + DistanceEstimate(neighbour);
                        priorityQueue.Enqueue(neighbour, fScore);
                        visited.Add(neighbour);
                        cellParents[neighbour] = currentCell;
                    }
                }
            }
        }
    }

    private void ClearData()
    {
        priorityQueue.Clear();
        visited.Clear();
        cellParents.Clear();
    }

    private int DistanceEstimate(Vector3 node)
    {
        float x = Mathf.Pow(node.x - gridData.endPosition.x, 2);
        float y = Mathf.Pow(node.y - gridData.endPosition.y, 2);
        float z = Mathf.Pow(node.z - gridData.endPosition.z, 2);

        return (int)Mathf.Sqrt(x + y + z);
    }
}
