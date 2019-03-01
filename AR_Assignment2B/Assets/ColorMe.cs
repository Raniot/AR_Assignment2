﻿using System.Collections;
using System.Collections.Generic;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UtilsModule;
using UnityEngine;
using Vuforia;
using Button = UnityEngine.UI.Button;
using Image = Vuforia.Image;

public class ColorMe : MonoBehaviour
{

    public Camera cam;

    public GameObject corner1;
    public GameObject corner2;
    public GameObject corner3;
    public GameObject corner4;

    public int imageTargetWidth = 1122;
    public int imageTargetHeight = 601;

    //Default values: guessed, not calibrated
    //public float fx = 650;
    //public float fy = 650;
    //public float cx = 320;
    //public float cy = 240;

    public float fx = 842.34573f;
    public float fy = 842.03391f;
    public float cx = 310.45239f;
    public float cy = 243.37875f;

    private MatOfPoint2f imagePoints;
    private Mat camImageMat;
    private byte[] texData;

    // Start is called before the first frame update
    void Start()
    {
        imagePoints = new MatOfPoint2f();
        imagePoints.alloc(4);
    }

    // Update is called once per frame
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
                var srcPoints = new List<Point> { imgPnt2, imgPnt1, imgPnt4, imgPnt3 };
                var dstPoints = new List<Point>
                {
                    new Point(0, imageTargetHeight),
                    new Point(imageTargetWidth, imageTargetHeight),
                    new Point(imageTargetWidth, 0),
                    new Point(0, 0),
                };

                var mat_obj = new MatOfPoint2f(srcPoints.ToArray());
                //mat_obj.alloc(4);
                var mat_dst = new MatOfPoint2f(dstPoints.ToArray());
                //mat_dst.alloc(4);

                var H = Calib3d.findHomography(mat_obj, mat_dst);

                //var srcPointsMat = Converters.vector_Point_to_Mat(srcPoints, CvType.CV_32F);
                //var dstPointsMat = Converters.vector_Point_to_Mat(dstPoints, CvType.CV_32F);
                //var H = Imgproc.getPerspectiveTransform(srcPointsMat, dstPointsMat);
                var warpedMat = new Mat(new Size(imageTargetWidth, imageTargetHeight), camImageMat.type());
                
                Imgproc.warpPerspective(camImageMat, warpedMat, H, new Size(imageTargetWidth, imageTargetHeight),
                    Imgproc.INTER_LINEAR);
                warpedMat.convertTo(warpedMat, CvType.CV_8UC3);

                var newTexture = new Texture2D(imageTargetWidth, imageTargetHeight, mipChain:false, textureFormat:TextureFormat.RGBA32);
                MatDisplay.MatToTexture(warpedMat, ref newTexture);
              
                transform.GetComponent<MeshRenderer>().material.mainTexture = newTexture;
            }

            //Display the Mat that includes video feed and debug points
            MatDisplay.DisplayMat(camImageMat, MatDisplaySettings.FULL_BACKGROUND);


            //---- MATCH INTRINSICS OF REAL CAMERA AND PROJECTION MATRIX OF VIRTUAL CAMERA ----

            //Replace with your own projection matrix. This approach only uses fy.
            //See lecture slides for why this formular works.

            cam.projectionMatrix = Projection.PerspectiveOffCenter(cam.nearClipPlane, cam.farClipPlane);
            //cam.fieldOfView = 2 * Mathf.Atan(camImg.Height * 0.5f / fy) * Mathf.Rad2Deg;
        }
    }
}
