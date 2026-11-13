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
        Assert.That(frames, Has.Length.EqualTo(21));
        Assert.That(frames.All(f => f != null && f.texture != null), Is.True);
        renderer = presentation.GetComponent<SpriteRenderer>();
        motion = controller.GetType().GetField("Motion").GetValue(controller);
        animation = presentation.GetType().GetField("Animation").GetValue(presentation);
        Physics2D.SyncTransforms();
        Tick(15);
        Assert.That(Grounded, Is.True);
    }

    [UnityTest]
    public IEnumerator TransformedCoursesKeepPaintedContactsShoreWaterAndCameraAligned()
    {
        foreach (float scale in new[] { 0.75f, 1.5f }) {
            var placement = new GameObject("Transformed course placement");
            placement.transform.position = new Vector3(scale < 1 ? -240 : 240, 7, 0);
            placement.transform.localScale = Vector3.one * scale;
            var placed = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("MovementCourse"), placement.transform, false);
            try {
                var component = placed.GetComponent(Runtime("MovementCourse"));
                var cat = (Component)Read(component, "Cat");
                ((Behaviour)cat.GetComponent(Runtime("CatInput"))).enabled = false;
                var catBody = cat.GetComponent<Rigidbody2D>(); catBody.interpolation = RigidbodyInterpolation2D.None;
                var scenery = placed.GetComponentInChildren(Runtime("SceneryRenderer"));
                var camera = (Camera)Read(scenery, "View");
                var platforms = (Vector3[])Runtime("MovementCourse").GetField("Platforms").GetValue(null);
                void AssertContacts() {
                    Physics2D.SyncTransforms();
                    var colliders = placed.GetComponentsInChildren<BoxCollider2D>()
                        .Where(c => c.name == "Standable platform").OrderBy(c => c.transform.localPosition.x).ToArray();
                    var painted = placed.GetComponentsInChildren<MeshFilter>()
                        .Where(m => m.name == "Painted grounded cliff" || m.name == "Painted floating rock")
                        .OrderBy(m => m.transform.localPosition.x).ToArray();
                    var paint = painted[0].GetComponent<MeshRenderer>().sharedMaterial;
                    Assert.That(paint.GetFloat("_CourseOriginY"), Is.EqualTo(placed.transform.position.y).Within(0.001f));
                    Assert.That(paint.GetFloat("_CourseScale"), Is.EqualTo(placed.transform.lossyScale.y).Within(0.001f));
                    Assert.That(painted.Length, Is.EqualTo(colliders.Length));
                    for (int i = 0; i < painted.Length; i++) {
                        var left = painted[i].transform.TransformPoint(painted[i].sharedMesh.vertices[2]);
                        var right = painted[i].transform.TransformPoint(painted[i].sharedMesh.vertices[3]);
                        Assert.That(left.y, Is.EqualTo(colliders[i].bounds.max.y).Within(0.001f));
                        Assert.That(right.y, Is.EqualTo(colliders[i].bounds.max.y).Within(0.001f));
                        Assert.That((left.x + right.x) * 0.5f, Is.EqualTo(colliders[i].bounds.center.x).Within(0.001f));
                    }
                    var feet = placed.GetComponentsInChildren<MeshRenderer>().Where(r => r.name == "Painted shore footing").ToArray();
                    var indices = new[] { 0, 2, 6, 10, 13 };
                    Assert.That(feet.Length, Is.EqualTo(indices.Length));
                    for (int i = 0; i < feet.Length; i++) {
                        int index = indices[i], side = index == 0 || index == 10 ? -1 : 1;
                        var p = platforms[index];
                        var expected = placed.transform.TransformPoint(new Vector3(p.x + side * (p.z * 0.5f + 1.25f), -2.65f, 3));
                        Assert.That(Vector3.Distance(feet[i].transform.position, expected), Is.LessThan(0.001f));
                    }
                }
                void TickPlaced() {
                    Physics2D.SyncTransforms();
                    for (int i = 0; i < 35; i++) { cat.SendMessage("FixedUpdate"); physics.Simulate(Time.fixedDeltaTime); }
                }
                void ViewAt(float localX) {
                    var point = placed.transform.TransformPoint(new Vector3(localX, 1));
                    scenery.GetType().GetMethod("ApplyView").Invoke(scenery, new object[] { point.x, point.y });
                }
                Vector2 WaterUv(Vector3 anchor) {
                    var panel = placed.GetComponentsInChildren<MeshRenderer>().First(r => r.name.StartsWith("Foreground water tile")
                        && r.bounds.min.x <= anchor.x && r.bounds.max.x >= anchor.x);
                    var local = panel.transform.InverseTransformPoint(anchor);
                    return new Vector2(local.x + 0.5f, local.y + 0.5f);
                }
                AssertContacts(); TickPlaced();
                Assert.That(Read(cat, "Grounded"), Is.True);
                camera.GetComponent(Runtime("CatCamera")).SendMessage("LateUpdate");
                Assert.That(camera.orthographicSize, Is.EqualTo(5.5f * scale).Within(0.001f));
                Assert.That(camera.transform.position.y, Is.EqualTo(placed.transform.TransformPoint(Vector3.up).y).Within(0.001f));
                var layers = placed.GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("Painted depth ")).ToArray();
                ViewAt(17); var before = layers.Select(t => t.position.x).ToArray();
                ViewAt(19);
                var travel = new[] { 0.05f, 0.14f, 0.08f };
                for (int i = 0; i < layers.Length; i++)
                    Assert.That(layers[i].position.x - before[i], Is.EqualTo(2 * scale * (1 - travel[i])).Within(0.001f));
                var anchor = placed.transform.TransformPoint(new Vector3(25, -3.8f));
                ViewAt(23); var uv = WaterUv(anchor); ViewAt(25);
                Assert.That(Vector2.Distance(uv, WaterUv(anchor)), Is.LessThan(0.0001f), "Water must not swim at a transformed tile recycle.");
                var checkpoint = (Vector2)placed.transform.TransformPoint(new Vector2(9, 0.52f));
                cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { checkpoint });
                TickPlaced(); component.SendMessage("Update");
                Assert.That(Read(Read(component, "Run"), "Checkpoint"), Is.EqualTo(1));
                component.GetType().GetMethod("RetryCheckpoint").Invoke(component, null);
                Assert.That(Vector2.Distance(catBody.position, checkpoint), Is.LessThan(0.001f));
                component.GetType().GetMethod("RestartRun").Invoke(component, null);
                Assert.That(Vector2.Distance(catBody.position, placed.transform.TransformPoint(new Vector2(-3, 0.48f))), Is.LessThan(0.001f));
                placement.transform.position += new Vector3(11, -2);
                placement.transform.localScale *= 1.2f;
                ViewAt(19);
                AssertContacts();
            } finally { UnityEngine.Object.Destroy(placement); }
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator RecoveryFollowsRuntimeCourseTransformsAfterCheckpointActivation()
    {
        var component = course.GetComponent(Runtime("MovementCourse"));
        var run = Read(component, "Run");
        var checkpoints = new[] { new Vector2(9, 0.52f), new Vector2(34, 1.52f), new Vector2(65, 1.52f) };
        void AtCheckpoint(int index) {
            var world = (Vector2)course.transform.TransformPoint(checkpoints[index]);
            controller.GetType().GetMethod("RespawnAt").Invoke(controller, new object[] { world });
            Physics2D.SyncTransforms(); Tick(25);
            Assert.That(Grounded, Is.True);
            component.SendMessage("Update");
        }
        void Action(string name) => component.GetType().GetMethod(name).Invoke(component, null);
        void AssertSpawn(Vector2 local) {
            var expected = (Vector2)course.transform.TransformPoint(local);
            Assert.That(Vector2.Distance(body.position, expected), Is.LessThan(0.001f));
            Assert.That(body.linearVelocity, Is.EqualTo(Vector2.zero));
            Assert.That(body.simulated, Is.True);
            Assert.That(Read(run, "Paused"), Is.False);
        }
        AtCheckpoint(1);
        Assert.That(Read(run, "Checkpoint"), Is.EqualTo(2));
        run.GetType().GetMethod("Advance").Invoke(run, new object[] { 5f });
        float elapsed = (float)Read(run, "ElapsedSeconds");
        Action("TogglePause");
        course.transform.position = new Vector3(30, 5);
        course.transform.localScale = Vector3.one * 1.5f;
        Physics2D.SyncTransforms();
        run.GetType().GetMethod("Advance").Invoke(run, new object[] { 3f });
        Assert.That(Read(run, "ElapsedSeconds"), Is.EqualTo(elapsed));
        Action("RetryCheckpoint");
        AssertSpawn(checkpoints[1]);
        Assert.That(Read(run, "Retries"), Is.EqualTo(1));
        Assert.That(Read(run, "Checkpoint"), Is.EqualTo(2));
        Assert.That(Read(run, "ElapsedSeconds"), Is.EqualTo(elapsed));
        body.position = course.transform.TransformPoint(new Vector2(checkpoints[1].x, -8));
        controller.SendMessage("FixedUpdate");
        AssertSpawn(checkpoints[1]);
        Assert.That(Read(run, "Retries"), Is.EqualTo(2));
        AtCheckpoint(0);
        Assert.That(Read(run, "Checkpoint"), Is.EqualTo(2), "Revisiting an earlier transformed marker must not regress progression.");
        AtCheckpoint(2);
        Assert.That(Read(run, "Checkpoint"), Is.EqualTo(3), "New checkpoints must activate after transforming the course.");
        Action("TogglePause");
        course.transform.position = new Vector3(-20, -12);
        course.transform.localScale = Vector3.one * 0.75f;
        Physics2D.SyncTransforms();
        Action("RetryCheckpoint");
        AssertSpawn(checkpoints[2]);
        Assert.That(Read(run, "Retries"), Is.EqualTo(3));
        Assert.That(Read(run, "Checkpoint"), Is.EqualTo(3));
        Action("TogglePause"); Action("RestartRun");
        AssertSpawn(new Vector2(-3, 0.48f));
        Assert.That(Read(run, "Checkpoint"), Is.EqualTo(0));
        Assert.That(Read(run, "Retries"), Is.EqualTo(0));
        Assert.That(Read(run, "ElapsedSeconds"), Is.EqualTo(0f));
        Physics2D.SyncTransforms(); Tick(25);
        Assert.That(Grounded, Is.True);
        Assert.That(Read(run, "Retries"), Is.EqualTo(0), "A valid course below world y=-7 must not repeatedly fall/retry.");
        body.position = course.transform.TransformPoint(new Vector2(-3, -8));
        controller.SendMessage("FixedUpdate");
        AssertSpawn(new Vector2(-3, 0.48f));
        Assert.That(Read(run, "Checkpoint"), Is.EqualTo(0));
        Assert.That(Read(run, "Retries"), Is.EqualTo(1));
        yield break;
    }

    [UnityTest]
    public IEnumerator PaintedCourseKeepsCheckpointsFallsPauseAndFullRestart()
    {
        var component = course.GetComponent(Runtime("MovementCourse"));
        var scenery = course.GetComponentInChildren(Runtime("SceneryRenderer"));
        Assert.That(scenery, Is.Not.Null);
        Assert.That(Read(scenery, "Cat"), Is.SameAs(controller), "Scenery must reuse the checkpoint player.");
        Assert.That(course.GetComponentsInChildren(Runtime("CatController")), Has.Length.EqualTo(1));
        Assert.That(course.GetComponentsInChildren<BoxCollider2D>(), Has.Length.EqualTo(15),
            "Fourteen platforms and one player; decoration must not add collision.");
        var camera = (Camera)Read(scenery, "View");
        Assert.That(camera.orthographicSize, Is.EqualTo(5.5f));
        var water = course.GetComponentsInChildren<MeshRenderer>().First(r => r.name == "Foreground water tile 0");
        Assert.That(water.sharedMaterial.shader.isSupported, Is.True);
        Assert.That(Resources.Load<Material>("BackgroundStudy/water-material"), Is.Not.Null,
            "A Resources material must retain the water shader in standalone builds.");
        var run = Read(component, "Run");
        int expected = 0;
        foreach (var spawnPoint in new[] {
            new Vector2(9, 0.52f), new Vector2(34, 1.52f),
            new Vector2(65, 1.52f), new Vector2(83, 0.52f)
        }) {
            controller.GetType().GetMethod("RespawnAt").Invoke(controller, new object[] { spawnPoint });
            Physics2D.SyncTransforms(); Tick(15);
            Assert.That(Grounded, Is.True);
            component.SendMessage("Update");
            expected++;
            Assert.That(Read(run, "Checkpoint"), Is.EqualTo(expected));
            component.GetType().GetMethod("TogglePause").Invoke(component, null);
            Assert.That(body.simulated, Is.False);
            component.GetType().GetMethod("RetryCheckpoint").Invoke(component, null);
            Assert.That(body.simulated, Is.True);
            Assert.That(Vector2.Distance(body.position, spawnPoint), Is.LessThan(0.001f));
            Assert.That(body.linearVelocity, Is.EqualTo(Vector2.zero));
            body.position = new Vector2(19, -8);
            controller.SendMessage("FixedUpdate");
            Assert.That(Vector2.Distance(body.position, spawnPoint), Is.LessThan(0.001f),
                "Falling must restore the active checkpoint, not the art lab's opening spawn.");
            camera.GetComponent(Runtime("CatCamera")).SendMessage("LateUpdate");
            scenery.SendMessage("LateUpdate");
            Assert.That(camera.transform.position.y, Is.EqualTo(1).Within(0.001f));
        }
        component.GetType().GetMethod("RestartRun").Invoke(component, null);
        Assert.That(Read(run, "Checkpoint"), Is.EqualTo(0));
        Assert.That(Read(run, "Retries"), Is.EqualTo(0));
        Assert.That(Vector2.Distance(body.position, spawn), Is.LessThan(0.001f));
        yield break;
    }

    [UnityTest]
    public IEnumerator GroundedJumpShowsTakeoffBeforeTheNextPhysicsStep()
    {
        Input("SetMove", 1f);
        controller.GetType().GetMethod("PressJump").Invoke(controller, null);
        Render();
        Assert.That(Grounded, Is.False);
        Assert.That(Frame, Is.EqualTo(10), "The same render frame must show the takeoff drawing.");
        Tick();
        Assert.That(body.linearVelocity.x, Is.GreaterThan(1));
        Assert.That(body.linearVelocity.y, Is.GreaterThan(10));
        Input("ReleaseJump"); Tick();
        Assert.That(body.linearVelocity.y, Is.LessThan(5));
        yield break;
    }

    [UnityTest]
    public IEnumerator ExtendedCourseGapsAreTraversableAndCameraReachesTheFinish()
    {
        var platforms = (Vector3[])Runtime("MovementCourse").GetField("Platforms").GetValue(null);
        Assert.That(platforms, Has.Length.EqualTo(14));
        for (int i = 0; i < platforms.Length - 1; i++) {
            var from = platforms[i];
            var to = platforms[i + 1];
            var start = new Vector2(from.x + from.z * 0.5f - 1.5f, from.y + 1.23f);
            controller.GetType().GetMethod("RespawnAt").Invoke(controller, new object[] { start });
            Physics2D.SyncTransforms(); Tick(15);
            Input("SetMove", 1f); Tick(8);
            Input("PressJump"); Tick();
            bool landed = false;
            for (int step = 0; step < 65; step++) {
                Tick();
                if (Grounded && body.position.x >= to.x - to.z * 0.5f && body.position.x <= to.x + to.z * 0.5f) {
                    landed = true; break;
                }
            }
            Assert.That(landed, Is.True, "Gap " + i + " must work with the existing motor and held jump.");
            Input("ReleaseJump");
        }
        var camera = course.GetComponentsInChildren<Camera>().First(c => c.name == "Course camera");
        camera.GetComponent(Runtime("CatCamera")).SendMessage("LateUpdate");
        Assert.That(camera.transform.position.x, Is.GreaterThan(90), "Camera must follow beyond the old x=48 boundary.");
        Assert.That(camera.WorldToViewportPoint(body.position).x, Is.InRange(0.1f, 0.9f));
        yield break;
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
        Tick(4); Assert.That(Frame, Is.EqualTo(20));
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
            if (!Grounded) { fell = true; Assert.That(Frame, Is.EqualTo(11).Or.EqualTo(19)); }
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
