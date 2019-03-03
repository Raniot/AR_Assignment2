using System.Collections.Generic;
using System.Linq;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using UnityEngine;
using Vuforia;
using Vector3 = UnityEngine.Vector3;

public class BlackBorder : MonoBehaviour
{

    public Camera Cam;
    public GameObject ObjectToColor;

    public int ImageTargetWidth = 1024;
    public int ImageTargetHeight = 1024;

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
        var blackWhiteMat = new Mat();
        Imgproc.cvtColor(_camImageMat, blackWhiteMat, Imgproc.COLOR_BGR2GRAY);

        Imgproc.threshold(blackWhiteMat, blackWhiteMat, 50, 255, Imgproc.THRESH_BINARY);
        var list = new List<MatOfPoint>();

        Imgproc.findContours(blackWhiteMat, list, new Mat(), Imgproc.RETR_LIST, Imgproc.CHAIN_APPROX_SIMPLE);
        var newList = new List<MatOfPoint2f>();
        var counter = 0;
        list.ForEach(x =>
        {
            counter++;
            var mat = new MatOfPoint2f();
            Imgproc.approxPolyDP(new MatOfPoint2f(x.toArray()), mat, 5, true);
            newList.Add(mat);
            //Imgproc.drawContours(_camImageMat, list, counter, new Scalar(0, 255, 0, 0.8), 1);

        });

        var candidates = new List<MatOfPoint2f>();

        foreach (MatOfPoint2f x in newList)
        {
            var arr = x.toArray();
            if (arr.Length != 4) continue;

            var corner1 = new Vector3((float)arr[0].x, (float)arr[0].y, 1f);
            var corner2 = new Vector3((float)arr[1].x, (float)arr[1].y, 1f);
            
            if (Vector3.Cross(corner1, corner2).z > 0) continue;
            if (Vector3.Distance(corner1, corner2) < 200) continue;

            candidates.Add(x);
            var matofpoint = new MatOfPoint();
            x.convertTo(matofpoint, CvType.CV_32S);
            var listmatofpoint = new List<MatOfPoint> { matofpoint };

            Imgproc.drawContours(_camImageMat, listmatofpoint, 0, new Scalar(255, 0, 255, 0.8), 5);
        }

        if (!candidates.Any())
        {
            MatDisplay.DisplayMat(_camImageMat, MatDisplaySettings.FULL_BACKGROUND);
            Cam.projectionMatrix = Projection.PerspectiveOffCenter(Cam.nearClipPlane, Cam.farClipPlane);
            return;
        }

        var srcPoints = new List<Point>
        {
            candidates[0].toArray()[1], candidates[0].toArray()[0], candidates[0].toArray()[3],
            candidates[0].toArray()[2]
        };
        var dstPoints = new List<Point>
        {
            new Point(0, ImageTargetHeight),
            new Point(ImageTargetWidth, ImageTargetHeight),
            new Point(ImageTargetWidth, 0),
            new Point(0, 0),
        };

        if (Input.GetKey(KeyCode.Space))
        {
            var texMat = MatDisplay.LoadRGBATexture(@"Models\flying_skull_tex.png");

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
        if (Input.GetKey(KeyCode.LeftControl))
        {
            var matObj = new MatOfPoint2f(srcPoints.ToArray());
            var matDst = new MatOfPoint2f(dstPoints.ToArray());
            var H = Calib3d.findHomography(matObj, matDst);

            var warpedMat = new Mat(new Size(ImageTargetWidth, ImageTargetHeight), _camImageMat.type());

            Imgproc.warpPerspective(_camImageMat, warpedMat, H, new Size(ImageTargetWidth, ImageTargetHeight),
                Imgproc.INTER_LINEAR);
            warpedMat.convertTo(warpedMat, CvType.CV_8UC3);

            var newTexture = new Texture2D(ImageTargetWidth, ImageTargetHeight, mipChain: false, textureFormat: TextureFormat.RGBA32);
            MatDisplay.MatToTexture(warpedMat, ref newTexture);

            ObjectToColor.GetComponent<MeshRenderer>().material.mainTexture = newTexture;
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
