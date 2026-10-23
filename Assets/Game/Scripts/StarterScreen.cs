using System;
using System.Linq;
using UnityEngine;

// Bootstrap screen only; gameplay will be developed separately.
public sealed class StarterScreen : MonoBehaviour
{
    void Start()
    {
        if (Environment.GetCommandLineArgs().Contains("--smoke-test"))
            Application.Quit(0);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(30, 30, 500, 40), "LegendOfCat — Unity starter " + Application.version);
        GUI.Label(new Rect(30, 70, 500, 40), "Project setup complete. Gameplay is not migrated yet.");
    }
}
