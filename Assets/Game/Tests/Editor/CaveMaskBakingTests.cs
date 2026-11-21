using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class CaveMaskBakingTests
{
    static Type Runtime(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);

    [Test]
    public void PreparedMasksPreserveEveryGeneratedCoveragePixel()
    {
        var layout=Runtime("CaveTerrainLayout");
        var frames=(Rect[])layout.GetField("EntranceFrames").GetValue(null);
        for(int index=-1;index<frames.Length;index++){
            string name=index<0?"cave-coverage-v1":"entry-boundary-"+index+"-v1";
            string path="Assets/Game/Resources/GoldenRiver/Descent/"+name+".png";
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            Assert.That(importer,Is.Not.Null,"Missing prepared mask: "+name);
            Assert.That(importer.sRGBTexture,Is.False,"Coverage is linear data.");
            Assert.That(importer.userData,Does.StartWith("cave-mask-v1:"));
            var actual=new Texture2D(2,2,TextureFormat.RGBA32,false,true);
            Texture2D expected=null;
            try{
                actual.LoadImage(File.ReadAllBytes(path));
                if(index<0)expected=(Texture2D)Runtime("CaveRoute").GetMethod("CreateMask").Invoke(null,new object[]{true});
                else{
                    var outline=(Vector2[])layout.GetField(index==3||index==5?"EntryRoof":"LeftShore").GetValue(null);
                    expected=(Texture2D)Runtime("CaveEntryStudy").GetMethod("EntranceBoundary").Invoke(null,new object[]{outline,frames[index],true});
                }
                Assert.That(actual.width,Is.EqualTo(expected.width));Assert.That(actual.height,Is.EqualTo(expected.height));
                var saved=actual.GetPixels32();var generated=expected.GetPixels32();
                for(int pixel=0;pixel<saved.Length;pixel++)if(!saved[pixel].Equals(generated[pixel]))
                    Assert.Fail("Prepared mask differs from current world contours: "+name+" pixel "+pixel);
            }
            finally{UnityEngine.Object.DestroyImmediate(actual);if(expected)UnityEngine.Object.DestroyImmediate(expected);}
        }
    }
}
