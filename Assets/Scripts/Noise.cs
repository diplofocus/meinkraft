using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {
    public static float Get2DPerlin (Vector2 pos, float offset, float scale) {
        return Mathf.PerlinNoise(
            (pos.x + .1f) / Chunk.ChunkWidth * scale + offset,
            (pos.y + .1f) / Chunk.ChunkWidth * scale + offset);
    }
}
