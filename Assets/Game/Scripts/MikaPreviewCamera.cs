using UnityEngine;

// Preview-only framing. The movement course keeps its existing camera and display ownership.
public sealed class MikaPreviewCamera : MonoBehaviour
{
    public CatController target;
    public SpriteRenderer previewFloor;
    Sprite floorSprite;
    bool snap = true;
    void OnEnable() => EnsureFloor();
    void Start() { if (target) target.ReturnedToStart += Snap; EnsureFloor(); }
    public void EnsureFloor()
    {
        if (!previewFloor) {
            var floor = System.Array.Find(gameObject.scene.GetRootGameObjects(),go => go.name == "Preview floor");
            if (floor) previewFloor = floor.GetComponent<SpriteRenderer>();
        }
        if (!previewFloor || floorSprite) return;
        var texture = Texture2D.whiteTexture;
        floorSprite = Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*0.5f,texture.width);
        previewFloor.sprite = floorSprite;
    }
    void Snap() => snap = true;
    void LateUpdate()
    {
        if (!target) return;
        var position = target.transform.position;
        var desired = new Vector3(position.x,1.25f+Mathf.Max(0,position.y-0.46f)*0.8f,-10);
        transform.position = snap ? desired : Vector3.Lerp(transform.position,desired,1-Mathf.Exp(-14*Time.deltaTime));
        snap = false;
    }
    void OnDestroy()
    {
        if (target) target.ReturnedToStart -= Snap;
        if (floorSprite) Destroy(floorSprite);
    }
}
