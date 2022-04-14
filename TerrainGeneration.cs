using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TerrainGeneration : MonoBehaviour
{
    [Header("Terrain generation")]
    public int width;
    public int depth;
    [SerializeField] private Gradient gradient;
    [SerializeField] private int seed;
    [Range(1, 100)] [SerializeField] private int octaves;
    [Range(1, 100)] [SerializeField] private float noiseScale;
    [Range(0, 1)] [SerializeField] private float persistance;
    [Range(1, 100)] [SerializeField] private float lacunarity;
    [Range(1, 100)] [SerializeField] private float heightMultiplier;
    [Range(0, 1)] [SerializeField] private float heightTreshhold;
    [SerializeField] private Vector2 offset;

    [Header("Entities")]
    [SerializeField] private GameObject player;
    [SerializeField] private NavMeshSurface playernavMeshSurface;
    [SerializeField] private GameObject enemy;
    [SerializeField] private NavMeshSurface enemyNavMeshSurface;

    private MeshCollider meshCollider;
    private Vector3[] vertices;
    private int[] trianglePoints;
    private Vector2[] uvs;
    private Color[] colors;
    private Mesh mesh;
    private MeshFilter meshFilter;
    private float minHeight, maxHeight;


    private void Start()
    {
        mesh = new Mesh();
        mesh.name = "Generated Terrain";
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        meshCollider = GetComponent<MeshCollider>();

        CreateMesh();
        UpdateMesh();

        playernavMeshSurface.BuildNavMesh();
        enemyNavMeshSurface.BuildNavMesh();
        meshCollider.sharedMesh = mesh;

        Instantiate(player, new Vector3(4f, 5.6f, 48.2f), Quaternion.identity);
        Instantiate(enemy, new Vector3(36.4f, 0f, 174.4f), Quaternion.identity);
    }

    private void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Debug.Log("Updated");
            CreateMesh();
            UpdateMesh();
        }
    }

    private void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = trianglePoints;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }

    private void CreateMesh()
    {
        //Start with vertices array
        vertices = new Vector3[(width + 1) * ( depth +1)];
        var noiseArray = perlinNoise();

        int i = 0;
        for (int z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                //loat y = Mathf.PerlinNoise(x * 0.4f, z * 0.4f) * 3f;
                //vertices[i] = new Vector3(x, y, z);
                //ComputerElevation(y);
                var currentHeight = noiseArray[i];
                if (currentHeight > heightTreshhold)
                {
                    currentHeight *= heightMultiplier;
                }
                vertices[i] = new Vector3(x, currentHeight, z);
                i++;
            }
        }

        //Traingles
        trianglePoints = new int[width * depth * 6]; //we have 6 as we have to build 2 triangle (3 + 3 points) 
        int currentTrianglePoint = 0, currentVertexPoint = 0;

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                //construction of traingles on face
                trianglePoints[currentTrianglePoint + 0] = currentVertexPoint + 0;
                trianglePoints[currentTrianglePoint + 1] = currentVertexPoint + width + 1;
                trianglePoints[currentTrianglePoint + 2] = currentVertexPoint + 1;
                trianglePoints[currentTrianglePoint + 3] = currentVertexPoint + 1;
                trianglePoints[currentTrianglePoint + 4] = currentVertexPoint + width+ 1;
                trianglePoints[currentTrianglePoint + 5] = currentVertexPoint + width +2;

                currentVertexPoint++;
                currentTrianglePoint += 6;
            }
            currentVertexPoint++;
        }

        //uvs
        uvs = new Vector2[vertices.Length];
        i = 0;
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                uvs[i] = new Vector2((float)x /width, (float)z / depth);
                i++;
            }
        }

        //colors
        colors = new Color[vertices.Length];
        i = 0;
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                //float height = Mathf.InverseLerp(minHeight, maxHeight, vertices[i].y);
                float height = Mathf.InverseLerp(minHeight * heightMultiplier, maxHeight * heightMultiplier, vertices[i].y);
                colors[i] = gradient.Evaluate(height);
                i++;
            }
        }
    }

    float[] perlinNoise()
    {
        float[] noiseArray = new float[(width + 1) * (depth + 1)];
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2((float)offsetX, (float)offsetY);
        }

        float halfWidth = width / 2f, halfDepth = depth / 2f;

        //Apply lacunarity and persistence
        int n = 0;
        for (int z = 0; z <= depth ; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                float amplitude = 1, frequency = 1, noiseHeight = 0;

                //Use multiple frequencies (octaves)
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / noiseScale * frequency + octaveOffsets[i].x;
                    float sampleY = (z - halfWidth) / noiseScale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                noiseArray[n] = ComputerElevation(noiseHeight);
                n++;
            }
        }

        //Normalize height
        int k = 0;
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                noiseArray[k] = Mathf.InverseLerp(minHeight, maxHeight, noiseArray[k]);
                k++;
            }
        }

        return noiseArray;
    }

    private float ComputerElevation(float noiseHeight)
    {
        if (noiseHeight > maxHeight)
        {
            maxHeight = noiseHeight;
        }
        if (noiseHeight < minHeight)
        {
            minHeight = noiseHeight;
        }
        return noiseHeight;
    }
}

