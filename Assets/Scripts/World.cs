using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public BiomeAttributes biome;
    public int seed = 1337;
    public Transform player;
    public Vector3 spawnPosition;
    public static readonly int WorldSizeInChunks = 100;
    public Material material;
    public BlockType[] blockTypes;
    public static readonly int ViewDistanceInChunks = 5;

    public GameObject debugScreen;
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    public ChunkCoord playerChunkCoord = new ChunkCoord();
    ChunkCoord playerLastChunkCoord;

    public static int WorldSizeInVoxels
    {
        get { return WorldSizeInChunks * Chunk.ChunkWidth; }
    }

    Chunk[,] chunks = new Chunk[WorldSizeInChunks, WorldSizeInChunks];

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();

    private void Start()
    {
        Random.InitState(seed);
        spawnPosition = new Vector3(WorldSizeInChunks * Chunk.ChunkWidth / 2f, Chunk.ChunkHeight - 2, WorldSizeInChunks * Chunk.ChunkWidth / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVec3(player.position);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVec3(player.position);

        if (!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        if (chunksToCreate.Count > 0 && !isCreatingChunks)
            StartCoroutine("CreateChunks");

        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
    }

    void GenerateWorld()
    {
        int center = WorldSizeInChunks / 2;
        for (int x = center - ViewDistanceInChunks; x < center + ViewDistanceInChunks; x++)
        {
            for (int z = center - ViewDistanceInChunks; z < center + ViewDistanceInChunks; z++)
            {
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x, z));
            }
        }

        player.position = spawnPosition;
    }

    private bool isCreatingChunks = false;
    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;
        while (chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }

        isCreatingChunks = false;
    }

    ChunkCoord GetChunkCoordFromVec3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / Chunk.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / Chunk.ChunkWidth);

        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / Chunk.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / Chunk.ChunkWidth);

        return chunks[x, z];
    }

    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVec3(player.position);
        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int x = coord.x - ViewDistanceInChunks; x < coord.x + ViewDistanceInChunks; x++)
        {
            for (int z = coord.z - ViewDistanceInChunks; z < coord.z + ViewDistanceInChunks; z++)
            {
                if (IsChunkInWorld(new ChunkCoord(x, z)))
                {
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
                    }
                    else if (!chunks[x, z].isActive)
                    {
                        chunks[x, z].isActive = true;
                    }
                    activeChunks.Add(new ChunkCoord(x, z));
                }
                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                    {
                        previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach (ChunkCoord c in previouslyActiveChunks)
            chunks[c.x, c.z].isActive = false;
    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        int x = coord.x;
        int z = coord.z;
        return x > 0 && x < WorldSizeInChunks - 1 && z > 0 && z < WorldSizeInChunks;
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        float x = pos.x;
        float y = pos.y;
        float z = pos.z;
        return x >= 0 && x < WorldSizeInVoxels && y >= 0 && y < Chunk.ChunkHeight && z >= 0 && z < WorldSizeInVoxels;
    }

    public bool CheckForBlock(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > Chunk.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isVoxelMapPopulated)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isSolid;

        return blockTypes[GetVoxel(pos)].isSolid;

    }

    public byte GetVoxel(Vector3 pos)
    {
        /* 0: Air,
           1: Bedrock,
           2: Stone,
           3: Grass,
           4: Furnace,
           5: Sand 
           6: Dirt */
        int yPos = Mathf.FloorToInt(pos.y);
        /* Immutable pass */

        // Outside world - air
        if (!IsVoxelInWorld(pos))
            return 0;

        // Bottom - bedrock
        if (yPos == 0)
            return 1;

        /* Basic terrain pass */
        int terrainHeight = Mathf.FloorToInt(
            biome.terrainHeight * Noise.Get2DPerlin(pos, 0, biome.terrainScale)) + biome.solidGroundHeight;

        byte voxelValue = 0;

        // Grass on top layer
        if (yPos == terrainHeight)
            voxelValue = 3;
        // Dirt in layers below topmost layer
        else if (yPos < terrainHeight && yPos > terrainHeight - 1 - Random.Range(2, 6))
            voxelValue = 6;
        // Air above terrain level
        else if (yPos > terrainHeight)
            return 0;
        // Stone elsewhere
        else
            voxelValue = 2;

        /* Second pass */
        if (voxelValue == 2)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (
                    yPos > lode.minHeight
                    && yPos < lode.maxHeight
                    && Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                {
                    voxelValue = lode.blockID;
                }
            }
        }

        return voxelValue;

    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;
    public Sprite icon;
    // Back, Front, Top, Bottom, Left, Right

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    public int GetTextureId(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureId, face out of bounds.");
                return 0;
        }
    }
}