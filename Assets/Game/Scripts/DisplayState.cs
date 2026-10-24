// Tracks observed display state separately from asynchronous Unity requests.
// No file I/O or native window handles; callers persist only settled observations.
public sealed class DisplayState
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool Fullscreen { get; private set; }
    public bool Pending { get; private set; }
    public bool Switching { get; private set; }
    public bool RequestedFullscreen { get; private set; }
    int observedWidth, observedHeight;
    bool observedFullscreen, dirty;
    float changedAt, requestedAt;
    int samples;

    public DisplayState(int width, int height, bool fullscreen)
    { Width = width; Height = height; Fullscreen = fullscreen; }

    public void Request(bool fullscreen, float now)
    {
        RequestedFullscreen = fullscreen;
        Pending = Switching = true;
        requestedAt = changedAt = now;
        samples = 0;
    }

    public bool Observe(int width, int height, bool fullscreen, float now, bool flush = false)
    {
        if (width < 1 || height < 1) return false; // Minimized/transient surface.
        if (width != observedWidth || height != observedHeight || fullscreen != observedFullscreen) {
            observedWidth = width; observedHeight = height; observedFullscreen = fullscreen;
            changedAt = now; samples = 0;
        }
        samples++;
        if (Pending) {
            // Re-enable controls once Unity confirms the mode across several frames.
            // Persistence still waits for resize/compositor events to settle.
            Switching = samples < 3 || fullscreen != RequestedFullscreen;
            if (samples < 3 || now - changedAt < 1.5f ||
                (fullscreen != RequestedFullscreen && now - requestedAt < 5)) return false;
            Pending = Switching = false;
        }
        if (!flush && now - changedAt < 1.5f) return false;
        // Remember window dimensions independently of fullscreen desktop dimensions.
        if (!fullscreen && width >= 320 && height >= 180 && width <= 16384 && height <= 16384) {
            dirty |= Width != width || Height != height;
            Width = width; Height = height;
        }
        dirty |= Fullscreen != fullscreen;
        Fullscreen = fullscreen;
        bool save = dirty; dirty = false;
        return save;
    }
}
