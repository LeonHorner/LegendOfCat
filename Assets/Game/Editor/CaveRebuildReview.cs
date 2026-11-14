using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CaveRebuildReview
{
    public static void VerifyGeometry()
    {
        BuildScenery.ImportGoldenRiverArt();SessionState.SetBool("CaveRebuildReview",true);EditorApplication.EnterPlaymode();
    }
    [InitializeOnLoadMethod]static void Install(){EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;}
    static void Changed(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool("CaveRebuildReview",false))return;
        SessionState.SetBool("CaveRebuildReview",false);int result=0;
        try{Run(true);}catch(System.Exception e){Debug.LogException(e);result=1;}
        if(Application.isBatchMode)EditorApplication.Exit(result);else EditorApplication.ExitPlaymode();
    }
    public static void Run(bool capture)
    {
        if(capture)Stencils();
        var scene=SceneManager.CreateScene("Shared cave geometry review",new CreateSceneParameters(LocalPhysicsMode.Physics2D));SceneManager.SetActiveScene(scene);
        var startup=System.Diagnostics.Stopwatch.StartNew();
        var root=new GameObject("Cave geometry review");root.SetActive(false);var course=root.AddComponent<MovementCourse>();course.LevelOne=true;root.SetActive(true);
        Debug.Log("Golden River review construction: "+startup.Elapsed.TotalMilliseconds.ToString("F1")+" ms");course.StartDescentPreview();
        IntegratedCaveReview.VerifyShoreRegistration(course);
        var cat=course.Cat;cat.GetComponent<CatInput>().enabled=false;cat.GetComponent<Rigidbody2D>().interpolation=RigidbodyInterpolation2D.None;
        var movers=root.GetComponentsInChildren<MovingSupport>();foreach(var m in movers)m.SendMessage("Start");
        var follow=root.GetComponentInChildren<CatCamera>();var scenery=root.GetComponentInChildren<SceneryRenderer>();
        if(capture){
            var textures=new HashSet<Texture>();var cave=root.GetComponentInChildren<CaveEntryStudy>();
            foreach(var renderer in cave.GetComponentsInChildren<Renderer>())foreach(var material in renderer.sharedMaterials)
                foreach(var property in material.GetTexturePropertyNames()){var texture=material.GetTexture(property);if(texture)textures.Add(texture);}
            long bytes=0;foreach(var texture in textures)bytes+=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture);
            System.IO.Directory.CreateDirectory("artifacts/cave-rebuild-review");System.IO.File.WriteAllText("artifacts/cave-rebuild-review/texture-memory.txt",
                "Distinct cave renderer textures and coverage masks: "+textures.Count+"\nUnity runtime memory estimate: "+bytes+" bytes\nThis is not a hardware frame-rate benchmark.\n");
        }
        bool recording=capture;int motionTick=0;string motion="entrance-motion";
        if(capture)System.IO.Directory.CreateDirectory("artifacts/cave-rebuild-review/"+motion);
        void Tick(){foreach(var m in movers)m.SendMessage("FixedUpdate");cat.SendMessage("FixedUpdate");scene.GetPhysicsScene2D().Simulate(.02f);course.SendMessage("Update");follow.Follow(.02f);scenery.SendMessage("LateUpdate");
            if(recording){cat.GetComponentInChildren<MikaSpritePresentation>().RenderPose(.02f);scenery.SetWaterTime(motionTick*.02f);
                if(motionTick%5==0)SaveFrame(follow.GetComponent<Camera>(),"artifacts/cave-rebuild-review/"+motion+"/frame-"+(motionTick/5).ToString("D4")+".png");motionTick++;}}
        void Reach(Vector3 point){var target=CaveRoute.ToCourse(new Vector2(point.x,point.y+.46f));bool jumped=false,reached=false;
            for(int i=0;i<600;i++){var p=(Vector2)cat.transform.localPosition;if(cat.Grounded&&Mathf.Abs(p.x-target.x)<.25f&&Mathf.Abs(p.y-target.y)<.2f){reached=true;cat.Motion.SetMove(0);for(int j=0;j<8;j++)Tick();break;}
                cat.Motion.SetMove(Mathf.Clamp((target.x-p.x)*3,-1,1));if(point.z>0&&!jumped&&cat.Grounded){cat.PressJump();jumped=true;}Tick();}
            Debug.Log("Shared cave target "+point+" reached="+reached+" actual="+CaveRoute.ToLocal(cat.transform.localPosition));
            if(!reached)throw new System.InvalidOperationException("Unreachable shared cave target "+point);
        }
        for(int i=0;i<15;i++)Tick();Reach(new Vector3(-6.6f,-1.2f,0));
        if(capture){cat.GetComponentInChildren<MikaSpritePresentation>().RenderPose(.25f);Save(follow.GetComponent<Camera>(),-1);}
        System.IO.Directory.CreateDirectory("artifacts/cave-rebuild-review");
        var path=CaveTerrainLayout.Path();path[0]=new Vector3(2.6f,-.045f,1);int index=0;foreach(var p in path){Reach(p);if(capture&&index<13){cat.GetComponentInChildren<MikaSpritePresentation>().RenderPose(.25f);Save(root.GetComponentInChildren<Camera>(),index);}if(capture&&index==9){recording=false;System.IO.File.WriteAllText("artifacts/cave-rebuild-review/"+motion+"/frames.txt",((motionTick+4)/5).ToString());}index++;}
        Debug.Log("Shared cave full forward geometry passed.");
        cat.RespawnAt(CaveRoute.ToCourse(new Vector2(101.5f,-19.49f)));for(int i=0;i<100;i++)Tick();
        if(!cat.Grounded||Mathf.Abs(CaveRoute.ToLocal(cat.transform.localPosition).y-(CaveTerrainLayout.CatchHeight+.51f))>.2f)throw new System.InvalidOperationException("The local miss did not reach its catch.");
        foreach(var p in new[]{new Vector3(95.7f,-26.5f,0),new Vector3(91.7f,-25.2f,1),new Vector3(87.7f,-23.9f,1),new Vector3(82.5f,-22.6f,1),new Vector3(81.3f,-21.3f,1),new Vector3(87.3f,-20,1),new Vector3(92.3f,-21.3f,0),new Vector3(98.7f,-20,1)})Reach(p);
        Debug.Log("Shared cave local recovery geometry passed.");
        cat.RespawnAt(CaveRoute.ToCourse(new Vector2(31,-11.49f)));for(int i=0;i<15;i++)Tick();
        Reach(new Vector3(35.3f,-12,0));Reach(new Vector3(31,-12,0));
        Debug.Log("Shared cave formerly mismatched seam passed in both directions without jumping.");
        cat.RespawnAt(CaveRoute.ToCourse(new Vector2(17.2f,-11.49f)));for(int i=0;i<15;i++)Tick();
        if(capture){motion="entrance-reverse";motionTick=0;recording=true;System.IO.Directory.CreateDirectory("artifacts/cave-rebuild-review/"+motion);}
        foreach(var p in new[]{new Vector3(14.7f,-10.7f,1),new Vector3(12,-9.3f,1),new Vector3(10.3f,-8,1),new Vector3(9,-6.405f,1),new Vector3(7.8f,-4.774f,1),new Vector3(6.5f,-3.234f,1),new Vector3(5,-1.585f,1),new Vector3(2.6f,-.045f,1)})Reach(p);
        Debug.Log("Shared cave entrance is reversible with actual jump input.");
        Reach(new Vector3(-10,-1.245f,0));Reach(new Vector3(2.6f,-.045f,0));
        Debug.Log("Connected approach shore passed in both directions.");
        cat.RespawnAt(CaveRoute.ToCourse(new Vector2(-4.3f,.5f)));for(int i=0;i<80;i++)Tick();
        var standing=cat.transform.localPosition;for(int i=0;i<100;i++)Tick();
        if(!cat.Grounded||Vector2.Distance(standing,cat.transform.localPosition)>.06f)throw new System.InvalidOperationException("Natural bank incline drifts while idle.");
        cat.PressJump();Tick();if(cat.GetComponent<Rigidbody2D>().linearVelocity.y<5)throw new System.InvalidOperationException("Jump from the natural bank incline failed.");
        Debug.Log("Natural bank incline supports idle and jumping.");
        if(capture){recording=false;System.IO.File.WriteAllText("artifacts/cave-rebuild-review/"+motion+"/frames.txt",((motionTick+4)/5).ToString());}
        cat.RespawnAt(CaveRoute.ToCourse(new Vector2(44,-12.16f)));for(int i=0;i<15;i++)Tick();
        course.TogglePause();var paused=cat.transform.position;for(int i=0;i<20;i++)Tick();
        if(cat.transform.position!=paused)throw new System.InvalidOperationException("Pause moved the cave player.");course.TogglePause();
        course.RetryCheckpoint();
        if(Vector2.Distance(cat.transform.localPosition,MovementCourse.LevelOneCheckpointSpawns[0])>.01f)throw new System.InvalidOperationException("Cave retry changed the main checkpoint.");
        foreach(float x in new[]{28,55,95,150}){
            cat.RespawnAt(CaveRoute.ToCourse(new Vector2(x,-2.9f)));cat.Launch(new Vector2(0,-20));for(int i=0;i<10;i++)Tick();
            if(cat.transform.localPosition.x>=CaveRoute.Origin-20)throw new System.InvalidOperationException("Main water admitted a fall into the cave at "+x);
        }
        course.RestartRun();if(course.Run.Checkpoint!=0||cat.transform.localPosition.x>=0)throw new System.InvalidOperationException("Restart did not return to the level beginning.");
        Debug.Log("Shared cave water, pause, retry and restart passed.");
        Object.DestroyImmediate(root);
    }
    static void Save(Camera view,int index)
    {
        foreach(int width in new[]{960,640}){
            int height=width*9/16;var target=new RenderTexture(width,height,24);target.Create();var previous=view.targetTexture;var active=RenderTexture.active;var rect=view.rect;
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);
            try{view.rect=new Rect(0,0,1,1);view.targetTexture=target;view.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
                string name="artifacts/cave-rebuild-review/entry-"+index+"-"+width;System.IO.File.WriteAllBytes(name+".png",image.EncodeToPNG());
                if(width==640){var pixels=image.GetPixels32();for(int i=0;i<pixels.Length;i++){byte v=(byte)(pixels[i].r*.2126f+pixels[i].g*.7152f+pixels[i].b*.0722f);pixels[i]=new Color32(v,v,v,255);}image.SetPixels32(pixels);image.Apply();System.IO.File.WriteAllBytes(name+"-gray.png",image.EncodeToPNG());}}
            finally{view.rect=rect;view.targetTexture=previous;RenderTexture.active=active;Object.DestroyImmediate(image);target.Release();Object.DestroyImmediate(target);}
        }
    }
    static void SaveFrame(Camera view,string path)
    {
        const int width=640,height=360;var target=new RenderTexture(width,height,24);target.Create();var previous=view.targetTexture;var active=RenderTexture.active;var rect=view.rect;
        var image=new Texture2D(width,height,TextureFormat.RGB24,false);
        try{view.rect=new Rect(0,0,1,1);view.targetTexture=target;view.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();System.IO.File.WriteAllBytes(path,image.EncodeToPNG());}
        finally{view.rect=rect;view.targetTexture=previous;RenderTexture.active=active;Object.DestroyImmediate(image);target.Release();Object.DestroyImmediate(target);}
    }
    public static void Stencils()
    {
        System.IO.Directory.CreateDirectory("artifacts/cave-rebuild-stencils");
        var frames=new[]{new Rect(-2.21f,-6.045f,6,6),new Rect(-6,-8.8f,6,6),new Rect(3.4f,-6.4f,6,6),new Rect(5.99f,-5.245f,6,6),new Rect(9,-13,6,6),new Rect(12,-10.5f,6,6)};
        for(int k=0;k<frames.Length;k++){
            var image=new Texture2D(1536,1536,TextureFormat.RGBA32,false);var pixels=new Color32[1536*1536];var frame=frames[k];
            for(int y=0;y<1536;y++)for(int x=0;x<1536;x++){
                var point=new Vector2(frame.x+(x+.5f)/256,frame.y+(y+.5f)/256);
                bool solid=CaveRoute.Contains(k==3||k==5?CaveTerrainLayout.EntryRoof:CaveTerrainLayout.LeftShore,point);
                pixels[y*1536+x]=solid?new Color32(116,100,142,255):new Color32(0,0,0,0);
            }
            image.SetPixels32(pixels);image.Apply();System.IO.File.WriteAllBytes("artifacts/cave-rebuild-stencils/entry-"+k+".png",image.EncodeToPNG());Object.DestroyImmediate(image);
        }
    }
}
