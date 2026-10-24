using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Real Resources prefabs, controller, sprite adapter and isolated 2D physics.
// Reflection crosses the existing predefined-runtime/test-asmdef boundary only.
public class MikaPresentationIntegrationTests
{
    Scene scene, previous;
    GameObject course;
    Component controller, presentation;
    Rigidbody2D body;
    BoxCollider2D shape;
    SpriteRenderer renderer;
    object motion, animation;
    Sprite[] frames;
    PhysicsScene2D physics;
    Vector2 spawn;
    static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
        .Select(a => a.GetType(name)).First(t => t != null);
    static object Read(object target, string property) => target.GetType().GetProperty(property).GetValue(target);
    void Input(string method, params object[] args) => motion.GetType().GetMethod(method).Invoke(motion, args);
    bool Grounded => (bool)Read(controller, "Grounded");
    int Frame => (int)Read(animation, "Frame");
    float Sole => shape.transform.TransformPoint(shape.offset - Vector2.up * shape.size.y * 0.5f).y;
    float Correction => presentation.transform.position.y - Sole;
    // Visible opaque paw landmarks in the unchanged original source cells.
    // These fixtures are independent of the importer's silhouette traversal.
    static readonly Vector2[] RunSupportPixels = {
        new Vector2(333, 40), new Vector2(232, 40), new Vector2(197, 39), new Vector2(40, 91)
    };
    float VisibleSupportY(Vector2 pixel)
    {
        var sprite = renderer.sprite;
        Assert.That(sprite.texture.GetPixel((int)sprite.rect.x + (int)pixel.x,
            (int)sprite.rect.y + (int)pixel.y).a, Is.GreaterThan(0.65f));
        return renderer.transform.TransformPoint(new Vector3(
            (pixel.x + 0.5f - sprite.pivot.x) / sprite.pixelsPerUnit,
            (pixel.y + 0.5f - sprite.pivot.y) / sprite.pixelsPerUnit, 0)).y;
    }
    void Render() => presentation.GetType().GetMethod("RenderPose").Invoke(presentation, new object[] { Time.fixedDeltaTime });
    void Tick(int count = 1)
    {
        for (int i = 0; i < count; i++) {
            controller.SendMessage("FixedUpdate");
            physics.Simulate(Time.fixedDeltaTime);
            // Manual playback must use the current rigidbody pose rather than render interpolation.
            Render();
            Assert.That(renderer.sprite, Is.SameAs(frames[Frame]), "Selected sprite reference must actually reach the renderer.");
        }
    }
    void Retry()
    {
        controller.GetType().GetMethod("ReturnToStart").Invoke(controller, null);
        Physics2D.SyncTransforms();
    }
    GameObject Block(string name, Vector2 position, Vector2 size, float rotation = 0)
    {
        var block = new GameObject(name);
        SceneManager.MoveGameObjectToScene(block, scene);
        block.transform.position = position;
        block.transform.rotation = Quaternion.Euler(0, 0, rotation);
        block.AddComponent<BoxCollider2D>().size = size;
        Physics2D.SyncTransforms();
        return block;
    }

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        previous = SceneManager.GetActiveScene();
        scene = SceneManager.CreateScene("Mika integration physics", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        SceneManager.SetActiveScene(scene);
        physics = scene.GetPhysicsScene2D();
        var prefab = Resources.Load<GameObject>("MovementCourse");
        Assert.That(prefab, Is.Not.Null);
        course = UnityEngine.Object.Instantiate(prefab);
        var component = course.GetComponent(Runtime("MovementCourse"));
        controller = (Component)Read(component, "Cat");
        Assert.That(controller, Is.Not.Null, "The real course must initialize its player.");
        var input = (Behaviour)controller.GetComponent(Runtime("CatInput"));
        input.enabled = false; // Feed device-independent actions, not headless focus-dependent keys.
        body = controller.GetComponent<Rigidbody2D>();
        spawn = body.position;
        body.interpolation = RigidbodyInterpolation2D.None;
        shape = controller.GetComponent<BoxCollider2D>();
        presentation = controller.GetComponentInChildren(Runtime("MikaSpritePresentation"));
        Assert.That(presentation, Is.Not.Null, "The course must load MikaOriginal rather than silently use the silhouette fallback.");
        Assert.That(presentation.GetType().GetField("controller").GetValue(presentation), Is.SameAs(controller));
        frames = (Sprite[])presentation.GetType().GetField("frames").GetValue(presentation);
        Assert.That(frames, Has.Length.EqualTo(17));
        Assert.That(frames.All(f => f != null && f.texture != null), Is.True);
        renderer = presentation.GetComponent<SpriteRenderer>();
        motion = controller.GetType().GetField("Motion").GetValue(controller);
        animation = presentation.GetType().GetField("Animation").GetValue(presentation);
        Assert.That(Read(animation, "OriginalFrames"), Is.True);
        Physics2D.SyncTransforms();
        Tick(15);
        Assert.That(Grounded, Is.True);
    }

    [UnityTest]
    public IEnumerator ActualCourseRunsBlocksTurnsAndRetries()
    {
        Vector2 start = body.position;
        Vector2 colliderSize = shape.size;
        Assert.That(renderer.sprite, Is.SameAs(frames[0]));
        Assert.That(presentation.transform.position.y, Is.EqualTo(0).Within(0.012f));
        var poses = new HashSet<int>();
        Input("SetMove", 1f);
        for (int i = 0; i < 27; i++) {
            Tick(); poses.Add(Frame);
            Assert.That(VisibleSupportY(RunSupportPixels[Frame - 4]), Is.EqualTo(0).Within(0.012f),
                $"Visible supporting paw in run frame {Frame} must meet the flat course surface.");
        }
        Assert.That(body.position.x, Is.GreaterThan(start.x + 2));
        Assert.That(poses, Is.EquivalentTo(new[] { 4, 5, 6, 7 }));
        Assert.That(presentation.transform.position.y, Is.EqualTo(0).Within(0.012f));

        var wall = Block("Blocking wall", new Vector2(body.position.x + 1, 1), new Vector2(0.25f, 2));
        Tick(25);
        float blockedX = body.position.x;
        int stoppedFrame = Frame;
        Tick(10);
        Assert.That(body.position.x, Is.EqualTo(blockedX).Within(0.002f));
        Assert.That(Frame, Is.EqualTo(stoppedFrame), "Pushing a wall must not cycle the running feet.");
        Assert.That(body.position.x + shape.size.x * 0.5f, Is.LessThan(wall.transform.position.x - 0.12f));
        Input("SetMove", -1f); Tick(8);
        Assert.That(renderer.flipX, Is.True);
        Assert.That(body.position.x, Is.LessThan(blockedX));

        Input("PressJump"); Tick(3);
        Retry();
        Assert.That(Vector2.Distance(body.position, spawn), Is.LessThan(0.001f));
        Assert.That(body.linearVelocity, Is.EqualTo(Vector2.zero));
        Assert.That(renderer.sprite, Is.SameAs(frames[0]));
        Assert.That(renderer.flipX, Is.False);
        Tick(15);
        Assert.That(body.position.x, Is.EqualTo(start.x).Within(0.001f));
        Assert.That(shape.size, Is.EqualTo(colliderSize));
        Assert.That(presentation.transform.localScale, Is.EqualTo(Vector3.one));
        Assert.That(presentation.transform.position.y, Is.EqualTo(0).Within(0.012f));
        yield break;
    }

    [UnityTest]
    public IEnumerator ActualJumpShowsApexDescentAndLandingWithoutChangingPhysics()
    {
        Vector2 start = body.position;
        Input("PressJump"); Tick();
        Assert.That(body.position.y, Is.GreaterThan(start.y));
        Assert.That(Grounded, Is.False);
        Assert.That(Frame, Is.EqualTo(10));
        bool apex = false, descent = false, landed = false;
        for (int i = 0; i < 90; i++) {
            Tick();
            if (!Grounded && Frame == 16) apex = true;
            if (!Grounded && Frame == 11) descent = true;
            Assert.That(Mathf.Abs(Correction), Is.LessThanOrEqualTo(0.0401f));
            if (Grounded) { landed = true; Assert.That(Frame, Is.EqualTo(12)); break; }
        }
        Assert.That(apex && descent && landed, Is.True);
        Tick(4); Assert.That(Frame, Is.EqualTo(13));
        Tick(15); Assert.That(Frame, Is.Zero);
        Assert.That(body.position.y, Is.EqualTo(start.y).Within(0.012f));
        Assert.That(presentation.transform.position.y, Is.EqualTo(0).Within(0.012f));
        yield break;
    }

    [UnityTest]
    public IEnumerator ScaledCourseKeepsTheVisibleSoleOnThePhysicalSurface()
    {
        course.transform.localScale = Vector3.one * 1.5f;
        Physics2D.SyncTransforms(); Tick(20);
        Assert.That(Grounded, Is.True);
        Assert.That(presentation.transform.position.y, Is.EqualTo(0).Within(0.012f),
            "The adapter must transform the collider's local sole rather than assume an unscaled player.");
        Assert.That(VisibleSupportY(new Vector2(252, 15)), Is.EqualTo(0).Within(0.012f));
        Vector2 position = body.position;
        Vector2 velocity = body.linearVelocity;
        Vector2 size = shape.size;
        Render();
        Assert.That(body.position, Is.EqualTo(position));
        Assert.That(body.linearVelocity, Is.EqualTo(velocity));
        Assert.That(shape.size, Is.EqualTo(size));
        yield break;
    }

    [UnityTest]
    public IEnumerator EdgeAndSlopeContactsCannotDragTheVisualAwayFromItsCollider()
    {
        // The real initial platform ends at x=5; retain overlap while approaching its edge.
        body.position = new Vector2(5.3f, 0.46f);
        body.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms(); Tick(8);
        Assert.That(Grounded, Is.True);
        Assert.That(Mathf.Abs(Correction), Is.LessThanOrEqualTo(0.0401f));
        Input("SetMove", 1f);
        bool fell = false;
        for (int i = 0; i < 20; i++) {
            float before = Correction;
            Tick();
            Assert.That(Mathf.Abs(Correction - before), Is.LessThanOrEqualTo(0.0081f), "Contact correction must not pop when support changes.");
            if (!Grounded) { fell = true; Assert.That(Frame, Is.EqualTo(11)); }
        }
        Assert.That(fell, Is.True);
        Assert.That(Correction, Is.EqualTo(0).Within(0.001f));

        Block("Sloped support", new Vector2(-12, -0.5f), new Vector2(4, 1), 20);
        Input("SetMove", 0f);
        body.position = new Vector2(-12, 1.1f); body.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
        bool slopeContact = false;
        for (int i = 0; i < 45; i++) {
            float before = Correction; Tick();
            slopeContact |= Grounded;
            Assert.That(Mathf.Abs(Correction), Is.LessThanOrEqualTo(0.0401f), "A remote slope corner must not sink the entire sprite.");
            Assert.That(Mathf.Abs(Correction - before), Is.LessThanOrEqualTo(0.0081f));
        }
        Assert.That(slopeContact, Is.True);
        yield break;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (SceneManager.GetActiveScene() == scene && previous.IsValid()) SceneManager.SetActiveScene(previous);
        if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        if (UnityEditor.EditorApplication.isPlaying) yield return new ExitPlayMode();
    }
}
