using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class MenuPresentationTests
{
    static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies()
        .Select(assembly => assembly.GetType(name)).First(type => type != null);
    [TestCase(1280, 720, 0, 0, 1280, 720)]
    [TestCase(640, 480, 30, 20, 580, 440)]
    [TestCase(390, 844, 0, 34, 390, 763)]
    [TestCase(2560, 1080, 100, 0, 2360, 1080)]
    public void ContentStaysInsideSafeArea(int width, int height, int x, int y, int safeWidth, int safeHeight)
    {
        var region = (Rect)RuntimeType("MenuLayout").GetMethod("ContentRegion").Invoke(null, new object[] { width, height, new Rect(x, y, safeWidth, safeHeight) });
        Assert.That(region.xMin, Is.EqualTo(x));
        Assert.That(region.yMin, Is.EqualTo(height - y - safeHeight));
        Assert.That(region.width, Is.EqualTo(safeWidth));
        Assert.That(region.height, Is.EqualTo(safeHeight));
        float scale = (float)RuntimeType("MenuLayout").GetMethod("Scale").Invoke(null, new object[] { region });
        Assert.That(scale, Is.InRange(0.85f, 1.2f));
        // At inspected sizes the scrollable panel retains readable text and usable controls.
        Assert.That(region.width / scale, Is.GreaterThanOrEqualTo(390));
    }

    [Test]
    public void UnsupportedServiceHasNoMessagesOrDesktopSideEffects()
    {
        var service = Activator.CreateInstance(RuntimeType("UnavailableMenuPlatformService"));
        using ((IDisposable)service) {
            var type = service.GetType();
            Assert.That(type.GetProperty("SupportsUpdates").GetValue(service), Is.False);
            foreach (var command in new[] { "check", "download", "apply" })
                type.GetMethod("Send").Invoke(service, new object[] { command });
            var args = new object[] { null };
            Assert.That(type.GetMethod("TryReceive").Invoke(service, args), Is.False);
            Assert.That(args[0], Is.Null);
            type.GetMethod("CompleteApply").Invoke(service, null);
        }
    }

    [Test]
    public void EditorDoesNotStartDesktopUpdater()
    {
        var service = RuntimeType("DesktopMenuPlatformService").GetMethod("Create").Invoke(null, null);
        using ((IDisposable)service)
            Assert.That(service.GetType().GetProperty("SupportsUpdates").GetValue(service), Is.False);
    }
}
