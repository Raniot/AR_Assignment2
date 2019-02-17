using System.Linq;
using UnityEngine;

public class Generate : MonoBehaviour
{
    public int m_SegmentCount = 100;
    public Texture2D m_HeightMap;

    private float m_Height;
    private float m_Width;

    private bool m_TextureIsReadable = false;
    private bool m_TextureChecked = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        m_Width = transform.parent.parent.Find("TerrainControllerImageTarget").position.x;
        m_Height = transform.parent.parent.Find("TerrainControllerImageTarget").position.z;

        Mesh mesh = new Mesh();

        float segmentSize = m_Width / m_SegmentCount;

        //Matrix4x4 meshTransform = transform.localToWorldMatrix;

        for (int i = 0; i <= m_SegmentCount; i++)
        {
            float z = segmentSize * i;
            float v = (1.0f / m_SegmentCount) * i;

            for (int j = 0; j <= m_SegmentCount; j++)
            {
                float x = segmentSize * j;
                float u = (1.0f / m_SegmentCount) * j;

                Vector3 offset = new Vector3(x, GetY(x, z), z);

                Vector2 uv = new Vector2(u, v);
                bool buildTriangles = i > 0 && j > 0;

                BuildQuadForGrid(mesh, offset, uv, buildTriangles, m_SegmentCount + 1);
            }
        }

        mesh.RecalculateNormals();

        //Look for a MeshFilter component attached to this GameObject:
        MeshFilter filter = GetComponent<MeshFilter>();

        //If the MeshFilter exists, attach the new mesh to it.
        //Assuming the GameObject also has a renderer attached, our new mesh will now be visible in the scene.
        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }
    }

    public float GetY(float x, float z)
    {
        if (IsTextureReadable(m_HeightMap))
        {
            var mapColor = m_HeightMap.GetPixelBilinear(x / m_Width, z / m_Width);

            return mapColor.grayscale * m_Height;
        }

        return 0.0f;
    }

    private void BuildQuadForGrid(Mesh meshBuilder, Vector3 position, Vector2 uv, bool buildTriangles, int vertsPerRow)
    {
        meshBuilder.vertices.ToList().Add(position);
        meshBuilder.uv.ToList().Add(uv);

        if (buildTriangles)
        {
            int baseIndex = meshBuilder.vertices.Length - 1;

            int index0 = baseIndex;
            int index1 = baseIndex - 1;
            int index2 = baseIndex - vertsPerRow;
            int index3 = baseIndex - vertsPerRow - 1;

            meshBuilder.triangles.ToList().Add(index0);
            meshBuilder.triangles.ToList().Add(index2);
            meshBuilder.triangles.ToList().Add(index1);

            meshBuilder.triangles.ToList().Add(index2);
            meshBuilder.triangles.ToList().Add(index3);
            meshBuilder.triangles.ToList().Add(index1);
        }
    }

    private bool IsTextureReadable(Texture2D texture)
    {
        if (m_TextureChecked)
            return m_TextureIsReadable;

        if (texture != null)
        {
            m_TextureChecked = true;

            try
            {
                texture.GetPixel(0, 0);
                m_TextureIsReadable = true;
                return true;
            }
            catch
            {
                Debug.LogError("Could not sample texture. Read/write may not be enabled. Please check the import settings.");
            }
        }

        return false;
    }
}
