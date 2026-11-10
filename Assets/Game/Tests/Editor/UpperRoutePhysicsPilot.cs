using System;
using UnityEngine;

// Editor-only input commands, shared by collision regression and normal-speed capture.
public sealed class UpperRoutePhysicsPilot
{
    readonly Component cat;
    readonly Rigidbody2D body;
    readonly object motion;
    readonly Vector3[] route;
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
        var steps = (Vector3[])type.GetField("Platforms").GetValue(null);
        var rejoin = (Vector2)type.GetProperty("Rejoin").GetValue(null);
        route = new Vector3[steps.Length + 1];
        steps.CopyTo(route, 0);
        route[steps.Length] = new Vector3(rejoin.x, rejoin.y - 1.27f, 10);
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
        if (Next == route.Length - 1) {
            // Walk off the final terrace onto the visible overlapping broad bank below.
            var source = route[Next - 1];
            float direction = Mathf.Sign(p.x - source.x);
            float exit = source.x + direction * (source.z / 2 + 1);
            launched = true;
            Move(Mathf.Abs(exit - body.position.x) > .2f ? Mathf.Sign(exit - body.position.x) : 0);
            return;
        }
        if (!launched) {
            float launch = body.position.x;
            if (Next > 0) {
                var source = route[Next - 1];
                float direction = Mathf.Sign(p.x - source.x);
                launch = source.x + direction * (source.z / 2 - .8f);
                if (Mathf.Abs(p.x - source.x) < (source.z + p.z) / 2)
                    launch = p.x - direction * (p.z / 2 + 1.1f);
            }
            float approach = launch - body.position.x;
            Move(grounded && Mathf.Abs(approach) > .18f ? Mathf.Sign(approach) : 0);
            if (!grounded || Mathf.Abs(approach) > .35f) { settle = 0; return; }
            if (++settle < 10) return;
            launched = true;
            cat.GetType().GetMethod("PressJump").Invoke(cat, null);
        }
        float target = p.x;
        if (Next > 0 && p.z >= 4)
            target -= Mathf.Sign(p.x - route[Next - 1].x) * (p.z / 2 - 1);
        float distance = target - body.position.x;
        Move(Mathf.Abs(distance) > .25f ? Mathf.Sign(distance) : 0);
    }
}
