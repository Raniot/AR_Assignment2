using System;
using System.Collections;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using UnityEngine;
using Vuforia;

public class Test : MonoBehaviour
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
            MatDisplay.DisplayMat(cameraImageGrayMat, MatDisplaySettings.FULL_BACKGROUND);
        }
    }
}
