using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DFS : MonoBehaviour
{
    [SerializeField] private GridGenerator gridData;
    private Stack<Vector3> stack;
    private HashSet<Vector3> visited;
    private Dictionary<Vector3, Vector3> cellParents;

    private void Start()
    {
        stack = new Stack<Vector3>();
        visited = new HashSet<Vector3>();
        cellParents = new Dictionary<Vector3, Vector3>();
    }

    public void Search()
    {
        ClearData();

        stack.Push(gridData.startPosition);
        visited.Add(gridData.startPosition);

        while (stack.Count > 0)
        {
            Vector3 currentCell = stack.Pop();

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
                    stack.Push(neighbour);
                    visited.Add(neighbour);
                    cellParents[neighbour] = currentCell;
                }
            }
        }
    }

    private void ClearData()
    {
        stack.Clear();
        visited.Clear();
        cellParents.Clear();
    }
}
