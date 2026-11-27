using System.Collections.Generic;
using UnityEngine;

public sealed partial class CaveEntryStudy
{
    readonly List<Transform> dust=new List<Transform>();
    void AddDiscoveryDust()
    {
        var texture=new Texture2D(16,16,TextureFormat.RGBA32,false);owned.Add(texture);
        for(int y=0;y<16;y++)for(int x=0;x<16;x++) {
            float radius=Vector2.Distance(new Vector2(x,y),new Vector2(7.5f,7.5f))/7.5f;
            texture.SetPixel(x,y,new Color(1,.83f,.49f,Mathf.Pow(Mathf.Clamp01(1-radius),2)*.4f));
        }
        texture.Apply();var sprite=Sprite.Create(texture,new Rect(0,0,16,16),Vector2.one*.5f,64);owned.Add(sprite);
        for(int i=0;i<12;i++) {
            var item=new GameObject("Fissure dust "+i);item.transform.SetParent(transform,false);
            item.transform.localScale=Vector3.one*(.12f+i%3*.025f);
            item.AddComponent<SpriteRenderer>().sprite=sprite;dust.Add(item.transform);
        }
        UpdateDiscoveryDust(0);
    }
    void UpdateDiscoveryDust(float time)
    {
        for(int i=0;i<dust.Count;i++) {
            float t=Mathf.Repeat(i*.0833f+time*.013f,1);
            dust[i].localPosition=Vector3.Lerp(new Vector3(56.5f,-6,1.6f),new Vector3(49,-12.2f,1.6f),t)
                +new Vector3(Mathf.Sin(i*2.3f+time*.27f)*.5f,Mathf.Cos(i+time*.2f)*.15f,0);
        }
    }
}
