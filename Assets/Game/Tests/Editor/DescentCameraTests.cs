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
    [UnityTest]
    public IEnumerator CaveCameraHidesMainRouteAndCatchWithoutChangingZoomOrRenderers()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        root=new GameObject("Connected cave camera");root.SetActive(false);
        var course=root.AddComponent(Runtime("MovementCourse"));course.GetType().GetField("LevelOne").SetValue(course,true);root.SetActive(true);
        var cat=root.GetComponentInChildren(Runtime("CatController"));((Behaviour)cat.GetComponent(Runtime("CatInput"))).enabled=false;
        var follow=root.GetComponentInChildren(Runtime("CatCamera"));var camera=follow.GetComponent<Camera>();
        var scenery=root.GetComponentInChildren(Runtime("SceneryRenderer"));
        var route=Runtime("CaveRoute");float origin=(float)route.GetProperty("Origin").GetValue(null);
        var fissure=root.GetComponentsInChildren<MeshRenderer>().Single(r=>r.name=="Sunlit recessed fissure wall");
        Assert.That(fissure.bounds.max.y,Is.LessThan(-4.3f),"The cave backdrop must not cover the main route above the riverbed.");
        var renderers=root.GetComponentsInChildren<Renderer>();var enabled=renderers.Select(r=>r.enabled).ToArray();
        int mask=camera.cullingMask;var layers=renderers.Select(r=>r.gameObject.layer).ToArray();
        var mode=Physics2D.simulationMode;Physics2D.simulationMode=SimulationMode2D.Script;
        try {
            foreach(var p in new[]{new Vector2(98.7f,-10.65f),new Vector2(44,-12.16f),new Vector2(100,-16.99f)}) {
                Call(cat,"RespawnAt",p+new Vector2(origin,.045f));Physics2D.SyncTransforms();
                for(int i=0;i<200;i++){Call(cat,"FixedUpdate");Physics2D.Simulate(.02f);Call(follow,"Follow",.02f);Call(scenery,"LateUpdate");}
                float bottom=camera.transform.position.y-camera.orthographicSize,top=camera.transform.position.y+camera.orthographicSize;
                Assert.That(top,Is.LessThan(-3.2f),"The main route and river must be above the cave view.");
                Assert.That(cat.transform.position.y,Is.InRange(bottom,top));
                Assert.That(camera.orthographicSize,Is.EqualTo(5.5f));
                Assert.That(camera.transform.position.z,Is.EqualTo(-10).Within(.001f));
                Assert.That(camera.cullingMask,Is.EqualTo(mask));
                if(p.x==98.7f)Assert.That(-17.4f,Is.LessThan(bottom),"Concealed catch is visible before takeoff.");
                if(p.x==100)Assert.That(-17.4f,Is.InRange(bottom,top),"Catch must become visible during a fall.");
            }
            Assert.That(renderers.Select(r=>r.enabled),Is.EqualTo(enabled));
            Assert.That(renderers.Select(r=>r.gameObject.layer),Is.EqualTo(layers));
        } finally {Physics2D.simulationMode=mode;}
        yield return new ExitPlayMode();
    }
    [UnityTearDown] public IEnumerator Cleanup(){if(Application.isPlaying){if(root)UnityEngine.Object.Destroy(root);yield return null;yield return new ExitPlayMode();}}
}
