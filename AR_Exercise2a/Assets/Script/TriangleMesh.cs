using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TriangleMesh : MonoBehaviour
{

    private MeshFilter meshFilter;

    private Mesh mesh = null;
    private Vector3[] vertices = new Vector3[25];
    private Color[] colors = new Color[25];
    private Vector2[] uvs = new Vector2[25];
    private Vector3[] normals = new Vector3[25];
    private int[] indices;
    
    public float alpha = 0.3f;
    public Vector3 upperWing = new Vector3(0, 1.25f,0);
    public Vector3 lowerWing = new Vector3(0,-1.25f,0);
    public Color upperWingColor = Color.red;
    public Color lowerWingColor = Color.red;


    void Start()
    {

    }

    void Update()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (mesh == null)
        {
            mesh = new Mesh();
        }

        setVertices();
        setUVs();
        setNormals();
        setColors();
        setIndicies();

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = indices;

        meshFilter.sharedMesh = mesh;
    }

    void setVertices()
    {
        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(0.5f, 0.5f, 0);
        vertices[2] = new Vector3(0.5f, -0.5f, 0);
        vertices[3] = new Vector3(3, 0.5f, 0);
        vertices[4] = new Vector3(3, -0.5f, 0);
        vertices[5] = new Vector3(4, 1, 0);
        vertices[6] = new Vector3(4, 0.5f, 0);
        vertices[7] = new Vector3(4, -1, 0);
        vertices[8] = new Vector3(4, -0.5f, 0);
        vertices[9] = new Vector3(4.5f, 0.75f, 0);
        vertices[10] = new Vector3(4.5f, -0.75f, 0);
        vertices[11] = new Vector3(2.25f, 1.25f, 0);
        vertices[12] = new Vector3(3, 2, 0);
        vertices[13] = new Vector3(3.5f, 1.75f, 0);
        vertices[14] = new Vector3(3, 2, 0);
        vertices[15] = new Vector3(2.5f, 2, 0);
        vertices[16] = upperWing;
        vertices[17] = new Vector3(0.75f, 1.75f, 0);
        vertices[18] = new Vector3(2.25f, -1.25f, 0);
        vertices[19] = new Vector3(3, -2, 0);
        vertices[20] = new Vector3(3.5f, -1.75f, 0);
        vertices[21] = new Vector3(3, -2, 0);
        vertices[22] = new Vector3(2.5f, -2, 0);
        vertices[23] = lowerWing;
        vertices[24] = new Vector3(0.75f, -1.75f, 0); ;
    }

    void setUVs()
    {
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0.5f);
        uvs[2] = new Vector2(0.5f, 0.15f);
        uvs[3] = new Vector2(0, 0);
        uvs[4] = new Vector2(1, 0.5f);
        uvs[5] = new Vector2(0.5f, 0.15f);
        uvs[6] = new Vector2(0, 0);
        uvs[7] = new Vector2(1, 0.5f);
        uvs[8] = new Vector2(0.5f, 0.15f);
        uvs[9] = new Vector2(0, 0);
        uvs[10] = new Vector2(1, 0.5f);
        uvs[11] = new Vector2(0.5f, 0.15f);
        uvs[12] = new Vector2(0, 0);
        uvs[13] = new Vector2(1, 0.5f);
        uvs[14] = new Vector2(0.5f, 0.15f);
        uvs[15] = new Vector2(0, 0);
        uvs[16] = new Vector2(1, 0.5f);
        uvs[17] = new Vector2(0.5f, 0.15f);
        uvs[18] = new Vector2(0, 0);
        uvs[19] = new Vector2(1, 0.5f);
        uvs[20] = new Vector2(0.5f, 0.15f);
        uvs[21] = new Vector2(0, 0);
        uvs[22] = new Vector2(1, 0.5f);
        uvs[23] = new Vector2(0.5f, 0.15f);
        uvs[24] = new Vector2(0, 0);

    }

    void setNormals()
    {
        normals[0] = Vector3.back;
        normals[1] = Vector3.back;
        normals[2] = Vector3.back;
        normals[3] = Vector3.back;
        normals[4] = Vector3.back;
        normals[5] = Vector3.back;
        normals[6] = Vector3.back;
        normals[7] = Vector3.back;
        normals[8] = Vector3.back;
        normals[9] = Vector3.back;
        normals[10] = Vector3.back;
        normals[11] = Vector3.back;
        normals[12] = Vector3.back;
        normals[13] = Vector3.back;
        normals[14] = Vector3.back;
        normals[15] = Vector3.back;
        normals[16] = Vector3.back;
        normals[17] = Vector3.back;
        normals[18] = Vector3.back;
        normals[19] = Vector3.back;
        normals[20] = Vector3.back;
        normals[21] = Vector3.back;
        normals[22] = Vector3.back;
        normals[23] = Vector3.back;
        normals[24] = Vector3.back;
    }

    void setColors()
    {
        colors[0] = Color.red;
        colors[1] = Color.red;
        colors[2] = Color.red;
        colors[3] = Color.red;
        colors[4] = Color.red;
        colors[5] = Color.red;
        colors[6] = Color.red;
        colors[7] = Color.red;
        colors[8] = Color.red;
        colors[9] = Color.red;
        colors[10] = Color.red;
        colors[11] = upperWingColor;
        colors[12] = Color.red;
        colors[13] = Color.red;
        colors[14] = Color.red;
        colors[15] = upperWingColor;
        colors[16] = upperWingColor;
        colors[17] = upperWingColor;
        colors[18] = lowerWingColor;
        colors[19] = Color.red;
        colors[20] = Color.red;
        colors[21] = Color.red;
        colors[22] = lowerWingColor;
        colors[23] = lowerWingColor;
        colors[24] = lowerWingColor;

        colors[0].a = alpha;
    }

    void setIndicies()
    {
        indices = new int[]
        {
            0, 1, 2,
            2, 1, 3,
            2, 3, 4,
            3, 5, 6,
            4, 3, 6,
            4, 6, 8,
            6, 5, 9,
            7, 8, 10,
            4, 8, 7,

            3, 12, 5,
            5, 12, 13,
            3, 11, 12,
            11, 14, 13,
            11, 15, 14,
            11, 16, 17,
            11, 17, 15,

            19, 4, 7,
            19, 7, 20,
            4, 19, 18, 
            18, 20, 19, 
            18, 21, 22, 
            18, 24, 23, 
            18, 22, 24

        };
    }
}
