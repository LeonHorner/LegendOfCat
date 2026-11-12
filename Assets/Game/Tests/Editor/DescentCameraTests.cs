using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class DescentCameraTests
{
    GameObject root;
    static Type Runtime(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
    static void Call(object value,string method,params object[] args)=>value.GetType().GetMethod(method,
        System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(value,args);
    static void Place(Component cat,Component follow,Vector2 point) { Call(cat,"RespawnAt",point);Call(follow,"Snap");Call(follow,"Follow",.02f); }
    [UnityTest]
    public IEnumerator ContinuousCameraSeparatesMainRouteAndDoesNotAdvertiseTheCatch()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        root=new GameObject("Dry descent camera checks");root.SetActive(false);
        var course=root.AddComponent(Runtime("MovementCourse"));course.GetType().GetField("LevelOne").SetValue(course,true);root.SetActive(true);
        var cat=root.GetComponentInChildren(Runtime("CatController"));
        var follow=root.GetComponentInChildren(Runtime("CatCamera"));var camera=follow.GetComponent<Camera>();
        var route=Runtime("UpperRoute");
        var steps=(Vector3[])route.GetField("Platforms").GetValue(null);
        var catches=(Vector3[])route.GetField("Recoveries").GetValue(null);
        var main=(Vector3[])course.GetType().GetField("CompletePlatforms").GetValue(null);
        var renderers=root.GetComponentsInChildren<Renderer>();var enabled=renderers.Select(r=>r.enabled).ToArray();
        int risk=(int)route.GetField("RiskIndex").GetValue(null),room=(int)route.GetField("LookoutIndex").GetValue(null);
        Place(cat,follow,new Vector2(steps[risk-1].x,steps[risk-1].y+1.22f));
        Assert.That(catches[0].y+.95f,Is.LessThan(camera.transform.position.y-camera.orthographicSize),"Catch including its small grass rim must be below the takeoff view.");
        float takeoffCameraY = camera.transform.position.y;
        cat.transform.position += Vector3.up * 1.8f;
        Call(follow,"Follow",.02f);
        Assert.That(camera.transform.position.y,Is.EqualTo(takeoffCameraY).Within(.01f),"Ordinary jump motion should stay within the camera dead zone.");
        foreach(int index in new[]{risk-1,room}) {
            Place(cat,follow,new Vector2(steps[index].x,steps[index].y+1.22f));
            float top=camera.transform.position.y+camera.orthographicSize,half=camera.orthographicSize*16/9;
            foreach(var platform in main)
                if(platform.x+platform.z/2>=camera.transform.position.x-half&&platform.x-platform.z/2<=camera.transform.position.x+half)
                    Assert.That(platform.y+.75f,Is.GreaterThan(top),"Main route still visible at descent step "+index);
            Assert.That(camera.orthographicSize,Is.EqualTo(5.5f));
        }
        Place(cat,follow,new Vector2(catches[0].x,catches[0].y+1.22f));
        Assert.That(catches[0].y+.75f,Is.InRange(camera.transform.position.y-camera.orthographicSize,camera.transform.position.y+camera.orthographicSize));
        Place(cat,follow,(Vector2)route.GetProperty("Entrance").GetValue(null));
        for(int i=0;i<renderers.Length;i++) Assert.That(renderers[i].enabled,Is.EqualTo(enabled[i]),"Renderer changed on entry/return: "+renderers[i].name);
        yield return new ExitPlayMode();
    }
    [UnityTearDown]
    public IEnumerator Cleanup() { if(Application.isPlaying){if(root)UnityEngine.Object.Destroy(root);yield return null;yield return new ExitPlayMode();} }
}
