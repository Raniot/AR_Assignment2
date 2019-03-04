using System.Collections.Generic;
using System.Linq;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using UnityEngine;
using Vuforia;
using Vector3 = UnityEngine.Vector3;

public class BlackBorder : MonoBehaviour
{

    public Camera Cam;
    public GameObject ObjectToColor;
    private Mat _texMat;
    public string RgbaTexturePath;

    private int _imageTargetWidth;
    private int _imageTargetHeight;

    private MatOfPoint2f _imagePoints;
    private Mat _camImageMat;
    private byte[] _texData;

    void Start()
    {
        _imagePoints = new MatOfPoint2f();
        _imagePoints.alloc(4);
        _texMat = MatDisplay.LoadRGBATexture(RgbaTexturePath);
        _imageTargetHeight = _texMat.height();
        _imageTargetWidth = _texMat.width();
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


        var corners = FindCorners();

        if (corners == null)
        {
            Display(_camImageMat);
            return;
        }

        var srcPoints = new List<Point>
        {
            corners[1], corners[0], corners[3], corners[2]
        };
        var dstPoints = new List<Point>
        {
            new Point(0, _imageTargetHeight),
            new Point(_imageTargetWidth, _imageTargetHeight),
            new Point(_imageTargetWidth, 0),
            new Point(0, 0),
        };

        if (Input.GetKey(KeyCode.Space))
        {
            var H = CalcHomography(srcPoints, dstPoints);

            var warpedMat = new Mat();
            Imgproc.warpPerspective(_texMat, warpedMat, H.inv(), _camImageMat.size(),
                Imgproc.INTER_LINEAR);
            warpedMat.convertTo(warpedMat, _camImageMat.type());

            var blendTex = new Mat();
            Core.addWeighted(_camImageMat, 0.95f, warpedMat, 0.4f, 0.0, blendTex);

            Display(blendTex);
            return;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            var H = CalcHomography(srcPoints, dstPoints);

            var warpedMat = new Mat(new Size(_imageTargetWidth, _imageTargetHeight), _camImageMat.type());

            Imgproc.warpPerspective(_camImageMat, warpedMat, H, new Size(_imageTargetWidth, _imageTargetHeight),
                Imgproc.INTER_LINEAR);
            warpedMat.convertTo(warpedMat, CvType.CV_8UC3);

            var newTexture = new Texture2D(_imageTargetWidth, _imageTargetHeight, mipChain: false, textureFormat: TextureFormat.RGBA32);
            MatDisplay.MatToTexture(warpedMat, ref newTexture);

            ObjectToColor.GetComponent<MeshRenderer>().material.mainTexture = newTexture;
        }
        
        Display(_camImageMat);
    }

    private Point[] FindCorners()
    {
        var blackWhiteMat = BlackWhiteImage();
        var approxContours = FindContours(blackWhiteMat);
        var candidateCorners = ValidateForOuterSquareCorners(approxContours);

        return candidateCorners.FirstOrDefault()?.toArray();
    }

    private List<MatOfPoint2f> ValidateForOuterSquareCorners(List<MatOfPoint2f> approxContours)
    {
        var candidateCorners = new List<MatOfPoint2f>();

        approxContours.ForEach(x =>
        {
            var arr = x.toArray();
            if (arr.Length != 4) return;

            var corner1 = new Vector3((float) arr[0].x, (float) arr[0].y, 1f);
            var corner2 = new Vector3((float) arr[1].x, (float) arr[1].y, 1f);
            var corner3 = new Vector3((float)arr[2].x, (float)arr[2].y, 1f);

            if (Vector3.Cross(corner1, corner2).z > 0) return; //Checks for outer bounds objects
            if (Vector3.Distance(corner1, corner2) < 200) return;
            if (Vector3.Distance(corner2, corner3) < 200) return;

            candidateCorners.Add(x);
            DrawContour(x);
        });

        return candidateCorners;
    }

    private static List<MatOfPoint2f> FindContours(Mat blackWhiteMat)
    {
        var contours = new List<MatOfPoint>();
        Imgproc.findContours(blackWhiteMat, contours, new Mat(), Imgproc.RETR_LIST, Imgproc.CHAIN_APPROX_SIMPLE);

        var approxContours = new List<MatOfPoint2f>();
        contours.ForEach(x =>
        {
            var mat = new MatOfPoint2f();
            Imgproc.approxPolyDP(new MatOfPoint2f(x.toArray()), mat, 5, true);
            approxContours.Add(mat);
        });
        return approxContours;
    }

    private Mat BlackWhiteImage()
    {
        var blackWhiteMat = new Mat();
        Imgproc.cvtColor(_camImageMat, blackWhiteMat, Imgproc.COLOR_BGR2GRAY);

        Imgproc.threshold(blackWhiteMat, blackWhiteMat, 50, 255, Imgproc.THRESH_BINARY);
        return blackWhiteMat;
    }

    private void DrawContour(MatOfPoint2f x)
    {
        var matofpoint = new MatOfPoint();
        x.convertTo(matofpoint, CvType.CV_32S);
        var listmatofpoint = new List<MatOfPoint> {matofpoint};

        Imgproc.drawContours(_camImageMat, listmatofpoint, 0, new Scalar(255, 0, 255, 0.8), 5);
    }

    private void Display(Mat srcMat)
    {
        MatDisplay.DisplayMat(srcMat, MatDisplaySettings.FULL_BACKGROUND);
        Cam.projectionMatrix = Projection.PerspectiveOffCenter(Cam.nearClipPlane, Cam.farClipPlane);
    }

    private static Mat CalcHomography(List<Point> srcPoints, List<Point> dstPoints)
    {
        var xy1 = srcPoints[0];
        var xy2 = srcPoints[1];
        var xy3 = srcPoints[2];
        var xy4 = srcPoints[3];

        var uvs = Uv.GetUvs(dstPoints);

        var matA = new Mat(8, 8, CvType.CV_64FC1);
        matA.put(0, 0,
            xy1.x, xy1.y, 1, 0, 0, 0, -uvs[0].U * xy1.x, -uvs[0].U * xy1.y,
            0, 0, 0, xy1.x, xy1.y, 1, -uvs[0].V * xy1.x, -uvs[0].V * xy1.y,

            xy2.x, xy2.y, 1, 0, 0, 0, -uvs[1].U * xy2.x, -uvs[1].U * xy2.y,
            0, 0, 0, xy2.x, xy2.y, 1, -uvs[1].V * xy2.x, -uvs[1].V * xy2.y,

            xy3.x, xy3.y, 1, 0, 0, 0, -uvs[2].U * xy3.x, -uvs[2].U * xy3.y,
            0, 0, 0, xy3.x, xy3.y, 1, -uvs[2].V * xy3.x, -uvs[2].V * xy3.y,

            xy4.x, xy4.y, 1, 0, 0, 0, -uvs[3].U * xy4.x, -uvs[3].U * xy4.y,
            0, 0, 0, xy4.x, xy4.y, 1, -uvs[3].V * xy4.x, -uvs[3].V * xy4.y);

        var b = new Mat(8, 1, CvType.CV_64FC1);
        b.put(0, 0,
            uvs[0].U, uvs[0].V, uvs[1].U, uvs[1].V, uvs[2].U, uvs[2].V,
            uvs[3].U, uvs[3].V);

        var H = new Mat(3, 3, CvType.CV_64FC1);
        Core.solve(matA, b, H);

        var H00 = H.get(0, 0).First();
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

        return returnval;
    }
}
