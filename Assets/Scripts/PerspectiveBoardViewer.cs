using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveBoardViewer : MonoBehaviour {

    [SerializeField]
    private float distance;

    [SerializeField]
    private Transform board;

    /// <summary>
    /// angle from camera to subject
    /// </summary>
    /// <remarks>
    /// requires vector to be normalized
    /// </remarks>
    public Vector3 angle { get; set; }

    void LateUpdate() {
        Vector3 offset = angle * distance * -1;
        transform.position = board.position + offset;
        transform.rotation = Quaternion.LookRotation(angle, Vector3.forward);
    }
}
