using System.Collections.Generic;
using UnityEngine;

// Native cave paintings and physics for the river cave connected to Golden River.
public sealed partial class CaveEntryStudy : MonoBehaviour
{
    public CatController Cat { get; private set; }
    public Camera View { get; private set; }
    public CatCamera Follow { get; private set; }
    readonly List<Object> owned = new List<Object>();
    float cameraHeight=1, waterTime;

    float GuideHeight(Vector2 position)
    {
        if(Cat.Grounded) cameraHeight=position.y>-.5f?1:position.y+Mathf.Lerp(-.5f,2.1f,Mathf.InverseLerp(-1.6f,-9.5f,position.y));
        else if(position.y<cameraHeight-2.5f) cameraHeight=position.y+1.8f;
        return cameraHeight;
    }
    void Update()
    {
        if(!Cat)return;
        AdvanceVisuals(Time.deltaTime);
    }
    public void AdvanceVisuals(float seconds)
    {
        if(!Cat.Paused)waterTime+=seconds;
        UpdateDiscoveryDust(waterTime);
        if (grottoWall) grottoWall.SetFloat("_StreamClock",waterTime);
    }
    MeshRenderer Painting(string name,Rect bounds,float z,string path)
    {
        var material=new Material(Resources.Load<Material>("GoldenRiver/Descent/entry-study-material"));
        owned.Add(material);material.mainTexture=Resources.Load<Texture2D>(path);
        RegisterMaterial(material);
        material.SetFloat("_Recessed",0);
        return MeshObject(name,Rectangle(bounds),bounds,z,material);
    }
    static Vector2[] Rectangle(Rect b)=>new[]{new Vector2(b.xMin,b.yMin),new Vector2(b.xMax,b.yMin),new Vector2(b.xMax,b.yMax),new Vector2(b.xMin,b.yMax)};
    MeshRenderer MeshObject(string name,Vector2[] polygon,Rect uvBounds,float z,Material material)
    {
        var item=new GameObject(name);item.transform.SetParent(transform,false);
        item.transform.localPosition=new Vector3(0,0,z);
        var mesh=new Mesh{name=name};owned.Add(mesh);
        var vertices=new Vector3[polygon.Length];var uv=new Vector2[polygon.Length];
        for(int i=0;i<vertices.Length;i++) {vertices[i]=polygon[i];uv[i]=new Vector2((polygon[i].x-uvBounds.x)/uvBounds.width,(polygon[i].y-uvBounds.y)/uvBounds.height);}
        var triangles=CaveGeometry.Triangulate(polygon);
        mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateBounds();
        item.AddComponent<MeshFilter>().sharedMesh=mesh;
        var renderer = item.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        return renderer;
    }
    void OnDestroy(){if(Cat)Cat.ReturnedToStart-=ResetLevelView;foreach(var item in owned) if(item) Destroy(item);}
}
