using UnityEngine;

// World-space landmarks share a paw language, but each checkpoint has its own setting.
public sealed class CheckpointMarker : MonoBehaviour
{
    public SpriteRenderer WorldVisual { get; private set; }
    public static Rect ActiveArtworkUv => new Rect(1f / 3, 0, 1f / 3, 1);
    public int State { get; private set; }
    RunLifecycle run;
    int sequence;
    float clock;
    Material material;
    Sprite stoneSprite, frameSprite;
    PolygonCollider2D touchZone;
    Mesh footingMesh;
    Mesh sparkMesh;
    Material sparkMaterial;
    MeshRenderer sparkRenderer;
    float activeLight, burstAge = 10;
    readonly Vector3[] sparkVertices = new Vector3[56];
    readonly Color[] sparkColors = new Color[56];

    public bool Touches(Collider2D cat)
    {
        var contact = touchZone.Distance(cat);
        return contact.isValid && (contact.isOverlapped || contact.distance <= .01f);
    }

    public static CheckpointMarker Create(Transform parent, Vector2 spawn, int sequence, RunLifecycle run, Transform ground)
    {
        var root = new GameObject("Checkpoint waystone " + sequence);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(spawn.x - .8f, spawn.y - .60f, 1);
        var marker = root.AddComponent<CheckpointMarker>();
        marker.run = run; marker.sequence = sequence;
        var atlas = Resources.Load<Texture2D>("GoldenRiver/checkpoint-states-v1");
        marker.stoneSprite = Sprite.Create(atlas, new Rect(0, 0, atlas.width, atlas.height), Vector2.one * .5f, atlas.height, 0, SpriteMeshType.FullRect);
        var stone = new GameObject("Engraved paw").AddComponent<SpriteRenderer>();
        marker.WorldVisual = stone;
        stone.transform.SetParent(root.transform, false);
        // The shader selects one square cell from the three-state atlas.
        stone.transform.localPosition = new Vector3(sequence == 1 ? 0 : .8f, .77f, 0);
        stone.transform.localScale = new Vector3(.7f, 2.1f, 1);
        stone.sprite = marker.stoneSprite;
        marker.material = new Material(Resources.Load<Material>("GoldenRiver/checkpoint-material"));
        stone.sharedMaterial = marker.material;
        float stoneX = sequence == 1 ? 0 : .8f;
        marker.touchZone = root.AddComponent<PolygonCollider2D>();
        marker.touchZone.isTrigger = true;
        marker.touchZone.offset = new Vector2(stoneX, 0);
        marker.touchZone.points = new[] {
            new Vector2(-.6f, -.04f), new Vector2(.6f, -.04f), new Vector2(.48f, .65f),
            new Vector2(.27f, 1.55f), new Vector2(.05f, 1.65f), new Vector2(-.2f, 1.48f), new Vector2(-.45f, .8f)
        };
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
        marker.AddFooting(ground, spawn.x, sequence == 1 ? -7 : -2.6f, sequence == 1 ? .5f : 2.6f);
        marker.CreateSparks(stoneX);
        marker.Refresh(0);
        return marker;
    }

    void CreateSparks(float stoneX)
    {
        sparkMesh = new Mesh { name = "Checkpoint activation motes" };
        var uv = new Vector2[56]; var triangles = new int[84];
        for (int i = 0; i < 14; i++) {
            int v = i * 4, t = i * 6;
            uv[v] = Vector2.zero; uv[v+1] = Vector2.right; uv[v+2] = Vector2.up; uv[v+3] = Vector2.one;
            triangles[t] = v; triangles[t+1] = v+2; triangles[t+2] = v+1;
            triangles[t+3] = v+1; triangles[t+4] = v+2; triangles[t+5] = v+3;
        }
        sparkMesh.vertices = sparkVertices; sparkMesh.uv = uv; sparkMesh.triangles = triangles;
        var sparks = new GameObject("Checkpoint sparks", typeof(MeshFilter), typeof(MeshRenderer));
        sparks.transform.SetParent(transform, false);
        sparks.transform.localPosition = new Vector3(stoneX,0,-.2f);
        sparks.GetComponent<MeshFilter>().sharedMesh = sparkMesh;
        sparkMaterial = new Material(material); sparkMaterial.SetFloat("_Spark",1);
        sparkRenderer = sparks.GetComponent<MeshRenderer>(); sparkRenderer.sharedMaterial = sparkMaterial;
        sparkRenderer.enabled = false;
    }

    void DrawSparks()
    {
        bool visible = State == 1 && burstAge < 2.2f;
        sparkRenderer.enabled = visible;
        if (!visible) return;
        for (int i = 0; i < 14; i++) {
            float seed = Mathf.Repeat(i * .618034f,1);
            float age = burstAge - i * .035f;
            float life = 1.1f + seed * .55f;
            float progress = Mathf.Clamp01(age/life);
            float alpha = age < 0 ? 0 : Mathf.Sin(progress * Mathf.PI) * .95f;
            var center = new Vector3((seed-.5f)*.65f + Mathf.Sin(age*3+i)*progress*.18f,
                .8f + (i%3)*.16f + progress*(.7f+seed*.5f));
            float size = .035f + seed*.035f;
            int v = i*4;
            sparkVertices[v] = center + new Vector3(-size,-size);
            sparkVertices[v+1] = center + new Vector3(size,-size);
            sparkVertices[v+2] = center + new Vector3(-size,size);
            sparkVertices[v+3] = center + new Vector3(size,size);
            for (int j = 0; j < 4; j++) sparkColors[v+j] = new Color(1,.65f+seed*.2f,.15f,alpha);
        }
        sparkMesh.vertices = sparkVertices; sparkMesh.colors = sparkColors; sparkMesh.RecalculateBounds();
    }

    void AddFooting(Transform ground, float center, float left, float right)
    {
        // Sample the original bank's exact UVs: this is its grass edge in front of the embedded feet.
        var source = ground.GetComponent<MeshFilter>().sharedMesh;
        var vertices = source.vertices; var uv = source.uv;
        float contact = vertices[2].y;
        float x0 = center + left - ground.localPosition.x, x1 = center + right - ground.localPosition.x;
        float u0 = Mathf.Lerp(uv[0].x, uv[1].x, Mathf.InverseLerp(vertices[0].x, vertices[1].x, x0));
        float u1 = Mathf.Lerp(uv[0].x, uv[1].x, Mathf.InverseLerp(vertices[0].x, vertices[1].x, x1));
        float bottom = contact - .5f, top = vertices[4].y;
        float v0 = Mathf.Lerp(uv[0].y, uv[2].y, Mathf.InverseLerp(vertices[0].y, contact, bottom));
        float v1 = Mathf.Lerp(uv[2].y, uv[4].y, Mathf.InverseLerp(contact, vertices[4].y, top));
        footingMesh = new Mesh { name = "Registered checkpoint grass edge" };
        footingMesh.vertices = new[] { new Vector3(x0,bottom), new Vector3(x1,bottom),
            new Vector3(x0,contact), new Vector3(x1,contact), new Vector3(x0,top), new Vector3(x1,top) };
        footingMesh.uv = new[] { new Vector2(u0,v0), new Vector2(u1,v0), new Vector2(u0,uv[2].y),
            new Vector2(u1,uv[2].y), new Vector2(u0,v1), new Vector2(u1,v1) };
        footingMesh.triangles = new[] { 0,2,1, 1,2,3, 2,4,3, 3,4,5 };
        footingMesh.RecalculateBounds();
        var edge = new GameObject("Checkpoint grass overlap", typeof(MeshFilter), typeof(MeshRenderer));
        edge.transform.SetParent(transform, false);
        edge.transform.position = ground.TransformPoint(new Vector3(0,0,.5f));
        edge.GetComponent<MeshFilter>().sharedMesh = footingMesh;
        edge.GetComponent<MeshRenderer>().sharedMaterial = ground.GetComponent<MeshRenderer>().sharedMaterial;
        var properties = new MaterialPropertyBlock(); ground.GetComponent<MeshRenderer>().GetPropertyBlock(properties);
        edge.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
    }

    public void Refresh(float deltaTime)
    {
        int next = run.Checkpoint < sequence ? 0 : run.Checkpoint == sequence ? 1 : 2;
        if (next != State) {
            if (next == 1) burstAge = 0;
            if (next != 1) { activeLight = 0; burstAge = 10; }
            State = next;
        }
        float dt = run.Paused ? 0 : Mathf.Max(0,deltaTime);
        clock += dt; burstAge += dt;
        activeLight = Mathf.MoveTowards(activeLight, State == 1 ? 1 : 0, dt / (State == 1 ? 1.15f : .8f));
        material.SetFloat("_State", State);
        material.SetFloat("_Active", activeLight);
        material.SetFloat("_Visited", State == 2 ? 1 : 0);
        material.SetFloat("_Pulse", .5f + .5f * Mathf.Sin(clock * Mathf.PI * 2 / 3.6f));
        DrawSparks();
    }

    void OnDestroy()
    {
        if (Application.isPlaying) { Destroy(material); Destroy(stoneSprite); Destroy(frameSprite); Destroy(footingMesh); }
        else { DestroyImmediate(material); DestroyImmediate(stoneSprite); DestroyImmediate(frameSprite); DestroyImmediate(footingMesh); }
        if (Application.isPlaying) { Destroy(sparkMesh); Destroy(sparkMaterial); }
        else { DestroyImmediate(sparkMesh); DestroyImmediate(sparkMaterial); }
    }
}
