using System;
using UnityEngine;

// Whole-level evidence uses real move/jump input, including the optional loop and finale.
public sealed class UpperLevelPhysicsPilot
{
    readonly Component course, cat;
    readonly object motion, run;
    readonly Rigidbody2D body;
    readonly Vector3[] platforms;
    readonly Component[] movers;
    readonly float entrance;
    int next;
    bool entered;
    UpperRoutePhysicsPilot upper;
    FinalePhysicsPilot finale;
    public bool Complete => (bool)course.GetType().GetProperty("HasReachedExit").GetValue(course);
    public UpperLevelPhysicsPilot(Component course)
    {
        this.course = course;
        cat = (Component)course.GetType().GetProperty("Cat").GetValue(course);
        run = course.GetType().GetProperty("Run").GetValue(course);
        motion = cat.GetType().GetField("Motion").GetValue(cat);
        body = cat.GetComponent<Rigidbody2D>();
        platforms = (Vector3[])((Vector3[])course.GetType().GetField("CompletePlatforms").GetValue(null)).Clone();
        movers = course.GetComponentsInChildren(course.GetType().Assembly.GetType("MovingSupport"));
        entrance = ((Vector2)course.GetType().Assembly.GetType("CaveRoute").GetProperty("Entrance").GetValue(null)).x;
    }
    void Move(float value) => motion.GetType().GetMethod("SetMove").Invoke(motion, new object[] { value });
    public UpperLevelPhysicsPilot(Component course, bool includeDescent) : this(course) { entered = !includeDescent; }
    public void Command()
    {
        if ((int)run.GetType().GetProperty("Retries").GetValue(run) != 0)
            throw new InvalidOperationException("Whole-level capture fell at " + body.position);
        foreach (var mover in movers) {
            int index = Array.FindIndex(platforms, p => Mathf.Abs(p.x - mover.transform.position.x) < 2 && Mathf.Abs(p.z - mover.transform.localScale.x) < .01f);
            if (index >= 0) platforms[index] = new Vector3(mover.transform.position.x, mover.transform.position.y, mover.transform.localScale.x);
        }
        bool grounded = (bool)cat.GetType().GetProperty("Grounded").GetValue(cat);
        if (!entered && body.position.x >= entrance - .6f) {
            Move(0);
            if (grounded && Mathf.Abs(body.linearVelocity.x) < .1f) { entered = true; upper = new UpperRoutePhysicsPilot(course); }
            return;
        }
        if (upper != null && !upper.Complete) { upper.Command(); return; }
        if ((int)run.GetType().GetProperty("Checkpoint").GetValue(run) == 2 && finale == null) finale = new FinalePhysicsPilot(course);
        if (finale != null) { finale.Command(); return; }
        while (next < platforms.Length - 1 && body.position.x > platforms[next].x + platforms[next].z / 2) next++;
        float axis = !grounded && next >= 43 && body.position.x >= platforms[next].x && body.position.x < platforms[next].x + platforms[next].z / 2 - 1 ? 0 : 1;
        bool wait = next == 94 && grounded && platforms[next + 1].y - platforms[next].y > 1.8f;
        if (wait && body.position.x > platforms[next].x + platforms[next].z / 2 - 1.5f) axis = 0;
        Move(axis);
        if (grounded && !wait && next < platforms.Length - 1 && body.position.x >= platforms[next].x + platforms[next].z / 2 - .7f)
            cat.GetType().GetMethod("PressJump").Invoke(cat, null);
    }
}
