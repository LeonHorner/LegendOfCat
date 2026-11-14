using System.Collections.Generic;
using UnityEngine;

// Shared permanent contours for the level connection, water and checkpoint recovery.
public static class CaveRoute
{
    public const int EntranceIndex=10,RejoinIndex=39;
    public const float VerticalOffset=.045f,DeathFloor=-34;
    public static float Origin=>MovementCourse.AscentPlatforms[EntranceIndex].x+MovementCourse.AscentPlatforms[EntranceIndex].z/2-3.79f;
    public static Vector2 Entrance=>ToCourse(new Vector2(2.6f,.52f));
    public static Vector2 Approach {get{var p=MovementCourse.AscentPlatforms[9];return new Vector2(p.x,p.y+1.27f);}}
    public static Vector2 Rejoin {get{var p=MovementCourse.AscentPlatforms[RejoinIndex];return new Vector2(p.x,p.y+1.27f);}}
    public static Vector2 ToLocal(Vector2 p)=>p-new Vector2(Origin,VerticalOffset);
    public static Vector2 ToCourse(Vector2 p)=>p+new Vector2(Origin,VerticalOffset);
    public static float RejoinLocal=>Rejoin.x-Origin;
    public const float ExitStart=166.7f;
    public static Rect MaskBounds=>new Rect(Origin-44,-44,334,60);
    public static bool ReplacesAscent(int index)=>index==9||index==10||index==11||index==33||index==34||index==39;
    static Vector2[] Box(float x,float y,float width,float height)=>new[]{new Vector2(x,y),new Vector2(x+width,y),new Vector2(x+width,y+height),new Vector2(x,y+height)};
    // These are shore envelopes, not the shape of the stairs through the air.
    public static readonly Vector2[][] ShoreOpenings={
        new[]{new Vector2(-5.6f,-2.8f),new Vector2(12,-2.8f),new Vector2(13.4f,-4.8f),new Vector2(22,-8),
            new Vector2(22,-44),new Vector2(-18,-44),new Vector2(-18,-8),new Vector2(-7.7f,-5.1f)},
        new[]{new Vector2(185,-2.8f),new Vector2(246.5f,-2.8f),new Vector2(249.8f,-6),new Vector2(251,-44),new Vector2(183,-44),new Vector2(183,-6)}};
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
        if(x<18)return -8.6f;
        if(x<34)return Mathf.Lerp(-8.6f,-8.2f,Mathf.InverseLerp(29,34,x));
        if(x<44)return Mathf.Lerp(-8.2f,-5.7f,Mathf.InverseLerp(34,44,x));
        if(x<58)return -5.7f;
        if(x<72)return Mathf.Lerp(-5.7f,-7.5f,Mathf.InverseLerp(58,72,x));
        if(x<104)return -7.5f;
        if(x<179)return -6;
        return Mathf.Lerp(-6,-4.6f,Mathf.InverseLerp(179,183,x));
    }
    public static bool ShouldRecover(Vector2 previous,Vector2 current)
    {
        const float immersedCenter=-2.65f;
        if(previous.y<immersedCenter&&previous.y> -4.4f&&!InOpening(ToLocal(previous-Vector2.up*.45f)))return true;
        if(previous.y>=immersedCenter&&current.y<immersedCenter) {
            var crossing=Vector2.Lerp(previous,current,(immersedCenter-previous.y)/(current.y-previous.y));
            if(!InOpening(ToLocal(crossing-Vector2.up*.45f)))return true;
        }
        if(current.y<immersedCenter&&current.y> -4.4f&&!InOpening(ToLocal(current-Vector2.up*.45f)))return true;
        return current.y<(IsDry(current)?DeathFloor+VerticalOffset:-7);
    }
    public static bool WaterRemoved(Vector2 local)
    {
        if(InOpening(local))return true;
        // The lower render boundary lies inside the permanent solid riverbed, never in open air.
        return local.y< -5.2f;
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
        var result=new Texture2D(4096,256,TextureFormat.RGBA32,false,true){name="Permanent level cave contours",wrapMode=TextureWrapMode.Clamp};
        var pixels=new Color32[result.width*result.height];var b=MaskBounds;
        for(int y=0;y<result.height;y++)for(int x=0;x<result.width;x++) {
            var p=new Vector2(b.x+(x+.5f)/result.width*b.width,b.y+(y+.5f)/result.height*b.height);var local=ToLocal(p);
            pixels[y*result.width+x]=new Color32(WaterRemoved(local)?(byte)255:(byte)0,IsDry(p)?(byte)255:(byte)0,InAir(local)?(byte)255:(byte)0,255);
        }
        result.SetPixels32(pixels);result.Apply(false,!readable);return result;
    }
    public static Vector3[] ReturnFloors=>CaveTerrainLayout.Floors;
    public static Vector3[] CatchFloors=>CaveTerrainLayout.Recovery;
    public static Vector2[][] CatchAir=>CaveTerrainLayout.RecoveryAir;
}
