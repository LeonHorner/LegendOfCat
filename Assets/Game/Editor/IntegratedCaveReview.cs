using System.Collections.Generic;
using UnityEngine;

// Review entry points retained for the whole-level input pilot and regression suite.
public static class IntegratedCaveReview
{
    public static void Record()=>CaveRebuildReview.VerifyGeometry();
    public static List<Vector3> Path()=>CaveTerrainLayout.Path();
    public static List<Vector3> ApproachPath(){var result=Path();result[0]=new Vector3(2.6f,-.045f,1);result.Insert(0,new Vector3(-6.6f,-1.2f,0));return result;}
    public static void Verify()=>CaveRebuildReview.Run(false);
    public static void VerifyShoreRegistration(MovementCourse course)
    {
        Physics2D.SyncTransforms();var shapes=course.GetComponentsInChildren<Collider2D>();
        bool Effective(Collider2D c)=>c.enabled&&!c.isTrigger&&c.compositeOperation==Collider2D.CompositeOperation.None;
        foreach(int i in new[]{9,10,11,33,34,39}){
            var p=MovementCourse.AscentPlatforms[i];var point=new Vector2(p.x,p.y+.74f);int contacts=0;
            foreach(var c in shapes){if(Effective(c)&&c.OverlapPoint(point))contacts++;
                Require(c.name!="Standable platform"||Mathf.Abs(c.transform.position.x-p.x)>=.01f,"Old main collision remains at shore "+i);}
            Require(contacts==1,"Duplicate or missing native contact at shore "+i+": "+contacts);
        }
        foreach(float x in new[]{-35f,-25,-10,15,40,75,100,130,170,180,250,275}){
            float y=x< -5.8f||x>=247?-5.3f:-4.85f;bool solid=false;
            foreach(var c in shapes)if(Effective(c)&&c.OverlapPoint(CaveRoute.ToCourse(new Vector2(x,y)))){solid=true;break;}
            Require(solid,"Lower water boundary is exposed at "+x);
        }
        foreach(float x in new[]{-5.6f,12,185,246.5f}){
            bool solid=false;foreach(var c in shapes)if(Effective(c)&&c.OverlapPoint(CaveRoute.ToCourse(new Vector2(x,-3.1f)))){solid=true;break;}
            Require(solid,"Water ends outside solid shore at "+x);
        }
    }
    static void Require(bool valid,string message){if(!valid)throw new System.InvalidOperationException(message);}
}
