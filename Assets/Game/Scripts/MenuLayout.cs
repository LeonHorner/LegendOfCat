using UnityEngine;

public static class MenuLayout
{
    public static Rect Letterbox16By9(int width, int height)
    {
        if (width <= 0 || height <= 0) return new Rect(0, 0, 1, 1);
        float ratio = (float)width / height;
        const float target = 16f / 9f;
        return ratio > target
            ? new Rect((1f - target / ratio) * 0.5f, 0, target / ratio, 1)
            : new Rect(0, (1f - ratio / target) * 0.5f, 1, ratio / target);
    }

    // Screen.safeArea uses a bottom-left origin; IMGUI uses top-left.
    public static Rect ContentRegion(int width, int height, Rect safeArea)
    {
        float x = Mathf.Clamp(safeArea.x, 0, width);
        float top = Mathf.Clamp(height - safeArea.yMax, 0, height);
        float right = Mathf.Clamp(safeArea.xMax, x, width);
        float bottom = Mathf.Clamp(height - safeArea.y, top, height);
        return new Rect(x, top, right - x, bottom - top);
    }

    public static float Scale(Rect region)
        => Mathf.Clamp(Mathf.Min(region.width / 420f, region.height / 600f), 0.85f, 1.2f);
}
