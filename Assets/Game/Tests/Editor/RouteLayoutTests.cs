using System;
using System.Linq;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public sealed class RouteLayoutTests
{
    [Serializable]
    sealed class Snapshot {
        public Vector3[] platforms, recoveries;
        public int[] materials, recoveryEntries;
        public Vector2[] checkpoints;
        public Vector2 exit;
    }
    static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
    static object Property(object value,string name) => value.GetType().GetProperty(name).GetValue(value);

    [Test]
    public void NamedRouteDataPreservesTheApprovedLayout()
    {
        var expected=JsonUtility.FromJson<Snapshot>(File.ReadAllText(
            Path.Combine(Application.dataPath,"Game/Tests/Editor/Fixtures/approved-golden-river-layout.json")));
        var course=RuntimeType("MovementCourse");
        var actual=(Vector3[])course.GetField("CompletePlatforms").GetValue(null);
        Assert.That(actual,Has.Length.EqualTo(expected.platforms.Length));
        for(int i=0;i<actual.Length;i++)
            Assert.That(Vector3.Distance(actual[i],expected.platforms[i]),Is.LessThan(.00001f),"Platform "+i);
        Assert.That((Vector2[])course.GetField("LevelOneCheckpointSpawns").GetValue(null),Is.EqualTo(expected.checkpoints));
        Assert.That((Vector2)course.GetField("LevelOneExit").GetValue(null),Is.EqualTo(expected.exit));
        var finale=RuntimeType("FinaleRoute");
        var platforms=((IEnumerable)finale.GetProperty("Definitions").GetValue(null)).Cast<object>().ToArray();
        Assert.That(platforms.Select(p=>Convert.ToInt32(Property(p,"Material"))),Is.EqualTo(expected.materials));
        var recoveries=((IEnumerable)finale.GetProperty("Recoveries").GetValue(null)).Cast<object>().ToArray();
        Assert.That(recoveries,Has.Length.EqualTo(expected.recoveries.Length));
        for(int i=0;i<recoveries.Length;i++) {
            var floor=(Vector3)Property(Property(recoveries[i],"Floor"),"Geometry");
            Assert.That(Vector3.Distance(floor,expected.recoveries[i]),Is.LessThan(.00001f),"Recovery "+i);
            Assert.That(Property(recoveries[i],"ReturnPlatformIndex"),Is.EqualTo(expected.recoveryEntries[i]));
            Assert.That((int)Property(recoveries[i],"SupportPlatformIndex"),Is.InRange(-1,platforms.Length-1));
        }
    }
}
