using UnityEngine;

public sealed class CatCamera : MonoBehaviour
{
    public Transform target;
    public bool fixedHeight;
    bool snap = true;
    public void Snap() => snap = true;
    void LateUpdate()
    {
        // Read the interpolated render transform, never the fixed-step Rigidbody position.
        float halfWidth = GetComponent<Camera>().orthographicSize * (16f / 9f);
        var desired = new Vector3(Mathf.Clamp(target.position.x + 2, halfWidth - 5, 48 - halfWidth),
            fixedHeight ? BackgroundStudy.CameraFloor : Mathf.Clamp(target.position.y + 1.2f, 2.5f, 5), -10);
        transform.position = snap ? desired : Vector3.Lerp(transform.position, desired, 1 - Mathf.Exp(-8 * Time.deltaTime));
        snap = false;
    }
}
