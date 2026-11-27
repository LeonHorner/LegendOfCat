using System;
using UnityEngine;

// Fast-player input for the river cave: hold right the whole way and jump at fixed takeoff points.
// Unlike the review pilot it never stops on a ledge, so its time is comparable with the surface pilot.
public sealed class CaveSpeedPilot
{
    // Local cave x of each takeoff: boulder, four waterfall shelves and the bank, six seep shelves, daylight hollow.
    public static readonly float[] Takeoffs = { 24.3f, 80.65f, 87.65f, 94.05f, 100.65f, 107.45f,
        145f, 151.25f, 157.75f, 164.05f, 172.25f, 178.25f, 196.3f };
    readonly Component cat;
    readonly object motion;
    readonly Rigidbody2D body;
    readonly Type route;
    int next;
    bool wasGrounded = true;
    public CaveSpeedPilot(Component course)
    {
        cat = (Component)course.GetType().GetProperty("Cat").GetValue(course);
        motion = cat.GetType().GetField("Motion").GetValue(cat);
        body = cat.GetComponent<Rigidbody2D>();
        route = course.GetType().Assembly.GetType("CaveRoute");
    }
    public float LocalX => body.position.x - (float)route.GetProperty("Origin").GetValue(null);
    public bool Complete => LocalX >= (float)route.GetProperty("RejoinLocal").GetValue(null);
    public void Command()
    {
        motion.GetType().GetMethod("SetMove").Invoke(motion, new object[] { 1f });
        bool grounded = (bool)cat.GetType().GetProperty("Grounded").GetValue(cat);
        float localY = body.position.y - .045f;
        if (grounded != wasGrounded)
            Debug.Log((grounded ? "Cave speed run landed at " : "Cave speed run left ground at ") + LocalX.ToString("F2") + ", " + localY.ToString("F2"));
        wasGrounded = grounded;
        while (next < Takeoffs.Length && LocalX > Takeoffs[next] + 1.2f) next++;
        if (next < Takeoffs.Length && grounded && LocalX >= Takeoffs[next]) {
            Debug.Log("Cave speed run takeoff " + next + " at " + LocalX.ToString("F2") + ", " + localY.ToString("F2"));
            cat.GetType().GetMethod("PressJump").Invoke(cat, null);
            next++;
        }
    }
}
