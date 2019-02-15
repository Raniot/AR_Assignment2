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
    private Vector3[] vertices = new Vector3[3];
    private Color[] colors = new Color[3];
    private Vector2[] uvs = new Vector2[3];
    private Vector3[] normals = new Vector3[3];
    private int[] indices = new int[3];
    public Vector3[] test;
    
    public float alpha = 0.3f;

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
        vertices[0] = new Vector3(-0.5f, -0.4f, 0);
        vertices[1] = new Vector3(0.1f, 0.6f, 0);
        vertices[2] = new Vector3(0.5f, -0.2f, 0);

        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0.5f);
        uvs[2] = new Vector2(0.5f, 0.15f);

        normals[0] = Vector3.back;
        normals[1] = Vector3.back;
        normals[2] = Vector3.back;

        colors[0] = Color.red;
        colors[1] = Color.green;
        colors[2] = Color.blue;
        colors[0].a = alpha;

        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = indices;

        meshFilter.sharedMesh = mesh;
    }
}
