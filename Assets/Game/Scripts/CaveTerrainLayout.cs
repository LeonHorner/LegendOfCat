using System.Collections.Generic;
using UnityEngine;

// One authored solid contour supplies both the visible rock and static collision.
public static class CaveTerrainLayout
{
    public const float Bottom=-42, CatchHeight=-26.5f;
    public static readonly Rect[] EntranceFrames={new Rect(-2.21f,-6.045f,6,6),new Rect(-6,-8.8f,6,6),new Rect(3.4f,-6.4f,6,6),
        new Rect(5.99f,-5.245f,6,6),new Rect(9,-13,6,6),new Rect(12,-10.5f,6,6)};
    public sealed class Section
    {
        public readonly string Name;
        public readonly Vector2[] Outline;
        public readonly Vector3[] Treads;
        public Section(string name,Vector2[] outline,params Vector3[] treads){Name=name;Outline=outline;Treads=treads;}
    }
    public static readonly Vector3[] Floors={
        new Vector3(59,-14.1f,4),new Vector3(65,-15.55f,4),new Vector3(71,-17,4),new Vector3(77,-18.5f,4),
        new Vector3(83,-20,5),new Vector3(89,-21.3f,4),new Vector3(95.5f,-20,4.5f),
        new Vector3(103,-20,3),new Vector3(107.5f,-20,2.5f),
        new Vector3(110,-18.6f,5),new Vector3(116,-17.2f,4),new Vector3(109.7f,-15.8f,4),new Vector3(117.3f,-14.4f,3.7f),
        new Vector3(121,-14.8f,2),new Vector3(123,-14,5),new Vector3(130,-16.7f,6),
        new Vector3(136,-16.4f,5),new Vector3(142,-15,4),new Vector3(135.7f,-13.6f,4),new Vector3(143.3f,-12.2f,3.7f),
        new Vector3(147,-12.6f,2),new Vector3(149,-12.2f,5),new Vector3(155,-10.8f,4),
        new Vector3(148.7f,-9.4f,4),new Vector3(156.3f,-8.14045f,4),new Vector3(162,-9.54045f,4.5f),
        new Vector3(168,-8.14045f,5),new Vector3(175,-9.54045f,4),new Vector3(181,-8.14045f,4),
        new Vector3(187,-8.14045f,4),new Vector3(193,-9.54045f,4),new Vector3(199,-8.14045f,3.5f),
        new Vector3(204.18f,-6.64955f,4.18f),new Vector3(208.36f,-5.195f,3.96f),new Vector3(212.32f,-3.70409f,3.89f),
        new Vector3(216.21f,-2.395f,4.18f),new Vector3(220.39f,-.90409f,3.89f),new Vector3(224.28f,.58682f,3.96f),
        new Vector3(228.24f,2.15045f,3.89f),new Vector3(232.13f,3.64136f,3.56f),new Vector3(235.69f,5.205f,10)
    };
    public static readonly Vector3[] Recovery={new Vector3(95,CatchHeight,9),new Vector3(91,-25.2f,3),
        new Vector3(87,-23.9f,3),new Vector3(81.5f,-22.6f,3),new Vector3(78.5f,-21.3f,3.5f)};
    public static readonly Vector2[][] RecoveryAir={Box(78,-28,27,8.5f)};
    public static readonly Vector2[] LeftShore=RoundWetShore(new[]{new Vector2(-2.65f,-.045f),new Vector2(3.79f,-.045f),new Vector2(3.79f,-.7f),
        new Vector2(3.6f,-1.585f),new Vector2(5.57f,-1.585f),new Vector2(5.57f,-2.15f),new Vector2(4.8f,-2.6f),
        new Vector2(4.2f,-3.234f),new Vector2(7.1f,-3.234f),new Vector2(7.1f,-3.8f),new Vector2(5.4f,-4.4f),
        new Vector2(5,-4.774f),new Vector2(8.49f,-4.774f),new Vector2(8.49f,-5.4f),new Vector2(6.8f,-6.1f),
        new Vector2(6.2f,-6.405f),new Vector2(9.82f,-6.405f),new Vector2(9.82f,-7),new Vector2(7.7f,-7.8f),
        new Vector2(7.2f,-8),new Vector2(10.9f,-8),new Vector2(10.9f,-8.6f),new Vector2(8.8f,-9.2f),
        new Vector2(10.4f,-9.3f),new Vector2(13.6f,-9.3f),new Vector2(13.6f,-10.7f),new Vector2(16.3f,-10.7f),
        new Vector2(16.3f,-11.3f),new Vector2(15.7f,-12),new Vector2(18.06f,-12),new Vector2(18.06f,Bottom),new Vector2(-14,Bottom),
        new Vector2(-14,-7),new Vector2(-11.8f,-6.4f),new Vector2(-10.6f,-5.9f),new Vector2(-10.1f,-5.4f),
        new Vector2(-9.7f,-5.3f),new Vector2(-9.1f,-4.9f),new Vector2(-8.9f,-4.4f),new Vector2(-8.2f,-4.2f),
        new Vector2(-8,-3.7f),new Vector2(-7.3f,-3.6f),new Vector2(-7.1f,-3.05f),new Vector2(-6.6f,-2.85f),
        new Vector2(-6.4f,-2.6f),new Vector2(-5.8f,-2.55f),new Vector2(-5.5f,-2.1f),new Vector2(-4.2f,-1.45f),new Vector2(-3.3f,-.7f)});
    static Vector2[] RoundWetShore(Vector2[] source)
    {
        var result=new List<Vector2>();bool wet=false;
        for(int i=0;i<source.Length;i++){
            if(source[i]==new Vector2(-14,-7)){wet=true;result.Add(source[i]);continue;}
            if(!wet||i==source.Length-1){result.Add(source[i]);continue;}
            var start=Vector2.Lerp(source[i-1],source[i],.65f);var end=Vector2.Lerp(source[i],source[i+1],.35f);
            for(int sample=0;sample<=5;sample++){float t=sample/5f;result.Add((1-t)*(1-t)*start+2*(1-t)*t*source[i]+t*t*end);}
        }
        return result.ToArray();
    }
    public const float ApproachRiseStart=-5.86f,ApproachRiseEnd=-2.65f;
    public static float ApproachHeight(float x)=>Mathf.Lerp(-1.245f,-.045f,Mathf.SmoothStep(0,1,Mathf.InverseLerp(ApproachRiseStart,ApproachRiseEnd,x)));
    static Section ApproachShore()
    {
        var platform=MovementCourse.AscentPlatforms[9];
        float left=platform.x-platform.z/2-CaveRoute.Origin;
        const float floor=-1.245f;
        var outline=new List<Vector2>{new Vector2(left+.12f,floor)};
        for(int i=0;i<=24;i++){float x=Mathf.Lerp(ApproachRiseStart,ApproachRiseEnd,i/24f);outline.Add(new Vector2(x,ApproachHeight(x)));}
        outline.AddRange(new[]{new Vector2(-2.2f,-.045f),new Vector2(-2.2f,Bottom),new Vector2(left-.7f,Bottom),
            new Vector2(left-.7f,-4.8f),new Vector2(left-.35f,-3.5f),new Vector2(left-.45f,-2.8f),new Vector2(left-.12f,-1.7f),new Vector2(left,-1.36f)});
        return new Section("Connected approach shore",outline.ToArray(),new Vector3(left+.12f,floor,ApproachRiseStart-left-.12f));
    }
    public static float RiverbedSurface(float x)=>-4.45f+.16f*Mathf.Sin(x*2.7f)+.07f*Mathf.Sin(x*5.9f);
    public static readonly Vector2[] EntryRoof=BuildEntranceRoof();
    static Vector2[] BuildEntranceRoof()
    {
        // Both existing main landings belong to one massive cliff. Their original
        // contact heights and widths remain; the shallow cleft still invites a jump.
        var points=new List<Vector2>{new Vector2(5.99f,.755f),new Vector2(9.99f,.755f),new Vector2(10.2f,-.35f),
            new Vector2(11.35f,-.35f),new Vector2(11.79f,.955f),new Vector2(17.79f,.955f),new Vector2(18.25f,.55f),
            new Vector2(18.7f,-.6f),new Vector2(19.2f,-1.7f),new Vector2(20.3f,-3.1f)};
        for(float x=22;x<=34;x+=.5f)points.Add(new Vector2(x,RiverbedSurface(x)));
        points.AddRange(new[]{new Vector2(34,-8.2f),new Vector2(29,-8.6f),new Vector2(24,-8.6f),new Vector2(18,-8.6f),
            new Vector2(16,-7.8f),new Vector2(14,-7),new Vector2(12,-5.8f),new Vector2(11.53f,-5.5f),new Vector2(10.7f,-5.15f),new Vector2(10.01f,-4.5f),
            new Vector2(9.23f,-3),new Vector2(8.7f,-2.7f),new Vector2(8.04f,-1.6f),new Vector2(7.5f,-1.35f),new Vector2(7,-.7f),new Vector2(6.2f,-.2f)});
        return points.ToArray();
    }    public static Vector2[] Box(float x,float y,float w,float h)=>new[]{new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h)};
    static Section Bank(string name,Vector3 p,bool carve=false)
    {
        var outline=new[]{new Vector2(p.x,p.y),new Vector2(p.x+p.z,p.y),new Vector2(p.x+p.z,Bottom),new Vector2(p.x,Bottom)};
        return new Section(name,outline,p);
    }
    public static List<Section> Build()
    {
        var result=new List<Section>{ApproachShore(),new Section("Entrance shore",LeftShore,
            new Vector3(-2.65f,-.045f,6.44f),new Vector3(3.6f,-1.585f,1.97f),new Vector3(4.2f,-3.234f,2.9f),
            new Vector3(5,-4.774f,3.49f),new Vector3(6.2f,-6.405f,3.62f),new Vector3(7.2f,-8,3.7f),
            new Vector3(10.4f,-9.3f,3.2f),new Vector3(13.6f,-10.7f,2.7f)),
            new Section("Massive entrance overhang",EntryRoof,new Vector3(5.99f,.755f,4),new Vector3(11.79f,.955f,6))};
        var first=(Vector2[])CaveEntryStudy.FloorLeft.Clone();var second=(Vector2[])CaveEntryStudy.FloorRight.Clone();
        for(int i=0;i<first.Length;i++)first[i].y=first[i].y< -18?Bottom:first[i].y-2;
        for(int i=0;i<second.Length;i++)second[i].y=second[i].y< -18?Bottom:second[i].y-2;
        result.Add(new Section("First passage floor",first,new Vector3(-1,-12,19.06f)));
        result.Add(new Section("Second passage floor",second,new Vector3(20.34f,-12,15.536f)));
        result.Add(new Section("Fountain room floor",new[]{new Vector2(32,-12),new Vector2(35.876f,-12),new Vector2(35.876f,-12.3f),
            new Vector2(38.839f,-12.3f),new Vector2(38.839f,-12.67f),new Vector2(57.106f,-12.67f),
            new Vector2(57.106f,Bottom),new Vector2(32,Bottom)},new Vector3(32,-12,3.876f),new Vector3(35.876f,-12.3f,2.963f),new Vector3(38.839f,-12.67f,18.267f)));
        for(float x=34;x<183;x+=8){
            float right=Mathf.Min(183,x+8);var outline=new List<Vector2>();
            for(float sample=x;sample<right;sample+=.5f)outline.Add(new Vector2(sample,RiverbedSurface(sample)));
            outline.Add(new Vector2(right,RiverbedSurface(right)));outline.Add(new Vector2(right,CaveRoute.Ceiling(right)));outline.Add(new Vector2(x,CaveRoute.Ceiling(x)));
            result.Add(new Section("Cave roof "+x,outline.ToArray()));
        }
        result.Add(new Section("Main exit cliff",new[]{new Vector2(183,-3.45f),new Vector2(184.5f,-3.1f),new Vector2(185,-1.7f),
            new Vector2(186.2f,.655f),new Vector2(186.99f,4.755f),new Vector2(191.99f,4.755f),new Vector2(191.99f,4.155f),
            new Vector2(205.79f,4.155f),new Vector2(205.9f,3.6f),new Vector2(205.3f,2.4f),new Vector2(205.8f,1.1f),
            new Vector2(205.5f,-.5f),new Vector2(205.9f,-1.6f),new Vector2(205.1f,-3.1f),new Vector2(205.79f,-4.6f),
            new Vector2(202,-4.8f),new Vector2(198,-4.65f),new Vector2(194,-4.6f),new Vector2(190,-4.9f),new Vector2(186,-4.6f),new Vector2(183,-4.6f)},
            new Vector3(186.99f,4.755f,5),new Vector3(191.99f,4.155f,13.8f)));
        foreach(var p in Floors){
            if(p.x==109.7f||p.x==135.7f||p.x==148.7f){float root=p.x-3.7f;result.Add(new Section("Ceiling rooted climbing ledge "+p.x,
                new[]{new Vector2(root,-5.8f),new Vector2(p.x-.7f,-5.8f),new Vector2(p.x-.7f,p.y),new Vector2(p.x+p.z,p.y),
                    new Vector2(p.x+p.z,p.y-.5f),new Vector2(root,p.y-.5f)},p));}
            else if(p.x==117.3f||p.x==143.3f||p.x==156.3f)result.Add(new Section("Side rooted climbing ledge "+p.x,
                new[]{new Vector2(p.x,p.y),new Vector2(p.x+p.z+.15f,p.y),new Vector2(p.x+p.z+.15f,p.y-.5f),new Vector2(p.x,p.y-.5f)},p));
            else if(p.x==235.69f)result.Add(new Section("Outer exit shore",new[]{new Vector2(p.x,p.y),new Vector2(245.69f,p.y),
                new Vector2(246.1f,4.055f),new Vector2(245.6f,1.955f),new Vector2(246.2f,.58682f),new Vector2(245.7f,-.90409f),
                new Vector2(247,-3.145f),new Vector2(250,-5.045f),new Vector2(250,Bottom),new Vector2(p.x,Bottom)},p));
            else {var section=Bank("Climbing bank "+p.x,p);if(p.x>=77&&p.x<106){var pieces=new List<Vector2[]>{section.Outline};foreach(var air in RecoveryAir)pieces=CaveGeometry.Subtract(pieces,air);foreach(var piece in pieces)result.Add(new Section(section.Name,piece));result.Add(new Section("Recovery roof contact "+p.x,Box(p.x,p.y-.3f,p.z,.3f),p));}else result.Add(section);}
        }
        foreach(var p in Recovery)result.Add(Bank("Recovery stair "+p.x,p));
        foreach(var side in new[]{new Vector2(-44,-5.8f),new Vector2(247,290)}){
            float left=side.x,right=side.y;var edge=new List<Vector2>();
            for(float x=left;x<right;x+=.4f)edge.Add(new Vector2(x,-4.85f+.1f*Mathf.Sin(x*2.7f)+.06f*Mathf.Sin(x*5.9f)));
            edge.Add(new Vector2(right,-4.85f+.1f*Mathf.Sin(right*2.7f)+.06f*Mathf.Sin(right*5.9f)));
            edge.Add(new Vector2(right,Bottom));edge.Add(new Vector2(left,Bottom));result.Add(new Section("Outer river foundation",edge.ToArray()));}
        return result;
    }
    public static List<Vector3> Path()
    {
        var result=new List<Vector3>{new Vector3(2.6f,-.045f,0),new Vector3(5,-1.585f,0),new Vector3(6.5f,-3.234f,0),
            new Vector3(7.8f,-4.774f,0),new Vector3(9,-6.405f,0),new Vector3(10.3f,-8,0),new Vector3(12,-9.3f,0),
            new Vector3(14.7f,-10.7f,0),new Vector3(17.2f,-12,0),new Vector3(22,-12,1),new Vector3(34,-12,0),new Vector3(37.5f,-12.3f,0),
            new Vector3(44,-12.67f,0),new Vector3(55.8f,-12.67f,0)};
        float previous=-12.67f,end=57.106f;
        for(int i=0;i<Floors.Length;i++){var p=Floors[i];bool right=i+1==Floors.Length||Floors[i+1].x>=p.x;
            float target=p.x>232&&p.x<233?234.3f:right?p.x+p.z-.7f:p.x+.7f;
            bool jump=p.y>previous+.15f||Mathf.Abs(p.y-previous)<.15f&&p.x>end+.1f;
            result.Add(new Vector3(target,p.y,jump?1:0));previous=p.y;end=p.x+p.z;}
        var exit=CaveRoute.ToLocal(CaveRoute.Rejoin);result.Add(new Vector3(exit.x,exit.y-.52f,0));return result;
    }
}
