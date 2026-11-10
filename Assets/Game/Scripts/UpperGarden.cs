using System.Collections.Generic;
using UnityEngine;

// Entrance foliage conceals the route until Mika actually chooses the first tread.
// Physics stays present; concealed return treads are above the main jump envelope.
public sealed class UpperGarden : MonoBehaviour
{
    public CatController Cat;
    public readonly List<Renderer> Concealed = new List<Renderer>();
    public bool Exploring { get; private set; }
    void LateUpdate() => Refresh();
    public void Refresh()
    {
        if (!Cat) return;
        Vector3 p = transform.InverseTransformPoint(Cat.transform.position);
        float entry = UpperRoute.Entrance.x;
        if (p.y < .9f || p.x > UpperRoute.Rejoin.x + 3) Exploring = false;
        else if (p.x < entry - 1 && p.y > 1.05f) Exploring = true;
        foreach (var renderer in Concealed) if (renderer) renderer.enabled = Exploring;
    }
}
