using UnityEngine;

// Three level-owned voices; wind is environmental, never driven by movement.
[DefaultExecutionOrder(100)]
public sealed class GoldenRiverAudio : MonoBehaviour
{
    MovementCourse course;
    AudioSource river, wind, landmark;
    AudioClip checkpoint;
    AudioClip[] gusts;
    readonly System.Random variation = new System.Random();
    int previousCheckpoint, previousGust;
    float windWait, gustAge = -1;
    bool wasPaused, finished;

    public void Initialize(MovementCourse owner)
    {
        course = owner;
        river = Source("River ambience", .14f);
        river.clip = Resources.Load<AudioClip>("GoldenRiverAudio/river-loop");
        river.loop = true;
        wind = Source("Passing wind", .75f);
        landmark = Source("Checkpoint sound", .7f);
        gusts = new[] { Resources.Load<AudioClip>("GoldenRiverAudio/wind-01"),
            Resources.Load<AudioClip>("GoldenRiverAudio/wind-02"),
            Resources.Load<AudioClip>("GoldenRiverAudio/wind-03"),
            Resources.Load<AudioClip>("GoldenRiverAudio/wind-04") };
        checkpoint = Resources.Load<AudioClip>("GoldenRiverAudio/checkpoint");
        course.Run.ResetTransientState += ResetSounds;
        course.ExitReached += Finish;
        ResetSounds();
    }
    AudioSource Source(string label, float volume)
    {
        var child = new GameObject(label);
        child.transform.SetParent(transform, false);
        var source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0;
        source.volume = volume;
        return source;
    }
    void ResetSounds()
    {
        StopSources();
        previousCheckpoint = course.Run.Checkpoint;
        wasPaused = finished = false;
        previousGust = -1;
        gustAge = -1;
        windWait = 4;
        if (isActiveAndEnabled && Application.isPlaying && river.clip) river.Play();
    }
    void Finish()
    {
        finished = true;
        StopSources();
        // A separate completion sound has not been selected; retain the success UI.
    }
    void LateUpdate()
    {
        if (!course) return;
        bool paused = course.Run.Paused;
        if (paused != wasPaused) {
            foreach (var source in new[] { river, wind, landmark }) {
                if (paused) source.Pause(); else source.UnPause();
            }
            wasPaused = paused;
        }
        if (paused || finished) return;
        if (course.Run.Checkpoint > previousCheckpoint && checkpoint && Application.isPlaying) {
            landmark.Stop(); landmark.clip = checkpoint; landmark.Play();
        }
        previousCheckpoint = course.Run.Checkpoint;
        float height = course.transform.InverseTransformPoint(course.Cat.transform.position).y;
        river.volume = Mathf.Lerp(.14f, .06f, Mathf.InverseLerp(0, 14, height));
        AdvanceAmbience(Time.deltaTime);
    }
    void AdvanceAmbience(float deltaSeconds)
    {
        if (course.Run.Paused || finished || !isActiveAndEnabled) return;
        if (gustAge >= 0) {
            gustAge += deltaSeconds;
            if (gustAge >= wind.clip.length) {
                wind.Stop(); gustAge = -1;
                windWait = 10 + (float)variation.NextDouble() * 14;
            }
            return;
        }
        windWait -= deltaSeconds;
        if (windWait > 0) return;
        int selected = variation.Next(previousGust < 0 ? gusts.Length : gusts.Length - 1);
        if (previousGust >= 0 && selected >= previousGust) selected++;
        previousGust = selected;
        wind.clip = gusts[selected];
        if (!wind.clip) { windWait = 8; return; }
        gustAge = 0;
        if (Application.isPlaying) wind.Play();
    }
    void StopSources()
    {
        if (river) river.Stop();
        if (wind) wind.Stop();
        if (landmark) landmark.Stop();
    }
    void OnDisable() => StopSources();
    void OnDestroy()
    {
        if (!course) return;
        course.Run.ResetTransientState -= ResetSounds;
        course.ExitReached -= Finish;
    }
}
