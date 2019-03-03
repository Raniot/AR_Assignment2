using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.UtilsModule;
using OpenCVForUnity.UnityUtils;

public class colorJens : MonoBehaviour
{
    public Camera cam;

    [SerializeField]
    GameObject corner1, corner2, corner3, corner4;

    public GameObject projectionObject;

    //Default values: guessed, not calibrated
    public float fx = 650;
    public float fy = 650;
    public float cx = 320;
    public float cy = 240;

    private MatOfPoint2f imagePoints;
    private Mat camImageMat;
    private byte[] texData;


    void Start()
    {
        imagePoints = new MatOfPoint2f();
        imagePoints.alloc(4);
    }

    void Update()
    {
        //Access camera image provided by Vuforia
        Image camImg = CameraDevice.Instance.GetCameraImage(Image.PIXEL_FORMAT.RGBA8888);

        if (camImg != null)
        {
            if (camImageMat == null)
            {
                //First valid camera frame -> instantiate camera image with frame dimensions
                camImageMat = new Mat(camImg.Height, camImg.Width, CvType.CV_8UC4);  //Note: rows=height, cols=width
            }
            camImageMat.put(0, 0, camImg.Pixels);


            //---- TRANSFORM/PROJECT CORNER WORLD COORDINATES TO IMAGE COORDINATES ----
            Vector3 worldPnt1 = corner1.transform.position;
            Vector3 worldPnt2 = corner2.transform.position;
            Vector3 worldPnt3 = corner3.transform.position;
            Vector3 worldPnt4 = corner4.transform.position;

            //See lecture slides
            Matrix4x4 Rt = cam.transform.worldToLocalMatrix;
            Matrix4x4 A = Matrix4x4.identity;
            A.m00 = fx;
            A.m11 = fy;
            A.m02 = cx;
            A.m12 = cy;

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
            imagePoints.put(0, 0, uv1.x, maxV - uv1.y);
            imagePoints.put(1, 0, uv2.x, maxV - uv2.y);
            imagePoints.put(2, 0, uv3.x, maxV - uv3.y);
            imagePoints.put(3, 0, uv4.x, maxV - uv4.y);


            //Debug draw points using OpenCV's drawing functions
            Point imgPnt1 = new Point(imagePoints.get(0, 0));
            Point imgPnt2 = new Point(imagePoints.get(1, 0));
            Point imgPnt3 = new Point(imagePoints.get(2, 0));
            Point imgPnt4 = new Point(imagePoints.get(3, 0));
            Imgproc.circle(camImageMat, imgPnt1, 5, new Scalar(255, 0, 0, 255));
            Imgproc.circle(camImageMat, imgPnt2, 5, new Scalar(0, 255, 0, 255));
            Imgproc.circle(camImageMat, imgPnt3, 5, new Scalar(0, 0, 255, 255));
            Imgproc.circle(camImageMat, imgPnt4, 5, new Scalar(255, 255, 0, 255));
            Scalar lineCl = new Scalar(200, 120, 0, 160);
            Imgproc.line(camImageMat, imgPnt1, imgPnt2, lineCl);
            Imgproc.line(camImageMat, imgPnt2, imgPnt3, lineCl);
            Imgproc.line(camImageMat, imgPnt3, imgPnt4, lineCl);
            Imgproc.line(camImageMat, imgPnt4, imgPnt1, lineCl);


            if (Input.GetKeyDown(KeyCode.Space))
            {
                //making scr points mat
                List<Point> srcPoints = new List<Point>();
                srcPoints.Add(imgPnt1);
                srcPoints.Add(imgPnt2);
                srcPoints.Add(imgPnt3);
                srcPoints.Add(imgPnt4);
                Mat srcPointsMat = Converters.vector_Point_to_Mat(srcPoints, CvType.CV_32F);

                //making destination mat
                MatOfPoint2f destinationPoints = new MatOfPoint2f();
                destinationPoints.alloc(4);

                List<Point> dstPoints = new List<Point>();
                dstPoints.Add(new Point(0, 601));
                dstPoints.Add(new Point(1122, 601));
                dstPoints.Add(new Point(1122, 0));
                dstPoints.Add(new Point(0, 0));
                Mat dstPointsMat = Converters.vector_Point_to_Mat(dstPoints, CvType.CV_32F);

                //make perspective transform
                Mat M = Imgproc.getPerspectiveTransform(srcPointsMat, dstPointsMat);
                Mat warpedMat = new Mat(new Size(1122, 601), camImageMat.type());

                //crop and warp the image
                Imgproc.warpPerspective(camImageMat, warpedMat, M, new Size(1122, 601), Imgproc.INTER_LINEAR);
                warpedMat.convertTo(warpedMat, CvType.CV_8UC3);

                var newTexture = new Texture2D(1122, 601);
                MatDisplay.MatToTexture(warpedMat, ref newTexture);
                projectionObject.GetComponent<SkinnedMeshRenderer>().material.mainTexture = newTexture;
            }

            //Display the Mat that includes video feed and debug points
            MatDisplay.DisplayMat(camImageMat, MatDisplaySettings.FULL_BACKGROUND);
        }
    }
}
