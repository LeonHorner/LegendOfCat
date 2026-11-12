using System;
using UnityEngine;

// Editor-only input commands, shared by collision regression and normal-speed capture.
public sealed class UpperRoutePhysicsPilot
{
    readonly Component cat;
    readonly Rigidbody2D body;
    readonly object motion;
    readonly Vector3[] route;
    readonly float origin;
    readonly Vector3 initialSurface;
    int settle;
    bool launched;
    public int Next { get; private set; }
    public bool Complete => Next >= route.Length;
    public UpperRoutePhysicsPilot(Component course)
    {
        cat = (Component)course.GetType().GetProperty("Cat").GetValue(course);
        body = cat.GetComponent<Rigidbody2D>();
        motion = cat.GetType().GetField("Motion").GetValue(cat);
        var type = course.GetType().Assembly.GetType("UpperRoute");
        origin=(float)type.GetProperty("Origin").GetValue(null);
        initialSurface=new Vector3(origin,-.75f,8);
        var steps = (Vector3[])type.GetField("Platforms").GetValue(null);
        var rejoin = (Vector2)type.GetProperty("Rejoin").GetValue(null);
        route = new Vector3[steps.Length + 1];
        steps.CopyTo(route, 0);
        var ascent=(Vector3[])course.GetType().GetField("AscentPlatforms").GetValue(null);
        int rejoinIndex=(int)type.GetField("RejoinIndex").GetValue(null);
        route[steps.Length] = new Vector3(rejoin.x, rejoin.y - 1.27f, ascent[rejoinIndex].z);
    }
    public UpperRoutePhysicsPilot(Component course, Vector3[] custom, Vector3 start) : this(course)
    {
        route = custom; initialSurface = start;
    }
    void Move(float value) => motion.GetType().GetMethod("SetMove").Invoke(motion, new object[] { value });
    public void Command()
    {
        if (Complete) { Move(0); return; }
        var p = route[Next];
        bool grounded = (bool)cat.GetType().GetProperty("Grounded").GetValue(cat);
        if (launched && grounded && Mathf.Abs(body.position.y - (p.y + 1.22f)) < .15f &&
            Mathf.Abs(body.position.x - p.x) < p.z / 2) {
            Next++; settle = 0; launched = false; Move(0); return;
        }
        if (!launched) {
            float launch = body.position.x;
            {
                var source = Next > 0 ? route[Next - 1] : initialSurface;
                float direction = Mathf.Sign(p.x - source.x);
                launch = source.x + direction * (source.z / 2 - .8f);
                if (Mathf.Abs(p.x - source.x) < (source.z + p.z) / 2)
                    launch = p.x - direction * (p.z / 2 + 1.1f);
                if(p.y>source.y+.3f) launch=direction<0
                    ? Mathf.Max(launch,p.x+p.z/2+.7f) : Mathf.Min(launch,p.x-p.z/2-.7f);
                if (Mathf.Abs(p.x - source.x) <= (source.z + p.z) / 2 + .05f && p.y <= source.y + .15f)
                    launch = body.position.x;
            }
            float approach = launch - body.position.x;
            Move(grounded && Mathf.Abs(approach) > .18f ? Mathf.Sign(approach) : 0);
            if (!grounded || Mathf.Abs(approach) > .35f) { settle = 0; return; }
            if (++settle < 5) return;
            launched = true;
            var from = Next > 0 ? route[Next - 1] : initialSurface;
            bool connected = Mathf.Abs(p.x-from.x) <= (p.z+from.z)/2 + .05f && p.y <= from.y + .15f;
            float gap = Mathf.Abs(p.x-from.x)-(p.z+from.z)/2;
            if (!connected && (p.y >= from.y - .55f || gap > 1.2f)) cat.GetType().GetMethod("PressJump").Invoke(cat, null);
        }
        float target = p.x;
        if (Next > 0 && p.z >= 4)
            target -= Mathf.Sign(p.x - route[Next - 1].x) * (p.z / 2 - 1);
        var previous = Next > 0 ? route[Next - 1] : initialSurface;
        if (p.y < previous.y - .55f && Mathf.Abs(p.x - previous.x) < (p.z + previous.z) / 2) {
            float direction = Mathf.Sign(p.x - previous.x);
            float clearSource = previous.x + direction * (previous.z / 2 + .7f);
            target = direction > 0 ? Mathf.Max(target, clearSource) : Mathf.Min(target, clearSource);
        }
        float distance = target - body.position.x;
        Move(Mathf.Abs(distance) > .25f ? Mathf.Sign(distance) : 0);
    }
}
