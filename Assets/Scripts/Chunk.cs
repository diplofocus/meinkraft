using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Do not use voids.
public class Chunk {
    public ChunkCoord coord;
    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 16;


    static int nFaces = 6; // Number of faces

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    World world;

    byte[,,] voxelMap = new byte[ChunkWidth, ChunkHeight, ChunkWidth];
    private int vertexIndex = 0;


    public Chunk(ChunkCoord _coord, World _world) {
        world = _world;
        coord = _coord;
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * ChunkWidth, 0, coord.z * ChunkWidth);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.z;

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
                byte blockId = voxelMap[(int)position.x, (int)position.y, (int)position.z];

                vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[face, 0]]);
                vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[face, 1]]);
                vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[face, 2]]);
                vertices.Add(position + VoxelData.voxelVertices[VoxelData.voxelTriangles[face, 3]]);

                AddTexture(world.blockTypes[blockId].GetTextureId(face));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);

                vertexIndex += 4;
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
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + position);
                }
            }
        }
    }

    public bool isActive {
        get { return chunkObject.activeSelf; }
        set { chunkObject.SetActive(value); }
    }

    public Vector3 position {
        get { return chunkObject.transform.position; }
    }

    bool IsVoxelInChunk (int x, int y, int z) {
        return !(x < 0 || y < 0 || z < 0 || x > ChunkWidth - 1 || y > ChunkHeight - 1 || z > ChunkWidth - 1);
    }

    bool CullVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
        {
            return world.blockTypes[world.GetVoxel(pos + position)].isSolid;
        }

        return world.blockTypes[voxelMap[x, y, z]].isSolid;
    }

    void AddTexture(int textureId) {
        float y = textureId / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureId - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }


}

public class ChunkCoord {
    public int x, z;
    public ChunkCoord(int _x, int _z) {
        x = _x;
        z = _z;
    }
}