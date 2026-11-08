using System.Collections.Generic;
using UnityEngine;

// Authored continuation only; the introduction/ascent and optional rejoin keep their indices.
public static class FinaleRoute
{
    public enum Form { Garden, Wall, Pier, Root }
    public static readonly List<Form> Forms = new List<Form>();
    public static readonly List<int> RecoveryEntries = new List<int>();
    public static readonly List<Vector3> Recoveries = new List<Vector3>();
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
        var route = new List<Vector3>();
        float edge = StartX;
        void Add(float width, float height, float gap, Form form) {
            edge += gap;
            route.Add(new Vector3(edge + width / 2, height - .75f, width));
            Forms.Add(form); edge += width;
        }
        void Catch(int entry, float x, float contact, float width) {
            RecoveryEntries.Add(entry); Recoveries.Add(new Vector3(x, contact - .75f, width));
        }
        // Roof gardens: longer landings alternate with compact steps, never a flat run extension.
        float[] gardenWidths = { 12,5,7,4,9,6,3.5f,8,5,12,4,6,9,4,7,5,10,4,8,6 };
        float[] gardenHeights = { 12.6f,13.5f,14.4f,13.5f,12.7f,13.8f,14.8f,14,13.1f,12.5f,13.4f,14.3f,13.2f,12.2f,11.4f,12.4f,13.3f,12.3f,11.4f,10.4f };
        for (int i=0;i<gardenWidths.Length;i++) {
            Add(gardenWidths[i],gardenHeights[i],i%3==0?2.2f:1.8f,Form.Garden);
            if (i==2 || i==10) Catch(i-1,route[i].x-(i==10?1:2), gardenHeights[i-1]-(i==10?1:1.6f),6);
        }
        BeatStarts[1]=route.Count;
        // The lower approach goes right; the high stair doubles back left before climbing right.
        Add(16,10,2,Form.Wall);
        float courtyard = edge;
        Add(4,11.5f,2,Form.Pier);
        CourtyardTurn=route.Count;
        route.Add(new Vector3(courtyard-3,12.25f,4)); Forms.Add(Form.Wall);
        route.Add(new Vector3(courtyard-10,13.75f,4)); Forms.Add(Form.Wall);
        route.Add(new Vector3(courtyard-3,15.25f,4)); Forms.Add(Form.Wall);
        route.Add(new Vector3(courtyard+4,16.75f,4)); Forms.Add(Form.Pier);
        edge=courtyard+6;
        Add(16,16.1f,2,Form.Garden);
        float[] courtWidths={5,3,8,4,6,3.5f,10,4,7,5,3,9,6,4,8,5};
        float[] courtHeights={15.1f,14,14.8f,15.7f,14.7f,13.6f,12.8f,13.9f,14.9f,14,12.9f,13.8f,14.7f,13.6f,12.7f,13.4f};
        for(int i=0;i<courtWidths.Length;i++) Add(courtWidths[i],courtHeights[i],i%2==0?2:1.8f,Form.Wall);
        BeatStarts[2]=route.Count;
        Add(12,13.3f,2,Form.Garden);
        LiftIndex=route.Count;
        Add(5,14.5f,1.8f,Form.Wall);
        Add(8,16.9f,1.8f,Form.Wall);
        Catch(LiftIndex-1,route[LiftIndex].x,11.6f,8);
        float[] liftWidths={4,7,3,9,5,4,8,3.5f,6,5,10,4,7,3,8,5,6,4};
        float[] liftHeights={15.8f,14.7f,15.7f,16.6f,15.6f,14.6f,13.6f,14.6f,15.5f,14.5f,13.5f,14.4f,15.4f,14.4f,13.4f,14.3f,15.2f,14.2f};
        for(int i=0;i<liftWidths.Length;i++) Add(liftWidths[i],liftHeights[i],i%3==0?2.1f:1.8f,Form.Root);
        BeatStarts[3]=route.Count;
        Add(12,14,2,Form.Garden);
        float crown=edge;
        Add(4,15.5f,2,Form.Pier);
        CrownTurn=route.Count;
        route.Add(new Vector3(crown-3,16.25f,4)); Forms.Add(Form.Wall);
        route.Add(new Vector3(crown-10,17.75f,4)); Forms.Add(Form.Wall);
        route.Add(new Vector3(crown-3,19.25f,4)); Forms.Add(Form.Wall);
        route.Add(new Vector3(crown+4,19.25f,5)); Forms.Add(Form.Wall);
        edge=crown+6.5f;
        Add(16,18.1f,2,Form.Garden);
        float[] crownWidths={3,6,4,8,3.5f,5,7,4,9,3,6,4};
        float[] crownHeights={16.8f,15.7f,16.7f,17.6f,16.6f,15.6f,14.6f,15.6f,16.5f,15.4f,14.3f,13.3f};
        for(int i=0;i<crownWidths.Length;i++) {
            Add(crownWidths[i],crownHeights[i],1.9f,Form.Wall);
            if(i==3) Catch(route.Count-2,route[route.Count-1].x-2,15.1f,6);
        }
        Add(10,13.4f,2,Form.Garden);
        DiagonalIndex=route.Count;
        Add(5,14.7f,2.5f,Form.Wall);
        // Axis (1,.75), amplitude 1: both endpoints remain close to fixed approaches.
        Add(9,17.1f,2.5f,Form.Wall);
        Catch(DiagonalIndex-1,route[DiagonalIndex].x-1,11.7f,8);
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
        route.Add(new Vector3(tower-3,18.45f,4)); Forms.Add(Form.Wall);
        route.Add(new Vector3(tower-10,19.95f,4)); Forms.Add(Form.Wall);
        route.Add(new Vector3(tower-3,21.45f,4)); Forms.Add(Form.Wall);
        route.Add(new Vector3(tower+4,21.45f,5)); Forms.Add(Form.Wall);
        edge=tower+6.5f;
        // A supported flank is too tall to bypass from the lower approach.
        Add(18,20.3f,2,Form.Garden);
        Catch(towerBank,tower+3,14.6f,6);
        float[] towerWidths={3,7,4,8,3.5f,6,4,9,3,7};
        float[] towerHeights={19.1f,18.1f,19,20,18.9f,17.8f,18.7f,17.6f,16.5f,17.1f};
        for(int i=0;i<towerWidths.Length;i++) Add(towerWidths[i],towerHeights[i],i%3==0?2.2f:1.9f,Form.Wall);
        BeatStarts[4]=route.Count;
        Add(6,16.2f,2,Form.Root);
        Add(4,15.3f,2,Form.Root);
        Add(8,14.4f,2,Form.Root);
        Add(5,15.3f,2,Form.Garden);
        Add(7,14.5f,2,Form.Garden);
        Add(18,14.5f,2.5f,Form.Garden);
        return route.ToArray();
    }
}
