using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {
    public int seed = 1337;
    public Transform player;
    public Vector3 spawnPosition;
    public static readonly int WorldSizeInChunks = 100;
    public Material material;
    public BlockType[] blockTypes;
    public static readonly int ViewDistanceInChunks = 5;
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord playerLastCoords;

    public static int WorldSizeInVoxels {
        get { return WorldSizeInChunks * Chunk.ChunkWidth; }
    }

    Chunk[,] chunks = new Chunk[WorldSizeInChunks, WorldSizeInChunks];

    private void Start() {
        Random.InitState(seed);
        spawnPosition = new Vector3(WorldSizeInChunks * Chunk.ChunkWidth / 2f, Chunk.ChunkHeight + 2, WorldSizeInChunks * Chunk.ChunkWidth / 2f);
        GenerateWorld();
        playerLastCoords = GetChunkCoordFromVec3(player.position);
    }

    private void Update() {
        ChunkCoord currentChunkLocation = GetChunkCoordFromVec3(player.position);
        if (!currentChunkLocation.Equals(playerLastCoords)) {
            CheckViewDistance();
            playerLastCoords = currentChunkLocation;
        }
    }

    void GenerateWorld() {
        int center = WorldSizeInChunks / 2;
        for (int x = center - ViewDistanceInChunks; x < center + ViewDistanceInChunks; x++) {
            for (int z = center - ViewDistanceInChunks; z < center + ViewDistanceInChunks; z++) {
                CreateNewChunk(x, z);
            }
        }

        player.position = spawnPosition;
    }

    void CreateNewChunk(int x, int z) {
        ChunkCoord coords = new ChunkCoord(x, z);
        Chunk newChunk = new Chunk(coords, this);
        activeChunks.Add(coords);
        chunks[x, z] = newChunk;
    }

    ChunkCoord GetChunkCoordFromVec3(Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x / Chunk.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / Chunk.ChunkWidth);

        return new ChunkCoord(x, z);
    }

    void CheckViewDistance() {
        ChunkCoord coord = GetChunkCoordFromVec3(player.position);

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int x = coord.x - ViewDistanceInChunks; x < coord.x + ViewDistanceInChunks; x++) {
            for (int z = coord.z - ViewDistanceInChunks; z < coord.z + ViewDistanceInChunks; z++) {
                if (IsChunkInWorld(new ChunkCoord(x, z))) {
                    if (chunks[x, z] == null) {
                        CreateNewChunk(x, z);
                    } else if (!chunks[x, z].isActive) {
                        chunks[x, z].isActive = true;
                        activeChunks.Add(new ChunkCoord(x, z));
                    }
                }
                for (int i  = 0; i < previouslyActiveChunks.Count; i++) {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z))) {
                        previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach(ChunkCoord c in previouslyActiveChunks)
            chunks[c.x, c.z].isActive = false;
    }

    bool IsChunkInWorld (ChunkCoord coord) {
        int x = coord.x;
        int z = coord.z;
        return x > 0 && x < WorldSizeInChunks - 1 && z > 0 && z < WorldSizeInChunks;
    }

    bool IsVoxelInWorld (Vector3 pos) {
        float x = pos.x;
        float y = pos.y;
        float z = pos.z;
        return x >= 0 && x < WorldSizeInVoxels && y >= 0 && y < Chunk.ChunkHeight && z >= 0 && z < WorldSizeInVoxels;
    }

    public byte GetVoxel (Vector3 pos) {
        if (!IsVoxelInWorld(pos)) 
            return 0;
 
        if (pos.y == 0) {
            return 1;
        } else if (pos.y == Chunk.ChunkHeight - 1) {
            float tempNoise = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, 0.1f);
            if (tempNoise < 0.5f) {
                return 3;
            } else {
                return 5;
            }
        } else {
            return 2;
        }
    }
}

[System.Serializable]
public class BlockType {
    public string blockName;
    public bool isSolid;
    // Back, Front, Top, Bottom, Left, Right

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    public int GetTextureId(int faceIndex) {
        switch (faceIndex) {
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