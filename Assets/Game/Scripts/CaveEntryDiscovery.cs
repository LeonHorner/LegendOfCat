using System.Collections.Generic;
using UnityEngine;

public sealed partial class CaveEntryStudy
{
    public static readonly Rect DiscoveryFloorBounds=new Rect(30,-22.975f,42,17.5f);
    readonly List<Transform> dust=new List<Transform>();
    void EntryRoofOpening()
    {
        var bounds=new Rect(7,-12.15f,30,20);
        var visible=bounds;visible.yMax=.22f;
        var material=new Material(Resources.Load<Material>("GoldenRiver/Descent/entry-study-material"));owned.Add(material);
        material.mainTexture=Resources.Load<Texture2D>("GoldenRiver/Descent/entry-study-right-v2");
        var opening=Rectangle(new Rect(26,-13.5f,12,7.7f));
        foreach(var piece in CaveGeometry.Subtract(new List<Vector2[]>{Rectangle(visible)},opening))
            MeshObject("Permanent continuation through end rock",piece,bounds,1,material);
    }
    void AddDiscovery()
    {
        Painting("Sunlit recessed fissure wall",new Rect(31,-22,42,24),2.8f,"GoldenRiver/Descent/discovery-rear-v1",true);
        transform.Find("Sunlit recessed fissure wall").GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Recessed",2);
        Painting("Connected discovery roof",new Rect(24,-13,48,22),.9f,"GoldenRiver/Descent/discovery-ceiling-v2");
        transform.Find("Connected discovery roof").GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_RoofJoin",1);
        Painting("Discovery floor and return steps",DiscoveryFloorBounds,.7f,"GoldenRiver/Descent/discovery-floor-v2");
        Painting("Rooted fountain landmark",new Rect(43,-12.976f,10.2f,6.8f),1.4f,"GoldenRiver/Descent/ruin-v1");
        Collider("Solid discovery roof",DiscoveryRoof);
        Collider("Solid discovery floor and attached stairs",DiscoveryFloor);
        var future=new GameObject("FutureCollectibleAnchor");future.transform.SetParent(transform,false);
        future.transform.localPosition=new Vector3(54.8f,-12.5f,0);
        AddDiscoveryDust();
    }
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
    public static readonly Vector2[] DiscoveryFloor=new[]{new Vector2(32f,-10.03f),new Vector2(35.876f,-10.03f),new Vector2(35.876f,-11.33f),new Vector2(38.839f,-11.33f),new Vector2(38.839f,-12.67f),new Vector2(57.106f,-12.67f),new Vector2(57.106f,-11.29f),new Vector2(59.549f,-11.29f),new Vector2(59.549f,-9.84f),new Vector2(62.08f,-9.84f),new Vector2(62.08f,-8.43f),new Vector2(64.697f,-8.43f),new Vector2(64.697f,-6.96f),new Vector2(72f,-6.96f),new Vector2(72f,-23f),new Vector2(31.8f,-23f),new Vector2(31f,-21f),new Vector2(32.4f,-17.3f),new Vector2(31.1f,-15.4f),new Vector2(32.3f,-13.2f),new Vector2(31.2f,-11.8f)};
    public static readonly Vector2[] DiscoveryRoof=new[]{new Vector2(24f,9f),new Vector2(72f,9f),new Vector2(72f,-14f),new Vector2(68.4f,-14f),new Vector2(68.4f,-4.2f),new Vector2(66f,-4.345f),new Vector2(63f,-4.941f),new Vector2(59f,-5.537f),new Vector2(55f,-5.641f),new Vector2(51f,-5.252f),new Vector2(47f,-5.744f),new Vector2(43f,-5.9f),new Vector2(39f,-6.263f),new Vector2(36f,-6.988f),new Vector2(33f,-7.273f),new Vector2(29f,-7.506f),new Vector2(24f,-6.95f)};
}
