using UnityEngine;

// World-space landmarks share a paw language, but each checkpoint has its own setting.
public sealed class CheckpointMarker : MonoBehaviour
{
    public int State { get; private set; }
    RunLifecycle run;
    int sequence;
    float clock;
    Material material;
    Sprite stoneSprite, frameSprite;

    public static CheckpointMarker Create(Transform parent, Vector2 spawn, int sequence, RunLifecycle run)
    {
        var root = new GameObject("Checkpoint waystone " + sequence);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(spawn.x - .8f, spawn.y - .52f, 1);
        var marker = root.AddComponent<CheckpointMarker>();
        marker.run = run; marker.sequence = sequence;
        var atlas = Resources.Load<Texture2D>("GoldenRiver/checkpoint-states-v1");
        marker.stoneSprite = Sprite.Create(atlas, new Rect(0, 0, atlas.width, atlas.height), Vector2.one * .5f, atlas.height, 0, SpriteMeshType.FullRect);
        var stone = new GameObject("Engraved paw").AddComponent<SpriteRenderer>();
        stone.transform.SetParent(root.transform, false);
        // The shader selects one square cell from the three-state atlas.
        stone.transform.localPosition = new Vector3(sequence == 1 ? 0 : .8f, .77f, 0);
        stone.transform.localScale = new Vector3(.7f, 2.1f, 1);
        stone.sprite = marker.stoneSprite;
        marker.material = new Material(Resources.Load<Material>("GoldenRiver/checkpoint-material"));
        stone.sharedMaterial = marker.material;
        string frameName = sequence == 1 ? "checkpoint-tree-v1" : "checkpoint-arch-v1";
        var frameTexture = Resources.Load<Texture2D>("GoldenRiver/" + frameName);
        marker.frameSprite = Sprite.Create(frameTexture, new Rect(0, 0, frameTexture.width, frameTexture.height),
            new Vector2(.5f, 0), frameTexture.width);
        var frame = new GameObject(sequence == 1 ? "Checkpoint tree" : "Checkpoint ruin arch").AddComponent<SpriteRenderer>();
        frame.transform.SetParent(root.transform, false);
        frame.sprite = marker.frameSprite;
        float width = sequence == 1 ? 10 : 4.8f;
        float baseUV = sequence == 1 ? .03f : .04f;
        frame.transform.localScale = Vector3.one * width;
        frame.transform.localPosition = new Vector3(sequence == 1 ? -.7f : .8f,
            -baseUV * width * frameTexture.height / frameTexture.width, 1);
        marker.Refresh(0);
        return marker;
    }

    public void Refresh(float deltaTime)
    {
        State = run.Checkpoint < sequence ? 0 : run.Checkpoint == sequence ? 1 : 2;
        if (!run.Paused) clock += deltaTime;
        material.SetFloat("_State", State);
        material.SetFloat("_Pulse", .5f + .5f * Mathf.Sin(clock * Mathf.PI * 2 / 3.6f));
    }

    void OnDestroy()
    {
        if (Application.isPlaying) { Destroy(material); Destroy(stoneSprite); Destroy(frameSprite); }
        else { DestroyImmediate(material); DestroyImmediate(stoneSprite); DestroyImmediate(frameSprite); }
    }
}
