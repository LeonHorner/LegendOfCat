using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class BackgroundStudyTests
{
    Type Study => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("BackgroundStudy")).First(t => t != null);
    float Call(string method, params object[] args) => (float)Study.GetMethod(method).Invoke(null, args);

    [TestCase(0f)] [TestCase(0.08f)] [TestCase(0.28f)] [TestCase(0.55f)] [TestCase(1f)]
    public void MirroredTilesCoverViewAcrossNegativeAndPositiveWraps(float speed)
    {
        // Three 64-unit tiles must cover the widest supported 32:9 review viewport.
        for (int step = -2000; step <= 2000; step++) {
            float camera = step * 0.5f;
            float center = Call("Offset", camera, speed) + Call("TileCenter", camera, speed, 64f);
            Assert.That(center - 96, Is.LessThan(camera - 20));
            Assert.That(center + 96, Is.GreaterThan(camera + 20));
        }
    }

    [Test] public void ReversalAndTeleportPreserveAbsoluteRegistration()
    {
        float initial = Call("Offset", 17f, 0.28f);
        Call("Offset", 240f, 0.28f); Call("Offset", -240f, 0.28f);
        Assert.That(Call("Offset", 17f, 0.28f), Is.EqualTo(initial));
        // A camera move of one metre produces the requested relative scenery motion.
        foreach (float speed in new[] { 0.08f, 0.28f, 0.55f })
            Assert.That(1 - (Call("Offset", 18f, speed) - InitialFor(speed)), Is.EqualTo(speed).Within(0.00001));
    }
    float InitialFor(float speed) => Call("Offset", 17f, speed);
}
