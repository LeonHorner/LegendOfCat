using UnityEngine;

// Preview-only framing. The movement course keeps its existing camera and display ownership.
public sealed class MikaPreviewCamera : MonoBehaviour
{
    public CatController target;
    bool snap = true;
    void Start() { if (target) target.ReturnedToStart += Snap; }
    void Snap() => snap = true;
    void LateUpdate()
    {
        if (!target) return;
        var position = target.transform.position;
        var desired = new Vector3(position.x,1.25f+Mathf.Max(0,position.y-0.46f)*0.8f,-10);
        transform.position = snap ? desired : Vector3.Lerp(transform.position,desired,1-Mathf.Exp(-14*Time.deltaTime));
        snap = false;
    }
    void OnDestroy() { if (target) target.ReturnedToStart -= Snap; }
}
