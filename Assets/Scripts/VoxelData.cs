﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly Vector3[] voxelVertices = new Vector3[8] {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
    };


    // Back, Front, Top, Bottom, Left, Right
    public static readonly int[,] voxelTriangles = new int[6, 4] {
        // TODO: Remove duplicate verts
        {0, 3, 1, 2}, // Back face
        {5, 6, 4, 7}, // Front face
        {3, 7, 2, 6}, // Top face
        {1, 5, 0, 4}, // Bottom face
        {4, 7, 0, 3}, // Left face
        {1, 2, 5, 6}  // Right face
    };

    public static readonly Vector2[] voxelUVs = new Vector2[4] {
        new  Vector2(0.0f, 0.0f),
        new  Vector2(0.0f, 1.0f),
        new  Vector2(1.0f, 0.0f),
        new  Vector2(1.0f, 1.0f)
    };

    public static readonly Vector3[] neighborLookups = new Vector3[6] {
        new Vector3(0.0f, 0.0f, -1.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, -1.0f, 0.0f),
        new Vector3(-1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
    };

    public static readonly int TextureAtlasSizeInBlocks = 4;
    public static float NormalizedBlockTextureSize {
        get { return 1f / TextureAtlasSizeInBlocks; }
    }
}
