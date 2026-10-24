using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class MikaGaitTests
{
    Type type;
    object gait;
    Vector2[] Feet => (Vector2[])type.GetField("Feet").GetValue(gait);
    bool[] Supporting => (bool[])type.GetField("Supporting").GetValue(gait);
    void Step(float dt, bool grounded, Vector2 velocity, Vector2 origin, float distance) =>
        type.GetMethod("Step").Invoke(gait, new object[] {dt, grounded, velocity, origin, distance});

    [SetUp]
    public void Create()
    {
        type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("MikaGait")).First(t => t != null);
        gait = Activator.CreateInstance(type);
    }

    [Test]
    public void AllFourSolesRemainOnFloorAtRest()
    {
        for (int frame = 0; frame < 120; frame++) {
            Step(1f/60, true, Vector2.zero, new Vector2(2,3), 0);
            for (int leg = 0; leg < 4; leg++) {
                Assert.That(Feet[leg].y, Is.EqualTo(3).Within(0.0001f));
                Assert.That(Supporting[leg], Is.True);
            }
        }
    }

    [Test]
    public void StanceFeetDoNotSlideAndEachPawReturnsToFloor()
    {
        var previous = new Vector2[4];
        var support = new bool[4];
        var landings = new int[4];
        for (int frame = 0; frame < 600; frame++) {
            Step(1f/120, true, new Vector2(7,0), new Vector2(frame*7f/120,0), frame == 0 ? 0 : 7f/120);
            int contacts = 0;
            for (int leg = 0; leg < 4; leg++) {
                if (Supporting[leg]) {
                    contacts++;
                    Assert.That(Feet[leg].y, Is.Zero.Within(0.0001f));
                    if (support[leg]) Assert.That(Feet[leg].x, Is.EqualTo(previous[leg].x).Within(0.0001f));
                    else landings[leg]++;
                }
                previous[leg] = Feet[leg]; support[leg] = Supporting[leg];
            }
            Assert.That(contacts, Is.GreaterThanOrEqualTo(2), "At least a diagonal support pair stays grounded.");
        }
        Assert.That(landings.All(n => n > 5), Is.True);
    }

    [Test]
    public void StopFinishesSwingAndPlantsAllFourPaws()
    {
        Step(0.05f, true, new Vector2(7,0), Vector2.zero, 0.35f);
        for (int i = 0; i < 120; i++) Step(1f/60, true, Vector2.zero, Vector2.zero, 0);
        Assert.That(Supporting.All(v => v), Is.True);
        Assert.That(Feet.All(v => Mathf.Abs(v.y) < 0.0001f), Is.True);
    }

    [Test]
    public void TwoBoneSolutionPreservesBothSegmentLengths()
    {
        Vector2 hip = new Vector2(0.37f,0.50f), ankle = new Vector2(0.15f,0.065f);
        var knee = (Vector2)type.GetMethod("Knee").Invoke(null, new object[] {hip, ankle, 0.36f, 0.35f, -1});
        Assert.That(Vector2.Distance(hip,knee), Is.EqualTo(0.36f).Within(0.0001f));
        Assert.That(Vector2.Distance(knee,ankle), Is.EqualTo(0.35f).Within(0.0001f));
    }

    [Test]
    public void AirborneFeetFollowRootTranslationWithoutWorldSpaceLag()
    {
        Step(1f/60,true,Vector2.zero,Vector2.zero,0);
        Step(1f/60,false,new Vector2(7,12),new Vector2(0.1f,0.2f),0.1f);
        for (int i = 0; i < 4; i++)
            Assert.That(Feet[i].y,Is.GreaterThanOrEqualTo(0.2f),"Airborne feet must be carried by the jump root.");
    }
}
