using UnityEngine;

public sealed class CatCamera : MonoBehaviour
{
    public Transform target;
    public bool fixedHeight;
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
        float left = courseRoot ? courseRoot.TransformPoint(new Vector3(-5, 0)).x : -5;
        float right = courseRoot ? courseRoot.TransformPoint(new Vector3(rightBoundary, 0)).x : rightBoundary;
        float floor = courseRoot ? courseRoot.TransformPoint(new Vector3(0, BackgroundStudy.CameraFloor)).y : BackgroundStudy.CameraFloor;
        var desired = new Vector3(Mathf.Clamp(target.position.x + 2 * scale, halfWidth + left, right - halfWidth),
            fixedHeight ? floor : Mathf.Clamp(target.position.y + 1.2f, 2.5f, 5), -10);
        transform.position = snap ? desired : Vector3.Lerp(transform.position, desired, 1 - Mathf.Exp(-8 * Time.deltaTime));
        snap = false;
    }
}
