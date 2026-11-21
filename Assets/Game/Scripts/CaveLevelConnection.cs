using System.Collections.Generic;
using UnityEngine;

public sealed partial class CaveEntryStudy
{
    Transform levelFrame;
    Material grottoWall;
    bool travelling;
    List<CaveTerrainLayout.Section> terrain;
    public CompositeCollider2D TerrainCollider { get; private set; }
    public void AttachToLevel(CatController cat,Camera view,CatCamera follow,SceneryRenderer scenery,Transform frame)
    {
        var started=Time.realtimeSinceStartupAsDouble;
        Cat=cat;View=view;Follow=follow;levelFrame=frame;AddLand();
        var presentation=cat.GetComponentInChildren<MikaSpritePresentation>();
        if (presentation) presentation.RiverbankTerrain=TerrainCollider;
        if(Debug.isDebugBuild)Debug.Log("Cave terrain startup: "+((Time.realtimeSinceStartupAsDouble-started)*1000).ToString("F1")+" ms");started=Time.realtimeSinceStartupAsDouble;
        var coverage=Resources.Load<Texture2D>("GoldenRiver/Descent/cave-coverage-v1");
        if(!coverage){coverage=CaveRoute.CreateMask();owned.Add(coverage);}
        scenery.SetDescentCoverage(coverage,CaveRoute.MaskBounds);
        if(Debug.isDebugBuild)Debug.Log("Cave coverage startup: "+((Time.realtimeSinceStartupAsDouble-started)*1000).ToString("F1")+" ms");
        foreach(var item in owned)if(item is Material material&&material.HasProperty("_UseDryMask")){
            var bounds=CaveRoute.MaskBounds;material.SetTexture("_DryMask",coverage);material.SetVector("_DryRect",new Vector4(bounds.x,bounds.y,bounds.width,bounds.height));material.SetFloat("_UseDryMask",1);
        }
        var water = Cat.GetComponent<WaterRecovery>();
        if (water) water.RiverContact = CaveRoute.TouchesRiver;
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
        var grain=Resources.Load<Texture2D>("GoldenRiver/Descent/concept-entry-rock-v1");
        material.mainTexture=grain?grain:Resources.Load<Texture2D>("GoldenRiver/Descent/terrain-stone-v1");
        material.SetTexture("_BaseTex",material.mainTexture);material.SetVector("_Frame",new Vector4(0,0,8,5.333333f));
        material.SetTexture("_ChannelStone", Resources.Load<Texture2D>("GoldenRiver/Descent/river-cut-slate"));
        material.SetTexture("_Cliff",Resources.Load<Texture2D>("GoldenRiver/bank-tall-ascent-v1"));
        material.SetFloat("_Shade",shade);return material;
    }
    void AddEmbeddedLand()
    {
        terrain=CaveTerrainLayout.Build();
        var body=gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Static;
        TerrainCollider=gameObject.AddComponent<CompositeCollider2D>();TerrainCollider.geometryType=CompositeCollider2D.GeometryType.Polygons;
        TerrainCollider.generationType=CompositeCollider2D.GenerationType.Manual;TerrainCollider.vertexDistance=.001f;
        var stone=RockMaterial();
        var shelfMaterials = new Dictionary<TerrainArtProfile, Material>();
        foreach(var section in terrain){
            // The upstream bank stays behind the river; the dry throat remains in front.
            bool upstream=section.Name=="Connected approach shore" || section.Name=="Entrance shore";
            float depth=upstream ? -1.8f : -3;
            if (section.Shelf == null)
                MeshObject(section.Name,section.Outline,new Rect(0,0,8,8),depth,stone);
            else {
                if (!shelfMaterials.TryGetValue(section.Shelf, out var painted)) {
                    painted = RockMaterial();
                    painted.mainTexture = Resources.Load<Texture2D>(section.Shelf.TexturePath);
                    painted.SetFloat("_NativeShelf", 1);
                    shelfMaterials.Add(section.Shelf, painted);
                }
                MeshObject(section.Name,Rectangle(section.ArtFrame),section.ArtFrame,-3.06f,painted);
            }
            var item=new GameObject(section.Name+" collision source");item.transform.SetParent(transform,false);
            var collider=item.AddComponent<PolygonCollider2D>();collider.points=section.Outline;collider.compositeOperation=Collider2D.CompositeOperation.Merge;
        }
        TerrainCollider.GenerateGeometry();Physics2D.SyncTransforms();AddEntranceDetails();AddAuthoredEntrance();AddExposedGrass();AddPaintedBanks();AddCaveMargins();
        var background=RockMaterial(.52f);
        background.renderQueue=(int)UnityEngine.Rendering.RenderQueue.Transparent;
        grottoWall = background;
        background.SetTexture("_Falls", Resources.Load<Texture2D>("GoldenRiver/Descent/fern-falls"));
        background.SetFloat("_ChamberWall", 1);
        background.SetTexture("_RootWall", Resources.Load<Texture2D>("GoldenRiver/Descent/rooted-channel-wall"));
        background.SetTexture("_Fissure", Resources.Load<Texture2D>("GoldenRiver/Descent/fountain-fissure-wall"));
        var rear=new[]{new Vector2(8,-42),new Vector2(206,-42),new Vector2(206,-4.5f),new Vector2(34,-4.5f),
            new Vector2(18,-8.4f),new Vector2(14,-6.8f),new Vector2(12.3f,-5.7f),new Vector2(11.8f,-6.1f),
            new Vector2(11.5f,-6.5f),new Vector2(11.1f,-6.4f),new Vector2(10.8f,-6.8f),new Vector2(10.2f,-7.2f),
            new Vector2(10.4f,-7.6f),new Vector2(10,-7.9f),new Vector2(9.7f,-8.2f),new Vector2(9.8f,-8.7f),
            new Vector2(9.4f,-8.9f),new Vector2(9,-9.1f),new Vector2(9.1f,-9.5f),new Vector2(8.8f,-9.7f),new Vector2(8.7f,-10.1f),new Vector2(8,-10.5f)};
        MeshObject("Permanent recessed cave rock",rear,new Rect(0,0,24,24),2,background);
        Painting("Disused spring spillway",new Rect(125.9f,-16.8f,14.4f,7.2f),1.45f,"GoldenRiver/Descent/old-spillway");
        AddUnderstory();
        AddRecessedMouth();
        var buttress=Painting("Entrance root buttress",new Rect(6,-6.8f,9.4f,7.72f),1.6f,"GoldenRiver/Descent/fern-arch");
        buttress.sharedMaterial.SetFloat("_RightFeather",.34f);
        Painting("Rooted fountain landmark",new Rect(43,-12.976f,10.2f,6.8f),1.4f,"GoldenRiver/Descent/ruin-v1");
        var future=new GameObject("FutureCollectibleAnchor");future.transform.SetParent(transform,false);future.transform.localPosition=new Vector3(54.8f,-12.5f,0);
        AddDiscoveryDust();
    }
    void AddUnderstory()
    {
        // Crop each isolated plant at its original aspect ratio and bury its roots in a tread.
        var material=RockMaterial();
        material.mainTexture=Resources.Load<Texture2D>("GoldenRiver/Descent/cave-plants");
        material.SetFloat("_NativeShelf",1);
        int[] columns={0,499,896,1536};
        void Plant(int cell,float x,float ground,float width) {
            float scale=width/(columns[cell+1]-columns[cell]);
            var frame=new Rect(x-columns[cell]*scale,ground-212*scale,1536*scale,1024*scale);
            var bounds=new Rect(x,ground-20*scale,width,688*scale);
            MeshObject("Cave bank understory",Rectangle(bounds),frame,1.2f,material);
        }
        Plant(0,20.6f,-12,2.25f);
        Plant(2,33.1f,-12,2.6f);
        Plant(1,61.1f,-13.6f,1.75f);
        Plant(0,78,-18.5f,1.7f);
        Plant(0,96,-26.5f,2.1f);
        Plant(2,108,-20,2.6f);
        Plant(1,150,-15,1.4f);
        Plant(2,169,-10.9f,2.5f);
        Plant(0,188,-8.14f,2.15f);
    }
    bool SolidAt(Vector2 point)
    {
        foreach(var section in terrain)if(CaveRoute.Contains(section.Outline,point))return true;return false;
    }
    void AddAuthoredEntrance()
    {
        // These paintings are registered to the solid outlines, never to the camera.
        // They supply whole cliff faces instead of repeating a small material patch.
        var frame = new Rect(5.5f,-9,18,11);
        var material = RockMaterial();
        material.mainTexture = Resources.Load<Texture2D>("GoldenRiver/Descent/concept-entry-roof-v1");
        material.SetFloat("_WorldMapped",0); material.SetFloat("_Cutout",1); material.SetFloat("_AuthoredFace",1);
        MeshObject("Authored overhang",CaveTerrainLayout.EntryRoof,frame,-3.045f,material);
    }
    void AddRecessedMouth()
    {
        var texture=Resources.Load<Texture2D>("GoldenRiver/Descent/concept-entry-recess-v1");if(!texture)return;
        var material=RockMaterial(.52f);material.SetFloat("_Alcove",1);material.mainTexture=texture;material.SetFloat("_WorldMapped",0);
        // The upper seam runs inside the opaque roof, rather than cutting across the
        // opening at a fixed height. The outer flank is permanent and behind Mika.
        var outline=new[]{new Vector2(5.9f,-24),new Vector2(36,-24),new Vector2(36,-7.8f),new Vector2(24,-7.9f),
            new Vector2(20,-7),new Vector2(18,-6.4f),new Vector2(16,-5.3f),new Vector2(14,-4.15f),new Vector2(12,-3.2f),
            new Vector2(10,-1.55f),new Vector2(8,-.35f),new Vector2(7,.03f),new Vector2(6.4f,.32f),
            new Vector2(6.7f,-1.4f),new Vector2(6.8f,-4.5f),new Vector2(6.1f,-8),new Vector2(5.9f,-11.6f)};
        MeshObject("Layered recessed entrance alcove",outline,new Rect(5.9f,-20,30.1f,20.06667f),1.8f,material);
    }
    void AddEntranceDetails()
    {
        var frames=CaveTerrainLayout.EntranceFrames;
        foreach(int i in new[]{3,5}){
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
        foreach(var section in terrain)if(section.Shelf==null && section.Name!="Connected approach shore")foreach(var tread in section.Treads){
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
        var material = new Material(Resources.Load<Material>("GoldenRiver/Descent/cave-grass-material"));
        owned.Add(material);
        RegisterMaterial(material);
        material.mainTexture = Resources.Load<Texture2D>("GoldenRiver/bank-narrow-v3");
        material.SetTexture("_ChannelStone", Resources.Load<Texture2D>("GoldenRiver/Descent/river-cut-slate"));
        foreach(var span in merged){
            var bounds=new Rect(span.x,span.y-.12f,span.z,.72f);
            var renderer = MeshObject("Vegetation over solid contact",Rectangle(bounds),bounds,-3.1f,material);
            var properties = new MaterialPropertyBlock();
            properties.SetFloat("_Contact", span.y);
            renderer.SetPropertyBlock(properties);
        }
    }
    void AddCaveMargins()
    {
        var bed=RockMaterial();
        bed.mainTexture=Resources.Load<Texture2D>("GoldenRiver/Descent/mossy-bedrock");
        bed.SetFloat("_RockMargin",1);
        bed.renderQueue=2450;
        foreach (var section in terrain)
            if (section.ExposedBed!=null) DrawRockEdge(section.ExposedBed,bed,false,-3.025f);
        // This inset ledge seats the entire ruined drain behind the playable shelves.
        var plinth=new[]{new Vector2(120,-20),new Vector2(123.6f,-17.15f),new Vector2(126.2f,-15.81f),
            new Vector2(128,-15.72f),new Vector2(134,-15.95f),new Vector2(138.5f,-15.85f),
            new Vector2(140.5f,-16.35f),new Vector2(143,-18.8f),new Vector2(146,-20)};
        var backing=new List<Vector2>(plinth){new Vector2(146,-26),new Vector2(120,-26)};
        MeshObject("Spillway stone footing",backing.ToArray(),new Rect(120,-26,26,10),1.38f,RockMaterial(.75f));
        DrawRockEdge(plinth,bed,false,1.31f);

        var vault=RockMaterial();
        vault.mainTexture=Resources.Load<Texture2D>("GoldenRiver/Descent/rooted-vault");
        vault.SetFloat("_RockMargin",2);
        vault.renderQueue=2450;
        // One margin crosses all roof collider sections and wraps around the daylight bluff.
        var soffit=new List<Vector2>(CaveTerrainLayout.EntranceSoffit);
        for(float x=34.25f;x<183;x+=.25f)soffit.Add(new Vector2(x,CaveRoute.Ceiling(x)));
        soffit.AddRange(CaveTerrainLayout.DaylightSoffit);
        DrawRockEdge(soffit,vault,true,-3.052f);
    }
    void DrawRockEdge(IList<Vector2> line,Material material,bool ceiling,float depth)
    {
        var vertices=new Vector3[line.Count*2];
        var uv=new Vector2[vertices.Length];
        var triangles=new int[(line.Count-1)*6];
        var limits=new Vector2[vertices.Length];
        float arc=line[0].x;
        for(int i=0;i<line.Count;i++){
            var p=line[i];
            if(i>0)arc+=Vector2.Distance(p,line[i-1]);
            Vector2 inside=ceiling?Vector2.up:Vector2.down;
            float body=3.55f;
            if(ceiling) {
                // Preserve the river cover and both playable landings above the thin roof.
                float top=p.x<18 ? .72f : p.x<186 ? CaveTerrainLayout.RiverbedSurface(p.x) : 4.155f;
                body=Mathf.Clamp(top-p.y-.07f,.015f,2.9f);
                if(p.x<12.2f)body=Mathf.Min(body,.48f);
                if(p.x>202) {
                    var tangent=(line[Mathf.Min(i+1,line.Count-1)]-line[Mathf.Max(0,i-1)]).normalized;
                    inside=new Vector2(-tangent.y,tangent.x);
                    body=Mathf.Min(body,1.55f);
                }
            }
            int j=2*i;
            vertices[j]=(Vector3)(p-inside*1.52f)+Vector3.forward*depth;
            vertices[j+1]=(Vector3)(p+inside*body)+Vector3.forward*depth;
            uv[j]=new Vector2(arc,-1.52f); uv[j+1]=new Vector2(arc,body);
            limits[j]=limits[j+1]=new Vector2(body,0);
            if(i+1==line.Count)continue;
            int t=6*i;
            triangles[t]=j;triangles[t+1]=j+1;triangles[t+2]=j+2;
            triangles[t+3]=j+2;triangles[t+4]=j+1;triangles[t+5]=j+3;
        }
        SetStripMesh("Water-worn rock margin",vertices,uv,triangles,material,limits);
    }
    void AddPaintedBanks()
    {
        var paint = RockMaterial();
        paint.mainTexture = Resources.Load<Texture2D>("GoldenRiver/bank-medium-v3");
        paint.SetFloat("_BankFace",1);
        paint.renderQueue=2451;
        paint.SetTexture("_Bedrock",Resources.Load<Texture2D>("GoldenRiver/Descent/mossy-bedrock"));
        var left = MovementCourse.AscentPlatforms[9];
        float start = left.x-left.z/2-CaveRoute.Origin+.12f;
        var approach = new List<Vector2> { new Vector2(start,-1.245f) };
        for (int i=0;i<=24;i++) {
            float x = Mathf.Lerp(CaveTerrainLayout.ApproachRiseStart,CaveTerrainLayout.ApproachRiseEnd,i/24f);
            approach.Add(new Vector2(x,CaveTerrainLayout.ApproachHeight(x)));
        }
        foreach (var p in CaveTerrainLayout.LeftShore) {
            if (p.y < -20) break;
            if (p.x > CaveTerrainLayout.ApproachRiseEnd) approach.Add(p);
        }
        DrawBankFace(approach, paint, true);
        DrawBankFace(CaveTerrainLayout.EntranceCrown, paint, overhang:true);
        DrawBankFace(CaveTerrainLayout.ExitBank, paint);
    }
    void DrawBankFace(IList<Vector2> face, Material paint, bool upstream=false, bool overhang=false)
    {
        // Sample the river bank painting at a constant scale along the physical contour.
        var vertices = new Vector3[face.Count*2];
        var uv = new Vector2[vertices.Length];
        var triangles = new int[(face.Count-1)*6];
        var limits = new Vector2[vertices.Length];
        for (int i=0;i<face.Count;i++) {
            var p=face[i]; int j=2*i;
            if(i==0 && !overhang)p.x-=.84f;
            if(i+1==face.Count)p.x+=.84f;
            float depth=upstream ? Mathf.Lerp(-1.815f,-3.055f,Mathf.InverseLerp(-5.9f,-3.4f,p.x)) : -3.055f;
            float thickness=5;
            if(overhang){
                float soffit=CaveTerrainLayout.AlongFace(CaveTerrainLayout.EntranceSoffit,p.x);
                thickness=Mathf.Clamp(p.y-soffit,.045f,5);
            }
            limits[j]=limits[j+1]=new Vector2(thickness,0);
            vertices[j]=new Vector3(p.x,p.y-thickness,depth);
            vertices[j+1]=new Vector3(p.x,p.y+.75f,depth);
            uv[j]=new Vector2(p.x,-thickness); uv[j+1]=new Vector2(p.x,.75f);
            if(i+1==face.Count)continue;
            int t=6*i;
            triangles[t]=j; triangles[t+1]=j+1; triangles[t+2]=j+2;
            triangles[t+3]=j+2; triangles[t+4]=j+1; triangles[t+5]=j+3;
        }
        var renderer=SetStripMesh("Painted riverbank contour",vertices,uv,triangles,paint,limits);
        var properties=new MaterialPropertyBlock();
        properties.SetVector("_BankEnds",new Vector4(face[0].x-.84f,face[face.Count-1].x+.84f,0,0));
        renderer.SetPropertyBlock(properties);
    }
    MeshRenderer SetStripMesh(string label,Vector3[] vertices,Vector2[] uv,int[] triangles,Material material,Vector2[] limits=null)
    {
        var mesh = new Mesh { name=label, vertices=vertices, uv=uv, triangles=triangles };
        if(limits!=null)mesh.uv2=limits;
        owned.Add(mesh); mesh.RecalculateBounds();
        var bank=new GameObject(mesh.name,typeof(MeshFilter),typeof(MeshRenderer));
        bank.transform.SetParent(transform,false);
        bank.GetComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=bank.GetComponent<MeshRenderer>();
        renderer.sharedMaterial=material;
        return renderer;
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
