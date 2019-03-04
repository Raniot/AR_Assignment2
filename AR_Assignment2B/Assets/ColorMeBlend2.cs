using System.Collections.Generic;
using System.Linq;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UtilsModule;
using UnityEngine;
using Vuforia;

public class ColorMeBlend2 : MonoBehaviour
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

        if (Input.GetKey(KeyCode.Space))
        {
            var texMat = MatDisplay.LoadRGBATexture(@"Models\flying_skull_tex.png");

            var srcPoints = new List<Point> { imgPnt2, imgPnt1, imgPnt4, imgPnt3 };
            var dstPoints = new List<Point>
            {
                new Point(0, ImageTargetHeight),
                new Point(ImageTargetWidth, ImageTargetHeight),
                new Point(ImageTargetWidth, 0),
                new Point(0, 0),
            };

            var matObj = new MatOfPoint2f(srcPoints.ToArray());
            var matDst = new MatOfPoint2f(dstPoints.ToArray());
            var H = Calib3d.findHomography(matObj, matDst);

            var warpedMat = new Mat();

            Imgproc.warpPerspective(texMat, warpedMat, H.inv(), _camImageMat.size(),
                Imgproc.INTER_LINEAR);
            warpedMat.convertTo(warpedMat, _camImageMat.type());

            var blendTex = new Mat();

            Core.addWeighted(_camImageMat, 0.95f, warpedMat, 0.4f, 0.0, blendTex);

            MatDisplay.DisplayMat(blendTex, MatDisplaySettings.FULL_BACKGROUND);
        }
        else
        {
            //Display the Mat that includes video feed and debug points
            MatDisplay.DisplayMat(_camImageMat, MatDisplaySettings.FULL_BACKGROUND);
        }

        //---- MATCH INTRINSICS OF REAL CAMERA AND PROJECTION MATRIX OF VIRTUAL CAMERA ----
        Cam.projectionMatrix = Projection.PerspectiveOffCenter(Cam.nearClipPlane, Cam.farClipPlane);
    }
}
