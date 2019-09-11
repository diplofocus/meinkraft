using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {
    public static readonly int WorldSizeInChunks = 5;
    public Material material;
    public BlockType[] blockTypes;

    public static int WorldSizeInVoxels {
        get { return WorldSizeInChunks * Chunk.ChunkWidth; }
    }

    Chunk[,] chunks = new Chunk[WorldSizeInChunks, WorldSizeInChunks];

    private void Start() {
        GenerateWorld();
    }

    void GenerateWorld() {
        for (int x = 0; x < WorldSizeInChunks; x++) {
            for (int z = 0; z < WorldSizeInChunks; z++) {
                CreateNewChunk(x, z);
            }
        }
    }

    void CreateNewChunk(int x, int z) {
        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
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
            return 3;
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