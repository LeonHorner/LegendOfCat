using System.Collections.Generic;
using UnityEngine;

// Convex pieces use one authored outline for paint and collision, including carved openings.
public static class CaveGeometry
{
    public static int[] Triangulate(Vector2[] points)
    {
        var remaining=new List<int>();float signed=0;
        for(int i=0;i<points.Length;i++){remaining.Add(i);var a=points[i];var b=points[(i+1)%points.Length];signed+=a.x*b.y-b.x*a.y;}
        float winding=Mathf.Sign(signed);var triangles=new List<int>();
        while(remaining.Count>3) {
            bool found=false;
            for(int i=0;i<remaining.Count;i++) {
                int a=remaining[(i+remaining.Count-1)%remaining.Count],b=remaining[i],c=remaining[(i+1)%remaining.Count];
                if(Side(points[c],points[a],points[b])*winding<=.000001f)continue;
                bool occupied=false;
                foreach(int index in remaining) {
                    if(index==a||index==b||index==c)continue;
                    var p=points[index];
                    if(Side(p,points[a],points[b])*winding>=0&&Side(p,points[b],points[c])*winding>=0&&Side(p,points[c],points[a])*winding>=0){occupied=true;break;}
                }
                if(occupied)continue;
                triangles.Add(a);triangles.Add(b);triangles.Add(c);remaining.RemoveAt(i);found=true;break;
            }
            if(!found)throw new System.InvalidOperationException("Terrain contour must be a simple polygon without duplicate edges.");
        }
        triangles.AddRange(remaining);return triangles.ToArray();
    }
    public static Vector2[] Bevel(Rect rect, float amount=.22f)
    {
        float b=Mathf.Min(amount,Mathf.Min(rect.width,rect.height)*.2f);
        return new[] {new Vector2(rect.xMin+b,rect.yMin),new Vector2(rect.xMax-b,rect.yMin),
            new Vector2(rect.xMax,rect.yMin+b),new Vector2(rect.xMax,rect.yMax-b),
            new Vector2(rect.xMax-b,rect.yMax),new Vector2(rect.xMin+b,rect.yMax),
            new Vector2(rect.xMin,rect.yMax-b),new Vector2(rect.xMin,rect.yMin+b)};
    }
    public static List<Vector2[]> Carve(Vector2[] source, bool rescue=true)
    {
        var pieces=new List<Vector2[]> {source};
        foreach(var hole in UpperRoute.PortalContours) pieces=Subtract(pieces,hole);
        if(rescue) foreach(var hole in UpperRoute.RescueAir) pieces=Subtract(pieces,Bevel(hole,.8f));
        return pieces;
    }
    public static List<Vector2[]> Subtract(List<Vector2[]> pieces, Vector2[] hole)
    {
        var result=new List<Vector2[]>();
        foreach(var piece in pieces) {
            var remaining=piece;
            for(int side=0;side<hole.Length && remaining.Length>=3;side++) {
                var a=hole[side];var b=hole[(side+1)%hole.Length];
                var outside=Clip(remaining,a,b,false);
                if(Area(outside)>.0001f) result.Add(outside);
                remaining=Clip(remaining,a,b,true);
            }
        }
        return result;
    }
    static float Side(Vector2 point,Vector2 a,Vector2 b) => (b.x-a.x)*(point.y-a.y)-(b.y-a.y)*(point.x-a.x);
    static Vector2[] Clip(Vector2[] polygon,Vector2 a,Vector2 b,bool inside)
    {
        var result=new List<Vector2>();
        if(polygon.Length<3) return result.ToArray();
        var previous=polygon[polygon.Length-1];
        float before=Side(previous,a,b)*(inside?1:-1);
        foreach(var current in polygon) {
            float after=Side(current,a,b)*(inside?1:-1);
            if((before>=0)!=(after>=0)) result.Add(Vector2.Lerp(previous,current,before/(before-after)));
            if(after>=0) result.Add(current);
            previous=current;before=after;
        }
        return result.ToArray();
    }
    public static bool Contains(Vector2[] polygon,Vector2 point)
    {
        for(int i=0;i<polygon.Length;i++) if(Side(point,polygon[i],polygon[(i+1)%polygon.Length])<-.00001f) return false;
        return true;
    }
    public static float Area(Vector2[] points)
    {
        float area=0;
        for(int i=0;i<points.Length;i++) {var a=points[i];var b=points[(i+1)%points.Length];area+=a.x*b.y-b.x*a.y;}
        return Mathf.Abs(area)*.5f;
    }
}
