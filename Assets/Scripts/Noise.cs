using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {
    public static float Get2DPerlin (Vector2 pos, float offset, float scale) {
        return Mathf.PerlinNoise(
            (pos.x + .1f) / Chunk.ChunkWidth * scale + offset,
            (pos.y + .1f) / Chunk.ChunkWidth * scale + offset);
    }

    public static float Get2DPerlin (Vector3 pos, float offset, float scale) {
        return Mathf.PerlinNoise(
            (pos.x + .1f) / Chunk.ChunkWidth * scale + offset,
            (pos.z + .1f) / Chunk.ChunkWidth * scale + offset);
    }

    public static bool Get3DPerlin (Vector3 pos, float offset, float scale, float threshold) {
        float x = (pos.x + offset + 0.1f) * scale;
        float y = (pos.y + offset + 0.1f) * scale;
        float z = (pos.z + offset + 0.1f) * scale;

        float xy = Mathf.PerlinNoise(x, y);
        float xz = Mathf.PerlinNoise(x, z);
        float yz = Mathf.PerlinNoise(y, z);

        float xyr = Mathf.PerlinNoise(y, x);
        float xzr = Mathf.PerlinNoise(z, x);
        float yzr = Mathf.PerlinNoise(z, y);

        return ((xy + xz + yz + xyr + xzr + yzr) / 6) > threshold;
    }
}
