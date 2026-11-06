using UnityEngine;

public sealed class CatCamera : MonoBehaviour
{
    public Transform target;
    public bool fixedHeight;
    public bool ascent;
    float trackedHeight = 1, verticalVelocity;
    public float leftBoundary = -5;
    public float rightBoundary = 48;
    public Transform courseRoot;
    bool snap = true;
    public void Snap() => snap = true;
    void LateUpdate()
    {
        // Read the interpolated render transform, never the fixed-step Rigidbody position.
        var camera = GetComponent<Camera>();
        float scale = courseRoot ? courseRoot.lossyScale.x : 1;
        if (courseRoot) camera.orthographicSize = 5.5f * scale;
        float halfWidth = camera.orthographicSize * (16f / 9f);
        float left = courseRoot ? courseRoot.TransformPoint(new Vector3(leftBoundary, 0)).x : leftBoundary;
        float right = courseRoot ? courseRoot.TransformPoint(new Vector3(rightBoundary, 0)).x : rightBoundary;
        float floor = courseRoot ? courseRoot.TransformPoint(new Vector3(0, SceneryRenderer.CameraFloor)).y : SceneryRenderer.CameraFloor;
        if (ascent) {
            float localY = courseRoot.InverseTransformPoint(target.position).y;
            float localX = courseRoot.InverseTransformPoint(target.position).x;
            // Follow near the upper edge, leaving ordinary jumps inside a broad resting zone.
            const float upperEdge = 2.6f, lowerEdge = -2.2f;
            if (localX < 425) trackedHeight = 1;
            else if (snap) trackedHeight = Mathf.Max(1, localY - upperEdge);
            else if (localY > trackedHeight + upperEdge) trackedHeight = localY - upperEdge;
            else if (localY < trackedHeight + lowerEdge) trackedHeight = Mathf.Max(1, localY - lowerEdge);
            floor = courseRoot.TransformPoint(new Vector3(0, trackedHeight)).y;
        }
        var desired = new Vector3(Mathf.Clamp(target.position.x + 2 * scale, halfWidth + left, right - halfWidth),
            fixedHeight ? floor : Mathf.Clamp(target.position.y + 1.2f, 2.5f, 5), -10);
        var next = Vector3.Lerp(transform.position, desired, 1 - Mathf.Exp(-8 * Time.deltaTime));
        if (ascent) {
            float smoothTime = desired.y > transform.position.y ? 0.22f : 0.12f;
            next.y = Mathf.SmoothDamp(transform.position.y, desired.y, ref verticalVelocity,
                smoothTime, 24 * scale, Time.deltaTime);
        }
        transform.position = snap ? desired : next;
        if (snap) verticalVelocity = 0;
        snap = false;
    }
}
