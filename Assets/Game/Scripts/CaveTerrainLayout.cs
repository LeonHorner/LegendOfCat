using System.Collections.Generic;
using UnityEngine;

// One authored solid contour supplies both the visible rock and static collision.
public static class CaveTerrainLayout
{
    public const float Bottom=-42, CatchHeight=-26.5f;
    public static readonly int[] PaintedEntranceModules={3,5};
    public static readonly Rect[] EntranceFrames={new Rect(-2.21f,-6.045f,6,6),new Rect(-6,-8.8f,6,6),new Rect(3.4f,-6.4f,6,6),
        new Rect(5.99f,-5.245f,6,6),new Rect(9,-13,6,6),new Rect(12,-10.5f,6,6)};
    public sealed class Section
    {
        public readonly string Name;
        public readonly Vector2[] Outline;
        public readonly Vector3[] Treads;
        internal TerrainArtProfile Shelf;
        internal Rect ArtFrame;
        internal Vector2[] ExposedBed;
        public Section(string name,Vector2[] outline,params Vector3[] treads){Name=name;Outline=outline;Treads=treads;}
    }
    public static readonly Vector3[] Floors={
        // A flood bench breaks into shorter shelves above the concealed catch.
        new Vector3(58.7f,-13.6f,7.6f), new Vector3(68,-15,4.9f),
        new Vector3(74,-16.6f,3), new Vector3(77,-18.5f,4),
        new Vector3(83,-20,5), new Vector3(89,-21.3f,4), new Vector3(95.5f,-20,4.5f),
        new Vector3(103,-20,3), new Vector3(107.5f,-20,4.8f),
        // Climb the inside of the abandoned channel. Broad benches interrupt the precision jumps.
        new Vector3(113.7f,-18.7f,3.9f), new Vector3(119.1f,-17.35f,6.5f),
        new Vector3(127,-15.95f,3.6f), new Vector3(132,-16.05f,7.2f),
        new Vector3(141,-16.1f,4.4f), new Vector3(147,-15,4.2f),
        new Vector3(152.6f,-13.65f,6.8f), new Vector3(160.7f,-12.25f,3.8f),
        new Vector3(165.8f,-10.9f,6.4f), new Vector3(173.5f,-9.6f,6),
        new Vector3(181,-8.14f,4), new Vector3(187,-8.14f,5.6f),
        new Vector3(194,-9.54f,4.4f), new Vector3(199.6f,-8.14f,3.4f),
        new Vector3(204.18f,-6.65f,1.8f), new Vector3(214,-3.5f,3.6f),
        new Vector3(219,-2.15f,3.7f), new Vector3(224,-.85f,3.1f),
        new Vector3(235.69f,5.205f,10)
    };
    public static readonly Vector3[] Recovery={new Vector3(96.1f,CatchHeight,7.9f),new Vector3(91.7f,-25.2f,2.3f),
        new Vector3(87,-23.9f,3),new Vector3(84,-22.6f,1),new Vector3(77.8f,-21.3f,4.4f)};
    public static readonly Vector2[][] RecoveryAir={Box(78,-28,27,8.5f)};
    public static readonly Vector2[] LeftShore=RoundWetShore(new[]{new Vector2(-2.65f,-.045f),new Vector2(2.45f,-.045f),
        new Vector2(2.75f,-.058f),new Vector2(3.05f,-.11f),new Vector2(3.35f,-.21f),
        new Vector2(3.7f,-.38f),new Vector2(4.1f,-.64f),new Vector2(5,-1.37f),
        new Vector2(8,-3.85f), new Vector2(12,-6.95f), new Vector2(16,-9.57f),
        new Vector2(18.5f,-11.46f), new Vector2(19.7f,-12), new Vector2(20.34f,-12),new Vector2(20.34f,Bottom),new Vector2(-14,Bottom),
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
    // Quadratic fillets preserve the authored endpoints and round changes of direction.
    static Vector2[] RoundedEdge(Vector2[] knots, float radius)
    {
        var edge=new List<Vector2> { knots[0] };
        for (int i=1;i<knots.Length-1;i++) {
            var p=knots[i];
            var incoming=p-knots[i-1];
            var outgoing=knots[i+1]-p;
            float cut=Mathf.Min(radius, .38f*Mathf.Min(incoming.magnitude,outgoing.magnitude));
            var start=p-incoming.normalized*cut;
            var end=p+outgoing.normalized*cut;
            for(int k=0;k<=4;k++) {
                float t=k*.25f;
                edge.Add(Vector2.Lerp(Vector2.Lerp(start,p,t),Vector2.Lerp(p,end,t),t));
            }
        }
        edge.Add(knots[knots.Length-1]);
        return edge.ToArray();
    }
    public static readonly Vector2[] EntranceSoffit=RoundedEdge(new[] {
        new Vector2(5.99f,.64f),new Vector2(5.98f,.48f),new Vector2(6.12f,.23f),
        new Vector2(6.45f,.02f),new Vector2(7,-.18f),new Vector2(8,-.55f),new Vector2(10,-1.8f),
        new Vector2(12,-3.45f),new Vector2(14,-4.4f),new Vector2(16,-5.6f),new Vector2(18,-6.7f),
        new Vector2(20,-7.3f),new Vector2(24,-8.2f),new Vector2(29,-8.6f),new Vector2(34,-8.2f)},1.4f);
    public static readonly Vector2[] DaylightSoffit=RoundedEdge(new[] {
        new Vector2(183,-4.6f),new Vector2(187.8f,-4.71f),new Vector2(192,-4.88f),
        new Vector2(198,-4.54f),new Vector2(202.4f,-4.66f),new Vector2(204.35f,-4.43f),
        new Vector2(205.08f,-3.96f),new Vector2(205.28f,-3.22f),new Vector2(205.04f,-2.3f),
        new Vector2(205.58f,-.86f),new Vector2(205.72f,.45f),new Vector2(205.47f,1.95f),
        new Vector2(205.85f,3.15f),new Vector2(205.79f,4.155f)},.85f);
    public static readonly Vector2[] EntranceCrown=RoundedEdge(new[] {
        new Vector2(5.99f,.64f),new Vector2(6.15f,.735f),new Vector2(6.48f,.755f),
        new Vector2(9.65f,.755f),new Vector2(11.9f,.955f),new Vector2(17.79f,.955f)},.8f);
    public static readonly Vector2[] EntryRoof=BuildEntranceRoof();
    static Vector2[] BuildEntranceRoof()
    {
        var points=new List<Vector2>(EntranceCrown) {
            new Vector2(18.25f,.55f),new Vector2(18.7f,-.6f),new Vector2(19.2f,-1.7f),new Vector2(20.3f,-3.1f) };
        for(float x=22;x<=34;x+=.5f)points.Add(new Vector2(x,RiverbedSurface(x)));
        for (int i=EntranceSoffit.Length-1;i>0;i--) points.Add(EntranceSoffit[i]);
        return points.ToArray();
    }
    public static Vector2[] Box(float x,float y,float w,float h)=>new[]{new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h)};
    // Register the native Golden River shelf painting and the stone underside together.
    static Section RootShelf(Vector3 tread)
    {
        bool compact = tread.z <= 4.25f;
        var profile = TerrainArtProfile.Find(compact ? "shelf-root-compact-v3" : "shelf-root");
        float width = tread.z / profile.LandingFraction;
        float height = width / (compact ? 2f : 3f);
        var frame = new Rect(tread.x - (width-tread.z)/2, tread.y - profile.ContactUV*height, width, height);
        var edge = new List<Vector2> { new Vector2(tread.x,tread.y), new Vector2(tread.x+tread.z,tread.y) };
        foreach (var uv in profile.Underside) edge.Add(frame.min + Vector2.Scale(uv, frame.size));
        return new Section("Painted rooted shelf",edge.ToArray(),tread) { Shelf=profile, ArtFrame=frame };
    }
    // Heights along the entrance and exits are sampled from the same segments that paint them.
    public static float AlongFace(Vector2[] outline, float x)
    {
        for (int i=1; i<outline.Length; i++) {
            var a=outline[i-1]; var b=outline[i];
            if (a.x <= x && x <= b.x && a.x < b.x) return Mathf.Lerp(a.y,b.y,(x-a.x)/(b.x-a.x));
        }
        return outline[0].y;
    }
    // Broad rests divide the outer bank into a shaded hollow, the daylight sill and the last climb.
    public static readonly Vector2[] ExitBank=RoundedEdge(new[] {
        new Vector2(188,-8.14f),new Vector2(192.6f,-8.14f),new Vector2(195,-9.54f),
        new Vector2(198.4f,-9.54f),new Vector2(200.4f,-8.14f),new Vector2(202.7f,-8.14f),
        new Vector2(205.12f,-6.65f),new Vector2(206.2f,-6.65f),new Vector2(208,-6.05f),
        new Vector2(212.4f,-3.88f),new Vector2(214,-3.5f),new Vector2(217.1f,-3.5f),
        new Vector2(219.5f,-2.15f),new Vector2(222.1f,-2.15f),new Vector2(224.5f,-.85f),
        new Vector2(227.1f,-.85f),new Vector2(228.4f,-.35f),new Vector2(233.8f,4.46f),
        new Vector2(235.69f,5.205f),new Vector2(245.69f,5.205f)},.9f);

    // Adjacent banks share endpoints, so both the painted rim and the collider continue across each join.
    static Section ChannelBed(string name,params Vector2[] crest)
    {
        var edge=RoundedEdge(crest,1.3f);
        var outline=new List<Vector2>(edge) {
            new Vector2(edge[edge.Length-1].x,Bottom),new Vector2(edge[0].x,Bottom) };
        return new Section(name,outline.ToArray()) { ExposedBed=edge };
    }
    public static List<Section> Build()
    {
        var result=new List<Section>{ApproachShore(),new Section("Entrance shore",LeftShore),
            new Section("Massive entrance overhang",EntryRoof)};
        result.Add(ChannelBed("Fountain channel bed",new Vector2(20.34f,-12),
            new Vector2(24.8f,-12),new Vector2(26.7f,-12.78f),new Vector2(29.1f,-12.78f),
            new Vector2(30.85f,-12),new Vector2(35.5f,-12),new Vector2(36.65f,-12.3f),
            new Vector2(37.9f,-12.3f),new Vector2(39.5f,-12.67f),new Vector2(56.1f,-12.67f),
            new Vector2(58.1f,-13.6f),new Vector2(60.2f,-13.6f),new Vector2(64,-15),new Vector2(66,-17.4f),new Vector2(71,-16.4f),
            new Vector2(73,-18.05f),new Vector2(77.8f,-21.3f)));
        for(float x=34;x<183;x+=8){
            float right=Mathf.Min(183,x+8);var outline=new List<Vector2>();
            for(float sample=x;sample<right;sample+=.5f)outline.Add(new Vector2(sample,RiverbedSurface(sample)));
            outline.Add(new Vector2(right,RiverbedSurface(right)));outline.Add(new Vector2(right,CaveRoute.Ceiling(right)));
            for(float sample=right-.5f;sample>x;sample-=.5f)outline.Add(new Vector2(sample,CaveRoute.Ceiling(sample)));
            outline.Add(new Vector2(x,CaveRoute.Ceiling(x)));
            result.Add(new Section("Cave roof "+x,outline.ToArray()));
        }
        var exitCliff=new List<Vector2> {new Vector2(183,-3.45f),new Vector2(184.5f,-3.1f),new Vector2(185,-1.7f),
            new Vector2(186.2f,.655f),new Vector2(186.99f,4.755f),new Vector2(191.99f,4.755f),new Vector2(191.99f,4.155f)};
        for(int i=DaylightSoffit.Length-1;i>=0;i--)exitCliff.Add(DaylightSoffit[i]);
        result.Add(new Section("Main exit cliff",exitCliff.ToArray(),new Vector3(186.99f,4.755f,5),
            new Vector3(191.99f,4.155f,13.8f)));
        var catchEdge=new List<Vector2>();
        for (int i=Recovery.Length-1;i>=0;i--){
            var ledge=Recovery[i];
            catchEdge.Add(new Vector2(ledge.x,ledge.y));
            catchEdge.Add(new Vector2(ledge.x+ledge.z,ledge.y));
        }
        result.Add(ChannelBed("Lower catch footing",catchEdge.ToArray()));
        result.Add(ChannelBed("Spillway bank",
            new Vector2(104,CatchHeight),new Vector2(107,-25.6f),new Vector2(111,-22.8f),new Vector2(115.75f,-18.7f),
            new Vector2(117.6f,-18.7f),new Vector2(120,-17.35f),new Vector2(125.3f,-17.35f),
            new Vector2(127.7f,-15.95f),new Vector2(130.6f,-15.95f),new Vector2(133.2f,-16.12f),
            new Vector2(138.7f,-16.05f),new Vector2(141.8f,-16.22f),new Vector2(145.4f,-16.1f)));
        result.Add(ChannelBed("Upper seep bank",
            new Vector2(145.4f,-16.1f),new Vector2(149,-18.55f),new Vector2(153,-18.25f),new Vector2(158,-16.1f),
            new Vector2(161.4f,-16.85f),new Vector2(164,-16.75f),new Vector2(168.8f,-14.8f),
            new Vector2(175,-13.55f),new Vector2(181,-12.35f),new Vector2(184.5f,-11.4f),ExitBank[0]));
        foreach (var tread in Floors)
            if (tread.x<113 || tread.x>=147 && tread.x<187) result.Add(RootShelf(tread));
        var exitOutline=new List<Vector2>(ExitBank);
        var end=ExitBank[ExitBank.Length-1];
        exitOutline.AddRange(RoundedEdge(new[]{end+new Vector2(.03f,-.22f),end+new Vector2(-.18f,-1.15f),
            end+new Vector2(.30f,-2.55f),end+new Vector2(4,-10.25f)},.5f));
        exitOutline.Add(new Vector2(end.x+4,Bottom));
        exitOutline.Add(new Vector2(ExitBank[0].x,Bottom));
        result.Add(new Section("Daylight shore",exitOutline.ToArray()));
        foreach(var side in new[]{new Vector2(-44,-5.8f),new Vector2(247,290)}){
            float left=side.x,right=side.y;var edge=new List<Vector2>();
            for(float x=left;x<right;x+=.4f)edge.Add(new Vector2(x,-4.85f+.1f*Mathf.Sin(x*2.7f)+.06f*Mathf.Sin(x*5.9f)));
            edge.Add(new Vector2(right,-4.85f+.1f*Mathf.Sin(right*2.7f)+.06f*Mathf.Sin(right*5.9f)));
            edge.Add(new Vector2(right,Bottom));edge.Add(new Vector2(left,Bottom));result.Add(new Section("Outer river foundation",edge.ToArray()));}
        return result;
    }
    public static List<Vector3> Path()
    {
        var result=new List<Vector3>{new Vector3(2.6f,-.045f,0),new Vector3(21.9f,-12,0),new Vector3(28.4f,-12.78f,0),new Vector3(34,-12,1),new Vector3(37.5f,-12.3f,0),
            new Vector3(44,-12.67f,0),new Vector3(55.8f,-12.67f,0)};
        float previous=-12.67f,end=57.106f;
        for(int i=0;i<Floors.Length;i++){var p=Floors[i];bool right=i+1==Floors.Length||Floors[i+1].x>=p.x;
            float target=right?p.x+p.z-.7f:p.x+.7f;
            bool jump=p.y>previous+.15f||Mathf.Abs(p.y-previous)<.15f&&p.x>end+.1f;
            if (p.x>=113 && p.x<147) jump=false;
            if (p.x>=187) {
                target=p.x+p.z*.5f;
                result.Add(new Vector3(target,AlongFace(ExitBank,target),0));
            } else result.Add(new Vector3(target,p.y,jump?1:0));
            previous=p.y;end=p.x+p.z;}
        var exit=CaveRoute.ToLocal(CaveRoute.Rejoin);result.Add(new Vector3(exit.x,exit.y-.52f,0));return result;
    }
}
