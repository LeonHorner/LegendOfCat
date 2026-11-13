using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class CaveEntryStudyTests
{
    static Type Runtime(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);

    [UnityTest]
    public IEnumerator EntryAndFirstPassageSupportForwardReversePauseAndRecovery()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        var scene=SceneManager.CreateScene("Isolated entrance physics",new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        SceneManager.SetActiveScene(scene);
        var root=new GameObject("Entrance under test");
        try {
            var study=root.AddComponent(Runtime("CaveEntryStudy"));
            var cat=(Component)study.GetType().GetProperty("Cat").GetValue(study);
            ((Behaviour)cat.GetComponent(Runtime("CatInput"))).enabled=false;
            var renderers=root.GetComponentsInChildren<Renderer>();
            var visible=renderers.Select(r=>r.enabled).ToArray();
            Runtime("CaveEntryStudyReview").GetMethod("VerifyMovement").Invoke(null,new object[]{study,scene});
            Runtime("CaveEntryStudyReview").GetMethod("VerifyPaintContact").Invoke(null,new object[]{study,scene});
            Assert.That(root.GetComponentsInChildren<Renderer>().Select(r=>r.GetInstanceID()),Is.EqualTo(renderers.Select(r=>r.GetInstanceID())));
            Assert.That(renderers.Select(r=>r.enabled),Is.EqualTo(visible),"Traversal must not toggle scenery visibility.");
        } finally {UnityEngine.Object.Destroy(root);}
        yield return new ExitPlayMode();
    }
}
