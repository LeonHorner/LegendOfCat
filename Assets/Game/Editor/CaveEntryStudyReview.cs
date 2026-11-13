using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CaveEntryStudyReview
{
    public static void Record()
    {
        BuildScenery.ImportGoldenRiverArt();
        SessionState.SetBool("CaveEntryStudyReview",true);EditorApplication.EnterPlaymode();
    }
    public static void BuildPreview(){BuildScenery.ImportGoldenRiverArt();BuildGame.BuildPreview();}
    [InitializeOnLoadMethod] static void Install()
    {
        EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;
    }
    static void Changed(PlayModeStateChange state)
    {
        if(state!=PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("CaveEntryStudyReview",false))return;
        SessionState.SetBool("CaveEntryStudyReview",false);
        int result=0;try{Capture();}catch(System.Exception e){Debug.LogException(e);result=1;}
        if(Application.isBatchMode)EditorApplication.Exit(result);else EditorApplication.ExitPlaymode();
    }
    static void Capture()
    {
        const string output="artifacts/entry-study-review";Directory.CreateDirectory(output);
        var scene=SceneManager.CreateScene("Entry study review",new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        SceneManager.SetActiveScene(scene);
        var study=new GameObject("Entry study").AddComponent<CaveEntryStudy>();
        var cat=study.Cat;cat.GetComponent<CatInput>().enabled=false;
        var body=cat.GetComponent<Rigidbody2D>();body.interpolation=RigidbodyInterpolation2D.None;
        var fixedUpdate=typeof(CatController).GetMethod("FixedUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
        var present=cat.GetComponentInChildren<MikaSpritePresentation>();
        VerifyPaintContact(study,scene);
        var poses=new[]{new Vector2(2.6f,.52f),new Vector2(5,-1.08f),new Vector2(7,-4.28f),new Vector2(12,-9.48f),new Vector2(23,-9.48f)};
        for(int i=0;i<poses.Length;i++) {
            cat.RespawnAt(poses[i]);
            for(int step=0;step<30;step++){fixedUpdate.Invoke(cat,null);scene.GetPhysicsScene2D().Simulate(.02f);}
            present.RenderPose(.25f);present.RenderPose(.25f);
            study.Follow.Snap();study.Follow.Follow(.02f);
            foreach(int width in new[]{960,640}) {
                var target=new RenderTexture(width,width*9/16,24);target.Create();
                study.View.targetTexture=target;study.View.Render();RenderTexture.active=target;
                var image=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();
                File.WriteAllBytes(output+"/pose-"+i+"-"+width+".png",image.EncodeToPNG());
                study.View.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(image);Object.DestroyImmediate(target);
            }
        }
        Debug.Log("Entry study captured five actual player views at 960x540 and 640x360.");
        VerifyMovement(study,scene);
        Object.DestroyImmediate(study.gameObject);
    }

    public static void VerifyMovement(CaveEntryStudy study,Scene scene)
    {
        var cat=study.Cat;var physics=scene.GetPhysicsScene2D();
        void Step(){cat.SendMessage("FixedUpdate");physics.Simulate(.02f);study.Follow.Follow(.02f);}
        void Settle(Vector2 position){cat.RespawnAt(position);for(int i=0;i<20;i++)Step();}
        void Require(bool condition,string message){if(!condition)throw new System.InvalidOperationException(message);}
        void Reach(Vector2 target,bool jump,float timeout=5)
        {
            bool jumped=false;var start=cat.transform.position;
            for(int i=0;i<timeout/.02f;i++) {
                var p=(Vector2)cat.transform.position;
                if(Mathf.Abs(p.x-target.x)<.3f && Mathf.Abs(p.y-target.y)<.25f && cat.Grounded){cat.Motion.SetMove(0);for(int j=0;j<10;j++)Step();return;}
                cat.Motion.SetMove(Mathf.Clamp((target.x-p.x)*3,-1,1));
                if(jump && !jumped && cat.Grounded){cat.PressJump();jumped=true;}
                Step();
                Require(Vector2.Distance(cat.transform.position,start)<45,"Study recovery or unexpected travel while reaching "+target);
            }
            throw new System.InvalidOperationException("Could not reach "+target+"; actual "+cat.transform.position+" grounded="+cat.Grounded);
        }
        Settle(new Vector2(2.6f,.52f));
        Reach(new Vector2(8.5f,.46f),true); // Normal main-path jump remains the obvious choice.
        Settle(new Vector2(2.6f,.52f));
        foreach(var p in new[]{new Vector2(5,-1.14f),new Vector2(6.5f,-2.74f),new Vector2(7.8f,-4.34f),new Vector2(9,-5.94f),new Vector2(10.3f,-7.54f),new Vector2(12,-9.54f)})Reach(p,false);
        Reach(new Vector2(17.6f,-9.54f),false);Reach(new Vector2(23,-9.54f),true);
        Require(study.View.transform.position.y+5.5f<0,"The main route is still in the enclosed passage's camera frame.");
        var before=cat.transform.position;cat.SetPaused(true);for(int i=0;i<10;i++)Step();
        Require(Vector3.Distance(before,cat.transform.position)<.001f,"Pause moved Mika.");cat.SetPaused(false);
        Settle(new Vector2(12,-9.54f));
        foreach(var p in new[]{new Vector2(10.3f,-7.54f),new Vector2(9,-5.94f),new Vector2(7.8f,-4.34f),new Vector2(6.5f,-2.74f),new Vector2(5,-1.14f),new Vector2(3,.46f)})Reach(p,true);
        cat.RespawnAt(new Vector2(20,-14.8f));Step();
        Require(Vector2.Distance(cat.transform.position,new Vector2(2.6f,.52f))<.15f,"Deep fall did not return to the entrance.");
        Require(CaveEntryStudy.CrossesRiver(new Vector2(0,-2),new Vector2(0,-4)),"River crossing is not dangerous.");
        Require(!CaveEntryStudy.CrossesRiver(new Vector2(6,-2),new Vector2(6,-4)),"The dry entrance triggered river recovery.");
        Directory.CreateDirectory("artifacts/entry-study-review");
        File.WriteAllText("artifacts/entry-study-review/physics.txt","Passed: main jump; all descending ledges; cave gap; reverse climb to entry; deep-fall reset; pause; spatial water crossing; main path outside lower camera frame.\nScripted physics review, not a human playthrough.\n");
    }

    public static void VerifyPaintContact(CaveEntryStudy study,Scene scene)
    {
        var cat=study.Cat;var body=cat.GetComponent<Rigidbody2D>();
        var presentation=cat.GetComponentInChildren<MikaSpritePresentation>();
        var sprite=cat.GetComponentInChildren<SpriteRenderer>();
        void Tick(){cat.SendMessage("FixedUpdate");scene.GetPhysicsScene2D().Simulate(.02f);}
        void Place(Vector2 p){cat.RespawnAt(p);for(int i=0;i<25;i++)Tick();presentation.RenderPose(.25f);presentation.RenderPose(.25f);}
        var painting=new Texture2D(2,2,TextureFormat.RGBA32,false);
        try {
            painting.LoadImage(File.ReadAllBytes("Assets/Game/Resources/GoldenRiver/Descent/entry-study-left-v2.png"));
            Place(new Vector2(2.6f,.52f));
            if(!cat.Grounded)throw new System.InvalidOperationException("Mika is not standing on the upper bank.");
            var frame=sprite.sprite;float lowest=float.PositiveInfinity;
            for(int y=0;y<frame.rect.height && float.IsPositiveInfinity(lowest);y++)
                for(int x=0;x<frame.rect.width;x++) if(frame.texture.GetPixel((int)frame.rect.x+x,(int)frame.rect.y+y).a>.95f) {
                    var paw=sprite.transform.TransformPoint(new Vector3((x+.5f-frame.pivot.x)/frame.pixelsPerUnit,(y+.5f-frame.pivot.y)/frame.pixelsPerUnit));
                    for(float gap=0;gap<.16f;gap+=.005f) {
                        float u=(paw.x+14)/27,v=(paw.y-gap+11)/18;
                        if(painting.GetPixelBilinear(u,v).a>.95f){lowest=Mathf.Min(lowest,gap);break;}
                    }
                }
            if(lowest>.03f)throw new System.InvalidOperationException("Upper bank visible paw-to-paint gap: "+lowest);
            // Sample the painted floor independently of its authored collider/stencil coordinates.
            painting.LoadImage(File.ReadAllBytes("Assets/Game/Resources/GoldenRiver/Descent/entry-study-floor-v1.png"));
            float paintedEdge=-1;
            for(int x=0;x<painting.width/2+100;x++) {
                float world=-1+(x+.5f)/painting.width*36;
                if(world>19)break;
                if(painting.GetPixel(x,Mathf.RoundToInt((-10.1f+19.93f)/12*painting.height)).a>.95f)paintedEdge=world;
            }
            foreach(float edge in new[]{3.84f,paintedEdge}) {
                float top=edge<10?-.045f:-10;
                // The idle rear paw lies 0.373 units behind the body centre. Once all drawn
                // feet have left the painted stone, a wider invisible shelf must not hold Mika.
                Place(new Vector2(edge-.7f,top+.52f));
                cat.RespawnAt(new Vector2(edge+.42f,top+.46f));
                Tick();Tick();
                if(cat.Grounded || body.linearVelocity.y>=-.1f)
                    throw new System.InvalidOperationException("Invisible support beyond painted edge "+edge+" at "+body.position);
                if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-entryContactFrames")>=0) {
                    Place(new Vector2(edge-.9f,top+.52f));cat.Motion.SetMove(1);
                    string prefix=edge<10?"upper":"lower";
                    for(int tick=0;tick<24;tick++) {
                        Tick();presentation.RenderPose(.02f);study.Follow.Follow(.02f);
                        if(tick%3==0)SaveContactFrame(study,"artifacts/entry-study-review/"+prefix+"-walkoff-"+tick+".png");
                    }
                }
            }
            Directory.CreateDirectory("artifacts/entry-study-review");
            File.WriteAllText("artifacts/entry-study-review/contact.txt","Upper standing paw gap <= 0.03 world units; upper and lower painted exits release Mika within two physics ticks after the last visible idle paw passes the rock. Player collider and motor unchanged.\n");
        } finally {Object.DestroyImmediate(painting);}
    }

    static void SaveContactFrame(CaveEntryStudy study,string path)
    {
        var target=new RenderTexture(960,540,24);target.Create();
        var previous=RenderTexture.active;var previousTarget=study.View.targetTexture;
        var image=new Texture2D(960,540,TextureFormat.RGB24,false);
        try {
            study.View.targetTexture=target;study.View.Render();RenderTexture.active=target;
            image.ReadPixels(new Rect(0,0,960,540),0,0);image.Apply();
            File.WriteAllBytes(path,image.EncodeToPNG());
        } finally {
            RenderTexture.active=previous;study.View.targetTexture=previousTarget;
            Object.DestroyImmediate(image);target.Release();Object.DestroyImmediate(target);
        }
    }
}
