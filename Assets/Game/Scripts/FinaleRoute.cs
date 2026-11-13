using System.Collections.Generic;
using UnityEngine;

// Authored continuation only; the introduction/ascent and optional rejoin keep their indices.
public static class FinaleRoute
{
    public enum Form { Garden, Wall, Pier, Root }
    public readonly struct Platform {
        public float CenterX { get; }
        public float Elevation { get; }
        public float Width { get; }
        public Form Material { get; }
        public float ContactHeight => Elevation + .75f;
        public Vector3 Geometry => new Vector3(CenterX, Elevation, Width);
        public Platform(float centerX, float elevation, float width, Form material) {
            CenterX=centerX; Elevation=elevation; Width=width; Material=material;
        }
    }
    public readonly struct Recovery {
        public Platform Floor { get; }
        public int ReturnPlatformIndex { get; }
        public int SupportPlatformIndex { get; }
        public Recovery(Platform floor, int returnPlatformIndex, int supportPlatformIndex) {
            Floor=floor; ReturnPlatformIndex=returnPlatformIndex; SupportPlatformIndex=supportPlatformIndex;
        }
    }
    public static IReadOnlyList<Platform> Definitions { get; private set; }
    public static IReadOnlyList<Recovery> Recoveries { get; private set; }
    public static IReadOnlyList<int> ObstacleBanks { get; private set; }
    public static readonly int[] BeatStarts = new int[5];
    public static int LiftIndex { get; private set; }
    public static int DiagonalIndex { get; private set; }
    public static int CourtyardTurn { get; private set; }
    public static int CrownTurn { get; private set; }
    public static int TowerStart { get; private set; }
    public static int TowerTurn { get; private set; }
    public static Vector3[] Platforms { get; private set; }
    public static Vector3 Last => Platforms[Platforms.Length - 1];
    public static float StartX { get; private set; }
    internal static void Initialize(float start)
    {
        if(Platforms!=null) return;
        StartX=start; Platforms=Build();
    }

    public static float CameraMinimum(float x)
    {
        float minimum=1;
        for(int i=0;i<2;i++) {
            int index=i==0?LiftIndex:DiagonalIndex;
            float blend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(24,10,Mathf.Abs(x-Platforms[index].x)));
            minimum=Mathf.Max(minimum,Mathf.Lerp(1,Platforms[index+1].y-3.7f,blend));
        }
        return minimum;
    }

    static Vector3[] Build()
    {
        var route = new List<Platform>();
        var recoveries = new List<Recovery>();
        var obstacleBanks = new List<int>();
        void AddAt(float x,float elevation,float width,Form form) => route.Add(new Platform(x,elevation,width,form));
        float edge = StartX;
        void Add(float width, float height, float gap, Form form) {
            edge += gap;
            AddAt(edge + width / 2, height - .75f, width, form);
            edge += width;
        }
        void Catch(int entry, float x, float contact, float width, int support = -1) {
            recoveries.Add(new Recovery(new Platform(x,contact-.75f,width,Form.Wall),entry,support));
        }
        // Roof gardens: longer landings alternate with compact steps, never a flat run extension.
        var gardenSteps = new (float Width,float ContactHeight)[] {
            (12,12.6f),(5,13.5f),(7,14.4f),(4,13.5f),(9,12.7f),
            (6,13.8f),(3.5f,14.8f),(8,14),(5,13.1f),(12,12.5f),
            (4,13.4f),(6,14.3f),(9,13.2f),(4,12.2f),(7,11.4f),
            (5,12.4f),(10,13.3f),(4,12.3f),(8,11.4f),(6,10.4f)
        };
        for (int i=0;i<gardenSteps.Length;i++) {
            Add(gardenSteps[i].Width,gardenSteps[i].ContactHeight,i%3==0?2.2f:1.8f,Form.Garden);
            if(i==0 || i==9) obstacleBanks.Add(route.Count-1);
            if (i==2 || i==10) Catch(i-1,route[i].CenterX-(i==10?1:2), gardenSteps[i-1].ContactHeight-(i==10?1:1.6f),6);
        }
        BeatStarts[1]=route.Count;
        // The lower approach goes right; the high stair doubles back left before climbing right.
        Add(16,10,2,Form.Wall);
        float courtyard = edge;
        Add(4,11.5f,2,Form.Pier);
        CourtyardTurn=route.Count;
        AddAt(courtyard-3,12.25f,4,Form.Wall);
        AddAt(courtyard-10,13.75f,4,Form.Wall);
        AddAt(courtyard-3,15.25f,4,Form.Wall);
        AddAt(courtyard+4,16.75f,4,Form.Pier);
        edge=courtyard+6;
        Add(16,16.1f,2,Form.Garden);
        var courtSteps = new (float Width,float ContactHeight)[] {
            (5,15.1f),(3,14),(8,14.8f),(4,15.7f),(6,14.7f),
            (3.5f,13.6f),(10,12.8f),(4,13.9f),(7,14.9f),(5,14),
            (3,12.9f),(9,13.8f),(6,14.7f),(4,13.6f),(8,12.7f),
            (5,13.4f)
        };
        for(int i=0;i<courtSteps.Length;i++) Add(courtSteps[i].Width,courtSteps[i].ContactHeight,i%2==0?2:1.8f,Form.Wall);
        BeatStarts[2]=route.Count;
        Add(12,13.3f,2,Form.Garden);
        obstacleBanks.Add(route.Count-1);
        LiftIndex=route.Count;
        Add(5,14.5f,1.8f,Form.Wall);
        Add(8,16.9f,1.8f,Form.Wall);
        Catch(LiftIndex-1,route[LiftIndex].CenterX,11.6f,8,LiftIndex);
        var liftSteps = new (float Width,float ContactHeight)[] {
            (4,15.8f),(7,14.7f),(3,15.7f),(9,16.6f),(5,15.6f),
            (4,14.6f),(8,13.6f),(3.5f,14.6f),(6,15.5f),(5,14.5f),
            (10,13.5f),(4,14.4f),(7,15.4f),(3,14.4f),(8,13.4f),
            (5,14.3f),(6,15.2f),(4,14.2f)
        };
        for(int i=0;i<liftSteps.Length;i++) Add(liftSteps[i].Width,liftSteps[i].ContactHeight,i%3==0?2.1f:1.8f,Form.Root);
        BeatStarts[3]=route.Count;
        Add(12,14,2,Form.Garden);
        float crown=edge;
        Add(4,15.5f,2,Form.Pier);
        CrownTurn=route.Count;
        AddAt(crown-3,16.25f,4,Form.Wall);
        AddAt(crown-10,17.75f,4,Form.Wall);
        AddAt(crown-3,19.25f,4,Form.Wall);
        AddAt(crown+4,19.25f,5,Form.Wall);
        edge=crown+6.5f;
        Add(16,18.1f,2,Form.Garden);
        var crownSteps = new (float Width,float ContactHeight)[] {
            (3,16.8f),(6,15.7f),(4,16.7f),(8,17.6f),(3.5f,16.6f),
            (5,15.6f),(7,14.6f),(4,15.6f),(9,16.5f),(3,15.4f),
            (6,14.3f),(4,13.3f)
        };
        for(int i=0;i<crownSteps.Length;i++) {
            Add(crownSteps[i].Width,crownSteps[i].ContactHeight,1.9f,Form.Wall);
            if(i==3) Catch(route.Count-2,route[route.Count-1].CenterX-2,15.1f,6);
        }
        Add(10,13.4f,2,Form.Garden);
        DiagonalIndex=route.Count;
        Add(5,14.7f,2.5f,Form.Wall);
        // Axis (1,.75), amplitude 1: both endpoints remain close to fixed approaches.
        Add(9,17.1f,2.5f,Form.Wall);
        // Keep the return-jump edge fixed while supporting the complete right bearing.
        Catch(DiagonalIndex-1,route[DiagonalIndex].CenterX,11.7f,10,DiagonalIndex);
        // The collapsed watchtower adds decisions and a short reversal, not lift waiting.
        TowerStart=route.Count;
        Add(6,16.2f,2,Form.Root);
        Add(4,15.3f,2,Form.Root);
        Add(8,14.4f,2,Form.Root);
        Add(5,15.3f,2,Form.Garden);
        Add(7,14.5f,2,Form.Garden);
        Add(14,16.2f,2,Form.Garden);
        int towerBank=route.Count-1;
        float tower=edge;
        Add(4,17.7f,2,Form.Pier);
        TowerTurn=route.Count;
        AddAt(tower-3,18.45f,4,Form.Wall);
        AddAt(tower-10,19.95f,4,Form.Wall);
        AddAt(tower-3,21.45f,4,Form.Wall);
        AddAt(tower+4,21.45f,5,Form.Wall);
        edge=tower+6.5f;
        // A supported flank is too tall to bypass from the lower approach.
        Add(18,20.3f,2,Form.Garden);
        Catch(towerBank,tower+3,14.6f,6);
        var towerSteps = new (float Width,float ContactHeight)[] {
            (3,19.1f),(7,18.1f),(4,19),(8,20),(3.5f,18.9f),
            (6,17.8f),(4,18.7f),(9,17.6f),(3,16.5f),(7,17.1f)
        };
        for(int i=0;i<towerSteps.Length;i++) Add(towerSteps[i].Width,towerSteps[i].ContactHeight,i%3==0?2.2f:1.9f,Form.Wall);
        BeatStarts[4]=route.Count;
        Add(6,16.2f,2,Form.Root);
        Add(4,15.3f,2,Form.Root);
        Add(8,14.4f,2,Form.Root);
        Add(5,15.3f,2,Form.Garden);
        Add(7,14.5f,2,Form.Garden);
        Add(18,14.5f,2.5f,Form.Garden);
        Definitions=route.AsReadOnly(); Recoveries=recoveries.AsReadOnly(); ObstacleBanks=obstacleBanks.AsReadOnly();
        // Existing course/capture interfaces consume geometry; authors edit the named records above.
        var geometry=new Vector3[route.Count];
        for(int i=0;i<geometry.Length;i++) geometry[i]=route[i].Geometry;
        return geometry;
    }
}
