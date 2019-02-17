using UnityEngine;
using Vuforia;

public class Generate : MonoBehaviour
{
    public Material Material;
    public Texture2D HeightMap;
    public float SegmentSize = 0.01f;
    public float scale;

    private float mHeight;
    private float mWidth;

    private Transform targetBase;
    private Transform targetController;
    private ImageTargetBehaviour targetBaseBehaviour;
    private ImageTargetBehaviour targetControllerBehaviour;
    private bool mTextureIsReadable;
    private bool mTextureChecked;

    // Start is called before the first frame update
    void Start()
    {
        targetBase = transform.parent;
        targetController = transform.parent.parent.Find("TerrainControllerImageTarget");

        targetBaseBehaviour = targetBase.GetComponent<ImageTargetBehaviour>();
        targetControllerBehaviour = targetController.GetComponent<ImageTargetBehaviour>();

        GetComponent<Renderer>().material = Material;
    }

    // Update is called once per frame
    void Update()
    {
        var conPos = targetController.position;
        conPos.y = 0f;
        if (targetBaseBehaviour.CurrentStatus != TrackableBehaviour.Status.TRACKED || 
            targetControllerBehaviour.CurrentStatus != TrackableBehaviour.Status.TRACKED) return;

        var meshBuilder = new MeshBuilder();

        mWidth = targetController.position.x;
        mHeight = targetController.position.z;

        float mSegmentWidthCount = mWidth / (SegmentSize * transform.lossyScale.x);
        float mSegmentHeightCount = mHeight / (SegmentSize * transform.lossyScale.z);

        for (int i = 0; i <= mSegmentHeightCount; i++)
        {
            float z = SegmentSize * i;
            float v = (1.0f / mSegmentHeightCount) * i;

            for (int j = 0; j <= mSegmentWidthCount; j++)
            {
                float x = SegmentSize * j;
                float u = (1.0f / mSegmentWidthCount) * j;

                Vector3 offset = new Vector3(x, GetY(x,z), z);

                Vector2 uv = new Vector2(u, v);
                bool buildTriangles = i > 0 && j > 0;

                BuildQuadForGrid(meshBuilder, offset, uv, buildTriangles, (int)mSegmentWidthCount + 1);
            }
        }

        Mesh mesh = meshBuilder.CreateMesh();

        mesh.RecalculateNormals();

        //Look for a MeshFilter component attached to this GameObject:
        MeshFilter filter = transform.GetComponent<MeshFilter>();

        //If the MeshFilter exists, attach the new mesh to it.
        //Assuming the GameObject also has a renderer attached, our new mesh will now be visible in the scene.
        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }
    }

    private void BuildQuadForGrid(MeshBuilder meshBuilder, Vector3 position, Vector2 uv, 
                    bool buildTriangles, int vertsPerRow)
    {
        meshBuilder.Vertices.Add(position);
        meshBuilder.UVs.Add(uv);

        if (buildTriangles)
        {
            int baseIndex = meshBuilder.Vertices.Count - 1;

            int index0 = baseIndex;
            int index1 = baseIndex - 1;
            int index2 = baseIndex - vertsPerRow;
            int index3 = baseIndex - vertsPerRow - 1;

            meshBuilder.AddTriangle(index0, index2, index1);
            meshBuilder.AddTriangle(index2, index3, index1);
        }
    }

    public float GetY(float x, float z)
    {
        var yaw = targetController.eulerAngles.y;

        if (IsTextureReadable(HeightMap))
        {
            Color mapColor = HeightMap.GetPixelBilinear(x, z);

            return (mapColor.grayscale / scale) * (yaw / 360);
        }

        return 0.0f;
    }

    private bool IsTextureReadable(Texture2D texture)
    {
        if (mTextureChecked)
            return mTextureIsReadable;

        if (texture != null)
        {
            mTextureChecked = true;

            try
            {
                texture.GetPixel(0, 0);
                mTextureIsReadable = true;
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
