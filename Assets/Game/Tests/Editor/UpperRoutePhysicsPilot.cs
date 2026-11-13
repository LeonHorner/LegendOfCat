using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Input pilot for the permanent connected cave, shared by whole-level regression/capture.
public sealed class UpperRoutePhysicsPilot
{
    readonly Component cat;
    readonly Rigidbody2D body;
    readonly object motion;
    readonly List<Vector3> route;
    readonly Vector2 origin;
    bool launched;
    int settle;
    public int Next {get;private set;}
    public bool Complete=>Next>=route.Count;
    public UpperRoutePhysicsPilot(Component course)
    {
        cat=(Component)course.GetType().GetProperty("Cat").GetValue(course);
        body=cat.GetComponent<Rigidbody2D>();motion=cat.GetType().GetField("Motion").GetValue(cat);
        var review=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("IntegratedCaveReview")).First(t=>t!=null);
        route=(List<Vector3>)review.GetMethod("Path").Invoke(null,null);
        var data=course.GetType().Assembly.GetType("CaveRoute");
        origin=new Vector2((float)data.GetProperty("Origin").GetValue(null),.045f);
    }
    void Move(float axis)=>motion.GetType().GetMethod("SetMove").Invoke(motion,new object[]{axis});
    public void Command()
    {
        if(Complete){Move(0);return;}
        var node=route[Next];var target=origin+new Vector2(node.x,node.y+.46f);
        bool grounded=(bool)cat.GetType().GetProperty("Grounded").GetValue(cat);
        if(grounded&&Mathf.Abs(body.position.x-target.x)<.3f&&Mathf.Abs(body.position.y-target.y)<.25f) {
            Move(0);if(++settle>=8){Next++;settle=0;launched=false;}return;
        }
        Move(Mathf.Clamp((target.x-body.position.x)*3,-1,1));
        if(node.z>0&&!launched&&grounded){cat.GetType().GetMethod("PressJump").Invoke(cat,null);launched=true;}
    }
}
