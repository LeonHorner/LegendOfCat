using UnityEngine;

// Fixed side-opening light and deterministic dust. Scaled time keeps pause still.
public sealed class DescentLight : MonoBehaviour
{
    Texture2D shaft, dust;
    Sprite shaftSprite, dustSprite;
    Sprite openingSprite;
    readonly Transform[] motes = new Transform[12];
    float clock;
    CatController cat;

    public void Initialize(CatController owner)
    {
        cat = owner;
        shaft = new Texture2D(128, 256, TextureFormat.RGBA32, false);
        shaft.name = "Original procedural side-opening light";
        for (int y = 0; y < shaft.height; y++)
            for (int x = 0; x < shaft.width; x++) {
                float v = y / 255f, u = x / 127f;
                float center = Mathf.Lerp(.25f, .8f, v);
                float width = Mathf.Lerp(.5f, .10f, v);
                float alpha = Mathf.Pow(Mathf.Clamp01(1 - Mathf.Abs(u - center) / width), 2)
                    * Mathf.Sin(v * Mathf.PI) * .13f;
                shaft.SetPixel(x, y, new Color(1, .85f, .52f, alpha));
            }
        shaft.Apply();
        shaftSprite = Sprite.Create(shaft, new Rect(0, 0, 128, 256), Vector2.one * .5f, 32);
        var beam = new GameObject("Permanent warm light from side cleft", typeof(SpriteRenderer));
        beam.transform.SetParent(transform, false);
        var room=UpperRoute.Platforms[UpperRoute.LookoutIndex];
        beam.transform.localPosition = new Vector3(room.x+2,room.y+5.35f,1.35f);
        beam.transform.localScale = new Vector3(2, 1.1f, 1);
        beam.GetComponent<SpriteRenderer>().sprite = shaftSprite;
        // A small fixed fissure in the side-facing rear rock explains the warm shaft.
        openingSprite=Sprite.Create(shaft,new Rect(36,128,56,112),Vector2.one*.5f,32);
        var opening=new GameObject("Warm side fissure",typeof(SpriteRenderer));
        opening.transform.SetParent(transform,false);
        opening.transform.localPosition=new Vector3(room.x+3.25f,room.y+7.1f,1.32f);
        opening.transform.localScale=new Vector3(.35f,.5f,1);
        opening.GetComponent<SpriteRenderer>().sprite=openingSprite;
        opening.GetComponent<SpriteRenderer>().color=new Color(1,.88f,.64f,2);
        dust = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        dust.name = "Original procedural dust mote";
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++) {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f)) / 7.5f;
                dust.SetPixel(x, y, new Color(1, .88f, .62f, Mathf.Pow(Mathf.Clamp01(1 - distance), 2) * .35f));
            }
        dust.Apply();
        dustSprite = Sprite.Create(dust, new Rect(0, 0, 16, 16), Vector2.one * .5f, 160);
        for (int i = 0; i < motes.Length; i++) {
            var mote = new GameObject("Dust in side light", typeof(SpriteRenderer));
            mote.transform.SetParent(transform, false);
            mote.GetComponent<SpriteRenderer>().sprite = dustSprite;
            motes[i] = mote.transform;
        }
        Update();
    }

    void Update()
    {
        Advance(Time.deltaTime);
    }

    public void Advance(float seconds)
    {
        if (!cat || !cat.Paused) clock += seconds;
        for (int i = 0; i < motes.Length; i++) {
            if (!motes[i]) continue;
            float phase = Mathf.Repeat(i * .618034f + clock * .018f, 1);
            var room=UpperRoute.Platforms[UpperRoute.LookoutIndex];
            motes[i].localPosition = new Vector3(room.x-1+(i%4)*.8f
                + Mathf.Sin(clock*.18f+i)*.12f,room.y+2.25f+phase*6,1.3f);
        }
    }

    void OnDestroy()
    {
        if (shaftSprite) Destroy(shaftSprite);
        if (dustSprite) Destroy(dustSprite);
        if (openingSprite) Destroy(openingSprite);
        if (shaft) Destroy(shaft);
        if (dust) Destroy(dust);
    }
}
