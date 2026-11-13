using System.Collections.Generic;
using UnityEngine;

// Shared permanent contours for the level connection, water and checkpoint recovery.
public static class CaveRoute
{
    public const int EntranceIndex=10,RejoinIndex=39;
    public const float VerticalOffset=.045f,DeathFloor=-26;
    public static float Origin=>MovementCourse.AscentPlatforms[EntranceIndex].x+MovementCourse.AscentPlatforms[EntranceIndex].z/2-3.79f;
    public static Vector2 Entrance=>ToCourse(new Vector2(2.6f,.52f));
    public static Vector2 Rejoin {get{var p=MovementCourse.AscentPlatforms[RejoinIndex];return new Vector2(p.x,p.y+1.27f);}}
    public static Vector2 ToLocal(Vector2 p)=>p-new Vector2(Origin,VerticalOffset);
    public static Vector2 ToCourse(Vector2 p)=>p+new Vector2(Origin,VerticalOffset);
    public static float RejoinLocal=>Rejoin.x-Origin;
    public const float ExitStart=166.7f;
    public static Rect MaskBounds=>new Rect(Origin-16,-34,RejoinLocal+48,50);
    static Vector2[] Air(float top,float bottom,float leftTop,float rightTop,float leftBottom,float rightBottom)
        =>new[]{new Vector2(leftBottom,bottom),new Vector2(rightBottom,bottom),new Vector2(rightTop,top),new Vector2(leftTop,top)};
    public static readonly Vector2[][] Openings={
        Air(.2f,-1.65f,3.75f,6,5.65f,8.2f),Air(-1.65f,-3.25f,5.65f,8.2f,7.15f,9.4f),
        Air(-3.25f,-4.85f,7.15f,9.4f,8.55f,10.2f),Air(-4.85f,-6.5f,8.55f,10.2f,9.9f,12.1f),
        Air(-6.5f,-10,9.9f,12.1f,10.9f,15),
        Air(-8.1f,-11.9f,186.7f,195.7f,178.7f,187.7f),Air(-4.1f,-8.1f,196.7f,204.7f,186.7f,195.7f),Air(.9f,-4.1f,206.7f,216.7f,196.7f,204.7f),
        Air(6.9f,.9f,224.7f,242.7f,206.7f,216.7f),
        new[]{new Vector2(202.7f,-1.5f),new Vector2(244.7f,-1.5f),new Vector2(244.7f,7.9f),new Vector2(202.7f,7.9f)}
    };
    public static bool InOpening(Vector2 local)
    {
        foreach(var opening in Openings)if(CaveGeometry.Contains(opening,local))return true;
        return false;
    }
    public static bool IsDry(Vector2 course)
    {
        var p=ToLocal(course);
        return InOpening(p)||p.x>=-4.3f&&p.x<=325&&p.y>DeathFloor-1&&p.y<Ceiling(p.x)+.25f;
    }
    public static float Ceiling(float x)
    {
        if(x<14)return -8;
        if(x<65)return -4.2f;
        if(x<ExitStart)return -6.2f;
        return Mathf.Lerp(-6.2f,7.9f,Mathf.InverseLerp(ExitStart,RejoinLocal,x));
    }
    public static bool ShouldRecover(Vector2 previous,Vector2 current)
    {
        if(previous.y>=-3.1f&&current.y< -3.1f) {
            var crossing=Vector2.Lerp(previous,current,(-3.1f-previous.y)/(current.y-previous.y));
            if(!InOpening(ToLocal(crossing)))return true;
        }
        return current.y<(IsDry(current)?DeathFloor+VerticalOffset:-7);
    }
    public static bool WaterRemoved(Vector2 local)
    {
        if(InOpening(local))return true;
        // Actual solid shore rock borders the entrance; the river remains everywhere else.
        return local.y< -2&&local.y> -5.3f&&local.x>5.45f&&local.x<11&&
            (Contains(CaveEntryStudy.Left,local)||Contains(CaveEntryStudy.Right,local));
    }
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
            pixels[y*result.width+x]=new Color32(WaterRemoved(local)?(byte)255:(byte)0,IsDry(p)?(byte)255:(byte)0,InOpening(local)?(byte)255:(byte)0,255);
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
        new Vector3(188.4f,-7.85f,3f),
        new Vector3(192.4f,-6.25f,3f),
        new Vector3(196.4f,-4.65f,3f),
        new Vector3(200.4f,-3.05f,3f),
        new Vector3(204.4f,-1.45f,3f),
        new Vector3(208.4f,0.15f,4f),
        new Vector3(213.4f,-0.1f,6f),
        new Vector3(219.4f,-0.1f,6f),
        new Vector3(224.2f,1.1f,3f),
        new Vector3(228.2f,2.6f,3f),
        new Vector3(232.5f,3.35f,3f)
    
    };
    public static readonly Vector3[] CatchFloors={new Vector3(96,-17.5f,8),new Vector3(90,-17.5f,7),
        new Vector3(85,-16.1f,3),new Vector3(81,-14.7f,3),new Vector3(77,-13.3f,3),
        new Vector3(73,-11.9f,3),new Vector3(76.5f,-10.3f,3)};
    public static readonly Vector2[][] CatchAir={CaveGeometry.Bevel(new Rect(72,-19,33,6),.3f),
        CaveGeometry.Bevel(new Rect(72,-16,7,7.1f),.15f),CaveGeometry.Bevel(new Rect(79,-16,5,6.4f),.15f)};
}