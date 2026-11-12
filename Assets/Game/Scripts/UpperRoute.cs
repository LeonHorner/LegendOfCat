using UnityEngine;

// One forward cave route. All coordinates below are relative to the original entry bank.
public static class UpperRoute
{
    public const int EntranceIndex = 10, RejoinIndex = 49, LookoutIndex = 19, RiskIndex = 13, DescentTurnIndex = 6;
    public const float DeathFloor = -35, FoundationBottom = -48;
    public static float Origin => MovementCourse.AscentPlatforms[EntranceIndex].x;
    public static Vector2 Entrance => new Vector2(Origin + 2, .52f);
    public static Vector2 Rejoin { get { var p = MovementCourse.AscentPlatforms[RejoinIndex]; return new Vector2(p.x, p.y + 1.27f); } }
    public static readonly Vector3[] Platforms = Build(new[] {
        new Vector3(8.2f,-1.5f,10), new Vector3(15.7f,-3.1f,3), new Vector3(18.7f,-4.7f,3),
        new Vector3(15,-6.3f,3), new Vector3(11,-7.9f,3), new Vector3(15,-9.5f,3), new Vector3(19,-11.1f,5),
        new Vector3(26,-11,5), new Vector3(34,-11.4f,6), new Vector3(43,-10.9f,6), new Vector3(52,-11.5f,6),
        new Vector3(61,-11,6), new Vector3(70,-11.3f,4), new Vector3(76.5f,-11,2.5f), new Vector3(83,-11.8f,5),
        new Vector3(91,-13.3f,4), new Vector3(95,-14.8f,4), new Vector3(99,-16.3f,4), new Vector3(103,-17.8f,4),
        new Vector3(111,-18.5f,12), new Vector3(120,-17,3), new Vector3(124,-15.5f,3), new Vector3(128,-14,3),
        new Vector3(136f,-13.5f,6), new Vector3(145f,-13.05f,6), new Vector3(154f,-12.6f,6), new Vector3(163f,-12.15f,6), new Vector3(172f,-11.7f,6), new Vector3(181f,-11.25f,6), new Vector3(190f,-10.8f,6), new Vector3(199f,-10.35f,6), new Vector3(208f,-10.35f,6), new Vector3(217f,-10.35f,6), new Vector3(226f,-10.35f,6), new Vector3(235f,-10.35f,6), new Vector3(244f,-10.35f,6),
        new Vector3(251f,-10.35f,6f), new Vector3(258.2f,-10.35f,6f), new Vector3(263.2f,-8.95f,3f), new Vector3(267.2f,-7.35f,3f),
        new Vector3(271.2f,-5.75f,3f), new Vector3(275.2f,-4.15f,3f), new Vector3(279.2f,-2.55f,3f), new Vector3(283.2f,-0.95f,3f),
        new Vector3(287.2f,0.65f,3f), new Vector3(291.2f,2.25f,3f), new Vector3(297.2f,2f,5f), new Vector3(303.2f,2f,8f),
        new Vector3(307f,3.2f,3f), new Vector3(311f,4.7f,3f), new Vector3(315.3f,5.45f,3f)
    });
    public static readonly Vector3[] Recoveries = Build(new[] {
        new Vector3(74,-18,7), new Vector3(64,-18,8), new Vector3(60,-16.4f,3),
        new Vector3(56,-14.8f,3), new Vector3(61.5f,-14.8f,3), new Vector3(66,-13.2f,3)
    });
    public static readonly Vector2[] Roof = {
        new Vector2(-24,-6.8f), new Vector2(30,-6.8f), new Vector2(62,-6.8f), new Vector2(84,-7),
        new Vector2(91,-8.8f), new Vector2(99,-10.8f), new Vector2(104,-11), new Vector2(119,-11),
        new Vector2(128,-9.3f), new Vector2(191,-6.1f), new Vector2(250.2f,-6.15f), new Vector2(258.2f,-5.95f), new Vector2(263.2f,-4.65f), new Vector2(267.2f,-3.05f),
        new Vector2(271.2f,-1.45f), new Vector2(275.2f,0.15f), new Vector2(279.2f,1.75f), new Vector2(283.2f,3.35f),
        new Vector2(287.2f,4.95f), new Vector2(291.2f,6.55f), new Vector2(297.2f,8.15f), new Vector2(303.2f,9.75f), new Vector2(368.2f,9.75f)
    };
    // Overlapping shaft volumes define physical shore connections, not a horizontal water exemption.
    public static readonly Rect[] Portals = {
        new Rect(12,-4.5f,8,4), new Rect(10,-8,12,4), new Rect(7,-12,15,4.5f),
        new Rect(261.2f,-9.85f,26,9), new Rect(277.2f,-1.85f,12,4), new Rect(282.2f,1.15f,20,8)
    };
    public static readonly Rect[] RescueAir = {
        new Rect(58,-18,20,3.1f), new Rect(54,-16.4f,9,3.7f),
        new Rect(54,-14.8f,10,4), new Rect(58,-13.2f,11,4.1f)
    };
    public static readonly Vector2[][] PortalContours = BuildPortalContours();
    static Vector2[][] BuildPortalContours() {
        var shapes=new Vector2[Portals.Length][];
        for(int i=0;i<shapes.Length;i++) shapes[i]=CaveGeometry.Bevel(Portals[i]);
        return shapes;
    }
    public static readonly Rect MaskBounds = new Rect(-24,-48,400,62);
    static Vector3[] Build(Vector3[] points) {
        for (int i=0;i<points.Length;i++) points[i]=new Vector3(Origin+points[i].x,points[i].y-.75f,points[i].z);
        return points;
    }
    public static float CeilingAt(float relativeX) {
        float value=Roof[Roof.Length-1].y;
        for (int i=1;i<Roof.Length;i++) if(relativeX<=Roof[i].x)
            {value=Mathf.Lerp(Roof[i-1].y,Roof[i].y,Mathf.InverseLerp(Roof[i-1].x,Roof[i].x,relativeX));break;}
        return value+(relativeX>24 && relativeX<136 ? .18f*Mathf.Sin(relativeX*.61f)+.06f*Mathf.Sin(relativeX*1.83f) : 0);
    }
    public static bool IsPortal(Vector2 local) {
        var p=new Vector2(local.x-Origin,local.y);
        foreach(var portal in PortalContours) if(CaveGeometry.Contains(portal,p)) return true;
        return false;
    }
    public static bool IsDry(Vector2 local) {
        float x=local.x-Origin;
        return IsPortal(local) || x>=MaskBounds.xMin && x<=MaskBounds.xMax && local.y>DeathFloor-1 && local.y<CeilingAt(x);
    }
    public static bool IsWaterCovered(Vector2 local) => IsPortal(local) || local.y < RiverBedTop(local.x-Origin);
    public static float RiverBedTop(float x) => -4.76f + Mathf.Sin(x*.8f)*.06f + Mathf.Sin(x*2.3f)*.035f;
    public static float RearBankTop(float x) {
        float entry=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(8,15,Mathf.Abs(x-15)));
        float exit=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(11,18,Mathf.Abs(x-279)));
        return Mathf.Lerp(RiverBedTop(x),-1.45f+.16f*Mathf.Sin(x*1.4f),Mathf.Max(entry,exit));
    }
    public static float CameraHeight(Vector2 local) {
        var room=Platforms[LookoutIndex];
        float roomBlend=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(5,15,Mathf.Abs(local.x-room.x)));
        return Mathf.Max(DeathFloor+1,local.y+Mathf.Lerp(-.2f,1.8f,roomBlend));
    }
    public static bool ShouldRecover(Vector2 previous,Vector2 current) {
        const float surface=-3.1f;
        if(previous.y>=surface && current.y<surface) {
            float t=(surface-previous.y)/(current.y-previous.y);
            var crossing=Vector2.Lerp(previous,current,t);
            float x=crossing.x-Origin;
            if(x>=MaskBounds.xMin && x<=MaskBounds.xMax && !IsPortal(crossing)) return true;
        }
        return current.y < (IsDry(current)?DeathFloor:-7);
    }
    public static Texture2D CreateMask(bool keepReadable=false) {
        var result=new Texture2D(2048,512,TextureFormat.RGBA32,false,true) {
            name="Authored cave coverage",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear
        };
        var colors=new Color32[result.width*result.height];
        for(int y=0;y<result.height;y++) for(int x=0;x<result.width;x++) {
            var p=new Vector2(Origin+MaskBounds.xMin+(x+.5f)/result.width*MaskBounds.width,
                MaskBounds.yMin+(y+.5f)/result.height*MaskBounds.height);
            colors[y*result.width+x]=new Color32(IsWaterCovered(p)?(byte)255:(byte)0,
                IsDry(p)?(byte)255:(byte)0,IsPortal(p)?(byte)255:(byte)0,255);
        }
        result.SetPixels32(colors);result.Apply(false,!keepReadable);return result;
    }
}
