using System.Collections.Generic;
using UnityEngine;

// Shared permanent contours for the level connection, water and checkpoint recovery.
public static class CaveRoute
{
    public const int EntranceIndex=10,RejoinIndex=39;
    public const float VerticalOffset=.045f,DeathFloor=-26;
    public static float Origin=>MovementCourse.AscentPlatforms[EntranceIndex].x+MovementCourse.AscentPlatforms[EntranceIndex].z/2-3.79f;
    public static Vector2 Entrance=>ToCourse(new Vector2(2.6f,.52f));
    public static Vector2 Approach {get{var p=MovementCourse.AscentPlatforms[9];return new Vector2(p.x,p.y+1.27f);}}
    public static Vector2 Rejoin {get{var p=MovementCourse.AscentPlatforms[RejoinIndex];return new Vector2(p.x,p.y+1.27f);}}
    public static Vector2 ToLocal(Vector2 p)=>p-new Vector2(Origin,VerticalOffset);
    public static Vector2 ToCourse(Vector2 p)=>p+new Vector2(Origin,VerticalOffset);
    public static float RejoinLocal=>Rejoin.x-Origin;
    public const float ExitStart=166.7f;
    public static Rect MaskBounds=>new Rect(Origin-44,-40,334,56);
    public static bool ReplacesAscent(int index)=>index==10||index==11||index==33||index==34||index==39;
    static Vector2[] Box(float x,float y,float width,float height)=>new[]{new Vector2(x,y),new Vector2(x+width,y),new Vector2(x+width,y+height),new Vector2(x,y+height)};
    // These are shore envelopes, not the shape of the stairs through the air.
    public static readonly Vector2[][] ShoreOpenings={Box(-5.3f,-40,17.3f,42),Box(185,-40,61.8f,48)};
    // Only recovery solids intersecting the actual entrance/exit air are carved.
    public static readonly Vector2[][] Openings={Box(3.8f,-12,9,12),Box(185,-14,50.6f,17.5f)};
    public static bool InOpening(Vector2 local){foreach(var p in ShoreOpenings)if(Contains(p,local))return true;return false;}
    public static bool InAir(Vector2 local){foreach(var p in Openings)if(Contains(p,local))return true;return false;}
    public static bool IsDry(Vector2 course)
    {
        var p=ToLocal(course);
        if(InAir(p))return true;
        return InOpening(p)||p.x>=10&&p.x<=205.8f&&p.y>DeathFloor-1&&p.y<Ceiling(p.x);
    }
    public static float Ceiling(float x)
    {
        if(x<14)return -8;
        if(x<60)return -5;
        if(x<82)return -5.2f;
        if(x<90)return Mathf.Lerp(-5.2f,-6.2f,Mathf.InverseLerp(82,90,x));
        if(x<ExitStart)return -6.2f;
        if(x<183)return Mathf.Lerp(-6.2f,-5.6f,Mathf.InverseLerp(ExitStart,183,x));
        if(x<186)return -5.6f;
        if(x<187)return Mathf.Lerp(-5.6f,-4.6f,x-186);
        return -4.6f;
    }
    public static bool ShouldRecover(Vector2 previous,Vector2 current)
    {
        const float immersedCenter=-2.65f;
        if(previous.y<immersedCenter&&previous.y> -4.4f&&!InOpening(ToLocal(previous)))return true;
        if(previous.y>=immersedCenter&&current.y<immersedCenter) {
            var crossing=Vector2.Lerp(previous,current,(immersedCenter-previous.y)/(current.y-previous.y));
            if(!InOpening(ToLocal(crossing)))return true;
        }
        if(current.y<immersedCenter&&current.y> -4.4f&&!InOpening(ToLocal(current)))return true;
        return current.y<(IsDry(current)?DeathFloor+VerticalOffset:-7);
    }
    public static bool WaterRemoved(Vector2 local)
    {
        if(InOpening(local))return true;
        // The lower render boundary lies inside the permanent solid riverbed, never in open air.
        return local.x>=-40&&local.x<=286&&local.y<RiverbedTop(local.x)-.15f;
    }
    public static float RiverbedTop(float x)=>x< -24?Mathf.Lerp(-8,-4.45f,Mathf.InverseLerp(-40,-24,x)):
        x>270?Mathf.Lerp(-4.45f,-8,Mathf.InverseLerp(270,286,x)):-4.45f;
    public static bool Contains(Vector2[] polygon,Vector2 p)
    {
        bool inside=false;
        for(int i=0,j=polygon.Length-1;i<polygon.Length;j=i++) {
            var a=polygon[i];var b=polygon[j];
            if((a.y>p.y)!=(b.y>p.y)&&p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x)inside=!inside;
        }
        return inside;
    }
    public static Texture2D CreateMask(bool readable=false)
    {
        var result=new Texture2D(4096,1024,TextureFormat.RGBA32,false,true){name="Permanent level cave contours",wrapMode=TextureWrapMode.Clamp};
        var pixels=new Color32[result.width*result.height];var b=MaskBounds;
        for(int y=0;y<result.height;y++)for(int x=0;x<result.width;x++) {
            var p=new Vector2(b.x+(x+.5f)/result.width*b.width,b.y+(y+.5f)/result.height*b.height);var local=ToLocal(p);
            pixels[y*result.width+x]=new Color32(WaterRemoved(local)?(byte)255:(byte)0,IsDry(p)?(byte)255:(byte)0,InAir(local)?(byte)255:(byte)0,255);
        }
        result.SetPixels32(pixels);result.Apply(false,!readable);return result;
    }
    public static readonly Vector3[] ReturnFloors={
        new Vector3(72f,-8.36f,4f),
        new Vector3(79f,-9.1f,3f),
        new Vector3(84f,-11.16f,16f),
        new Vector3(103f,-11.16f,3f),
        new Vector3(106f,-9.76f,15f),
        new Vector3(121f,-11.16f,40f),
        new Vector3(164f,-11.16f,2.7f),
        new Vector3(166.7f,-12.45f,12f),
        new Vector3(180.4f,-11.05f,3f),
        new Vector3(184.4f,-9.45f,3f),
        new Vector3(188.4f,-8.14045f,15.78f),
        new Vector3(204.18f,-6.64955f,4.18f),
        new Vector3(208.36f,-5.195f,3.96f),
        new Vector3(212.32f,-3.70409f,3.89f),
        new Vector3(216.21f,-2.395f,4.18f),
        new Vector3(220.39f,-.90409f,3.89f),
        new Vector3(224.28f,.58682f,3.96f),
        new Vector3(228.24f,2.15045f,3.89f),
        new Vector3(232.13f,3.64136f,3.56f),
        new Vector3(235.69f,5.205f,10f)
    };
    public static readonly Vector3[] CatchFloors={new Vector3(96,-17.5f,8),new Vector3(90,-17.5f,7),
        new Vector3(85,-16.1f,3),new Vector3(81,-14.7f,3),new Vector3(77,-13.3f,3),
        new Vector3(73,-11.9f,3),new Vector3(76.5f,-10.3f,3)};
    public static readonly Vector2[][] CatchAir={CaveGeometry.Bevel(new Rect(72,-19,33,6),.3f),
        CaveGeometry.Bevel(new Rect(72,-16,7,7.1f),.15f),CaveGeometry.Bevel(new Rect(79,-16,5,6.4f),.15f)};
}
