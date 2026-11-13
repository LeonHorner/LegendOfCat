using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Static visual checks use the actual runtime course and imported paintings.
public static class GoldenRiverReview
{
    [MenuItem("LegendOfCat/Golden River/Capture variety review")]
    public static void Record()
    {
        BuildScenery.ImportGoldenRiverArt();
        SessionState.SetBool("GoldenRiverReview", true);
        EditorApplication.EnterPlaymode();
    }

    public static void ReimportPanoramas()
    {
        // A prior null-graphics import cannot initialize the 14k-wide panorama.
        foreach (string name in new[] { "near-extended-panorama-v1", "far-extended-panorama-v1" })
            AssetDatabase.ImportAsset("Assets/Game/Resources/GoldenRiver/" + name + ".png", ImportAssetOptions.ForceUpdate);
    }

    [InitializeOnLoadMethod]
    static void Install()
    {
        EditorApplication.playModeStateChanged -= StateChanged;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    static void StateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("GoldenRiverReview", false)) return;
        SessionState.SetBool("GoldenRiverReview", false);
        int result = 0;
        try { Capture(); }
        catch (System.Exception error) { Debug.LogException(error); result = 1; }
        if (Application.isBatchMode) EditorApplication.Exit(result);
        else EditorApplication.ExitPlaymode();
    }

    static void Capture()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        int outputIndex = System.Array.IndexOf(args, "-reviewOutput");
        string output = outputIndex >= 0 && outputIndex + 1 < args.Length
            ? args[outputIndex + 1]
            : "docs/design/golden-river-main-route/variety-evidence";
        Directory.CreateDirectory(output);
        long textureBytes = 0;
        var textureAudit = new System.Text.StringBuilder("Name,Width,Height,RuntimeBytes,Format\n");
        foreach (var texture in Resources.LoadAll<Texture2D>("GoldenRiver/Descent")) {
            long bytes = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture);
            textureBytes += bytes;
            textureAudit.AppendLine(texture.name + "," + texture.width + "," + texture.height + "," + bytes + "," + texture.format);
        }
        textureAudit.AppendLine("Total,,," + textureBytes);
        File.WriteAllText(output + "/descent-texture-memory.csv", textureAudit.ToString());
        var previousScene = SceneManager.GetActiveScene();
        var scene = SceneManager.CreateScene("Golden River visual review", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        SceneManager.SetActiveScene(scene);
        var root = new GameObject("Golden River review course");
        root.SetActive(false);
        var course = root.AddComponent<MovementCourse>();
        course.LevelOne = true;
        root.SetActive(true);
        var cat = course.Cat;
        cat.GetComponent<CatInput>().enabled = false;
        cat.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.None;
        var follow = root.GetComponentInChildren<CatCamera>();
        var camera = follow.GetComponent<Camera>();
        var scenery = root.GetComponentInChildren<SceneryRenderer>();
        var presentation = cat.GetComponentInChildren<MikaSpritePresentation>();
        var previousTarget = RenderTexture.active;
        var points = new[] { new Vector2(49, 1.52f), new Vector2(84, .52f), new Vector2(211, .27f), new Vector2(342, .27f), new Vector2(121, 1.52f), new Vector2(246, 1.52f), new Vector2(156, 1.52f), new Vector2(100, 1.02f), new Vector2(302.5f, -.48f), new Vector2(202, -.23f), new Vector2(-3, .52f), new Vector2(30, .52f), new Vector2(431, .52f), new Vector2(503, .52f), new Vector2(100, 1.02f), new Vector2(238, 2.27f), new Vector2(440, 1.02f), new Vector2(499, .52f), new Vector2(22, .52f), new Vector2(78, .52f), new Vector2(163, .52f), new Vector2(273, .52f), new Vector2(394, .52f), new Vector2(487, .52f), new Vector2(15.9f, .52f), new Vector2(16.1f, .52f), new Vector2(-5, .52f), new Vector2(507, .52f), new Vector2(55, 1.52f), new Vector2(138, .52f), new Vector2(235, 2.27f), new Vector2(303, -.48f), new Vector2(365, 2.27f), new Vector2(455, 1.52f) };
        var names = new[] { "suspended-steps", "watermill", "aqueduct", "willow", "watermill-edge", "aqueduct-edge", "masonry-sequence", "watermill-base", "low-root-crossing", "low-masonry-crossing", "panorama-start", "first-banks", "panorama-late", "panorama-exit", "watching-fish", "cat-king", "no-cats-sign", "cat-rock", "river-rock-0", "river-rock-1", "river-rock-2", "river-rock-3", "river-rock-4", "river-rock-5", "water-boundary-before", "water-boundary-after", "route-start-edge", "route-exit-edge", "river-rock-12", "river-rock-13", "river-rock-14", "river-rock-15", "river-rock-16", "river-rock-17" };
        if (System.Array.IndexOf(args, "-ascentReview") >= 0) {
            var ascent = MovementCourse.AscentPlatforms;
            points = new Vector2[10]; names = new string[10];
            for (int i = 0; i < 6; i++) {
                var p = ascent[new[] { 0, 10, 31, 45, 51, MovementCourse.OverlookIndex }[i]];
                points[i] = new Vector2(p.x, p.y + 1.22f); names[i] = "ascent-" + i;
            }
            points[6] = new Vector2(ascent[69].x + ascent[69].z / 2 + 1, -2);
            names[6] = "ascent-cliff-foot";
            points[7] = new Vector2(216, 0.52f); names[7] = "aqueduct-color";
            points[8] = new Vector2(342, 0.52f); names[8] = "willow-color";
            points[9] = new Vector2(ascent[20].x, ascent[20].y + 1.22f); names[9] = "landscape-join";
        }
        bool finaleReview=System.Array.IndexOf(args,"-finaleReview")>=0;
        if(finaleReview) {
            var a=FinaleRoute.Platforms;
            int[] indices={0,9,FinaleRoute.CourtyardTurn-1,FinaleRoute.CourtyardTurn,FinaleRoute.CourtyardTurn+1,
                FinaleRoute.LiftIndex-1,FinaleRoute.LiftIndex,FinaleRoute.CrownTurn-1,FinaleRoute.CrownTurn+1,
                FinaleRoute.DiagonalIndex-1,FinaleRoute.DiagonalIndex,FinaleRoute.DiagonalIndex+1,a.Length-1,FinaleRoute.CourtyardTurn+4,FinaleRoute.CrownTurn+4,FinaleRoute.TowerStart,FinaleRoute.TowerTurn-1,FinaleRoute.TowerTurn,FinaleRoute.TowerTurn+1,FinaleRoute.TowerTurn+4,FinaleRoute.TowerTurn+8};
            points=new Vector2[indices.Length];names=new string[indices.Length];
            for(int i=0;i<indices.Length;i++){var p=a[indices[i]];points[i]=new Vector2(p.x,p.y+1.22f);names[i]="finale-"+i;}
            course.StartFinalePreview();
        }
        bool checkpointReview = System.Array.IndexOf(args, "-checkpointReview") >= 0;
        if (checkpointReview) {
            var a = MovementCourse.AscentPlatforms;
            points = new[] { MovementCourse.LevelOneCheckpointSpawns[0], MovementCourse.LevelOneCheckpointSpawns[0],
                MovementCourse.LevelOneCheckpointSpawns[0], MovementCourse.LevelOneCheckpointSpawns[1],
                new Vector2(a[77].x, a[77].y + 1.22f), new Vector2(a[85].x, a[85].y + 1.22f), new Vector2(a[44].x, a[44].y + 1.22f),
                new Vector2(a[50].x + 3, a[50].y + 1.22f) };
            names = new[] { "checkpoint-first-unvisited", "checkpoint-first-active", "checkpoint-first-visited", "checkpoint-second-active", "broken-ruin-climb", "rooted-descent", "sleeping-cat-terrace", "lift-recovery-gap" };
        }
        bool upperReview = System.Array.IndexOf(args, "-upperReview") >= 0;
        if (upperReview) {
            points = new[] { CaveRoute.Entrance,CaveRoute.ToCourse(new Vector2(6.5f,-2.69f)),
                CaveRoute.ToCourse(new Vector2(98.7f,-10.65f)),CaveRoute.ToCourse(new Vector2(100,-16.99f)),
                CaveRoute.ToCourse(new Vector2(44,-12.16f)),CaveRoute.Rejoin };
            names = new[] { "descent-entrance", "descent-entry-bank", "descent-risk", "descent-catch", "descent-niche", "descent-rejoin" };
        }
        try {
            for (int index = 0; index < points.Length; index++) {
                if (checkpointReview) {
                    course.Run.Restart();
                    int state = index == 0 ? 0 : index == 1 || index > 3 ? 1 : 2;
                    if (state > 0) course.Run.TryActivateCheckpoint(state, MovementCourse.LevelOneCheckpointSpawns[state - 1], true);
                    foreach (var marker in root.GetComponentsInChildren<CheckpointMarker>()) marker.Refresh(2.5f);
                }
                cat.RespawnAt(points[index]);
                Physics2D.SyncTransforms();
                for (int step = 0; step < 20; step++) {
                    cat.SendMessage("FixedUpdate");
                    scene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
                }
                cat.transform.position = cat.GetComponent<Rigidbody2D>().position;
                presentation.RenderPose(0);
                follow.Snap();
                follow.SendMessage("LateUpdate");
                scenery.ApplyView(camera.transform.position.x, camera.transform.position.y);
                scenery.SetWaterTime(12);
                foreach (int width in new[] { 960, 640 }) {
                    int height = width * 9 / 16;
                    var target = new RenderTexture(width, height, 24);
                    var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
                    try {
                        camera.targetTexture = target;
                        camera.rect = new Rect(0, 0, 1, 1);
                        camera.Render();
                        RenderTexture.active = target;
                        pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                        pixels.Apply();
                        File.WriteAllBytes($"{output}/{names[index]}-{width}.png", pixels.EncodeToPNG());
                        if (checkpointReview && width == 960 && index <= 3) {
                            var savedPosition = camera.transform.position;
                            var savedSize = camera.orthographicSize;
                            var spawn = MovementCourse.LevelOneCheckpointSpawns[index == 3 ? 1 : 0];
                            camera.orthographicSize = 2.5f;
                            camera.transform.position = new Vector3(spawn.x - (index == 3 ? 0 : .8f), spawn.y + .43f, savedPosition.z);
                            camera.Render(); RenderTexture.active = target;
                            pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                            File.WriteAllBytes($"{output}/{names[index]}-detail.png", pixels.EncodeToPNG());
                            camera.orthographicSize = savedSize; camera.transform.position = savedPosition;
                        }
                        if (checkpointReview && width == 640 && index == 1) {
                            var savedPosition = camera.transform.position;
                            var savedSize = camera.orthographicSize;
                            var spawn = MovementCourse.LevelOneCheckpointSpawns[0];
                            camera.orthographicSize = 2.5f;
                            camera.transform.position = new Vector3(spawn.x-.8f,spawn.y+.43f,savedPosition.z);
                            course.Run.Restart();
                            foreach (var marker in root.GetComponentsInChildren<CheckpointMarker>()) marker.Refresh(0);
                            course.Run.TryActivateCheckpoint(1, MovementCourse.LevelOneCheckpointSpawns[0], true);
                            string frames = output + "/pulse";
                            Directory.CreateDirectory(frames);
                            for (int frame = 0; frame < 48; frame++) {
                                foreach (var marker in root.GetComponentsInChildren<CheckpointMarker>()) marker.Refresh(.075f);
                                camera.Render(); RenderTexture.active = target;
                                pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                                File.WriteAllBytes($"{frames}/frame-{frame:000}.png", pixels.EncodeToPNG());
                            }
                            camera.orthographicSize = savedSize; camera.transform.position = savedPosition;
                            camera.Render(); RenderTexture.active = target;
                            pixels.ReadPixels(new Rect(0,0,width,height),0,0); pixels.Apply();
                        }
                        if (width == 640 && names[index] == "watching-fish") {
                            const string frames = "Logs/watching-fish-frames";
                            Directory.CreateDirectory(frames);
                            camera.orthographicSize = 2.25f;
                            scenery.ApplyView(96, -3.05f);
                            for (int frame = 0; frame < 60; frame++) {
                                scenery.SetWaterTime(frame / 12f);
                                camera.Render();
                                RenderTexture.active = target;
                                pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                                File.WriteAllBytes($"{frames}/frame-{frame:000}.png", pixels.EncodeToPNG());
                            }
                            camera.orthographicSize = 5.5f;
                            follow.Snap(); follow.SendMessage("LateUpdate");
                            scenery.ApplyView(camera.transform.position.x, camera.transform.position.y);
                            scenery.SetWaterTime(12);
                            camera.Render(); RenderTexture.active = target;
                            pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                        }
                        if (width == 640 && names[index] == "cat-king") {
                            var oldPosition = camera.transform.position;
                            var king = scenery.transform.Find("Easter egg cat king");
                            camera.orthographicSize = 1.6f;
                            camera.transform.position = new Vector3(king.position.x, king.position.y, oldPosition.z);
                            camera.Render(); RenderTexture.active = target;
                            pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                            File.WriteAllBytes($"{output}/cat-king-detail.png", pixels.EncodeToPNG());
                            camera.orthographicSize = 5.5f; camera.transform.position = oldPosition;
                            camera.Render(); RenderTexture.active = target;
                            pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                        }
                        if (width == 640 && System.Array.IndexOf(args, "-ascentReview") >= 0 && (index == 2 || index == 4)) {
                            var saved = pixels.GetPixels();
                            var movers = root.GetComponentsInChildren<MovingSupport>();
                            foreach (var mover in movers) { mover.SendMessage("Start"); mover.ResetPhase(); }
                            string frames = output + "/motion-" + index;
                            Directory.CreateDirectory(frames);
                            for (int frame = 0; frame < 60; frame++) {
                                for (int tick = 0; tick < 4; tick++) {
                                    foreach (var mover in movers) mover.SendMessage("FixedUpdate");
                                    cat.SendMessage("FixedUpdate");
                                    scene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
                                }
                                presentation.SendMessage("LateUpdate"); follow.SendMessage("LateUpdate");
                                scenery.ApplyView(camera.transform.position.x, camera.transform.position.y);
                                scenery.SetWaterTime(12 + frame / 12.5f);
                                camera.Render(); RenderTexture.active = target;
                                pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                                File.WriteAllBytes($"{frames}/frame-{frame:000}.png", pixels.EncodeToPNG());
                            }
                            pixels.SetPixels(saved); pixels.Apply();
                        }
                        if (width == 640) {
                            var colors = pixels.GetPixels();
                            for (int i = 0; i < colors.Length; i++) {
                                float gray = colors[i].grayscale;
                                colors[i] = new Color(gray, gray, gray, 1);
                            }
                            pixels.SetPixels(colors); pixels.Apply();
                            File.WriteAllBytes($"{output}/{names[index]}-{width}-gray.png", pixels.EncodeToPNG());
                        }
                    } finally {
                        camera.targetTexture = null;
                        RenderTexture.active = previousTarget;
                        target.Release(); Object.Destroy(target); Object.Destroy(pixels);
                    }
                }
            }
            if(finaleReview && System.Array.IndexOf(args,"-finaleMotion")>=0) CaptureFinaleMotion(course,scene,camera,follow,scenery,presentation,output);
            if(upperReview && System.Array.IndexOf(args,"-stillReview")<0) CaptureUpperMotion(course,scene,camera,follow,scenery,presentation,output);
        } finally {
            RenderTexture.active = previousTarget;
            Object.Destroy(root);
            SceneManager.SetActiveScene(previousScene);
            SceneManager.UnloadSceneAsync(scene);
        }
    }
    static void CaptureUpperMotion(MovementCourse course,Scene scene,Camera camera,CatCamera follow,
        SceneryRenderer scenery,MikaSpritePresentation presentation,string output)
    {
        bool full=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-fullUpperRun")>=0;
        bool catchOnly=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-catchReview")>=0;
        string pilotName=full?"UpperLevelPhysicsPilot":"UpperRoutePhysicsPilot";
        var type=System.Array.Find(System.AppDomain.CurrentDomain.GetAssemblies(),a=>a.GetType(pilotName)!=null)?.GetType(pilotName);
        if(type==null) throw new System.InvalidOperationException("Upper route test pilot must be loaded.");
        bool mainOnly = System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-mainOnly") >= 0;
        var pilot=full ? System.Activator.CreateInstance(type,new object[]{course,!mainOnly})
            : System.Activator.CreateInstance(type,new object[]{course});
        course.RestartRun();
        if(!full) course.Cat.RespawnAt(CaveRoute.Entrance);
        if (catchOnly) {
            course.Cat.RespawnAt(CaveRoute.ToCourse(new Vector2(98.7f,-10.65f)));
        }
        var movers=course.GetComponentsInChildren<MovingSupport>();
        foreach(var mover in movers){mover.SendMessage("Start");mover.ResetPhase();}
        Physics2D.SyncTransforms();
        string frames=output+"/motion";Directory.CreateDirectory(frames);
        var target=new RenderTexture(640,360,24);var pixels=new Texture2D(640,360,TextureFormat.RGB24,false);
        camera.targetTexture=target;camera.rect=new Rect(0,0,1,1);follow.Snap();
        try {
            for(int tick=0;tick<(full?25000:5000);tick++) {
                foreach(var mover in movers)mover.SendMessage("FixedUpdate");
                if (catchOnly) {
                    float direction=1;
                    float edge=CaveRoute.Origin+101.5f;
                    course.Cat.Motion.SetMove(direction*(edge-course.Cat.transform.position.x)>.02f?direction:0);
                } else type.GetMethod("Command").Invoke(pilot,null);
                course.Cat.SendMessage("FixedUpdate");scene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
                course.Cat.transform.position=course.Cat.GetComponent<Rigidbody2D>().position;
                course.SendMessage("Update");follow.Follow(Time.fixedDeltaTime);
                course.GetComponent<DescentLight>()?.Advance(Time.fixedDeltaTime);
                if(tick%4==0) {
                    foreach(var mover in movers)mover.SendMessage("LateUpdate");
                    presentation.RenderPose(Time.fixedDeltaTime*4);
                    scenery.ApplyView(camera.transform.position.x,camera.transform.position.y);scenery.SetWaterTime(tick*Time.fixedDeltaTime);
                    camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,640,360),0,0);pixels.Apply();
                    File.WriteAllBytes($"{frames}/frame-{tick/4:00000}.png",pixels.EncodeToPNG());
                }
                bool complete = catchOnly ? tick > 150 && course.Cat.Grounded &&
                    Mathf.Abs(course.Cat.transform.position.y-(-17.5f+CaveRoute.VerticalOffset+.51f)) < .15f
                    : (bool)type.GetProperty("Complete").GetValue(pilot);
                if(complete) {
                    Debug.Log("Scripted optional loop recording completed in "+(tick*Time.fixedDeltaTime).ToString("F2")+"s; not human-input evidence.");return;
                }
            }
            throw new System.InvalidOperationException("Upper route recording stopped at "+course.Cat.transform.position);
        } finally {camera.targetTexture=null;RenderTexture.active=null;target.Release();Object.Destroy(target);Object.Destroy(pixels);}
    }

    static void CaptureFinaleMotion(MovementCourse course,Scene scene,Camera camera,CatCamera follow,
        SceneryRenderer scenery,MikaSpritePresentation presentation,string output)
    {
        var pilotType=System.Array.Find(System.AppDomain.CurrentDomain.GetAssemblies(),a=>a.GetType("FinalePhysicsPilot")!=null)?.GetType("FinalePhysicsPilot");
        if(pilotType==null) throw new System.InvalidOperationException("Editor test pilot assembly must be loaded for a scripted physics recording.");
        var pilot=System.Activator.CreateInstance(pilotType,new object[]{course});
        course.StartFinalePreview();
        var movers=course.GetComponentsInChildren<MovingSupport>();
        foreach(var mover in movers){mover.SendMessage("Start");mover.ResetPhase();}
        string frames=output+"/motion";Directory.CreateDirectory(frames);
        var target=new RenderTexture(640,360,24);var pixels=new Texture2D(640,360,TextureFormat.RGB24,false);
        camera.targetTexture=target;camera.rect=new Rect(0,0,1,1);follow.Snap();
        try {
            for(int tick=0;tick<12500;tick++) {
                foreach(var mover in movers)mover.SendMessage("FixedUpdate");
                pilotType.GetMethod("Command").Invoke(pilot,null);
                course.Cat.SendMessage("FixedUpdate");scene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
                course.Cat.transform.position=course.Cat.GetComponent<Rigidbody2D>().position;
                course.SendMessage("Update");follow.Follow(Time.fixedDeltaTime);
                if(tick%4!=0)continue;
                foreach(var mover in movers)mover.SendMessage("LateUpdate");
                presentation.RenderPose(Time.fixedDeltaTime*4);
                scenery.ApplyView(camera.transform.position.x,camera.transform.position.y);scenery.SetWaterTime(tick*Time.fixedDeltaTime);
                camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,640,360),0,0);pixels.Apply();
                File.WriteAllBytes($"{frames}/frame-{tick/4:00000}.png",pixels.EncodeToPNG());
                if(course.HasReachedExit){Debug.Log("Scripted finale recording completed in "+(tick*Time.fixedDeltaTime).ToString("F2")+"s; not human-input evidence.");return;}
            }
            throw new System.InvalidOperationException("Scripted finale recording did not reach the exit: "+pilot);
        } finally {camera.targetTexture=null;RenderTexture.active=null;target.Release();Object.Destroy(target);Object.Destroy(pixels);}
    }

}
