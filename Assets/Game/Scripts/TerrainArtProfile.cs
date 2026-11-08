using System.Collections.Generic;
using UnityEngine;

// Shared authored paint registration and stone contours; no asset-name geometry in the renderer.
internal sealed class TerrainArtProfile
{
    public string TexturePath { get; }
    public float ContactUV { get; }
    public float LandingFraction { get; }
    public IReadOnlyList<Vector2> Underside { get; }
    public Vector2? TopLeftUV { get; }
    public Vector2? TopRightUV { get; }
    TerrainArtProfile(string name,float contactUV,float landingFraction,Vector2[] underside,
        Vector2? topLeftUV=null,Vector2? topRightUV=null) {
        TexturePath="GoldenRiver/"+name; ContactUV=contactUV; LandingFraction=landingFraction;
        Underside=System.Array.AsReadOnly(underside); TopLeftUV=topLeftUV; TopRightUV=topRightUV;
    }
    static readonly Dictionary<string,TerrainArtProfile> Profiles = new Dictionary<string,TerrainArtProfile> {
        {"finale-rubble-v1",new TerrainArtProfile("finale-rubble-v1",0.665f,0.9f,new[] {
            new Vector2(.64f,.50f), new Vector2(.71f,.50f), new Vector2(.71f,.40f),
            new Vector2(.87f,.40f), new Vector2(.87f,.34f), new Vector2(.94f,.34f),
            new Vector2(.94f,.17f), new Vector2(.06f,.17f), new Vector2(.06f,.32f),
            new Vector2(.15f,.32f), new Vector2(.15f,.42f), new Vector2(.25f,.42f),
            new Vector2(.25f,.53f), new Vector2(.37f,.53f)
        },new Vector2(.37f,.665f),new Vector2(.64f,.665f))},
        {"finale-wall-v1",new TerrainArtProfile("finale-wall-v1",0.6f,0.94f,new[] {
            new Vector2(.97f,.51f), new Vector2(.90f,.32f), new Vector2(.80f,.25f),
            new Vector2(.67f,.23f), new Vector2(.50f,.22f), new Vector2(.32f,.24f),
            new Vector2(.17f,.24f), new Vector2(.05f,.46f)
        })},
        {"finale-garden-v1",new TerrainArtProfile("finale-garden-v1",0.655f,0.94f,new[] {
            new Vector2(.97f,.54f), new Vector2(.90f,.29f), new Vector2(.75f,.22f),
            new Vector2(.58f,.20f), new Vector2(.40f,.22f), new Vector2(.23f,.25f),
            new Vector2(.06f,.47f)
        })},
        {"finale-pier-v1",new TerrainArtProfile("finale-pier-v1",0.675f,0.92f,new[] {
            new Vector2(.95f,.58f), new Vector2(.86f,.28f), new Vector2(.70f,.18f),
            new Vector2(.51f,.16f), new Vector2(.30f,.19f), new Vector2(.14f,.28f),
            new Vector2(.05f,.56f)
        })},
        {"shelf-root",new TerrainArtProfile("shelf-root",0.68f,0.9f,new[] {
            new Vector2(.96f,.60f), new Vector2(.89f,.48f), new Vector2(.73f,.32f),
            new Vector2(.58f,.20f), new Vector2(.38f,.35f), new Vector2(.19f,.50f),
            new Vector2(.06f,.55f)
        })},
        {"shelf-root-compact-v3",new TerrainArtProfile("shelf-root-compact-v3",0.7f,0.9f,new[] {
            new Vector2(.95f,.58f), new Vector2(.81f,.40f), new Vector2(.63f,.25f),
            new Vector2(.42f,.13f), new Vector2(.24f,.24f), new Vector2(.10f,.43f),
            new Vector2(.05f,.57f)
        })},
        {"shelf-masonry-compact-v3",new TerrainArtProfile("shelf-masonry-compact-v3",0.66f,0.96f,new[] {
            new Vector2(.95f,.53f), new Vector2(.87f,.35f), new Vector2(.76f,.22f),
            new Vector2(.63f,.23f), new Vector2(.53f,.29f), new Vector2(.40f,.20f),
            new Vector2(.30f,.18f), new Vector2(.13f,.32f), new Vector2(.05f,.53f)
        })},
        {"shelf-ascent-masonry-v1",new TerrainArtProfile("shelf-ascent-masonry-v1",0.62f,0.9f,new[] {
            new Vector2(.97f,.49f), new Vector2(.91f,.37f), new Vector2(.76f,.31f),
            new Vector2(.61f,.24f), new Vector2(.42f,.22f), new Vector2(.24f,.22f),
            new Vector2(.10f,.23f), new Vector2(.04f,.46f)
        })},
        {"shelf-masonry",new TerrainArtProfile("shelf-masonry",0.66f,0.96f,new[] {
            new Vector2(.98f,.55f), new Vector2(.92f,.43f), new Vector2(.85f,.29f),
            new Vector2(.66f,.31f), new Vector2(.58f,.36f), new Vector2(.42f,.36f),
            new Vector2(.34f,.39f), new Vector2(.22f,.30f), new Vector2(.12f,.35f),
            new Vector2(.02f,.55f)
        })},
    };
    public static TerrainArtProfile Find(string textureName) => Profiles.TryGetValue(textureName,out var profile)?profile:null;
    public static TerrainArtProfile ForFinale(FinaleRoute.Form form) => form switch {
        FinaleRoute.Form.Pier => Find("finale-pier-v1"),
        FinaleRoute.Form.Garden => Find("finale-garden-v1"),
        FinaleRoute.Form.Wall => Find("finale-wall-v1"),
        _ => throw new System.ArgumentOutOfRangeException(nameof(form),"Root shelves use their width-specific profile.")
    };
}
