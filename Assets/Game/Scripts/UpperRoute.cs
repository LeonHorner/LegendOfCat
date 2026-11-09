using UnityEngine;

// Optional geometry is separate from the accepted main-route traversal/index contract.
public static class UpperRoute
{
    public const int EntranceIndex = 10, RejoinIndex = 14, LookoutIndex = 4;
    public static Vector2 Entrance => new Vector2(MovementCourse.AscentPlatforms[EntranceIndex].x, .52f);
    public static Vector2 Rejoin => new Vector2(MovementCourse.AscentPlatforms[RejoinIndex].x, .52f);
    public static readonly Vector3[] Platforms = Build();

    static Vector3[] Build()
    {
        float x = MovementCourse.AscentPlatforms[EntranceIndex].x;
        // x, contact height, standable width. Rise to the left, then turn above the bank.
        var steps = new[] {
            new Vector3(-2.5f, 1.6f, 2.5f), new Vector3(1.5f, 3.2f, 2.5f),
            new Vector3(5.5f, 4.8f, 2.5f), new Vector3(10.5f, 5, 2.5f),
            new Vector3(15.5f, 5, 4.5f), new Vector3(21.5f, 4.7f, 3),
            new Vector3(27.5f, 4.2f, 3)
        };
        for (int i = 0; i < steps.Length; i++) steps[i] = new Vector3(x + steps[i].x, steps[i].y - .75f, steps[i].z);
        return steps;
    }
}
