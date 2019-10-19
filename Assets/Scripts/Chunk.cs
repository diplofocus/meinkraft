using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Do not use voids.
public class Chunk
{
    public ChunkCoord coord;
    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;


    static int nFaces = 6; // Number of faces

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    World world;

    public byte[,,] voxelMap = new byte[ChunkWidth, ChunkHeight, ChunkWidth];
    private int vertexIndex = 0;


    public Chunk(ChunkCoord _coord, World _world, bool generateOnLoad)
    {
        coord = _coord;
        world = _world;
        isActive = true;

        if (generateOnLoad)
            Init();
    }

    public void Init()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * ChunkWidth, 0, coord.z * ChunkWidth);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.z;

        MakeVoxelMap();
        UpdateChunk();
    }
    void UpdateChunk()
    {
        ClearMeshData();
        for (int y = 0; y < ChunkHeight; y++)
        {
            for (int x = 0; x < ChunkWidth; x++)
            {
                for (int z = 0; z < ChunkWidth; z++)
                {
                    if (world.blockTypes[voxelMap[x, y, z]].isSolid)
                    {
                        UpdateMeshData(new Vector3(x, y, z));
                    }
                }
            }
        }
        CreateMesh();
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

    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }
    void UpdateMeshData(Vector3 position)
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
        isVoxelMapPopulated = true;
    }

    private bool _isActive;
    public bool isVoxelMapPopulated = false;


    public bool isActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);
        }
    }

    public Vector3 position
    {
        get { return chunkObject.transform.position; }
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        return !(x < 0 || y < 0 || z < 0 || x > ChunkWidth - 1 || y > ChunkHeight - 1 || z > ChunkWidth - 1);
    }

    bool CullVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
        {
            return world.CheckForBlock(pos + position);
        }

        return world.blockTypes[voxelMap[x, y, z]].isSolid;
    }


    public void EditVoxel(Vector3 pos, byte newId)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[xCheck, yCheck, zCheck] = newId;


        UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
        UpdateChunk();
    }

    void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);
        for (int face = 0; face < nFaces; face++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.neighborLookups[face];
            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.GetChunkFromVector3(currentVoxel + position).UpdateChunk();
            }
        }
    }


    public byte GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }

    void AddTexture(int textureId)
    {
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

public class ChunkCoord
{
    public int x, z;

    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }

    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / Chunk.ChunkWidth;
        z = zCheck / Chunk.ChunkWidth;
    }
    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null) return false;
        return x == other.x && z == other.z;
    }
}