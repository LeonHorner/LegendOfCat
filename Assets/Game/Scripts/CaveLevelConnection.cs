using System.Collections.Generic;
using UnityEngine;

public sealed partial class CaveEntryStudy
{
    Transform levelFrame;
    bool travelling;
    public void AttachToLevel(CatController cat,Camera view,CatCamera follow,SceneryRenderer scenery,Transform frame)
    {
        Cat=cat;View=view;Follow=follow;levelFrame=frame;
        AddLand();AddReturnPassage();
        var coverage=CaveRoute.CreateMask();owned.Add(coverage);scenery.SetDescentCoverage(coverage,CaveRoute.MaskBounds);
        Cat.RecoveryBoundary=CaveRoute.ShouldRecover;Cat.ReturnedToStart+=ResetLevelView;
        Follow.PassageHeightGuide=LevelHeight;
    }
    void ResetLevelView(){travelling=false;cameraHeight=1;}
    float? LevelHeight(Vector2 coursePosition)
    {
        var p=CaveRoute.ToLocal(coursePosition);
        if(!travelling&&p.y<-.5f&&(CaveRoute.IsDry(coursePosition)||p.x>3.5f&&p.x<12))travelling=true;
        if(!travelling)return null;
        if(p.x<3.2f&&p.y>-.5f||p.x>235.69f&&p.y>4.9f||p.x>250){travelling=false;return null;}
        float normal=GuideHeight(p)+CaveRoute.VerticalOffset;
        if(p.x<14)normal=Mathf.Lerp(normal,Mathf.Min(normal,-10.15f),Mathf.SmoothStep(0,1,Mathf.InverseLerp(-3,-9,p.y)));
        else if(p.x<210){float interior=Mathf.Min(normal,-10.15f);normal=Mathf.Lerp(interior,normal,Mathf.SmoothStep(0,1,Mathf.InverseLerp(203.5f,210,p.x)));}
        return normal;
    }
    void RegisterMaterial(Material material)
    {
        if(material.HasProperty("_CaveOrigin")){material.SetVector("_CaveOrigin",transform.position);material.SetFloat("_CaveScale",transform.lossyScale.x);}
        if(material.HasProperty("_TerrainStone"))material.SetTexture("_TerrainStone",Resources.Load<Texture2D>("GoldenRiver/Descent/terrain-stone-v1"));
    }
    static Vector2[] Below(Vector2[] source,float top)
    {
        var points=new List<Vector2>();var previous=source[source.Length-1];
        foreach(var current in source) {
            bool a=previous.y<=top,b=current.y<=top;
            if(a!=b)points.Add(Vector2.Lerp(previous,current,(top-previous.y)/(current.y-previous.y)));
            if(b)points.Add(current);previous=current;
        }
        return points.ToArray();
    }
    Vector2[] EmbeddedContour(string name,Vector2[] source)
    {
        if(name=="Solid discovery roof") {
            var roof=new List<Vector2>{source[0],new Vector2(66,9)};
            for(int i=5;i<source.Length;i++)roof.Add(source[i]);source=roof.ToArray();
        }
        return name.ToLowerInvariant().Contains("roof")?Below(source,-4.45f):source;
    }
    void NativeShore(string name,Rect bounds,string texture,Vector2[] solid,float shift=0)
    {
        bounds.y+=shift;var points=(Vector2[])solid.Clone();
        for(int i=0;i<points.Length;i++)points[i].y+=shift;
        var material=new Material(Resources.Load<Material>("GoldenRiver/Descent/entry-study-material"));owned.Add(material);RegisterMaterial(material);
        material.mainTexture=Resources.Load<Texture2D>("GoldenRiver/Descent/"+texture);
        if(texture=="shore-exit-floor-v1"||texture=="shore-exit-roof-v1")RegisterSharpShore(material,points,bounds);
        if(texture=="shore-right-v1")material.SetFloat("_RoofEndJoin",1);
        if(texture=="shore-exit-floor-v1"){material.SetFloat("_NativeFoundationJoin",1);material.SetTexture("_SecondTex",Resources.Load<Texture2D>("GoldenRiver/Descent/return-floor-a-v1"));}
        var paint=(Vector2[])points.Clone();
        float crown=material.GetFloat("_SharpTerrain")>.5f?.6f:.12f;
        for(int i=0;i<points.Length;i++){int next=(i+1)%points.Length;if(points[next].x>points[i].x&&Mathf.Abs(points[next].y-points[i].y)<.001f){paint[i].y+=crown;paint[next].y+=crown;}}
        if(texture=="shore-exit-roof-v1")for(int i=14;i<paint.Length;i++)paint[i].y-=.7f;
        MeshObject(name,paint,bounds,-1.8f,material);Collider(name+" solid",points);
        var lip=Below(paint,-2.8f);
        var wet=new Material(material);owned.Add(wet);wet.SetFloat("_WetFooting",1);
        wet.SetFloat("_WetShore",1);
        wet.SetFloat("_ShoreEdge",texture=="shore-left-v1"?-5.3f:texture=="shore-right-v1"?12:texture=="shore-exit-roof-v1"?185:246.8f);
        wet.SetFloat("_ShoreDirection",texture=="shore-left-v1"||texture=="shore-exit-roof-v1"?1:-1);
        if(lip.Length>=3)MeshObject(name+" permanent wet footing",lip,bounds,-3,wet);
    }
    void RegisterSharpShore(Material material,Vector2[] contour,Rect bounds)
    {
        var profile=new Texture2D(4096,1,TextureFormat.RGBAFloat,false,true){name="Native shore contact heights",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};owned.Add(profile);
        for(int x=0;x<profile.width;x++) {
            float position=bounds.x+(x+.5f)/profile.width*bounds.width,nearest=float.PositiveInfinity,height=0;
            for(int i=0;i<contour.Length;i++) {
                var a=contour[i];var b=contour[(i+1)%contour.Length];
                if(b.x<=a.x||Mathf.Abs(a.y-b.y)>.001f)continue;
                float distance=Mathf.Abs(position-Mathf.Clamp(position,a.x,b.x));
                if(distance<nearest){nearest=distance;height=a.y;}
            }
            profile.SetPixel(x,0,new Color(height,0,0,1));
        }
        profile.Apply(false,true);material.SetFloat("_SharpTerrain",1);material.SetTexture("_ContactProfile",profile);
        material.SetVector("_ContactRect",new Vector4(bounds.x,bounds.width,0,0));
        material.SetTexture("_SharpCrown",Resources.Load<Texture2D>("GoldenRiver/bank-narrow-v3"));
    }
    void AddEmbeddedLand()
    {
        AddExteriorBeds();
        NativeShore("Low entrance shore",new Rect(-14,-17.44f,28,19.429f),"shore-left-v1",ShoreLeft);
        NativeShore("Opposing entrance shore and riverbed",new Rect(5,-9.7524f,32,16),"shore-right-v1",ShoreRight,-CaveRoute.VerticalOffset);
        Painting("Connected cave floor",new Rect(-1,-19.93f,36,12),-3,"GoldenRiver/Descent/entry-study-floor-v1");
        Collider("First cave floor",FloorLeft);Collider("Second cave floor",FloorRight);
        AddDiscovery();
        var material=new Material(Resources.Load<Material>("GoldenRiver/Descent/entry-study-material"));owned.Add(material);RegisterMaterial(material);
        material.mainTexture=Resources.Load<Texture2D>("GoldenRiver/Descent/entry-study-rear-v1");material.SetFloat("_Recessed",3);
        var rear=(Vector2[])RearContour.Clone();for(int i=0;i<rear.Length;i++){if(rear[i].y> -7)rear[i].y+=.8f;if(rear[i].x>204)rear[i].x-=.5f;}
        MeshObject("Permanent cave rear wall",rear,new Rect(0,-28,40,44),1.9f,material);
        NativeShore("Permanent exit cliff and mouth",new Rect(182,-6,26,13),"shore-exit-roof-v1",ExitRoof,-CaveRoute.VerticalOffset);
        NativeShore("Connected outer shore ascent",new Rect(186,-23.986364f,65,32),"shore-exit-floor-v1",ExitFloor,-CaveRoute.VerticalOffset);
    }
    void AddExteriorBeds()
    {
        var material=new Material(Resources.Load<Material>("GoldenRiver/Descent/entry-study-material"));owned.Add(material);RegisterMaterial(material);
        material.mainTexture=Resources.Load<Texture2D>("GoldenRiver/Descent/return-floor-a-v1");material.SetFloat("_StoneBody",1);material.SetFloat("_WetFooting",1);
        foreach(var span in new[]{new Vector3(-40,-24,-5.3f),new Vector3(246.8f,270,286)}) {
            var shape=new[]{new Vector2(span.x,CaveRoute.RiverbedTop(span.x)),new Vector2(span.y,CaveRoute.RiverbedTop(span.y)),
                new Vector2(span.z,CaveRoute.RiverbedTop(span.z)),new Vector2(span.z,-35),new Vector2(span.x,-35)};
            var paint=(Vector2[])shape.Clone();for(int i=0;i<3;i++)paint[i].y+=.45f;
            MeshObject("Permanent outer riverbed",paint,new Rect(span.x,-35,36,32),-3,material);Collider("Solid outer riverbed",shape);
        }
    }
    void AddReturnPassage()
    {
        for(int i=0;i<CaveRoute.ReturnFloors.Length;i++) {
            var p=CaveRoute.ReturnFloors[i];
            if(p.x>=188.4f)continue; // Native exit contours supply both art and collision.
            if(p.x==72||p.x==79)AnchoredCatch(p);else ReturnBank(p,"Return bank "+i,true);
            if(i+1<CaveRoute.ReturnFloors.Length) {
                var next=CaveRoute.ReturnFloors[i+1];float end=p.x+p.z,gap=next.x-end;
                if(gap>.01f&&Mathf.Abs(end-76)>.01f&&Mathf.Abs(end-82)>.01f&&Mathf.Abs(end-100)>.01f&&Mathf.Abs(end-161)>.01f)
                    ReturnBank(new Vector3(end,Mathf.Min(p.y,next.y),gap),"Attached return footing "+i,true);
            }
        }
        foreach(var p in CaveRoute.CatchFloors){if(p.z>3.1f)ReturnBank(p,"Recovery landing foundation",false);else if(p.x==76.5f)AnchoredCatch(p);}
        AttachedStone("Connected recovery staircase",new[]{new Vector2(72,-11.9f),new Vector2(76,-11.9f),new Vector2(76,-13.3f),
            new Vector2(80,-13.3f),new Vector2(80,-14.7f),new Vector2(84,-14.7f),new Vector2(84,-16.1f),
            new Vector2(88,-16.1f),new Vector2(88,-31),new Vector2(72,-31)});
        foreach(var span in new[]{new Vector2(84,100),new Vector2(103,106)}) {
            var edge=new Material(Resources.Load<Material>("GoldenRiver/Descent/entry-study-material"));owned.Add(edge);RegisterMaterial(edge);edge.SetFloat("_StoneBody",1);
            var rim=new List<Vector2>{new Vector2(span.x,-11.8f),new Vector2(span.x,-13.05f)};
            for(float x=span.x+1;x<span.y;x+=1)rim.Add(new Vector2(x,-13.07f-.1f*Mathf.Sin(x*2.7f)));
            rim.Add(new Vector2(span.y,-13.05f));rim.Add(new Vector2(span.y,-11.8f));
            MeshObject("Rescue tunnel rock joint",rim.ToArray(),new Rect(span.x,-14,span.y-span.x,3),-3,edge);
        }
        for(float x=60;x<187;x+=18) {
            float right=Mathf.Min(x+18,187),leftHeight=CaveRoute.Ceiling(x),rightHeight=CaveRoute.Ceiling(right);
            var material=BandMaterial(true,x,right,leftHeight,rightHeight);
            var paint=new[]{new Vector2(x,leftHeight-11.71f),new Vector2(right,rightHeight-11.71f),new Vector2(right,-4.45f),new Vector2(x,-4.45f)};
            MeshObject("Permanent riverbed roof",paint,new Rect(x,leftHeight-11.71f,36,26),-3,material);
            Collider("Return solid roof",new[]{new Vector2(x,leftHeight),new Vector2(right,rightHeight),new Vector2(right,-4.45f),new Vector2(x,-4.45f)});
        }
    }
    static readonly Vector2[] ShoreLeft={new Vector2(-4.21f,-.045f),new Vector2(3.79f,-.045f),new Vector2(3.79f,-.7f),new Vector2(3.6f,-1.585f),new Vector2(5.57f,-1.585f),new Vector2(5.57f,-2.15f),new Vector2(4.8f,-2.6f),new Vector2(4.2f,-3.234f),new Vector2(7.1f,-3.234f),new Vector2(7.1f,-3.8f),new Vector2(5.4f,-4.4f),new Vector2(5,-4.774f),new Vector2(8.49f,-4.774f),new Vector2(8.49f,-5.4f),new Vector2(6.8f,-6.1f),new Vector2(6.2f,-6.405f),new Vector2(9.82f,-6.405f),new Vector2(9.82f,-7),new Vector2(7.7f,-7.8f),new Vector2(7.2f,-8),new Vector2(10.9f,-8),new Vector2(10.9f,-8.6f),new Vector2(8.8f,-9.2f),new Vector2(7.8f,-10),new Vector2(13,-11),new Vector2(13,-18),new Vector2(-14,-18),new Vector2(-14,-7),new Vector2(-8,-5),new Vector2(-6,-3.1f),new Vector2(-4.8f,-2.1f)};
    static readonly Vector2[] ShoreRight={new Vector2(5.99f,.8f),new Vector2(9.99f,.8f),new Vector2(10.4f,.4f),new Vector2(10.6f,-1),new Vector2(11.6f,-2.6f),new Vector2(12.5f,-3.45f),new Vector2(13.2f,-4.45f),new Vector2(37,-4.45f),new Vector2(37,-6.1f),new Vector2(26,-6.04f),new Vector2(25,-5.88f),new Vector2(24,-5.96f),new Vector2(22,-5.76f),new Vector2(20,-5.9f),new Vector2(18,-5.41f),new Vector2(16,-5.74f),new Vector2(14,-5.33f),new Vector2(12,-5.8f),new Vector2(11.53f,-5.5f),new Vector2(10.01f,-4.5f),new Vector2(9.23f,-3),new Vector2(8.04f,-1.6f),new Vector2(7,-.7f),new Vector2(6.2f,-.2f)};
    static readonly Vector2[] ExitRoof={new Vector2(183,-3.45f),new Vector2(184.5f,-3.1f),new Vector2(185,-1.7f),new Vector2(186.2f,.7f),new Vector2(186.99f,4.8f),new Vector2(191.99f,4.8f),new Vector2(191.99f,4.2f),new Vector2(205.79f,4.2f),new Vector2(205.9f,3.6f),new Vector2(205.3f,2.4f),new Vector2(205.8f,1.1f),new Vector2(205.5f,-.5f),new Vector2(205.9f,-1.6f),new Vector2(205.1f,-3.1f),new Vector2(205.79f,-4.55f),new Vector2(204,-4.59f),new Vector2(202,-4.77f),new Vector2(200,-4.83f),new Vector2(198,-4.65f),new Vector2(196,-5f),new Vector2(194,-4.58f),new Vector2(192,-5.24f),new Vector2(190,-4.87f),new Vector2(188,-4.74f),new Vector2(186,-4.56f),new Vector2(184,-4.56f),new Vector2(183,-4.53f)};
    static readonly Vector2[] ExitFloor={new Vector2(187,-8.09545f),new Vector2(204.18f,-8.09545f),new Vector2(204.18f,-6.60455f),new Vector2(208.36f,-6.60455f),new Vector2(208.36f,-5.15f),new Vector2(212.32f,-5.15f),new Vector2(212.32f,-3.65909f),new Vector2(216.21f,-3.65909f),new Vector2(216.21f,-2.35f),new Vector2(220.39f,-2.35f),new Vector2(220.39f,-.85909f),new Vector2(224.28f,-.85909f),new Vector2(224.28f,.63182f),new Vector2(228.24f,.63182f),new Vector2(228.24f,2.19545f),new Vector2(232.13f,2.19545f),new Vector2(232.13f,3.68636f),new Vector2(235.69f,3.68636f),new Vector2(235.69f,5.25f),new Vector2(245.69f,5.25f),new Vector2(246.1f,4.1f),new Vector2(245.6f,2),new Vector2(246.2f,.63182f),new Vector2(245.7f,-.85909f),new Vector2(247,-3.1f),new Vector2(250,-5),new Vector2(250,-24),new Vector2(187,-24)};
    static readonly Vector2[] RearContour={new Vector2(11,-20),new Vector2(205.5f,-28),new Vector2(205.5f,-8),new Vector2(205.4f,-7.2f),new Vector2(204.8f,-6),new Vector2(205.1f,-5.2f),new Vector2(205.79f,-4.6f),new Vector2(183,-4.6f),new Vector2(166.7f,-6.2f),new Vector2(84,-6.2f),new Vector2(65,-4.5f),new Vector2(36,-5.1f),new Vector2(24,-5.5f),new Vector2(16,-5.4f),new Vector2(11,-4.8f)};
    void AnchoredCatch(Vector3 p)
    {
        Vector2[] outline;
        if(p.x==72)outline=new[]{new Vector2(71.8f,p.y),new Vector2(76,p.y),new Vector2(75.8f,p.y-.35f),new Vector2(74.9f,p.y-.5f),new Vector2(71.8f,p.y-.5f)};
        else if(p.x==79)outline=new[]{new Vector2(79,p.y),new Vector2(82,p.y),new Vector2(82,-11.4f),new Vector2(84.2f,-11.4f),
            new Vector2(84.2f,-12.1f),new Vector2(81,-12.1f),new Vector2(79,-10.1f)};
        else outline=new[]{new Vector2(p.x,p.y),new Vector2(79.5f,p.y),new Vector2(79.5f,p.y-.5f),new Vector2(p.x+.2f,p.y-.35f)};
        AttachedStone("Attached recovery ledge",outline);
    }
    void AttachedStone(string name,Vector2[] outline)
    {
        float left=float.PositiveInfinity,right=float.NegativeInfinity,bottom=float.PositiveInfinity,top=float.NegativeInfinity;
        foreach(var p in outline){left=Mathf.Min(left,p.x);right=Mathf.Max(right,p.x);bottom=Mathf.Min(bottom,p.y);top=Mathf.Max(top,p.y);}
        var bounds=Rect.MinMaxRect(left,bottom,right,top+.6f);
        var material=new Material(Resources.Load<Material>("GoldenRiver/Descent/entry-study-material"));owned.Add(material);RegisterMaterial(material);RegisterSharpShore(material,outline,bounds);
        var paint=(Vector2[])outline.Clone();
        for(int i=0;i<outline.Length;i++){int next=(i+1)%outline.Length;if(outline[next].x>outline[i].x&&Mathf.Abs(outline[next].y-outline[i].y)<.001f){paint[i].y+=.6f;paint[next].y+=.6f;}}
        MeshObject(name,paint,bounds,-3,material);Collider(name+" solid",outline);
    }
    Material BandMaterial(bool roof,float left,float right,float first,float last)
    {
        var material=new Material(Resources.Load<Material>("GoldenRiver/Descent/entry-study-material"));owned.Add(material);RegisterMaterial(material);
        material.mainTexture=Resources.Load<Texture2D>("GoldenRiver/Descent/"+(roof?"return-roof-v1":"return-floor-a-v1"));
        material.SetFloat("_Band",1);material.SetVector("_BandProfile",new Vector4(left,first,right,last));
        material.SetVector("_BandRect",new Vector4(0,(roof?-11.71f:-22.868f)/3,12,26f/3));
        if(!roof){material.SetTexture("_SecondTex",Resources.Load<Texture2D>("GoldenRiver/Descent/return-floor-b-v1"));material.SetFloat("_BandFloorBlend",1);}
        return material;
    }
    void ReturnBank(Vector3 p,string name,bool carve,bool catchCap=false)
    {
        var material=BandMaterial(false,p.x,p.x+p.z,p.y,p.y);
        float depth=catchCap&&p.y> -13?.5f:22.8f;
        var solid=Rectangle(new Rect(p.x+.05f,p.y-depth,Mathf.Max(.01f,p.z-.1f),depth));
        var paint=Rectangle(new Rect(p.x,p.y-depth-.068f,p.z,depth+.25f));
        var solids=new List<Vector2[]>{solid};var paintings=new List<Vector2[]>{paint};
        if(carve)for(int i=0;i<(catchCap?1:CaveRoute.CatchAir.Length);i++){
            var air=CaveRoute.CatchAir[i];solids=CaveGeometry.Subtract(solids,air);
            // Let the native roof rim, rather than a flat subtraction line, own the bearing underside.
            if(i==0&&p.x>=84&&p.x<106&&p.y> -13){air=(Vector2[])air.Clone();for(int j=0;j<air.Length;j++)if(air[j].y> -13.4f)air[j].y+=.18f;}
            paintings=CaveGeometry.Subtract(paintings,air);
        }
        foreach(var piece in solids)Collider(name,piece);
        foreach(var piece in paintings)MeshObject(name,piece,new Rect(p.x,p.y-22.868f,36,26),.65f,material);
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
