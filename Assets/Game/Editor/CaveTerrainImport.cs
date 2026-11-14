using UnityEditor;
using UnityEngine;
using System.Linq;

public static class CaveTerrainImport
{
    public static readonly string[] MaskNames={"cave-coverage-v1","entry-boundary-0-v1","entry-boundary-1-v1","entry-boundary-2-v1",
        "entry-boundary-3-v1","entry-boundary-4-v1","entry-boundary-5-v1"};
    [InitializeOnLoadMethod]static void Install(){EditorApplication.playModeStateChanged-=BeforePlay;EditorApplication.playModeStateChanged+=BeforePlay;}
    static void BeforePlay(PlayModeStateChange state){if(state==PlayModeStateChange.ExitingEditMode)BakeMasks();}
    public static void Prepare()
    {
        ArchiveRejectedTerrain();
        for(int i=0;i<6;i++){
            string path="Assets/Game/Resources/GoldenRiver/Descent/entry-module-"+i+"-v1.png";
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(!importer)continue;
            importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Clamp;
            importer.npotScale=TextureImporterNPOTScale.None;
            importer.mipmapEnabled=false;importer.isReadable=false;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
            var platform=importer.GetPlatformTextureSettings("Standalone");platform.overridden=true;platform.maxTextureSize=2048;
            platform.format=TextureImporterFormat.RGBA32;importer.SetPlatformTextureSettings(platform);
            importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(texture.width<1152||texture.height<1152)throw new System.InvalidOperationException("Entrance module falls below 192 source pixels per world unit: "+path);
        }
        foreach(var pair in new[]{new[]{"cave-rock-material","LegendOfCat/CaveRock"},new[]{"cave-grass-material","LegendOfCat/CaveGrass"}}){
            string path="Assets/Game/Resources/GoldenRiver/Descent/"+pair[0]+".mat";
            if(AssetDatabase.LoadAssetAtPath<Material>(path))continue;
            var shader=Shader.Find(pair[1]);if(!shader)throw new System.InvalidOperationException("Missing cave terrain shader: "+pair[1]);
            AssetDatabase.CreateAsset(new Material(shader),path);
        }
        BakeMasks();AssetDatabase.SaveAssets();
    }
    public static void BakeMasks()
    {
        const string folder="Assets/Game/Resources/GoldenRiver/Descent/";
        string hash;
        using(var input=new System.IO.MemoryStream())using(var digest=System.Security.Cryptography.SHA256.Create()){
            foreach(var path in new[]{"Assets/Game/Scripts/CaveTerrainLayout.cs","Assets/Game/Scripts/CaveRoute.cs",
                "Assets/Game/Scripts/CaveLevelConnection.cs","Assets/Game/Scripts/MovementCourse.cs","Assets/Game/Editor/CaveTerrainImport.cs"}){
                var bytes=System.IO.File.ReadAllBytes(path);input.Write(bytes,0,bytes.Length);
            }
            hash="cave-mask-v1:"+System.Convert.ToBase64String(digest.ComputeHash(input.ToArray()));
        }
        if(MaskNames.All(name=>AssetImporter.GetAtPath(folder+name+".png") is TextureImporter importer&&importer.userData==hash))return;
        for(int i=0;i<MaskNames.Length;i++){
            int index=i-1;
            var texture=i==0?CaveRoute.CreateMask(true):CaveEntryStudy.EntranceBoundary(
                index==3||index==5?CaveTerrainLayout.EntryRoof:CaveTerrainLayout.LeftShore,CaveTerrainLayout.EntranceFrames[index],true);
            string path=folder+MaskNames[i]+".png";
            try{var bytes=texture.EncodeToPNG();if(!System.IO.File.Exists(path)||!System.IO.File.ReadAllBytes(path).SequenceEqual(bytes))System.IO.File.WriteAllBytes(path,bytes);}
            finally{Object.DestroyImmediate(texture);}
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;
            importer.sRGBTexture=false;importer.alphaIsTransparency=false;importer.mipmapEnabled=false;importer.isReadable=false;
            importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
            importer.maxTextureSize=i==0?4096:256;importer.textureCompression=TextureImporterCompression.Uncompressed;
            var platform=importer.GetPlatformTextureSettings("Standalone");platform.overridden=true;platform.maxTextureSize=importer.maxTextureSize;
            platform.format=TextureImporterFormat.RGBA32;importer.SetPlatformTextureSettings(platform);importer.userData=hash;importer.SaveAndReimport();
        }
    }
    public static void ReportBudget()
    {
        Prepare();long total=0;
        foreach(var name in BuildScenery.GoldenRiverArt){if(!name.StartsWith("Descent/",System.StringComparison.Ordinal))continue;
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Game/Resources/GoldenRiver/"+name+".png");
            long bytes=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture);total+=bytes;
            Debug.Log(name+" "+texture.width+"x"+texture.height+" "+texture.format+" "+bytes+" bytes");
        }
        Debug.Log("Complete active descent imported texture memory: "+total);
    }
    static void ArchiveRejectedTerrain()
    {
        const string root="Assets/Game/Art/GoldenRiverArchive";
        const string folder=root+"/terrain-rebuild-2026-11-14";
        if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder(root,"terrain-rebuild-2026-11-14");
        foreach(var name in new[]{"anchored-ledge-v1","shore-left-v1","shore-right-v1","shore-exit-roof-v1","shore-exit-floor-v1",
            "return-floor-a-v1","return-floor-b-v1","return-roof-v1","entry-bank-v3","cliff-stone-v2",
            "cave-edge-short-v2","cave-edge-medium-v2","cave-edge-wide-v2","entry-left-v1","entry-overhang-v1",
            "inner-corner-v1","ceiling-v1","rock-face-v1","rooted-face-v1","tread-short-v1","tread-medium-v1",
            "tread-wide-v1","rear-rock-v1","floor-v1","vegetation-v1","shore-entry-v1","shore-exit-v1"}){
            string source="Assets/Game/Resources/GoldenRiver/Descent/"+name+".png";
            if(!AssetDatabase.LoadAssetAtPath<Texture2D>(source))continue;
            string error=AssetDatabase.MoveAsset(source,folder+"/"+name+".png");
            if(!string.IsNullOrEmpty(error))throw new System.InvalidOperationException(error);
        }
    }
}
