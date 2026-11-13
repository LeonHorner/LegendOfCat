using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections.Generic;

public sealed class MikaModelPreview : MonoBehaviour
{
    MikaSpritePresentation sprite;
    GameObject model;
    PlayableGraph graph;
    AnimationPlayableOutput output;
    AnimationClipPlayable playing;
    string current;
    float direction;
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
        var pose = cat.Grounded
            ? Mathf.Abs(velocity.x) < .1f ? "IdleRelaxed" : Mathf.Abs(velocity.x) < 2.5f ? "Walk" : "Run"
            : velocity.y > 0 ? "Airborne" : "Fall";
        if (Mathf.Abs(velocity.x) > .1f) direction = Mathf.Sign(velocity.x);
        var position = sprite.transform.position;
        position.z -= .15f;
        model.transform.position = position;
        // The imported model faces left before mirroring.
        model.transform.localScale = new Vector3(-direction * scale, scale, scale);
        if (current != pose && clips.TryGetValue(pose, out var clip))
        {
            if (playing.IsValid()) playing.Destroy();
            playing = AnimationClipPlayable.Create(graph, clip);
            output.SetSourcePlayable(playing);
            current = pose;
        }
        if (playing.IsValid())
        {
            var activeClip = playing.GetAnimationClip();
            if (playing.GetTime() >= activeClip.length)
                playing.SetTime(cat.Grounded ? playing.GetTime() % activeClip.length : activeClip.length);
            playing.SetSpeed(cat.Paused ? 0 : 1);
        }
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
        if (!clips.ContainsKey("Run") || !clips.ContainsKey("Walk") || !clips.ContainsKey("IdleRelaxed"))
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
        direction = 1;
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
