using System.Collections.Generic;
using UnityEngine;

public sealed partial class CaveEntryStudy
{
    Transform levelFrame;
    bool travelling;
    List<CaveTerrainLayout.Section> terrain;
    public CompositeCollider2D TerrainCollider { get; private set; }
    public void AttachToLevel(CatController cat,Camera view,CatCamera follow,SceneryRenderer scenery,Transform frame)
    {
        var started=Time.realtimeSinceStartupAsDouble;
        Cat=cat;View=view;Follow=follow;levelFrame=frame;AddLand();
        if(Debug.isDebugBuild)Debug.Log("Cave terrain startup: "+((Time.realtimeSinceStartupAsDouble-started)*1000).ToString("F1")+" ms");started=Time.realtimeSinceStartupAsDouble;
        var coverage=Resources.Load<Texture2D>("GoldenRiver/Descent/cave-coverage-v1");
        if(!coverage){coverage=CaveRoute.CreateMask();owned.Add(coverage);}
        scenery.SetDescentCoverage(coverage,CaveRoute.MaskBounds);
        if(Debug.isDebugBuild)Debug.Log("Cave coverage startup: "+((Time.realtimeSinceStartupAsDouble-started)*1000).ToString("F1")+" ms");
        foreach(var item in owned)if(item is Material material&&material.HasProperty("_UseDryMask")){
            var bounds=CaveRoute.MaskBounds;material.SetTexture("_DryMask",coverage);material.SetVector("_DryRect",new Vector4(bounds.x,bounds.y,bounds.width,bounds.height));material.SetFloat("_UseDryMask",1);
        }
        Cat.RecoveryBoundary=CaveRoute.ShouldRecover;Cat.ReturnedToStart+=ResetLevelView;
        Follow.PassageHeightGuide=LevelHeight;
    }
    void ResetLevelView(){travelling=false;cameraHeight=1;}
    float? LevelHeight(Vector2 coursePosition)
    {
        var p=CaveRoute.ToLocal(coursePosition);
        if(!travelling&&p.y<-.5f&&CaveRoute.IsDry(coursePosition))travelling=true;
        if(!travelling)return null;
        if(p.x<3.2f&&p.y>-.5f||p.x>235.69f&&p.y>4.9f||p.x>250){travelling=false;return null;}
        float target=GuideHeight(p)+CaveRoute.VerticalOffset;
        if(p.x>=14&&p.x<=203.5f)target=Mathf.Min(target,-10.15f);
        else if(p.x>203.5f&&p.x<210)target=Mathf.Lerp(Mathf.Min(target,-10.15f),target,Mathf.SmoothStep(0,1,Mathf.InverseLerp(203.5f,210,p.x)));
        return target;
    }
    void RegisterMaterial(Material material)
    {
        if(material.HasProperty("_CaveOrigin")){material.SetVector("_CaveOrigin",transform.position);material.SetFloat("_CaveScale",transform.lossyScale.x);}
        if(material.HasProperty("_TerrainStone"))material.SetTexture("_TerrainStone",Resources.Load<Texture2D>("GoldenRiver/Descent/terrain-stone-v1"));
        if(material.HasProperty("_Origin"))material.SetVector("_Origin",new Vector4(transform.position.x,transform.position.y,transform.lossyScale.x,1));
    }
    Vector2[] EmbeddedContour(string name,Vector2[] source)=>source;
    Material RockMaterial(float shade=1)
    {
        var material=new Material(Resources.Load<Material>("GoldenRiver/Descent/cave-rock-material"));owned.Add(material);RegisterMaterial(material);
        var grain=Resources.Load<Texture2D>("GoldenRiver/Descent/entry-module-0-v1");
        material.mainTexture=grain?grain:Resources.Load<Texture2D>("GoldenRiver/Descent/terrain-stone-v1");
        material.SetTexture("_BaseTex",material.mainTexture);material.SetVector("_Frame",new Vector4(-2.21f,-6.045f,6,6));
        material.SetFloat("_Shade",shade);return material;
    }
    void AddEmbeddedLand()
    {
        terrain=CaveTerrainLayout.Build();
        var body=gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Static;
        TerrainCollider=gameObject.AddComponent<CompositeCollider2D>();TerrainCollider.geometryType=CompositeCollider2D.GeometryType.Polygons;
        TerrainCollider.generationType=CompositeCollider2D.GenerationType.Manual;TerrainCollider.vertexDistance=.001f;
        var stone=RockMaterial();
        foreach(var section in terrain){
            MeshObject(section.Name,section.Outline,new Rect(0,0,8,8),-3,stone);
            var item=new GameObject(section.Name+" collision source");item.transform.SetParent(transform,false);
            var collider=item.AddComponent<PolygonCollider2D>();collider.points=section.Outline;collider.compositeOperation=Collider2D.CompositeOperation.Merge;
        }
        TerrainCollider.GenerateGeometry();Physics2D.SyncTransforms();AddEntranceDetails();AddExposedGrass();
        var background=RockMaterial(.52f);
        var rear=new[]{new Vector2(8,-42),new Vector2(205,-42),new Vector2(205,-4.5f),new Vector2(183,-4.5f),
            new Vector2(179,-5.8f),new Vector2(104,-5.8f),new Vector2(72,-7.3f),new Vector2(44,-5.5f),new Vector2(34,-8),
            new Vector2(18,-8.4f),new Vector2(14,-6.8f),new Vector2(12.3f,-5.7f),new Vector2(11.8f,-6.1f),
            new Vector2(11.5f,-6.5f),new Vector2(11.1f,-6.4f),new Vector2(10.8f,-6.8f),new Vector2(10.2f,-7.2f),
            new Vector2(10.4f,-7.6f),new Vector2(10,-7.9f),new Vector2(9.7f,-8.2f),new Vector2(9.8f,-8.7f),
            new Vector2(9.4f,-8.9f),new Vector2(9,-9.1f),new Vector2(9.1f,-9.5f),new Vector2(8.8f,-9.7f),new Vector2(8.7f,-10.1f),new Vector2(8,-10.5f)};
        MeshObject("Permanent recessed cave rock",rear,new Rect(0,0,24,24),2,background);
        Painting("Rooted fountain landmark",new Rect(43,-12.976f,10.2f,6.8f),1.4f,"GoldenRiver/Descent/ruin-v1");
        var future=new GameObject("FutureCollectibleAnchor");future.transform.SetParent(transform,false);future.transform.localPosition=new Vector3(54.8f,-12.5f,0);
        AddDiscoveryDust();
    }
    bool SolidAt(Vector2 point)
    {
        foreach(var section in terrain)if(CaveRoute.Contains(section.Outline,point))return true;return false;
    }
    void AddEntranceDetails()
    {
        var frames=CaveTerrainLayout.EntranceFrames;
        for(int i=0;i<frames.Length;i++){
            var texture=Resources.Load<Texture2D>("GoldenRiver/Descent/entry-module-"+i+"-v1");if(!texture)continue;
            var material=RockMaterial();material.mainTexture=texture;material.SetFloat("_WorldMapped",0);material.SetFloat("_Cutout",1);
            var outline=i==3||i==5?CaveTerrainLayout.EntryRoof:CaveTerrainLayout.LeftShore;
            var boundary=Resources.Load<Texture2D>("GoldenRiver/Descent/entry-boundary-"+i+"-v1");
            if(!boundary){boundary=EntranceBoundary(outline,frames[i]);owned.Add(boundary);}
            material.SetTexture("_EdgeMask",boundary);
            MeshObject("Native entrance stone module "+i,outline,frames[i],-3.01f-i*.001f,material);
        }
    }
    public static Texture2D EntranceBoundary(Vector2[] outline,Rect frame,bool readable=false)
    {
        const int size=256;var pixels=new byte[size*size];
        for(int y=0;y<size;y++)for(int x=0;x<size;x++){
            var point=new Vector2(frame.x+(x+.5f)/size*frame.width,frame.y+(y+.5f)/size*frame.height);float distance=float.PositiveInfinity;
            for(int i=0;i<outline.Length;i++){
                var a=outline[i];var direction=outline[(i+1)%outline.Length]-a;
                var nearest=a+direction*Mathf.Clamp01(Vector2.Dot(point-a,direction)/Mathf.Max(.000001f,direction.sqrMagnitude));
                distance=Mathf.Min(distance,Vector2.Distance(point,nearest));
            }
            pixels[y*size+x]=(byte)Mathf.RoundToInt((1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.3f,.8f,distance)))*255);
        }
        var mask=new Texture2D(size,size,readable?TextureFormat.RGBA32:TextureFormat.R8,false,true){name="Native stone boundary treatment",wrapMode=TextureWrapMode.Clamp};
        if(readable){var colors=new Color32[pixels.Length];for(int i=0;i<pixels.Length;i++)colors[i]=new Color32(pixels[i],pixels[i],pixels[i],255);mask.SetPixels32(colors);}
        else mask.SetPixelData(pixels,0);
        mask.Apply(false,!readable);return mask;
    }
    void AddExposedGrass()
    {
        var exposed=new List<Vector3>();
        foreach(var section in terrain)foreach(var tread in section.Treads){
            var cuts=new List<float>{tread.x,tread.x+tread.z};float line=tread.y+.005f;
            foreach(var solid in terrain)for(int i=0;i<solid.Outline.Length;i++){
                var a=solid.Outline[i];var b=solid.Outline[(i+1)%solid.Outline.Length];
                if((a.y>line)==(b.y>line))continue;
                float x=Mathf.Lerp(a.x,b.x,(line-a.y)/(b.y-a.y));if(x>tread.x&&x<tread.x+tread.z)cuts.Add(x);
            }
            cuts.Sort();
            for(int i=1;i<cuts.Count;i++){float left=cuts[i-1],right=cuts[i];if(right-left<.001f)continue;
                float x=(left+right)/2;if(!SolidAt(new Vector2(x,line))&&SolidAt(new Vector2(x,tread.y-.01f)))exposed.Add(new Vector3(left,tread.y,right-left));}
        }
        exposed.Sort((a,b)=>Mathf.Abs(a.y-b.y)>.0001f?a.y.CompareTo(b.y):a.x.CompareTo(b.x));
        var merged=new List<Vector3>();
        foreach(var span in exposed){if(merged.Count>0){var previous=merged[merged.Count-1];if(Mathf.Abs(previous.y-span.y)<.0001f&&span.x<=previous.x+previous.z+.001f){previous.z=Mathf.Max(previous.x+previous.z,span.x+span.z)-previous.x;merged[merged.Count-1]=previous;continue;}}merged.Add(span);}
        foreach(var span in merged){
            var material=new Material(Resources.Load<Material>("GoldenRiver/Descent/cave-grass-material"));owned.Add(material);RegisterMaterial(material);
            material.mainTexture=Resources.Load<Texture2D>("GoldenRiver/bank-narrow-v3");material.SetFloat("_Contact",span.y);
            var bounds=new Rect(span.x,span.y-.12f,span.z,.72f);MeshObject("Vegetation over solid contact",Rectangle(bounds),bounds,-3.1f,material);
        }
    }
    public static void CarveRecovery(GameObject floor,Transform visual,Transform frame)
    {
        foreach(var shape in floor.GetComponents<PolygonCollider2D>()) {
            var points=shape.points;
            for(int i=0;i<points.Length;i++)points[i]=CaveRoute.ToLocal(frame.InverseTransformPoint(shape.transform.TransformPoint(points[i])));
            var pieces=new List<Vector2[]>{points};
            foreach(var opening in CaveRoute.Openings)pieces=CaveGeometry.Subtract(pieces,opening);
            bool changed=pieces.Count!=1||pieces[0].Length!=points.Length;
            if(!changed)for(int i=0;i<points.Length;i++)if(points[i]!=pieces[0][i]){changed=true;break;}
            if(changed)floor.name="Shore opening recovery";
            if(pieces.Count==0){shape.enabled=false;continue;}
            shape.pathCount=pieces.Count;
            for(int path=0;path<pieces.Count;path++) {
                var piece=pieces[path];
                for(int i=0;i<piece.Length;i++)piece[i]=shape.transform.InverseTransformPoint(frame.TransformPoint(CaveRoute.ToCourse(piece[i])));
                shape.SetPath(path,piece);
            }
        }
        var renderer=visual.GetComponent<MeshRenderer>();var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block);
        block.SetFloat("_CutCaveAir",1);renderer.SetPropertyBlock(block);
    }
}
