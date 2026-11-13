using System.Collections.Generic;
using UnityEngine;

public sealed partial class CaveEntryStudy
{
    Transform levelFrame;
    bool lining,travelling,wantsInside,inside,leaving;
    float outsideDepth=-10;
    readonly Dictionary<Material,Material> interiorMaterials=new Dictionary<Material,Material>();
    public void AttachToLevel(CatController cat,Camera view,CatCamera follow,SceneryRenderer scenery,Transform frame)
    {
        Cat=cat;View=view;Follow=follow;levelFrame=frame;outsideDepth=view.transform.position.z;
        View.cullingMask&=~(1<<31);
        scenery.PreserveCameraDepth=true;
        AddLand();AddReturnPassage();
        var coverage=CaveRoute.CreateMask();owned.Add(coverage);scenery.SetDescentCoverage(coverage,CaveRoute.MaskBounds);
        Cat.RecoveryBoundary=CaveRoute.ShouldRecover;Cat.ReturnedToStart+=ResetLevelView;
        Follow.PassageHeightGuide=LevelHeight;Follow.PassageDepthGuide=()=>wantsInside?levelFrame.TransformPoint(new Vector3(0,0,-.8f)).z:outsideDepth;
    }
    void ResetLevelView(){travelling=wantsInside=inside=leaving=false;cameraHeight=1;if(View)View.cullingMask&=~(1<<31);}
    float? LevelHeight(Vector2 coursePosition)
    {
        var p=CaveRoute.ToLocal(coursePosition);var camera=levelFrame.InverseTransformPoint(View.transform.position);
        if(!travelling && p.y<-.5f && (CaveRoute.IsDry(coursePosition)||p.x>3.5f&&p.x<12))travelling=true;
        if(!travelling)return null;
        float normal=GuideHeight(p)+CaveRoute.VerticalOffset;
        if(!inside && !leaving && p.y< -6.4f) {
            normal=Mathf.Min(normal,p.y-2.4f);
            if(camera.y+5.5f< -3.2f)wantsInside=true;
            if(wantsInside&&camera.z> -1.8f)inside=true;
        }
        bool exiting=p.x>219&&p.y> -1.8f || p.x<9&&p.y> -2.5f;
        if(inside&&exiting)leaving=true;
        if(leaving) {
            normal=Mathf.Max(2.7f,p.y+1.8f);
            if(camera.y-5.5f> -3)wantsInside=false;
            if(!wantsInside&&camera.z< -3) {
                inside=false;
                if(p.x<9){travelling=leaving=false;return null;}
                normal=Mathf.Lerp(normal,1.9f,Mathf.SmoothStep(0,1,Mathf.InverseLerp(CaveRoute.RejoinLocal-26,CaveRoute.RejoinLocal-6,p.x)));
                if(p.x>CaveRoute.RejoinLocal-5&&p.y>5.4f){travelling=leaving=false;return null;}
            }
        } else if(inside&&p.x<190)normal=Mathf.Min(normal,-8.9f);
        View.cullingMask=inside?View.cullingMask|(1<<31):View.cullingMask&~(1<<31);
        return normal;
    }
    void RegisterMaterial(Material material)
    {
        if(material.HasProperty("_CaveOrigin")){material.SetVector("_CaveOrigin",transform.position);material.SetFloat("_CaveScale",transform.lossyScale.x);}
    }
    Vector2[] EmbeddedContour(string name,Vector2[] source)
    {
        if(name=="Left cliff and descending ledges") {
            var points=new List<Vector2>{new Vector2(-4.21f,-.045f)};
            for(int i=8;i<source.Length-1;i++)points.Add(source[i]);points.Add(new Vector2(-4.21f,-16));return points.ToArray();
        }
        if(name=="Opposing bank, ceiling and end wall") {
            var bank=new List<Vector2>{new Vector2(7,-.045f),new Vector2(11,-.045f),new Vector2(11,-4.145f),new Vector2(26,-4.145f)};
            for(int i=2;i<source.Length;i++)bank.Add(source[i]);return bank.ToArray();
        }
        if(name=="Solid discovery roof") {
            var roof=new List<Vector2>{source[0],new Vector2(66,9)};
            for(int i=5;i<source.Length;i++)roof.Add(source[i]);source=roof.ToArray();
        }
        if(name.ToLowerInvariant().Contains("roof")) {
            var pieces=CaveGeometry.Subtract(new List<Vector2[]>{source},Rectangle(new Rect(-100,-4.145f,600,100)));
            return pieces.Count>0?pieces[0]:System.Array.Empty<Vector2>();
        }
        return source;
    }
    void AddInteriorLining(string name,Vector2[] source,Rect uv,float depth,Material material)
    {
        if(!Embedded||lining||depth<0||name=="Rooted fountain landmark")return;
        lining=true;
        if(!interiorMaterials.TryGetValue(material,out var front)) {
            front=new Material(material);front.SetFloat("_Interior",1);owned.Add(front);interiorMaterials.Add(material,front);
        }
        if(material.GetFloat("_Recessed")>.5f) {
            MeshObject("Interior recessed wall",source,uv,Mathf.Min(depth,1.9f),front);
        } else if(material.GetFloat("_ExteriorRoof")>.5f) {
            MeshObject("Interior ceiling mass",source,uv,-.2f,front);
        } else foreach(var piece in CaveGeometry.Subtract(new List<Vector2[]>{source},Rectangle(new Rect(-100,-3.345f,600,100))))
            MeshObject("Interior rock occlusion",piece,uv,-.2f,front);
        lining=false;
    }
    void AddReturnPassage()
    {
        for(int i=0;i<CaveRoute.ReturnFloors.Length;i++) {
            var p=CaveRoute.ReturnFloors[i];ReturnBank(p,"Return bank "+i,true);
            if(i+1<CaveRoute.ReturnFloors.Length) {
                var next=CaveRoute.ReturnFloors[i+1];float end=p.x+p.z,gap=next.x-end;
                if(gap>.01f&&Mathf.Abs(end-76)>.01f&&Mathf.Abs(end-82)>.01f&&Mathf.Abs(end-100)>.01f&&Mathf.Abs(end-161)>.01f&&Mathf.Abs(end-200)>.01f)
                    ReturnBank(new Vector3(end,Mathf.Min(p.y,next.y),gap),"Attached return footing "+i,true);
            }
        }
        foreach(var p in CaveRoute.CatchFloors)ReturnBank(p,"Concealed local return",p.y> -13,true);
        // Native undersides conceal the geometric rescue tunnel subtraction.
        foreach(var span in new[]{new Vector2(84,100),new Vector2(103,105)}) {
            var edge=BandMaterial(true,span.x,span.y,-13,-13);
            MeshObject("Rescue tunnel bearing rock",Rectangle(new Rect(span.x,-13.2f,span.y-span.x,1.85f)),
                new Rect(span.x,-24.71f,36,26),-.19f,edge);
        }
        for(float x=64;x<CaveRoute.RejoinLocal+5;x+=18) {
            float right=Mathf.Min(x+18,CaveRoute.RejoinLocal+5),leftHeight=ReturnCeiling(x),rightHeight=ReturnCeiling(right);
            var material=BandMaterial(true,x,right,leftHeight,rightHeight);
            var paint=new[]{new Vector2(x,leftHeight-11.71f),new Vector2(right,rightHeight-11.71f),
                new Vector2(right,rightHeight+14.29f),new Vector2(x,leftHeight+14.29f)};
            MeshObject("Return roof",paint,new Rect(x,leftHeight-11.71f,36,26),.9f,material);
            Collider("Return solid roof",new[]{new Vector2(x,leftHeight),new Vector2(right,rightHeight),
                new Vector2(right,rightHeight+14.29f),new Vector2(x,leftHeight+14.29f)});
        }
        for(float x=65;x<CaveRoute.RejoinLocal+5;x+=40)Painting("Quiet return recess",new Rect(x,-34,40,44),3,"GoldenRiver/Descent/entry-study-rear-v1",true);
    }
    static float ReturnCeiling(float x)
    {
        if(x<84)return Mathf.Lerp(-4.5f,-6.2f,Mathf.InverseLerp(64,84,x));
        if(x<CaveRoute.ExitStart)return -6.2f;
        return Mathf.Lerp(-6.2f,7.9f,Mathf.InverseLerp(CaveRoute.ExitStart,CaveRoute.RejoinLocal,x));
    }
    Material BandMaterial(bool roof,float left,float right,float first,float last)
    {
        var material=new Material(Resources.Load<Material>("GoldenRiver/Descent/entry-study-material"));owned.Add(material);RegisterMaterial(material);
        material.mainTexture=Resources.Load<Texture2D>("GoldenRiver/Descent/"+(roof?"return-roof-v1":"return-floor-a-v1"));
        material.SetFloat("_Band",1);material.SetVector("_BandProfile",new Vector4(left,first,right,last));
        material.SetVector("_BandRect",new Vector4(0,roof?-11.71f:-22.868f,36,26));
        material.SetFloat("_ExteriorRoof",roof?1:0);
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
