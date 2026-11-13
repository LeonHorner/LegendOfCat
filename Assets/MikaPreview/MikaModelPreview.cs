using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections.Generic;

[DefaultExecutionOrder(100)]
public sealed class MikaModelPreview : MonoBehaviour
{
    MikaSpritePresentation sprite;
    GameObject model;
    PlayableGraph graph;
    AnimationPlayableOutput output;
    AnimationClipPlayable playing;
    string current;
    int previousFrame;
    int facing = 1;
    int turnFrom = 1;
    float scale;
    readonly Dictionary<string, AnimationClip> clips = new();

    SpriteRenderer original;
    Material material;

    void Start() => Attach(GetComponent<MikaSpritePresentation>());

    void LateUpdate()
    {
        if (!model || !sprite || !sprite.controller) return;
        model.SetActive(original.enabled);
        if (!original.enabled) return;
        var cat = sprite.controller;
        var velocity = cat.PresentationVelocity;
        var state = sprite.Animation;
        if (state.Facing != facing)
        {
            turnFrom = facing;
            facing = state.Facing;
        }
        var position = sprite.transform.position;
        position.z -= .15f;
        model.transform.position = position;
        model.transform.localScale = Vector3.one * scale;
        // The imported model faces left at zero yaw.
        float yaw = facing > 0 ? 180f : 0f;
        if (state.Current == MikaAnimation.Pose.Turn)
            yaw = Mathf.Lerp(turnFrom > 0 ? 180f : 0f, yaw, Mathf.SmoothStep(0, 1, state.PoseElapsed / .10f));
        model.transform.localRotation = Quaternion.Euler(0, yaw, 0);

        string pose = SelectClip(state.Current, state.Frame, Mathf.Abs(velocity.x));
        if (current != pose || previousFrame == 16 && state.Frame != 16)
        {
            if (playing.IsValid()) playing.Destroy();
            playing = AnimationClipPlayable.Create(graph, clips[pose]);
            output.SetSourcePlayable(playing);
            current = pose;
        }
        previousFrame = state.Frame;
        float duration = playing.GetAnimationClip().length;
        double? sample = state.Frame switch
        {
            16 => duration * .5,
            19 => duration,
            _ when state.Current == MikaAnimation.Pose.Turn => duration * Mathf.Clamp01(state.PoseElapsed / .10f),
            _ when state.Current == MikaAnimation.Pose.Landing => duration * Mathf.Clamp01(state.PoseElapsed / MikaAnimation.LandingDuration),
            _ => null
        };
        bool loop = state.Current == MikaAnimation.Pose.Run || state.Current == MikaAnimation.Pose.Idle;
        playing.SetSpeed(cat.Paused || sample.HasValue ? 0 : 1);
        if (sample.HasValue) playing.SetTime(sample.Value);
        else if (playing.GetTime() >= duration)
            playing.SetTime(loop ? playing.GetTime() % duration : duration);
    }

    public static string SelectClip(MikaAnimation.Pose pose, int frame, float speed)
    {
        if (frame == 16) return "Airborne";
        return pose switch
        {
            MikaAnimation.Pose.Run => speed < 2.5f ? "Walk" : "Run",
            MikaAnimation.Pose.Turn => "Stop",
            MikaAnimation.Pose.Jump => frame == 10 ? "Takeoff" : "Airborne",
            MikaAnimation.Pose.Fall => "Fall",
            MikaAnimation.Pose.Landing => "Land",
            _ => "IdleRelaxed"
        };
    }

    void Attach(MikaSpritePresentation target)
    {
        var prefab = Resources.Load<GameObject>("mika-rig");
        if (!prefab || !target) return;
        foreach (var clip in Resources.LoadAll<AnimationClip>("mika-rig"))
        {
            var name = clip.name;
            clips[name.Substring(name.LastIndexOf('|') + 1)] = clip;
        }
        string[] required = { "IdleRelaxed", "Walk", "Run", "Stop", "Takeoff", "Airborne", "Fall", "Land" };
        if (System.Array.Exists(required, name => !clips.ContainsKey(name)))
        {
            Debug.LogError("Mika preview clips missing: " + string.Join(", ", clips.Keys));
            enabled = false;
            return;
        }
        sprite = target;
        original = sprite.GetComponent<SpriteRenderer>();
        model = Instantiate(prefab, sprite.transform.parent);
        model.name = "Mika animated model";
        var renderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        material = new Material(Resources.Load<Shader>("MikaModelUnlit"));
        material.mainTexture = Resources.Load<Texture2D>("mika-basecolor");
        foreach (var renderer in renderers) renderer.sharedMaterial = material;
        scale = original.bounds.size.y / Mathf.Max(.01f, renderers[0].bounds.size.y);
        facing = turnFrom = target.Animation.Facing;
        var animator = model.GetComponent<Animator>();
        if (!animator) animator = model.AddComponent<Animator>();
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        graph = PlayableGraph.Create("Mika preview clips");
        output = AnimationPlayableOutput.Create(graph, "Mika", animator);
        graph.Play();
        original.forceRenderingOff = true;
        Debug.Log("Mika model preview attached with " + clips.Count + " clips.");
    }

    void OnDestroy()
    {
        if (graph.IsValid()) graph.Destroy();
        if (original) original.forceRenderingOff = false;
        if (model) Destroy(model);
        if (material) Destroy(material);
    }
}
