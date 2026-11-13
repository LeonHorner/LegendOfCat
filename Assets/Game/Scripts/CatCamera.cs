using UnityEngine;

public sealed class CatCamera : MonoBehaviour
{
    public Transform target;
    public bool fixedHeight;
    public bool ascent;
    float trackedHeight = 1, verticalVelocity, lookAhead = 2, lookVelocity;
    int facing = 1;
    CatController rider;
    Camera view;
    public float leftBoundary = -5;
    public float rightBoundary = 48;
    public Transform courseRoot;
    public System.Func<Vector2,float> HeightGuide;
    public System.Func<Vector2,float?> PassageHeightGuide;
    public System.Func<float> PassageDepthGuide;
    bool snap = true;
    public void Snap() { snap = true; }
    void LateUpdate() => Follow(Time.deltaTime);

    public void Follow(float deltaSeconds)
    {
        // Read the interpolated render transform, never the fixed-step Rigidbody position.
        if(!view) view=GetComponent<Camera>();
        var camera=view;
        float scale = courseRoot ? courseRoot.lossyScale.x : 1;
        if (courseRoot) camera.orthographicSize = 5.5f * scale;
        float halfWidth = camera.orthographicSize * (16f / 9f);
        float left = courseRoot ? courseRoot.TransformPoint(new Vector3(leftBoundary, 0)).x : leftBoundary;
        float right = courseRoot ? courseRoot.TransformPoint(new Vector3(rightBoundary, 0)).x : rightBoundary;
        float floor = courseRoot ? courseRoot.TransformPoint(new Vector3(0, SceneryRenderer.CameraFloor)).y : SceneryRenderer.CameraFloor;
        if (HeightGuide!=null) floor=courseRoot.TransformPoint(new Vector3(0,HeightGuide(courseRoot.InverseTransformPoint(target.position)))).y;
        else if (ascent) {
            float localY = courseRoot.InverseTransformPoint(target.position).y;
            float localX = courseRoot.InverseTransformPoint(target.position).x;
            // Follow near the upper edge, leaving ordinary jumps inside a broad resting zone.
            const float upperEdge = 2.6f, lowerEdge = -2.2f;
            if (localX < 425) trackedHeight = 1;
            else if (snap) trackedHeight = Mathf.Max(1, localY - upperEdge);
            else if (localY > trackedHeight + upperEdge) trackedHeight = localY - upperEdge;
            else if (localY < trackedHeight + lowerEdge) trackedHeight = Mathf.Max(1, localY - lowerEdge);
            if(localX>=FinaleRoute.StartX && localY>10) trackedHeight=Mathf.Max(trackedHeight,FinaleRoute.CameraMinimum(localX));
            if(localX>FinaleRoute.Last.x-35 && localY>FinaleRoute.Last.y-1) {
                float reveal=Mathf.SmoothStep(0,1,Mathf.InverseLerp(FinaleRoute.Last.x-35,FinaleRoute.Last.x-8,localX));
                trackedHeight=Mathf.Lerp(trackedHeight,Mathf.Max(trackedHeight,FinaleRoute.Last.y+1.8f),reveal);
            }
            floor = courseRoot.TransformPoint(new Vector3(0, trackedHeight)).y;
        }
        float? passage=PassageHeightGuide?.Invoke(courseRoot.InverseTransformPoint(target.position));
        if(passage.HasValue)floor=courseRoot.TransformPoint(new Vector3(0,passage.Value)).y;
        if (HeightGuide!=null || passage.HasValue || ascent && (courseRoot.InverseTransformPoint(target.position).x >= FinaleRoute.StartX)) {
            if(!rider) rider=target.GetComponent<CatController>();
            float movement=rider?rider.PresentationVelocity.x:0;
            // Carrier drift must not reverse the look-ahead while Mika stands still.
            if (!snap && Mathf.Abs(movement) > .1f) facing = movement > 0 ? 1 : -1;
            lookAhead = snap ? facing * 2 : Mathf.SmoothDamp(lookAhead, facing * 2, ref lookVelocity, .3f, Mathf.Infinity, deltaSeconds);
        } else { lookAhead = 2; facing = 1; lookVelocity = 0; }
        var desired = new Vector3(Mathf.Clamp(target.position.x + lookAhead * scale, halfWidth + left, right - halfWidth),
            fixedHeight ? floor : Mathf.Clamp(target.position.y + 1.2f, 2.5f, 5), PassageDepthGuide?.Invoke()??-10);
        var next = Vector3.Lerp(transform.position, desired, 1 - Mathf.Exp(-8 * deltaSeconds));
        if (ascent || HeightGuide!=null || passage.HasValue) {
            float smoothTime = desired.y > transform.position.y ? 0.22f : 0.12f;
            next.y = Mathf.SmoothDamp(transform.position.y, desired.y, ref verticalVelocity,
                smoothTime, 24 * scale, deltaSeconds);
        }
        transform.position = snap ? desired : next;
        if (snap) verticalVelocity = 0;
        snap = false;
    }
}
