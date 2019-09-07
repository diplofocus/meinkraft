using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    public static readonly int ChunkWidth = 12;
    public static readonly int ChunkHeight = 2;

    static int nVerts = 6; // Vertices per face

    static int nFaces = 6; // Number of faces

    Vector3[] vertices = new Vector3[nVerts * nFaces * ChunkWidth * ChunkWidth * ChunkHeight];
    int[] triangles = new int[nVerts * nFaces * ChunkWidth * ChunkWidth * ChunkHeight];
    Vector2[] uvs = new Vector2[nVerts * nFaces * ChunkWidth * ChunkWidth * ChunkHeight];

    private int vertexIndex = 0;

    void Start() {
        CreateChunk();
        CreateMesh();
    }

    void CreateChunk() {
        for(int y = 0; y < ChunkHeight; y++) {
            for(int x = 0; x < ChunkWidth; x++) {
                for (int z = 0; z < ChunkWidth; z++) {
                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    void CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
    void AddVoxelDataToChunk(Vector3 position) {

        for (int face = 0; face < nFaces; face++) {
            for (int vert = 0; vert < nVerts; vert++) {
                int triangleIndex = VoxelData.voxelTriangles[face, vert];
                vertices[vertexIndex] = (VoxelData.voxelVertices[triangleIndex] + position);
                triangles[vertexIndex] = vertexIndex;

                uvs[vertexIndex] = VoxelData.voxelUVs[vert];

                vertexIndex++;
            }
        }
    }

}
