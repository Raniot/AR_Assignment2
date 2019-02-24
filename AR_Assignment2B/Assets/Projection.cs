using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;

public class Projection : MonoBehaviour
{
    public Matrix4x4 ProjectionMatrix;
    private Camera _cam;

    // Start is called before the first frame update
    void Start()
    {
        _cam = GetComponent<Camera>();
        ProjectionMatrix = PerspectiveOffCenter(_cam.nearClipPlane, _cam.farClipPlane);
        _cam.projectionMatrix = ProjectionMatrix;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static Matrix4x4 PerspectiveOffCenter(float near, float far)
    {
        //Intrinsic values (from AR-Toolkit see onenote)
        var fx = 842.34573f;
        var fy = 842.03391f;
        var cx = 310.45239f;
        var cy = 243.37875f;
        var h = 480f;
        var w = 640f;


        float x = 2.0f * fx / w;
        float y = 2.0f * fy / h;
        float a = (-2.0f * cx / w) + 1;
        float b = (-2.0f * cy / h) + 1;
        float c = -((far + near) / (far - near));
        float d = -((2*far*near) / (far - near));
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }
}
