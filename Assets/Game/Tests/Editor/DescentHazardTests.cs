using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class DescentHazardTests
{
    static Type Route => AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("CaveRoute")).First(t=>t!=null);
    static float Origin => (float)Route.GetProperty("Origin").GetValue(null);
    static bool Recover(Vector2 a,Vector2 b) => (bool)Route.GetMethod("ShouldRecover").Invoke(null,new object[]{a,b});
    [Test]
    public void FeetEnteringTheRiverRecoverBeforeTheBodyCanRestOnTheRiverbed()
    {
        Assert.That(Recover(new Vector2(Origin+28,-2.5f),new Vector2(Origin+28,-2.95f)),Is.True);
        Assert.That(Recover(new Vector2(Origin+7,-2.5f),new Vector2(Origin+7,-2.95f)),Is.False);
    }
    [Test]
    public void EntryAirDoesNotGrantWaterImmunityAboveThePassage()
    {
        Assert.That(Recover(new Vector2(Origin+12.4f,-7.6f),new Vector2(Origin+12.4f,-8.1f)),Is.False);
        Assert.That(Recover(new Vector2(Origin+12.4f,-2.5f),new Vector2(Origin+12.4f,-3)),Is.True);
    }
    [Test]
    public void MainWaterCannotBeBypassedByFallingIntoTheCaveBelow()
    {
        foreach(float x in new[]{-20f,-10,28,55,95,150,180,250,320,350}) {
            var before=new Vector2(Origin+x,-2.9f);
            Assert.That(Recover(before,new Vector2(before.x,-3.3f)),Is.True,"Main water at "+x);
            Assert.That(Recover(before,new Vector2(before.x,-20)),Is.True,"A fast fall must cross the water first.");
        }
    }
    [Test]
    public void OnlyTheTwoAuthoredShoreOpeningsPermitCrossingTheSurface()
    {
        foreach(float x in new[]{7.5f,202})
            Assert.That(Recover(new Vector2(Origin+x,-2.9f),new Vector2(Origin+x,-3.3f)),Is.False);
        Assert.That(Recover(new Vector2(Origin+28,-2.9f),new Vector2(Origin+28,-3.3f)),Is.True);
        Assert.That(Recover(new Vector2(Origin,-11),new Vector2(Origin,-12)),Is.False);
        Assert.That(Recover(new Vector2(Origin,-26),new Vector2(Origin,-27)),Is.True);
        Assert.That(Recover(new Vector2(Origin-100,-6),new Vector2(Origin-100,-8)),Is.True);
    }
    [Test]
    public void CoverageMaskAndHazardsUseTheSameTwoDimensionalContours()
    {
        var mask=(Texture2D)Route.GetMethod("CreateMask").Invoke(null,new object[]{true});
        var bounds=(Rect)Route.GetProperty("MaskBounds").GetValue(null);
        try {
            foreach(var p in new[]{new Vector2(0,-3.1f),new Vector2(7.5f,-3.1f),new Vector2(28,-3.1f),new Vector2(40,-3.1f),new Vector2(93,-18)}) {
                bool dry=(bool)Route.GetMethod("WaterRemoved").Invoke(null,new object[]{p-new Vector2(0,.045f)});
                int x=Mathf.Clamp((int)((p.x+Origin-bounds.xMin)/bounds.width*mask.width),0,mask.width-1);
                int y=Mathf.Clamp((int)((p.y-bounds.yMin)/bounds.height*mask.height),0,mask.height-1);
                Assert.That(mask.GetPixel(x,y).r>.5f,Is.EqualTo(dry),"Coverage at "+p);
                bool air=(bool)Route.GetMethod("InAir").Invoke(null,new object[]{p-new Vector2(0,.045f)});
                Assert.That(mask.GetPixel(x,y).b>.5f,Is.EqualTo(air),"Recovery-air coverage at "+p);
            }
        } finally { UnityEngine.Object.DestroyImmediate(mask); }
    }
}
