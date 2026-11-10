using UnityEngine;

// Optional geometry is separate from the accepted main-route traversal/index contract.
public static class UpperRoute
{
    public const int EntranceIndex = 10, RejoinIndex = 14, LookoutIndex = 8;
    public static Vector2 Entrance => new Vector2(MovementCourse.AscentPlatforms[EntranceIndex].x, .52f);
    public static Vector2 Rejoin => new Vector2(MovementCourse.AscentPlatforms[RejoinIndex].x, .52f);
    public static readonly Vector3[] Platforms = Build();

    static Vector3[] Build()
    {
        float x = MovementCourse.AscentPlatforms[EntranceIndex].x;
        // Leave the main camera corridor to the left, discover the enclosed garden,
        // then turn through its upper arcade before descending to the original bank.
        var steps = new[] {
            new Vector3(-2.5f, 1.6f, 2.5f), new Vector3(-6.5f, 3.2f, 3),
            new Vector3(-10.5f, 4.8f, 3), new Vector3(-14.5f, 6.4f, 3),
            new Vector3(-18.5f, 8, 3), new Vector3(-22.5f, 9.6f, 3),
            new Vector3(-26.5f, 11.2f, 3), new Vector3(-31, 12.5f, 4),
            new Vector3(-42, 13, 16),
            new Vector3(-48, 14.6f, 3), new Vector3(-44, 16.2f, 3),
            new Vector3(-39, 17, 4), new Vector3(-32, 17, 6),
            new Vector3(-24, 16, 6), new Vector3(-16, 14.5f, 6),
            new Vector3(-8, 13, 6), new Vector3(0, 11.5f, 6),
            new Vector3(8, 10, 6), new Vector3(16, 8.5f, 6),
            new Vector3(23, 6.5f, 5), new Vector3(29, 5.5f, 4)
        };
        for (int i = 0; i < steps.Length; i++) steps[i] = new Vector3(x + steps[i].x, steps[i].y - .75f, steps[i].z);
        return steps;
    }
}
