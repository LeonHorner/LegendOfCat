// Immutable session result for the presentation and future campaign consumers.
// Creating this value never writes player progress.
public sealed class LevelCompletion
{
    public string LevelId { get; }
    public float ElapsedSeconds { get; }
    public int Retries { get; }

    public LevelCompletion(string levelId, float elapsedSeconds, int retries)
    {
        LevelId = levelId;
        ElapsedSeconds = elapsedSeconds;
        Retries = retries;
    }
}
