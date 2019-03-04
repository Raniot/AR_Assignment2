using System;
using System.Collections;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using UnityEngine;
using Vuforia;
using Random = UnityEngine.Random;

public class Contours : MonoBehaviour
{
    private Mat cameraImageMat;
    private Mat cameraImageGrayMat = new Mat();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        var rand = Random.value;
        Shader.SetGlobalVector("_randValue", new Vector2(rand, rand));

        MatDisplay.SetCameraFoV(41.5f);

        Image cameraImage = CameraDevice.Instance.GetCameraImage(Image.PIXEL_FORMAT.RGBA8888);

        if (cameraImage != null)
        {
            if (cameraImageMat == null)
            {
                //First frame -> generate Mat with same dimensions as camera feed
                cameraImageMat = new Mat(cameraImage.Height, cameraImage.Width, CvType.CV_8UC4);
            }
            cameraImageMat.put(0, 0, cameraImage.Pixels); // transferring image data to Mat
            Imgproc.cvtColor(cameraImageMat, cameraImageGrayMat, Imgproc.COLOR_BGR2GRAY);

            Mat dst = new Mat();
            Imgproc.adaptiveThreshold(cameraImageGrayMat, dst, 256, Imgproc.ADAPTIVE_THRESH_MEAN_C,
                Imgproc.THRESH_BINARY, 11, 12);

            MatDisplay.DisplayMat(dst, MatDisplaySettings.FULL_BACKGROUND);
        }
    }
}
