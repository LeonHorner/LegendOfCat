using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class LevelProgress
{
    readonly Vector2[] route;
    readonly float[] distances;
    readonly float[] checkpoints;
    int segment;
    float distance;
    Vector2 previousPosition;
    readonly Texture2D fill, head;
    readonly SpriteRenderer[] checkpointVisuals;
    readonly Rect[] checkpointUvs;
    public const float Height = 58;
    public const float Width = 370;

    public bool Visible { get; set; } = true;
    public bool HasArtwork => fill && head;
    public string Artwork { get; }
    public int CheckpointCount => checkpoints.Length;
    public float? CheckpointFraction(int activeCheckpoint) =>
        activeCheckpoint > 0 && activeCheckpoint <= checkpoints.Length
            ? checkpoints[activeCheckpoint - 1] / distances[distances.Length - 1]
            : (float?)null;
    public float Fraction => distance / distances[distances.Length - 1];

    public LevelProgress(Vector2[] route, int[] checkpointWaypoints, string artwork)
    {
        if (route.Length < 2) throw new ArgumentException("A progress route needs a start and finish.");
        this.route = (Vector2[])route.Clone();
        distances = new float[route.Length];
        for (int i = 1; i < route.Length; i++) {
            float length = Vector2.Distance(route[i - 1], route[i]);
            if (!(length > 0) || float.IsInfinity(length))
                throw new ArgumentException("Progress route segments must have finite, positive length.");
            distances[i] = distances[i - 1] + length;
        }
        checkpoints = new float[checkpointWaypoints.Length];
        int previous = 0;
        for (int i = 0; i < checkpoints.Length; i++) {
            int waypoint = checkpointWaypoints[i];
            if (waypoint <= previous || waypoint >= route.Length - 1)
                throw new ArgumentException("Checkpoints must follow route order between start and finish.");
            checkpoints[i] = distances[waypoint];
            previous = waypoint;
        }
        checkpointVisuals = new SpriteRenderer[checkpoints.Length];
        checkpointUvs = new Rect[checkpoints.Length];
        Artwork = artwork;
        string prefix = "LevelProgress/" + artwork + "-";
        fill = Resources.Load<Texture2D>(prefix + "fill");
        head = Resources.Load<Texture2D>("LevelProgress/mika-progress-head");
        previousPosition = route[0];
    }

    public void Reset(Vector2 position, int activeCheckpoint, bool preview = false)
    {
        float anchor = activeCheckpoint > 0 ? checkpoints[activeCheckpoint - 1] : 0;
        segment = 0;
        while (segment + 1 < route.Length - 1 && distances[segment + 1] <= anchor) segment++;
        distance = anchor;
        previousPosition = position;
        if (preview) Project(position, distances[distances.Length - 1]);
    }

    public void UpdatePosition(Vector2 position, bool atFinish = false)
    {
        if (atFinish) {
            segment = route.Length - 2;
            distance = distances[distances.Length - 1];
            previousPosition = position;
            return;
        }

        // Limit projections to a continuous neighborhood along the route. A nearby
        // switchback on a different tier must not pull the marker across the route.
        float reach = Mathf.Max(4, Vector2.Distance(position, previousPosition) + 2);
        previousPosition = position;
        Project(position, reach);
    }

    void Project(Vector2 position, float reach)
    {
        float min = Mathf.Max(0, distance - reach);
        float max = Mathf.Min(distances[distances.Length - 1], distance + reach);
        float bestError = float.PositiveInfinity;
        float bestDistance = distance;
        int bestSegment = segment;
        int first = segment;
        while (first > 0 && distances[first] > min) first--;
        for (int i = first; i < route.Length - 1 && distances[i] <= max; i++) {
            Vector2 direction = route[i + 1] - route[i];
            float length = distances[i + 1] - distances[i];
            float along = Mathf.Clamp(Vector2.Dot(position - route[i], direction) / length,
                Mathf.Max(0, min - distances[i]), Mathf.Min(length, max - distances[i]));
            float error = (position - (route[i] + direction * (along / length))).sqrMagnitude;
            float candidate = distances[i] + along;
            if (error > bestError || (Mathf.Approximately(error, bestError) &&
                Mathf.Abs(candidate - distance) >= Mathf.Abs(bestDistance - distance))) continue;
            bestError = error;
            bestDistance = candidate;
            bestSegment = i;
        }
        distance = bestDistance;
        segment = bestSegment;
    }

    internal void SetCheckpointVisual(int sequence, SpriteRenderer worldRenderer, Rect uv)
    {
        checkpointVisuals[sequence - 1] = worldRenderer;
        checkpointUvs[sequence - 1] = uv;
    }

    public SpriteRenderer CheckpointVisual(int sequence) => checkpointVisuals[sequence - 1];

    public void Draw(Rect area, int activeCheckpoint)
    {
        if (!Visible || !HasArtwork || Event.current.type != EventType.Repaint) return;
        float scale = Mathf.Min(1, area.width / 290f);
        float left = area.x + 20 * scale;
        float width = area.width - 40 * scale;
        float centerY = area.y + 16 * scale;
        float tip = left + width * Fraction;
        // Keep the full route visible over both bright skies and dark terrain.
        DrawOutline(new Rect(left, centerY - 6 * scale, width, 12 * scale), scale);
        if (Fraction > 0)
            GUI.DrawTextureWithTexCoords(new Rect(left, centerY - 5 * scale, tip - left, 10 * scale),
                fill, new Rect(0, 0, Fraction, 1));
        float? marker = CheckpointFraction(activeCheckpoint);
        if (marker.HasValue && CheckpointVisual(activeCheckpoint) is SpriteRenderer world && world && world.sprite) {
            float x = left + width * marker.Value;
            float y = centerY - 12 * scale;
            if (Mathf.Abs(tip - x) < 26 * scale) y = centerY + 14 * scale;
            Color color = GUI.color;
            GUI.color *= world.color;
            Rect uv = checkpointUvs[activeCheckpoint - 1];
            float iconWidth = 24 * scale * (uv.width < 1 ? .65f :
                Mathf.Clamp01(world.transform.localScale.x / world.transform.localScale.y));
            GUI.DrawTextureWithTexCoords(new Rect(x - iconWidth / 2, y, iconWidth, 24 * scale),
                world.sprite.texture, uv);
            GUI.color = color;
        }
        GUI.DrawTexture(new Rect(tip - 14 * scale, centerY - 14 * scale, 28 * scale, 28 * scale), head);
    }

    static void DrawOutline(Rect rect, float scale)
    {
        Color previous = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0,
            new Color(.12f, .09f, .13f, .55f), 0, 5 * scale);
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0,
            new Color(.91f, .81f, .67f, .8f), scale, 5 * scale);
        GUI.color = previous;
    }

    internal static LevelProgress ForCourse(bool goldenRiver, Vector2 start)
    {
        var points = new List<Vector2> { start };
        var checkpointIndices = new List<int>();
        var platforms = goldenRiver ? MovementCourse.CompletePlatforms : MovementCourse.Platforms;
        var spawns = goldenRiver ? MovementCourse.LevelOneCheckpointSpawns : MovementCourse.CheckpointSpawns;
        Vector2 end = goldenRiver ? MovementCourse.LevelOneExit : MovementCourse.LabFinish;
        foreach (var platform in platforms) {
            Vector2 point = new Vector2(platform.x, platform.y + 1.27f);
            if (point != end && point != points[points.Count - 1]) points.Add(point);
            if (checkpointIndices.Count < spawns.Length) {
                Vector2 spawn = spawns[checkpointIndices.Count];
                if (Mathf.Abs(point.x - spawn.x) <= platform.z / 2 && Mathf.Abs(point.y - spawn.y) < .1f) {
                    if (points[points.Count - 1] != spawn) points.Add(spawn);
                    checkpointIndices.Add(points.Count - 1);
                }
            }
        }
        points.Add(end);
        if (checkpointIndices.Count != spawns.Length)
            throw new InvalidOperationException("A course checkpoint is missing from its progress route.");
        return new LevelProgress(points.ToArray(), checkpointIndices.ToArray(),
            goldenRiver ? "golden-river" : "movement-lab");
    }
}
