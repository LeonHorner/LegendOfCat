using UnityEngine;

// Course-owned sources: no global music manager or listener pause state.
[DefaultExecutionOrder(100)]
public sealed class GoldenRiverAudio : MonoBehaviour
{
    MovementCourse course;
    AudioSource river, movement, landmark;
    AudioClip jump, land, checkpoint, complete;
    bool previousGrounded, hasStood, wasPaused, finished;
    int previousCheckpoint;

    public void Initialize(MovementCourse owner)
    {
        course = owner;
        river = Source("River ambience", .22f);
        river.clip = Resources.Load<AudioClip>("GoldenRiverAudio/river");
        river.loop = true;
        movement = Source("Movement sounds", .35f);
        landmark = Source("Landmark sounds", .45f);
        jump = Resources.Load<AudioClip>("GoldenRiverAudio/jump");
        land = Resources.Load<AudioClip>("GoldenRiverAudio/land");
        checkpoint = Resources.Load<AudioClip>("GoldenRiverAudio/checkpoint");
        complete = Resources.Load<AudioClip>("GoldenRiverAudio/complete");
        course.Run.ResetTransientState += ResetSounds;
        course.ExitReached += Finish;
        course.Cat.ReturnedToStart += ResetContact;
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

    void ResetContact() { previousGrounded = hasStood = false; }

    void ResetSounds()
    {
        river.Stop(); movement.Stop(); landmark.Stop();
        ResetContact();
        previousCheckpoint = course.Run.Checkpoint;
        wasPaused = finished = false;
        if (isActiveAndEnabled && Application.isPlaying) river.Play();
    }

    void Finish()
    {
        if (finished) return;
        finished = true;
        river.Stop(); movement.Stop(); landmark.Stop();
        Play(landmark, complete);
    }

    static void Play(AudioSource source, AudioClip clip)
    {
        // One movement voice and one landmark voice bound overlap without stacking.
        if (!clip || !Application.isPlaying) return;
        source.Stop(); source.clip = clip; source.Play();
    }

    void LateUpdate()
    {
        if (!course) return;
        bool paused = course.Run.Paused;
        if (paused != wasPaused) {
            foreach (var source in new[] { river, movement, landmark }) {
                if (paused) source.Pause(); else source.UnPause();
            }
            wasPaused = paused;
        }
        if (paused || finished) return;
        if (course.Run.Checkpoint > previousCheckpoint) Play(landmark, checkpoint);
        previousCheckpoint = course.Run.Checkpoint;
        if (course.Cat.Recovering) { ResetContact(); return; }
        bool grounded = course.Cat.Grounded;
        if (hasStood && previousGrounded && !grounded && course.Cat.PresentationVelocity.y > .1f)
            Play(movement, jump);
        else if (hasStood && !previousGrounded && grounded) Play(movement, land);
        hasStood |= grounded;
        previousGrounded = grounded;
        // Keep the distant river audible above the valley without dominating the ascent.
        float height = course.transform.InverseTransformPoint(course.Cat.transform.position).y;
        river.volume = Mathf.Lerp(.22f, .07f, Mathf.InverseLerp(0, 14, height));
    }

    void OnDisable()
    {
        if (river) river.Stop();
        if (movement) movement.Stop();
        if (landmark) landmark.Stop();
    }

    void OnDestroy()
    {
        if (!course) return;
        course.Run.ResetTransientState -= ResetSounds;
        course.ExitReached -= Finish;
        if (course.Cat) course.Cat.ReturnedToStart -= ResetContact;
    }
}
