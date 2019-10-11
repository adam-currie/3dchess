
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using OpenCvSharp;
using OpenCvSharp.Demo;

public class FaceTracker3D : WebCamera {
    public TextAsset faces;
    public TextAsset eyes;
    public TextAsset shapes;

    public float distanceBetweenEyes = 60f;//mm
    public float eyeDistanceConversionConstant = .1f;
    public float vfov = 60;//degrees
    public Vector3 cameraOffset = new Vector3(0,0,0);

    private FaceProcessorLive<WebCamTexture> processor;

    private readonly ObserverMediator<Vector3> pointRelativeToCameraObservers = new ObserverMediator<Vector3>();
    public IObservable<Vector3> PointRelativeToCamera {
        get { return pointRelativeToCameraObservers; }
    }

    private readonly ObserverMediator<Vector3> pointRelativeToScreenObservers = new ObserverMediator<Vector3>();
    public IObservable<Vector3> PointRelativeToScreen {
        get { return pointRelativeToScreenObservers; }
    }

    /// <summary>
    /// Default initializer for MonoBehavior sub-classes
    /// </summary>
    protected override void Awake() {
        base.Awake();
        base.forceFrontalCamera = true; // we work with frontal cams here, let's force it for macOS s MacBook doesn't state frontal cam correctly

        byte[] shapeDat = shapes.bytes;
        if(shapeDat.Length == 0) {
            string errorMessage =
                "In order to have Face Landmarks working you must download special pre-trained shape predictor " +
                "available for free via DLib library website and replace a placeholder file located at " +
                "\"OpenCV+Unity/Assets/Resources/shape_predictor_68_face_landmarks.bytes\"\n\n" +
                "Without shape predictor demo will only detect face rects.";

#if UNITY_EDITOR
            // query user to download the proper shape predictor
            if(UnityEditor.EditorUtility.DisplayDialog("Shape predictor data missing", errorMessage, "Download", "OK, process with face rects only"))
                Application.OpenURL("http://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2");
#else
            UnityEngine.Debug.Log(errorMessage);
#endif
        }

        processor = new FaceProcessorLive<WebCamTexture>();
        processor.Initialize(faces.text, eyes.text, shapes.bytes);

        // data stabilizer - affects face rects, face landmarks etc.
        processor.DataStabilizer.Enabled = true;        // enable stabilizer
        processor.DataStabilizer.Threshold = 2.0;       // threshold value in pixels
        processor.DataStabilizer.SamplesCount = 2;      // how many samples do we need to compute stable data

        // performance data - some tricks to make it work faster
        processor.Performance.Downscale = 256;          // processed image is pre-scaled down to N px by long side
        processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)
    }

    /// <summary>
    /// Per-frame video capture processor
    /// </summary>
    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output) {
        // detect everything we're interested in
        processor.ProcessTexture(input, TextureParameters);

        if(processor.Faces.Count > 0) {
            DetectedFace face = processor.Faces[0];//todo: get the main face when there is more than 1

            DetectedObject leftEye = face.Elements[(int)DetectedFace.FaceElements.LeftEye];
            DetectedObject rightEye = face.Elements[(int)DetectedFace.FaceElements.RightEye];

            float pixelAngle = vfov / input.height;
            Point imageCenter = new Point(input.width / 2, input.height / 2);

            float angleBetweenEyes = (float)rightEye.Region.Center.DistanceTo(leftEye.Region.Center) * pixelAngle;

            //todo: use center of eyes as viewing point by default, allow changing dominant eye in options
            Point viewerPoint2d = leftEye.Region.Center;

            float viewerDistance = (vfov / angleBetweenEyes) * distanceBetweenEyes * eyeDistanceConversionConstant;

            Vector3 viewerAngleEuler = new Vector3(
                (viewerPoint2d.Y - imageCenter.Y) * pixelAngle * -1, 
                0,
                (viewerPoint2d.X - imageCenter.X) * pixelAngle * -1);

            Quaternion viewerAngle = Quaternion.Euler(viewerAngleEuler);

            Vector3 pointRelativeToCamera = (viewerAngle * Vector3.up) * viewerDistance;

            pointRelativeToCameraObservers.OnNext(pointRelativeToCamera);
            pointRelativeToScreenObservers.OnNext(pointRelativeToCamera + cameraOffset);
        }

        return faces;
    }

}