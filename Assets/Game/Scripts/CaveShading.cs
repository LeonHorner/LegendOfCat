using System.Collections.Generic;
using UnityEngine;

// Permanent world-space bevel and contact-shadow distances, derived from the solid rock outlines.
public static class CaveShading
{
    public static Texture2D Create(IEnumerable<Vector2[]> polygons,Rect bounds,int width=2048,int height=512,bool readable=false)
    {
        var solid=new bool[width*height];
        float sx=bounds.width/width,sy=bounds.height/height;
        foreach(var polygon in polygons) {
            float left=float.PositiveInfinity,right=float.NegativeInfinity,bottom=left,top=right,area=0;
            for(int i=0;i<polygon.Length;i++) {
                var a=polygon[i];var b=polygon[(i+1)%polygon.Length];area+=a.x*b.y-b.x*a.y;
                left=Mathf.Min(left,a.x);right=Mathf.Max(right,a.x);bottom=Mathf.Min(bottom,a.y);top=Mathf.Max(top,a.y);
            }
            int x0=Mathf.Clamp(Mathf.FloorToInt((left-bounds.xMin)/sx),0,width-1),x1=Mathf.Clamp(Mathf.CeilToInt((right-bounds.xMin)/sx),0,width-1);
            int y0=Mathf.Clamp(Mathf.FloorToInt((bottom-bounds.yMin)/sy),0,height-1),y1=Mathf.Clamp(Mathf.CeilToInt((top-bounds.yMin)/sy),0,height-1);
            float orientation=area>=0?1:-1;
            for(int y=y0;y<=y1;y++) for(int x=x0;x<=x1;x++) {
                int index=y*width+x;if(solid[index]) continue;
                var p=new Vector2(bounds.xMin+(x+.5f)*sx,bounds.yMin+(y+.5f)*sy);
                bool inside=true;
                for(int i=0;i<polygon.Length;i++) {
                    var a=polygon[i];var b=polygon[(i+1)%polygon.Length];
                    if(((b.x-a.x)*(p.y-a.y)-(b.y-a.y)*(p.x-a.x))*orientation<0) {inside=false;break;}
                }
                solid[index]=inside;
            }
        }
        var inner=new float[solid.Length];var outer=new float[solid.Length];
        for(int i=0;i<solid.Length;i++) {inner[i]=solid[i]?1000:0;outer[i]=solid[i]?0:1000;}
        Distance(inner,width,height,sx,sy);Distance(outer,width,height,sx,sy);
        var pixels=new Color32[solid.Length];
        for(int i=0;i<pixels.Length;i++) pixels[i]=new Color32(solid[i]?(byte)255:(byte)0,
            (byte)Mathf.RoundToInt(Mathf.Clamp01(inner[i]/1.5f)*255),
            (byte)Mathf.RoundToInt(Mathf.Clamp01(outer[i]/1.5f)*255),255);
        var texture=new Texture2D(width,height,TextureFormat.RGBA32,false,true) {
            name="Permanent cave rock distances",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp
        };
        texture.SetPixels32(pixels);texture.Apply(false,!readable);return texture;
    }
    static void Distance(float[] field,int width,int height,float sx,float sy)
    {
        float diagonal=Mathf.Sqrt(sx*sx+sy*sy);
        for(int pass=0;pass<2;pass++) {
            int step=pass==0?1:-1;
            for(int y=pass==0?0:height-1;y>=0&&y<height;y+=step)
                for(int x=pass==0?0:width-1;x>=0&&x<width;x+=step) {
                    int i=y*width+x,nx=x-step,ny=y-step;
                    float value=field[i];
                    if(nx>=0&&nx<width) value=Mathf.Min(value,field[y*width+nx]+sx);
                    if(ny>=0&&ny<height) {
                        value=Mathf.Min(value,field[ny*width+x]+sy);
                        if(x>0) value=Mathf.Min(value,field[ny*width+x-1]+diagonal);
                        if(x+1<width) value=Mathf.Min(value,field[ny*width+x+1]+diagonal);
                    }
                    field[i]=value;
                }
        }
    }
}
