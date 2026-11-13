using System;
using System.Collections.Generic;
using UnityEngine;

public static class LevelRegistry
{
    static readonly Dictionary<string, Func<MovementCourse>> levels =
        new Dictionary<string, Func<MovementCourse>>(StringComparer.Ordinal) {
            { "movement-lab", () => UnityEngine.Object.Instantiate(Resources.Load<MovementCourse>("MovementCourse")) },
            { "golden-river", CreateGoldenRiver }
        };

    public static IEnumerable<string> Ids => levels.Keys;

    public static MovementCourse Create(string id) => levels[id]();

    static MovementCourse CreateGoldenRiver()
    {
        var root = new GameObject("Golden River main route");
        root.SetActive(false);
        var course = root.AddComponent<MovementCourse>();
        course.LevelOne = true;
        root.SetActive(true);
        return course;
    }
}
