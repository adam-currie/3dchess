using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PerspectiveBoardViewer))]
public class FaceTrackingBoardViewer : MonoBehaviour, IObserver<Vector3> {

    /// <summary>
    /// !changing this after Start() has no effect awake!
    /// </summary>
    [SerializeField]
    private FaceTracker3D faceTracker;

    [SerializeField]
    private PerspectiveBoardViewer boardViewer;

    private IDisposable unsubscriber;

    private void Start() {
        unsubscriber = faceTracker.PointRelativeToScreen.Subscribe(this);
    }

    public void OnCompleted() {
        throw new NotImplementedException();
    }

    public void OnError(Exception error) {
        throw new NotImplementedException();
    }

    public void OnNext(Vector3 value) {
        boardViewer.angle = -1 * value.normalized;
    }

    private void OnDestroy() {
        unsubscriber.Dispose();
    }

}
