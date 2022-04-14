using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KMeans : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int depth;
    [SerializeField] private int points = 40;
    [SerializeField] private int centers = 3;
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private GameObject centerPrefab;
    [SerializeField] private Transform pointHolder;
    [SerializeField] private Transform centerHolder;
    [SerializeField] private List<GameObject> assetPrefab;

    private List<GameObject> pointsObjects;
    private List<GameObject> centersObjects;
    private List<Color> colours;
    private List<Vector3> previousCenters;
    private Dictionary<GameObject, List<GameObject>> clusters;
    private TerrainGeneration terrainData;
    


    void Start()
    {
        StartClustering();
        terrainData = GetComponent<TerrainGeneration>();
        width = terrainData.width;
        depth = terrainData.depth;
    }

    public void StartClustering()
    {
        ClearData();
        pointsObjects = GenerateGameObjects(pointPrefab, points, pointHolder);
        centersObjects = GenerateGameObjects(centerPrefab, centers, centerHolder);
        previousCenters = GetCentersList();
        colours = GenerateColours();
        SetColoursToCenters();
        Cluster();
    }

    private void ClearData()
    {
        DeleteChildren(pointHolder);
    }

    private void DeleteChildren(Transform parent)
    {
        foreach (Transform item in parent)
            Destroy(item.gameObject);
    }

    private List<GameObject> GenerateGameObjects(GameObject prefab, int size, Transform parent)
    {
        List<GameObject> result = new List<GameObject>();
        int indexPosition = 0;

        for (int i = 0; i < size; i++)
        {
            prefab = assetPrefab[indexPosition];

            float xScale = prefab.transform.localScale.x;
            float xPostion = width/2 + UnityEngine.Random.Range(-width / 2 + xScale, width / 2 - xScale);

            float zScale = prefab.transform.localScale.z;
            float zPostion = depth/2 + UnityEngine.Random.Range(-depth / 2 + zScale, depth / 2 - zScale);

            Vector3 newPosition = new Vector3(xPostion, prefab.transform.position.y, zPostion);
            GameObject newGameObject = Instantiate(prefab, newPosition, Quaternion.identity, parent);

            result.Add(newGameObject);

            indexPosition++;
            if (indexPosition == 6)
                indexPosition = 0;
        }
        return result;
    }

    private List<Vector3> GetCentersList()
    {
        List<Vector3> result = new List<Vector3>();

        foreach (GameObject item in centersObjects)
        {
            item.AddComponent<BoxCollider>();
            item.AddComponent<OnTouched>();
            result.Add(item.transform.position);
        }
            

        return result;
    }

    private List<Color> GenerateColours()
    {
        List<Color> result = new List<Color>();

        for (int i = 0; i < centers; i++)
        {
            Color colour = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            result.Add(colour);
        }
        return result;
    }

    private void SetColoursToCenters()
    {
        for (int i = 0; i < centersObjects.Count; i++)
            centersObjects[i].GetComponent<MeshRenderer>().material.color = colours[i];
    }

    public void Cluster()
    {
        clusters = InitializeClusters();
        AddPointsToClustes();
        CheckForEmptyClustes();
        SetColoursToClusterPoints();
        RecalculateCenterPositions();
        CheckForEnd();
        UpdatePreviousCenters();
    }

    private Dictionary<GameObject, List<GameObject>> InitializeClusters()
    {
        Dictionary<GameObject, List<GameObject>> result = new Dictionary<GameObject, List<GameObject>>();

        for (int i = 0; i < centers; i++)
            result.Add(centersObjects[i], new List<GameObject>());

        return result;
    }

    private void AddPointsToClustes()
    {
        for (int i = 0; i < points; i++)
        {
            Vector3 pointPosition = pointsObjects[i].transform.position;
            float minDistance = float.MaxValue;
            GameObject closestCenter = null;

            for (int j = 0; j < centers; j++)
            {
                var distance = Vector3.Distance(pointPosition, centersObjects[j].transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCenter = centersObjects[j];
                }
            }

            clusters[closestCenter].Add(pointsObjects[i]);
        }
    }

    private void CheckForEmptyClustes()
    {
        foreach (var cluster in clusters)
        {
            if (cluster.Value.Count == 0)
            {
                GameObject closestpoint = ExtractClosestPointToCluster(cluster.Key.transform.position);
                cluster.Value.Add(closestpoint);
            }
        }
    }

    private GameObject ExtractClosestPointToCluster(Vector3 clusterPosition)
    {
        GameObject closestPoint = pointsObjects[0];
        GameObject clusterThePointBelongsTo = null;
        float minDistance = float.MaxValue;

        foreach (var cluster in clusters)
        {
            foreach (GameObject point in cluster.Value)
            {
                var distance = Vector3.Distance(point.transform.position, clusterPosition);
                if (distance < minDistance && cluster.Value.Count > 1)
                {
                    closestPoint = point;
                    minDistance = distance;
                    clusterThePointBelongsTo = cluster.Key;
                }
            }
        }

        clusters[clusterThePointBelongsTo].Remove(closestPoint);
        return closestPoint;
    }

    private void SetColoursToClusterPoints()
    {
        int clusterCounter = 0;
        foreach (var cluster in clusters)
        {
            foreach (GameObject point in cluster.Value)
                point.GetComponent<MeshRenderer>().material.color = colours[clusterCounter];

            clusterCounter++;
        }
    }

    private void RecalculateCenterPositions()
    {
        int clusterCount = 0;
        foreach (var cluster in clusters)
        {
            Vector3 sum = Vector3.zero;
            foreach (var point in cluster.Value)
                sum += point.transform.position;

            Vector3 average = sum / cluster.Value.Count;
            centersObjects[clusterCount].transform.position = average;
            clusterCount++;
        }
    }

    private void CheckForEnd()
    {
        for (int i = 0; i < centersObjects.Count; i++)
        {
            if (centersObjects[i].transform.position != previousCenters[i])
                return;
        }
    }

    private void UpdatePreviousCenters()
    {
        for (int i = 0; i < centersObjects.Count; i++)
            previousCenters[i] = centersObjects[i].transform.position;
    }
}
