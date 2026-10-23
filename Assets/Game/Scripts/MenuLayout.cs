using UnityEngine;

public static class MenuLayout
{
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
