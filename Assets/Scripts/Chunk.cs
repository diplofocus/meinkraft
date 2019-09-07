using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Do not use voids.
public class Chunk : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    public static readonly int ChunkWidth = 12;
    public static readonly int ChunkHeight = 6;

    static int nVerts = 6; // Vertices per face

    static int nFaces = 6; // Number of faces

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    bool[,,] voxelMap = new bool[ChunkWidth, ChunkHeight, ChunkWidth];

    private int vertexIndex = 0;

    void Start()
    {
        MakeVoxelMap();
        CreateChunk();
        CreateMesh();
    }

    void CreateChunk()
    {
        for (int y = 0; y < ChunkHeight; y++)
        {
            for (int x = 0; x < ChunkWidth; x++)
            {
                for (int z = 0; z < ChunkWidth; z++)
                {
                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
    void AddVoxelDataToChunk(Vector3 position)
    {

        for (int face = 0; face < nFaces; face++)
        {
            if (!CullVoxel(position + VoxelData.neighborLookups[face]))
            {
                // TODO: Remove duplicate verts, add triangles manually (ie, unfold the for loop)
                for (int vert = 0; vert < nVerts; vert++)
                {
                    int triangleIndex = VoxelData.voxelTriangles[face, vert];
                    vertices.Add(VoxelData.voxelVertices[triangleIndex] + position);
                    triangles.Add(vertexIndex);

                    uvs.Add(VoxelData.voxelUVs[vert]);

                    vertexIndex++;
                }
            }
        }
    }

    void MakeVoxelMap()
    {
        for (int y = 0; y < ChunkHeight; y++)
        {
            for (int x = 0; x < ChunkWidth; x++)
            {
                for (int z = 0; z < ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = true;
                }
            }
        }
    }

    bool CullVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (x < 0 || y < 0 || z < 0 || x > ChunkWidth - 1 || y > ChunkHeight - 1 || z > ChunkWidth - 1)
        {
            return false;
        }

        return voxelMap[x, y, z];
    }

}
