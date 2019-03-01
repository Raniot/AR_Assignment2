using System.Collections.Generic;
using System.Linq;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UtilsModule;
using UnityEngine;
using Vuforia;

public class ColorMe : MonoBehaviour
{

    public Camera Cam;

    public GameObject Corner1;
    public GameObject Corner2;
    public GameObject Corner3;
    public GameObject Corner4;
    public GameObject ObjectToColor;


    public int ImageTargetWidth = 1122;
    public int ImageTargetHeight = 601;

    //Default values: guessed, not calibrated
    public float Fx = 650;
    public float Fy = 650;
    public float Cx = 320;
    public float Cy = 240;

    //public float Fx = 842.34573f;
    //public float Fy = 842.03391f;
    //public float Cx = 310.45239f;
    //public float Cy = 243.37875f;

    private MatOfPoint2f _imagePoints;
    private Mat _camImageMat;
    private byte[] _texData;

    void Start()
    {
        _imagePoints = new MatOfPoint2f();
        _imagePoints.alloc(4);
    }

    void Update()
    {
        //Access camera image provided by Vuforia
        Image camImg = CameraDevice.Instance.GetCameraImage(Image.PIXEL_FORMAT.RGBA8888);

        if (camImg == null) return;
        if (_camImageMat == null)
        {
            //First valid camera frame -> instantiate camera image with frame dimensions
            _camImageMat = new Mat(camImg.Height, camImg.Width, CvType.CV_8UC4);  //Note: rows=height, cols=width
        }

        _camImageMat.put(0, 0, camImg.Pixels);

        //---- TRANSFORM/PROJECT CORNER WORLD COORDINATES TO IMAGE COORDINATES ----

        Vector3 worldPnt1 = Corner1.transform.position;
        Vector3 worldPnt2 = Corner2.transform.position;
        Vector3 worldPnt3 = Corner3.transform.position;
        Vector3 worldPnt4 = Corner4.transform.position;

        //See lecture slides
        Matrix4x4 Rt = Cam.transform.worldToLocalMatrix;
        Matrix4x4 A = Matrix4x4.identity;
        A.m00 = Fx;
        A.m11 = Fy;
        A.m02 = Cx;
        A.m12 = Cy;

        //See equation for pinhole camera model
        Matrix4x4 worldToImage = A * Rt;

        //Apply transform to get homogeneous image coordinates
        Vector3 hUV1 = worldToImage.MultiplyPoint3x4(worldPnt1);
        Vector3 hUV2 = worldToImage.MultiplyPoint3x4(worldPnt2);
        Vector3 hUV3 = worldToImage.MultiplyPoint3x4(worldPnt3);
        Vector3 hUV4 = worldToImage.MultiplyPoint3x4(worldPnt4);

        //hUV are the image coordinates in homogeneous coordinates, we need to normalize, i.e., divide by Z to get to Cartesian coordinates
        Vector2 uv1 = new Vector2(hUV1.x, hUV1.y) / hUV1.z;
        Vector2 uv2 = new Vector2(hUV2.x, hUV2.y) / hUV2.z;
        Vector2 uv3 = new Vector2(hUV3.x, hUV3.y) / hUV3.z;
        Vector2 uv4 = new Vector2(hUV4.x, hUV4.y) / hUV4.z;

        //Do not forget to alloc before putting values into a MatOfPoint2f (see Start() above)
        //We need to flip the v-coordinates, see coordinate system overview
        float maxV = camImg.Height - 1;
        _imagePoints.put(0, 0, uv1.x, maxV - uv1.y);
        _imagePoints.put(1, 0, uv2.x, maxV - uv2.y);
        _imagePoints.put(2, 0, uv3.x, maxV - uv3.y);
        _imagePoints.put(3, 0, uv4.x, maxV - uv4.y);

        //Debug draw points using OpenCV's drawing functions
        Point imgPnt1 = new Point(_imagePoints.get(0, 0));
        Point imgPnt2 = new Point(_imagePoints.get(1, 0));
        Point imgPnt3 = new Point(_imagePoints.get(2, 0));
        Point imgPnt4 = new Point(_imagePoints.get(3, 0));
        Imgproc.circle(_camImageMat, imgPnt1, 5, new Scalar(255, 0, 0, 255));
        Imgproc.circle(_camImageMat, imgPnt2, 5, new Scalar(0, 255, 0, 255));
        Imgproc.circle(_camImageMat, imgPnt3, 5, new Scalar(0, 0, 255, 255));
        Imgproc.circle(_camImageMat, imgPnt4, 5, new Scalar(255, 255, 0, 255));
        Scalar lineCl = new Scalar(200, 120, 0, 160);
        Imgproc.line(_camImageMat, imgPnt1, imgPnt2, lineCl);
        Imgproc.line(_camImageMat, imgPnt2, imgPnt3, lineCl);
        Imgproc.line(_camImageMat, imgPnt3, imgPnt4, lineCl);
        Imgproc.line(_camImageMat, imgPnt4, imgPnt1, lineCl);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var srcPoints = new List<Point> { imgPnt2, imgPnt1, imgPnt4, imgPnt3 };
            var dstPoints = new List<Point>
            {
                new Point(0, ImageTargetHeight),
                new Point(ImageTargetWidth, ImageTargetHeight),
                new Point(ImageTargetWidth, 0),
                new Point(0, 0),
            };
            //var H = CalcHomogrphy(srcPoints, dstPoints);

            var matObj = new MatOfPoint2f(srcPoints.ToArray());
            var matDst = new MatOfPoint2f(dstPoints.ToArray());
            var H = Calib3d.findHomography(matObj, matDst);
            //Debug.Log("MatH2: " + H2.dump());

            var warpedMat = new Mat(new Size(ImageTargetWidth, ImageTargetHeight), _camImageMat.type());

            Imgproc.warpPerspective(_camImageMat, warpedMat, H, new Size(ImageTargetWidth, ImageTargetHeight),
                Imgproc.INTER_LINEAR);
            warpedMat.convertTo(warpedMat, CvType.CV_8UC3);

            var newTexture = new Texture2D(ImageTargetWidth, ImageTargetHeight, mipChain: false, textureFormat: TextureFormat.RGBA32);
            MatDisplay.MatToTexture(warpedMat, ref newTexture);

            ObjectToColor.GetComponent<MeshRenderer>().material.mainTexture = newTexture;
        }

        //Display the Mat that includes video feed and debug points
        MatDisplay.DisplayMat(_camImageMat, MatDisplaySettings.FULL_BACKGROUND);

        //---- MATCH INTRINSICS OF REAL CAMERA AND PROJECTION MATRIX OF VIRTUAL CAMERA ----
        Cam.projectionMatrix = Projection.PerspectiveOffCenter(Cam.nearClipPlane, Cam.farClipPlane);
    }

    private static Mat CalcHomogrphy(List<Point> srcPoints, List<Point> dstPoints)
    {
        var xy1 = srcPoints[0];
        var xy2 = srcPoints[1];
        var xy3 = srcPoints[2];
        var xy4 = srcPoints[3];

        var uvs = Uv.GetUvs(dstPoints);
        

        var matA = new Mat(8, 8, CvType.CV_64FC1);
        matA.put(0, 0,
            xy1.x*xy1.y, 1, 0, 0, 0, -uvs[0].U*xy1.x, -uvs[0].U*xy1.y,
            0, 0, 0, xy1.x, xy1.y, 1, -uvs[0].V*xy1.x, -uvs[0].V*xy1.y,

            xy2.x * xy2.y, 1, 0, 0, 0, -uvs[1].U * xy2.x, -uvs[1].U * xy2.y,
            0, 0, 0, xy2.x, xy2.y, 1, -uvs[1].V * xy2.x, -uvs[1].V * xy2.y,

            xy3.x * xy3.y, 1, 0, 0, 0, -uvs[2].U * xy3.x, -uvs[2].U * xy3.y,
            0, 0, 0, xy3.x, xy3.y, 1, -uvs[2].V * xy3.x, -uvs[2].V * xy3.y,

            xy4.x * xy4.y, 1, 0, 0, 0, -uvs[3].U * xy4.x, -uvs[3].U * xy4.y,
            0, 0, 0, xy4.x, xy4.y, 1, -uvs[3].V * xy4.x, -uvs[3].V * xy4.y);
        Debug.Log("MatA: " + matA.dump());

        var b = new Mat(8,1, CvType.CV_64FC1);
        b.put(0, 0,
            uvs[0].U, uvs[0].V, uvs[1].U, uvs[1].V, uvs[2].U, uvs[2].V,
            uvs[3].U, uvs[3].V);

        Debug.Log("MatB: " + b.dump());

        var H = new Mat();
        var test = Core.solve(matA, b, H);

        var H00 = H.get(0,0).First();
        var H01 = H.get(1, 0).First();
        var H02 = H.get(2, 0).First();
        var H10 = H.get(3, 0).First();
        var H11 = H.get(4, 0).First();
        var H12 = H.get(5, 0).First();
        var H20 = H.get(6, 0).First();
        var H21 = H.get(7, 0).First();
        var H22 = 1;

        var returnval = new Mat(3, 3, CvType.CV_64FC1);
        returnval.put(0, 0, H00, H01, H02, H10, H11, H12, H20, H21, H22);
        Debug.Log("MatH: " + returnval.dump());
        return returnval;
    }
}

public class Uv
{
    public double U { get; set; }
    public double V { get; set; }

    public static List<Uv> GetUvs(List<Point> points)
    {
        var list = new List<Uv>();
        points.ForEach(point => list.Add(new Uv() {U = point.x, V = point.y}));
        return list;
    }
}
