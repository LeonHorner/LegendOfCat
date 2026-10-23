using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

public class DesktopBehaviorTests
{
    static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
        .Select(a => a.GetType(name)).First(t => t != null);
    static object Call(object state, string method, params object[] args) => state.GetType().GetMethod(method).Invoke(state, args);
    static object Property(object state, string name) => state.GetType().GetProperty(name).GetValue(state);
    static object State() => Activator.CreateInstance(Runtime("DisplayState"), 1280, 720, false);

    [Test]
    public void ResizeDebouncesAndFlushesTheLastWindowSize()
    {
        var state = State();
        Assert.That(Call(state, "Observe", 960, 540, false, 0f, false), Is.False);
        Assert.That(Call(state, "Observe", 1000, 600, false, 1f, false), Is.False);
        Assert.That(Call(state, "Observe", 1000, 600, false, 2f, false), Is.False);
        Assert.That(Call(state, "Observe", 1000, 600, false, 3f, false), Is.True);
        Assert.That(Property(state, "Width"), Is.EqualTo(1000));
        Assert.That(Call(state, "Observe", 1000, 600, false, 8f, false), Is.False);
        Assert.That(Call(state, "Observe", 960, 540, false, 9f, true), Is.True);
        Assert.That(Property(state, "Width"), Is.EqualTo(960));
    }

    [Test]
    public void FullscreenWaitsForUnityAndPreservesWindowDimensions()
    {
        var state = State();
        Call(state, "Request", true, 0f);
        Assert.That(Call(state, "Observe", 1280, 720, false, 0.1f, false), Is.False);
        Assert.That(Call(state, "Observe", 1920, 1080, true, 0.2f, false), Is.False);
        Call(state, "Observe", 1920, 1080, true, 0.3f, false);
        Assert.That(Call(state, "Observe", 1920, 1080, true, 2f, false), Is.True);
        Assert.That(Property(state, "Fullscreen"), Is.True);
        Assert.That(Property(state, "Width"), Is.EqualTo(1280));
        Call(state, "Request", false, 3f);
        Assert.That(Call(state, "Observe", 1920, 1080, true, 3.1f, true), Is.False);
        Call(state, "Observe", 1280, 720, false, 3.2f, false);
        Call(state, "Observe", 1280, 720, false, 3.3f, false);
        Assert.That(Call(state, "Observe", 1280, 720, false, 5f, false), Is.True);
        Assert.That(Property(state, "Fullscreen"), Is.False);
    }

    [Test]
    public void RejectedRequestTimesOutWithoutPersistingTheRequestedMode()
    {
        var state = State();
        Call(state, "Request", true, 0f);
        Call(state, "Observe", 1280, 720, false, 0.1f, false);
        Call(state, "Observe", 1280, 720, false, 1f, false);
        Assert.That(Call(state, "Observe", 1280, 720, false, 6f, false), Is.False);
        Assert.That(Property(state, "Pending"), Is.False);
        Assert.That(Property(state, "Fullscreen"), Is.False);
    }

    static Array Parse(string text) => (Array)Runtime("ReleaseHistory").GetMethod("Parse").Invoke(null, new object[] { text });
    static string Text(object block) => (string)block.GetType().GetField("Text").GetValue(block);

    [TestCase("0.2.0 (2026-10-23)", "v0.2.0", true)]
    [TestCase("0.2.0", "v0.2.0", true)]
    [TestCase("Bug Fixes", "v0.2.0", false)]
    public void RepeatedVersionHeadingsUseTheLaunchersTagFormat(string heading, string version, bool expected)
    {
        Assert.That(Runtime("ReleaseHistory").GetMethod("IsVersionHeading").Invoke(null, new object[] { heading, version }), Is.EqualTo(expected));
    }

    [Test]
    public void DownloadSectionEndsAtConventionalReleaseHeading()
    {
        var blocks = Parse("## Download LegendOfCat\n- [installer](https://example.org/setup)\n> Early development\n# [0.2.0](https://example.org)\n### Bug Fixes\n* **menu:** preserve *settings* ([abcdef123](https://example.org/commit))");
        Assert.That(blocks.Length, Is.EqualTo(3));
        Assert.That(Text(blocks.GetValue(0)), Is.EqualTo("0.2.0"));
        Assert.That(blocks.GetValue(1).GetType().GetField("Heading").GetValue(blocks.GetValue(1)), Is.True);
        Assert.That(Text(blocks.GetValue(2)), Is.EqualTo("• menu: preserve settings"));
    }

    [TestCase("1. [Readable link](https://example.org/a_(b))", "• Readable link")]
    [TestCase("> A `code` and **bold** note", "A code and bold note")]
    [TestCase("- [x] Completed task", "• Completed task")]
    [TestCase("https://example.org/long/path", "")]
    [TestCase("[abcdef123](https://example.org/commit)", "")]
    [TestCase("Fix layout ([#36](https://example.org/pull/36))", "Fix layout")]
    [TestCase("### Fixes ###", "Fixes")]
    [TestCase("Commit abcdef123", "Commit")]
    public void CompactMarkdownHasReadableText(string markdown, string expected)
    {
        var blocks = Parse(markdown);
        Assert.That(blocks.Length == 0 ? "" : Text(blocks.GetValue(0)), Is.EqualTo(expected));
    }

    [Test]
    public void EmptyAndLongHistoriesAreBounded()
    {
        Assert.That(Parse(null).Length, Is.Zero);
        Assert.That(Parse("```\ncode\n```\n[ref]: https://example.org\n| table |").Length, Is.Zero);
        Assert.That(Parse(string.Join("\n", Enumerable.Repeat("- Entry", 10000))).Length, Is.LessThanOrEqualTo(121));
    }
}
