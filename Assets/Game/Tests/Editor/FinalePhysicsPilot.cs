using System;
using System.Reflection;
using UnityEngine;

// Editor-only command pilot: no teleports or velocity writes during traversal.
public sealed class FinalePhysicsPilot
{
    public int Current = -1;
    readonly Component course, cat;
    readonly object motion;
    readonly Vector3[] route;
    readonly Component[] movers;
    readonly int lift, diagonal;
    static object Get(object target, string name) => target.GetType().GetProperty(name).GetValue(target);
    public FinalePhysicsPilot(Component course)
    {
        this.course=course; cat=(Component)Get(course,"Cat");
        motion=cat.GetType().GetField("Motion").GetValue(cat);
        var type=course.GetType().Assembly.GetType("FinaleRoute");
        route=(Vector3[])type.GetProperty("Platforms").GetValue(null);
        lift=(int)type.GetProperty("LiftIndex").GetValue(null);
        diagonal=(int)type.GetProperty("DiagonalIndex").GetValue(null);
        movers=course.GetComponentsInChildren(course.GetType().Assembly.GetType("MovingSupport"));
    }
    Vector3 Surface(int index)
    {
        if(index<0) {
            var p=((Vector2[])course.GetType().GetField("LevelOneCheckpointSpawns").GetValue(null))[1];
            return new Vector3(p.x,p.y-1.27f,16);
        }
        var result=route[index];
        if(index==lift || index==diagonal) {
            var mover=movers[index==lift?2:3];
            result.x=mover.transform.localPosition.x; result.y=mover.transform.localPosition.y;
        }
        return result;
    }
    public void Command()
    {
        var position=(Vector2)course.transform.InverseTransformPoint(cat.GetComponent<Rigidbody2D>().position);
        bool ground=(bool)Get(cat,"Grounded");
        int next=Current+1;
        if(next<route.Length) {
            var target=Surface(next);
            if(ground && Mathf.Abs(position.y-(target.y+1.2f))<.22f && Mathf.Abs(position.x-target.x)<target.z/2+.2f) {
                if(Mathf.Abs(position.x-target.x)>.35f) {
                    motion.GetType().GetMethod("SetMove").Invoke(motion,new object[]{Mathf.Sign(target.x-position.x)});
                    return;
                }
                Current++; next++;
            }
        }
        if(next>=route.Length) {
            float exit=((Vector2)course.GetType().GetField("LevelOneExit").GetValue(null)).x;
            motion.GetType().GetMethod("SetMove").Invoke(motion,new object[]{position.x<exit?1f:0f}); return;
        }
        var from=Surface(Current); var to=Surface(next);
        float direction=Mathf.Sign(to.x-from.x);
        float axis=ground?direction:Mathf.Abs(position.x-to.x)<.35f?0:Mathf.Sign(to.x-position.x);
        bool nearEdge=direction*(position.x-from.x)>=from.z/2-.8f;
        bool waiting=to.y-from.y>1.8f;
        if(ground && nearEdge && waiting) axis=0;
        motion.GetType().GetMethod("SetMove").Invoke(motion,new object[]{axis});
        if(ground && !waiting) {
            var contacts=new ContactPoint2D[8];
            int count=cat.GetComponent<BoxCollider2D>().GetContacts(contacts);
            for(int i=0;i<count;i++) if(contacts[i].normal.x*axis<-.7f)
                cat.GetType().GetMethod("PressJump").Invoke(cat,null);
        }
        if(ground && nearEdge && !waiting) cat.GetType().GetMethod("PressJump").Invoke(cat,null);
    }
    public override string ToString() => Current + " from=" + Surface(Current) + " to=" + Surface(Mathf.Min(Current+1,route.Length-1));
}
