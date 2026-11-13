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
}
