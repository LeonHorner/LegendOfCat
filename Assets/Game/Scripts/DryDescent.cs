using UnityEngine;

// Owns the permanent coverage mask; presentation never depends on entering the route.
public sealed class DryDescent : MonoBehaviour
{
    Texture2D coverage;
    public void Initialize(CatController cat, SceneryRenderer scenery)
    {
        coverage = UpperRoute.CreateMask();
        scenery.SetDescentCoverage(coverage);
        scenery.CreateDescentScenery();
        gameObject.AddComponent<DescentLight>().Initialize(cat);
    }
    void OnDestroy() { if (coverage) Destroy(coverage); }
}
