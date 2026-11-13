using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IntegratedCaveReview
{
    public static void Record()
    {
        BuildScenery.ImportGoldenRiverArt();SessionState.SetBool("IntegratedCaveReview",true);EditorApplication.EnterPlaymode();
    }
    [InitializeOnLoadMethod]static void Install(){EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;}
    static void Changed(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool("IntegratedCaveReview",false))return;
        SessionState.SetBool("IntegratedCaveReview",false);int result=0;
        try{Run(true);}catch(System.Exception e){Debug.LogException(e);result=1;}
        if(Application.isBatchMode)EditorApplication.Exit(result);else EditorApplication.ExitPlaymode();
    }
    public static List<Vector3> Path()
    {
        var path=new List<Vector3>{new Vector3(2.6f,-.045f,0),new Vector3(5,-1.6f,0),new Vector3(6.5f,-3.2f,0),
            new Vector3(7.8f,-4.8f,0),new Vector3(9,-6.4f,0),new Vector3(10.3f,-8,0),new Vector3(12,-10,0),
            new Vector3(17.2f,-10,0),new Vector3(22,-10,1),new Vector3(34,-10,0),new Vector3(37.5f,-11.33f,0),
            new Vector3(44,-12.67f,0),new Vector3(55.8f,-12.67f,0),new Vector3(58.4f,-11.29f,1),
            new Vector3(60.9f,-9.84f,1),new Vector3(63.4f,-8.43f,1),new Vector3(66,-6.96f,1),new Vector3(70,-6.96f,0)};
        float previous=-6.96f,previousEnd=72;
        foreach(var p in CaveRoute.ReturnFloors) {
            bool jump=p.y>previous+.15f || Mathf.Abs(previousEnd-76)<.1f || Mathf.Abs(previousEnd-82)<.1f || Mathf.Abs(previousEnd-100)<.1f || Mathf.Abs(previousEnd-161)<.1f;
            float target=p.x>232&&p.x<233?234.3f:p.x==72?75.3f:p.x==79?79.8f:p.z>8?p.x+p.z-1.25f:p.x+p.z/2;
            path.Add(new Vector3(target,p.y,jump?1:0));previous=p.y;previousEnd=p.x+p.z;
        }
        var end=CaveRoute.ToLocal(CaveRoute.Rejoin);path.Add(new Vector3(end.x,end.y-.52f,1));
        return path;
    }
    public static List<Vector3> ApproachPath(){var result=Path();result[0]=new Vector3(2.6f,-.045f,1);result.Insert(0,new Vector3(-6.6f,-1.2f,0));return result;}
    public static void Verify()=>Run(false);
    public static void VerifyShoreRegistration(MovementCourse course)
    {
        Physics2D.SyncTransforms();var shapes=course.GetComponentsInChildren<Collider2D>();
        foreach(int i in new[]{10,11,33,34,39}) {
            var p=MovementCourse.AscentPlatforms[i];var point=new Vector2(p.x,p.y+.74f);int contacts=0;
            foreach(var c in shapes) {
                if(c.enabled&&!c.isTrigger&&c.OverlapPoint(point))contacts++;
                Require(c.name!="Standable platform"||Mathf.Abs(c.transform.position.x-p.x)>=.01f,"Old main collision remains at shore "+i);
            }
            Require(contacts==1,"Duplicate or missing native contact at shore "+i+": "+contacts);
        }
        foreach(float x in new[]{-35f,-25,-10,15,40,75,100,130,170,180,250,275}) {
            var point=new Vector2(CaveRoute.Origin+x,CaveRoute.RiverbedTop(x)-.15f+.045f);bool solid=false;
            foreach(var c in shapes)if(c.enabled&&!c.isTrigger&&c.OverlapPoint(point)){solid=true;break;}
            Require(solid,"Lower water boundary is exposed at "+x);
        }
        foreach(float x in new[]{-5.3f,12,185,246.8f}) {
            bool solid=false;foreach(var c in shapes)if(c.enabled&&!c.isTrigger&&c.OverlapPoint(new Vector2(CaveRoute.Origin+x,-3.1f))){solid=true;break;}
            Require(solid,"Water ends outside solid shore at "+x);
        }
    }
    static void Require(bool valid,string message){if(!valid)throw new System.InvalidOperationException(message);}
    static void Run(bool capture)
    {
        Directory.CreateDirectory("artifacts/integrated-cave-review");
        var scene=SceneManager.CreateScene("Integrated cave physics",new CreateSceneParameters(LocalPhysicsMode.Physics2D));SceneManager.SetActiveScene(scene);
        var root=new GameObject("Complete level cave integration");root.SetActive(false);
        var course=root.AddComponent<MovementCourse>();course.LevelOne=true;root.SetActive(true);course.StartDescentPreview();
        VerifyShoreRegistration(course);
        var cat=course.Cat;cat.GetComponent<CatInput>().enabled=false;cat.GetComponent<Rigidbody2D>().interpolation=RigidbodyInterpolation2D.None;
        var follow=root.GetComponentInChildren<CatCamera>();var view=follow.GetComponent<Camera>();
        var movers=root.GetComponentsInChildren<MovingSupport>();foreach(var mover in movers)mover.SendMessage("Start");
        var scenery=root.GetComponentInChildren<SceneryRenderer>();var cave=root.GetComponentInChildren<CaveEntryStudy>();
        var upper=MovementCourse.AscentPlatforms[15];var outer=MovementCourse.AscentPlatforms[35];
        var samples=new[]{CaveRoute.Entrance,CaveRoute.ToCourse(new Vector2(6.5f,-2.74f)),CaveRoute.ToCourse(new Vector2(44,-12.21f)),
            CaveRoute.ToCourse(new Vector2(98.7f,-10.7f)),CaveRoute.ToCourse(new Vector2(100,-17.04f)),
            CaveRoute.ToCourse(new Vector2(190,-7.39f)),CaveRoute.ToCourse(new Vector2(233.5f,4.165f)),CaveRoute.Rejoin,
            CaveRoute.Approach,CaveRoute.ToCourse(new Vector2(78.5f,-12.79f)),CaveRoute.ToCourse(new Vector2(207,-6.14f)),
            CaveRoute.ToCourse(new Vector2(215,-3.335f)),new Vector2(upper.x,upper.y+1.27f),
            CaveRoute.ToCourse(new Vector2(32,-9.49f)),CaveRoute.ToCourse(new Vector2(109,-9.25f)),new Vector2(outer.x,outer.y+1.27f)};
        bool recording=false;int motionTick=0;string motionDirectory="motion";
        if(capture)Directory.CreateDirectory("artifacts/integrated-cave-review/motion");
        void Tick(){foreach(var mover in movers)mover.SendMessage("FixedUpdate");cat.SendMessage("FixedUpdate");scene.GetPhysicsScene2D().Simulate(.02f);course.SendMessage("Update");follow.Follow(.02f);scenery.SendMessage("LateUpdate");cave.AdvanceVisuals(.02f);
            if(capture&&recording){cat.GetComponentInChildren<MikaSpritePresentation>().RenderPose(.02f);scenery.SetWaterTime(motionTick*.02f);
                if(motionTick%5==0)Save(view,"artifacts/integrated-cave-review/"+motionDirectory+"/frame-"+(motionTick/5).ToString("D4")+".png",640);motionTick++;}
            Require(Mathf.Abs(view.transform.position.z+10)<.001f&&view.cullingMask== -1,"Camera depth or layer visibility changed during traversal.");}
        void RecordSection(string name){motionDirectory=name;motionTick=0;if(capture)Directory.CreateDirectory("artifacts/integrated-cave-review/"+name);recording=true;}
        void StopSection(){recording=false;if(capture)File.WriteAllText("artifacts/integrated-cave-review/"+motionDirectory+"/frames.txt",((motionTick+4)/5).ToString());}
        var renderers=root.GetComponentsInChildren<Renderer>();var active=new bool[renderers.Length];
        for(int i=0;i<active.Length;i++)active[i]=renderers[i].enabled;
        if(capture)for(int i=0;i<samples.Length;i++) {
            if(i==12||i==15)course.StartAscentPreview();
            cat.RespawnAt(samples[i]);if(i==11)cat.Motion.SetMove(-.1f);for(int t=0;t<120;t++)Tick();
            cat.GetComponentInChildren<MikaSpritePresentation>().RenderPose(.25f);follow.Snap();follow.Follow(.02f);
            Debug.Log("Cave view "+i+" cat="+cat.transform.localPosition+" grounded="+cat.Grounded+" camera="+view.transform.position+" guide="+follow.PassageHeightGuide(cat.transform.localPosition)+" mask="+view.cullingMask);
            Save(view,"artifacts/integrated-cave-review/view-"+i+".png");
            Save(view,"artifacts/integrated-cave-review/view-"+i+"-640.png",640);
            Save(view,"artifacts/integrated-cave-review/view-"+i+"-gray.png",640,true);
        }
        course.StartDescentPreview();for(int i=0;i<15;i++)Tick();float seconds=0;
        void Reach(Vector3 point) {
            var target=CaveRoute.ToCourse(new Vector2(point.x,point.y+.46f));bool jumped=false,reached=false;
            for(int tick=0;tick<500;tick++) {
                var p=(Vector2)cat.transform.localPosition;
                if(cat.Grounded&&Mathf.Abs(p.x-target.x)<.3f&&Mathf.Abs(p.y-target.y)<.25f){reached=true;cat.Motion.SetMove(0);for(int j=0;j<8;j++){Tick();seconds+=.02f;}break;}
                cat.Motion.SetMove(Mathf.Clamp((target.x-p.x)*3,-1,1));
                if(point.z>0&&!jumped&&cat.Grounded){cat.PressJump();jumped=true;}
                Tick();seconds+=.02f;
                if(Vector2.Distance(cat.transform.localPosition,CaveRoute.Entrance)>400)throw new System.InvalidOperationException("Checkpoint recovery during cave traversal at "+point);
            }
            Debug.Log("Integrated cave target "+point+" actual "+cat.transform.localPosition+" reached="+reached+" recording="+motionDirectory+" frame="+motionTick/5);
            if(!reached)throw new System.InvalidOperationException("Cave target unreachable: "+point+" actual "+cat.transform.localPosition);
        }
        recording=true;foreach(var point in ApproachPath())Reach(point);StopSection();float directSeconds=seconds;
        Require(course.Run.Checkpoint==1,"The cave must not replace the main checkpoint.");
        Require(CaveRoute.Rejoin.x-CaveRoute.Entrance.x>230,"The cave must advance along the level.");
        Require(view.transform.position.z== -10&&view.cullingMask== -1,"The cave must retain the fixed camera plane and visibility mask.");
        // Re-enter the same permanent mouth by descending the outdoor shore and its approach.
        var exit=CaveRoute.ReturnFloors;
        RecordSection("exit-return");
        for(int i=exit.Length-2;i>=8;i--){var p=exit[i];Reach(new Vector3(p.x+p.z/2,p.y,0));}
        StopSection();
        // The central miss lands locally, then a separate shaft returns to its approach.
        RecordSection("recovery");cat.RespawnAt(CaveRoute.ToCourse(new Vector2(101.5f,-10.65f)));for(int i=0;i<100;i++)Tick();
        Require(cat.Grounded&&Mathf.Abs(cat.transform.localPosition.y-(-17.5f+.045f+.51f))<.2f,"Central fall missed its local catch.");
        foreach(var node in new[]{new Vector3(90.6f,-17.5f,0),new Vector3(87,-16.1f,1),new Vector3(85.6f,-16.1f,0),
            new Vector3(82.5f,-14.7f,1),new Vector3(81.6f,-14.7f,0),new Vector3(78.5f,-13.3f,1),new Vector3(77.6f,-13.3f,0),
            new Vector3(74.5f,-11.9f,1),new Vector3(77.1f,-10.3f,1),new Vector3(80.5f,-9.1f,1),new Vector3(98.75f,-11.16f,1)})Reach(node);
        Require(-17.4f<view.transform.position.y-view.orthographicSize,"The catch must be outside the takeoff camera.");
        StopSection();
        // Reverse all entrance ledges using the same real collision and jump input.
        RecordSection("entry-return");cat.RespawnAt(CaveRoute.ToCourse(new Vector2(12,-9.49f)));for(int i=0;i<15;i++)Tick();
        foreach(var node in new[]{new Vector3(10.3f,-8,1),new Vector3(9,-6.4f,1),new Vector3(7.8f,-4.8f,1),
            new Vector3(6.5f,-3.2f,1),new Vector3(5,-1.6f,1),new Vector3(2.6f,-.045f,1)})Reach(node);
        StopSection();
        cat.RespawnAt(CaveRoute.ToCourse(new Vector2(44,-12.16f)));for(int i=0;i<15;i++)Tick();
        course.TogglePause();var paused=cat.transform.position;for(int i=0;i<20;i++)Tick();
        Require(cat.transform.position==paused,"Pause must freeze the cave player.");course.TogglePause();
        course.RetryCheckpoint();Require(Vector2.Distance(cat.transform.localPosition,MovementCourse.LevelOneCheckpointSpawns[0])<.01f,"Retry must use the main checkpoint.");
        foreach(float x in new[]{28,55,95,150}) {
            cat.RespawnAt(CaveRoute.ToCourse(new Vector2(x,-2.9f)));cat.Launch(new Vector2(0,-20));
            for(int i=0;i<10;i++)Tick();
            Require(cat.transform.localPosition.x<CaveRoute.Origin-20,"Main water allowed a fall through to the cave.");
        }
        for(int i=0;i<active.Length;i++)Require(renderers[i].enabled==active[i],"Cave travel toggled a scenery renderer.");
        course.RestartRun();Require(course.Run.Checkpoint==0&&cat.transform.localPosition.x<0,"Restart must return to the beginning.");
        if(capture)File.WriteAllText("artifacts/integrated-cave-review/physics.txt","Complete integrated route reached later main bank. Direct scripted seconds: "+directSeconds+". Not a human timing. Catch return, reverse entrance, water falls, pause, retry, restart and renderer permanence also verified.\n");
        Object.DestroyImmediate(root);
    }
    static void Save(Camera view,string path,int width=960,bool grayscale=false)
    {
        int height=width*9/16;var target=new RenderTexture(width,height,24);target.Create();var old=view.targetTexture;var oldActive=RenderTexture.active;var oldRect=view.rect;
        var image=new Texture2D(width,height,TextureFormat.RGB24,false);
        try{view.rect=new Rect(0,0,1,1);view.targetTexture=target;view.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,width,height),0,0);
            if(grayscale){var pixels=image.GetPixels32();for(int i=0;i<pixels.Length;i++){byte value=(byte)(pixels[i].r*.2126f+pixels[i].g*.7152f+pixels[i].b*.0722f);pixels[i]=new Color32(value,value,value,255);}image.SetPixels32(pixels);}
            image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}
        finally{view.rect=oldRect;view.targetTexture=old;RenderTexture.active=oldActive;Object.DestroyImmediate(image);target.Release();Object.DestroyImmediate(target);}
    }
}
