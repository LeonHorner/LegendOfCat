using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class CaveGeometryTests
{
    static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);

    [Test]
    public void CarvingPreservesRockAreaOutsideTheOpening()
    {
        var rock=new[] {Vector2.zero,new Vector2(10,0),new Vector2(10,10),new Vector2(0,10)};
        var hole=new[] {new Vector2(3,3),new Vector2(7,3),new Vector2(7,7),new Vector2(3,7)};
        var geometry=Runtime("CaveGeometry");
        var pieces=(List<Vector2[]>)geometry.GetMethod("Subtract").Invoke(null,new object[]{new List<Vector2[]>{rock},hole});
        float area=pieces.Sum(p=>(float)geometry.GetMethod("Area").Invoke(null,new object[]{p}));
        Assert.That(area,Is.EqualTo(84).Within(.001));
        foreach(var piece in pieces)
            Assert.That((bool)geometry.GetMethod("Contains").Invoke(null,new object[]{piece,new Vector2(5,5)}),Is.False);
    }

    [Test]
    public void ContactShadingUsesDistanceToTheAuthoredSolidBoundary()
    {
        var rock=new[] {new Vector2(2,2),new Vector2(8,2),new Vector2(8,8),new Vector2(2,8)};
        var texture=(Texture2D)Runtime("CaveShading").GetMethod("Create").Invoke(null,
            new object[]{new List<Vector2[]>{rock},new Rect(0,0,10,10),100,100,true});
        try {
            Assert.That(texture.GetPixel(50,50).r,Is.EqualTo(1));
            Assert.That(texture.GetPixel(50,50).g,Is.GreaterThan(.95f));
            Assert.That(texture.GetPixel(20,50).g,Is.LessThan(.15f));
            Assert.That(texture.GetPixel(19,50).r,Is.Zero);
            Assert.That(texture.GetPixel(19,50).b,Is.LessThan(.15f));
            Assert.That(texture.GetPixel(0,50).b,Is.GreaterThan(.95f));
        } finally {UnityEngine.Object.DestroyImmediate(texture);}
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ConcaveTerrainLeavesThePassageEmptyInEitherWinding(bool reverse)
    {
        var points=new[]{new Vector2(0,0),new Vector2(4,0),new Vector2(4,4),new Vector2(3,4),
            new Vector2(3,1),new Vector2(1,1),new Vector2(1,4),new Vector2(0,4)};
        if(reverse)Array.Reverse(points);
        var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("CaveGeometry")).First(t=>t!=null);
        var indices=(int[])type.GetMethod("Triangulate").Invoke(null,new object[]{points});float area=0;
        for(int i=0;i<indices.Length;i+=3){var a=points[indices[i]];var b=points[indices[i+1]];var c=points[indices[i+2]];
            area+=Mathf.Abs((b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x))/2;
            var center=(a+b+c)/3;Assert.That(center.x>1&&center.x<3&&center.y>1,Is.False,"A terrain triangle covers the open cavity.");}
        Assert.That(area,Is.EqualTo(10).Within(.0001f));
    }
}
