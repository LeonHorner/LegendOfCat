using System.Collections.Generic;
using UnityEngine;

// Deliberately isolated visual/playability review: entrance and first enclosed passage only.
// The campaign route is not extended until this slice has been reviewed in the player.
public sealed class CaveEntryStudy : MonoBehaviour
{
    public CatController Cat { get; private set; }
    public Camera View { get; private set; }
    public System.Action BackToMenu;
    public CatCamera Follow { get; private set; }
    readonly List<Object> owned = new List<Object>();
    readonly List<Material> waterMaterials = new List<Material>();
    float cameraHeight=1, waterTime;
    static readonly Vector2 Spawn=new Vector2(2.6f,.52f);

    void Awake()
    {
        AddLand();
        var player=new GameObject("Mika entry study");player.transform.SetParent(transform,false);
        player.transform.localPosition=Spawn;
        player.AddComponent<Rigidbody2D>();player.AddComponent<BoxCollider2D>().size=Vector2.one*.9f;
        Cat=player.AddComponent<CatController>();Cat.fallReference=transform;
        Cat.RecoveryBoundary=(previous,current)=>current.y < -14.5f || CrossesRiver(previous,current);
        Cat.FellOutOfBounds+=ResetStudy;
        Cat.ReturnedToStart+=()=>{cameraHeight=1;Follow.Snap();};
        var input=player.AddComponent<CatInput>();input.controller=Cat;
        input.RetryCheckpoint=ResetStudy;input.RestartRun=ResetStudy;
        input.TogglePause=()=>Cat.SetPaused(!Cat.Paused);input.BackToMenu=()=>BackToMenu?.Invoke();
        var visual=Instantiate(Resources.Load<GameObject>("MikaOriginal"),player.transform);
        visual.GetComponent<MikaSpritePresentation>().controller=Cat;
        var cameraObject=new GameObject("Entry study camera");cameraObject.transform.SetParent(transform,false);
        View=cameraObject.AddComponent<Camera>();View.orthographic=true;View.orthographicSize=5.5f;
        View.depth=1;View.clearFlags=CameraClearFlags.SolidColor;View.backgroundColor=new Color(.22f,.23f,.30f);
        var bars=new GameObject("Entry viewport bars").AddComponent<Camera>();bars.transform.SetParent(transform,false);
        bars.depth=.5f;bars.clearFlags=CameraClearFlags.SolidColor;bars.backgroundColor=Color.black;bars.cullingMask=0;
        Follow=cameraObject.AddComponent<CatCamera>();Follow.target=player.transform;
        Follow.fixedHeight=true;Follow.courseRoot=transform;Follow.leftBoundary=-14;Follow.rightBoundary=34;
        Follow.HeightGuide=GuideHeight;Follow.Snap();Follow.Follow(0);
    }

    float GuideHeight(Vector2 position)
    {
        if(Cat.Grounded) cameraHeight=position.y>-.5f?1:position.y+Mathf.Lerp(-.5f,2.1f,Mathf.InverseLerp(-1.6f,-9.5f,position.y));
        else if(position.y<cameraHeight-2.5f) cameraHeight=position.y+1.8f;
        return cameraHeight;
    }
    public void ResetStudy() { Cat.RespawnAt(transform.TransformPoint(Spawn)); }
    public static bool CrossesRiver(Vector2 previous,Vector2 current)
    {
        if(previous.y<=-3.1f || current.y> -3.1f) return false;
        var at=Vector2.Lerp(previous,current,(-3.1f-previous.y)/(current.y-previous.y));
        foreach(var polygon in WaterContours) if(CaveGeometry.Contains(polygon,at)) return true;
        return false;
    }
    void Update()
    {
        if(!Cat.Paused) waterTime+=Time.deltaTime;
        foreach(var material in waterMaterials) material.SetFloat("_WaterTime",waterTime);
        View.rect=MenuLayout.Letterbox16By9(Screen.width,Screen.height);View.aspect=16f/9;
    }
    void OnGUI()
    {
        var label=new GUIStyle(GUI.skin.label){fontSize=14,normal={textColor=Color.white}};
        string text=Cat.Paused?"Paused · P: resume":Cat.transform.localPosition.x>26 && Cat.transform.localPosition.y< -8
            ?"End of review section · R: return to entrance"
            :"Entrance study · A/D: move · Space: jump · R: reset · P: pause · Esc: menu";
        GUI.Box(new Rect(12,12,Mathf.Min(Screen.width-24,610),32),GUIContent.none);
        GUI.Label(new Rect(22,17,Screen.width-44,24),text,label);
    }

    void AddLand()
    {
        Painting("Valley",new Rect(-24,-22,72,40.5f),18,"GoldenRiver/Descent/entry-study-valley-v1",false,true);
        Painting("Recessed cave wall",new Rect(-4,-20,45,30),3,"GoldenRiver/Descent/entry-study-rear-v1",true);
        Painting("Left connected cliff",new Rect(-14,-11,27,18),1,"GoldenRiver/Descent/entry-study-left-v2");
        Painting("Right overhang and end wall",new Rect(7,-12.15f,30,20),1,"GoldenRiver/Descent/entry-study-right-v2",maximumY:.22f);
        Painting("Connected cave floor",new Rect(-1,-19.93f,36,12),.8f,"GoldenRiver/Descent/entry-study-floor-v1");
        Collider("Left cliff and descending ledges",Left);
        Collider("Opposing bank, ceiling and end wall",Right);
        Collider("First cave floor",FloorLeft);Collider("Second cave floor",FloorRight);
        AddWater();
    }
    void Collider(string name,Vector2[] contour)
    {
        var rock=new GameObject(name);rock.transform.SetParent(transform,false);
        rock.AddComponent<PolygonCollider2D>().points=contour;
    }
    void Painting(string name,Rect bounds,float z,string path,bool recessed=false,bool valley=false,float maximumY=float.PositiveInfinity)
    {
        var material=new Material(Resources.Load<Material>(valley?"BackgroundStudy/background-material":"GoldenRiver/Descent/entry-study-material"));
        owned.Add(material);material.mainTexture=Resources.Load<Texture2D>(path);
        if(valley){material.SetFloat("_Cutout",1);material.SetFloat("_Fog",.36f);material.SetFloat("_Saturation",.76f);}
        else material.SetFloat("_Recessed",recessed?1:0);
        var visible=bounds;visible.yMax=Mathf.Min(visible.yMax,maximumY);
        MeshObject(name,Rectangle(visible),bounds,z,material);
    }
    static Vector2[] Rectangle(Rect b)=>new[]{new Vector2(b.xMin,b.yMin),new Vector2(b.xMax,b.yMin),new Vector2(b.xMax,b.yMax),new Vector2(b.xMin,b.yMax)};
    void MeshObject(string name,Vector2[] polygon,Rect uvBounds,float z,Material material)
    {
        var item=new GameObject(name);item.transform.SetParent(transform,false);
        item.transform.localPosition=new Vector3(0,0,z);
        var mesh=new Mesh{name=name};owned.Add(mesh);
        var vertices=new Vector3[polygon.Length];var uv=new Vector2[polygon.Length];
        for(int i=0;i<vertices.Length;i++) {vertices[i]=polygon[i];uv[i]=new Vector2((polygon[i].x-uvBounds.x)/uvBounds.width,(polygon[i].y-uvBounds.y)/uvBounds.height);}
        var triangles=new int[(vertices.Length-2)*3];
        for(int i=0;i<vertices.Length-2;i++){triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=i+2;}
        mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateBounds();
        item.AddComponent<MeshFilter>().sharedMesh=mesh;item.AddComponent<MeshRenderer>().sharedMaterial=material;
    }
    void AddWater()
    {
        foreach(var contour in WaterContours) {
            // Tile UVs keep the existing river's wave scale. Both shore contours also drive hazards.
            for(float x=-40;x<60;x+=16) {
                var material=new Material(Resources.Load<Material>("BackgroundStudy/water-material"));owned.Add(material);waterMaterials.Add(material);
                material.mainTexture=Resources.Load<Texture2D>("GoldenRiver/river-water-v2");material.SetFloat("_UseCaptureTime",1);
                var tile=Rectangle(new Rect(x,-6,16,4));
                var outside=CaveGeometry.Subtract(new List<Vector2[]>{tile},contour);
                // The water contours are convex; subtracting their complement avoids separate render bounds.
                var pieces=new List<Vector2[]>{tile};
                foreach(var cut in outside) pieces=CaveGeometry.Subtract(pieces,cut);
                foreach(var piece in pieces) MeshObject("River outside dry cleft",piece,new Rect(x,-5.9733f,16,10.6667f),2,material);
            }
        }
    }
    void OnDestroy(){foreach(var item in owned) if(item) Destroy(item);}

    public static readonly Vector2[][] WaterContours={
        new[]{new Vector2(-40,-4.5f),new Vector2(3.8f,-4.5f),new Vector2(4.2f,-3.3f),new Vector2(3.8f,-2.85f),new Vector2(-40,-2.85f)},
        new[]{new Vector2(12,-4.5f),new Vector2(60,-4.5f),new Vector2(60,-2.85f),new Vector2(11.1f,-2.85f),new Vector2(11.5f,-3.6f)}
    };
    // Paint registration is authored alongside these contact contours in entry-study-geometry.json.
    public static readonly Vector2[] Left=new[]{new Vector2(-14f,9f),new Vector2(-3.6f,9f),new Vector2(-2.6f,7.2f),new Vector2(-1.8f,5.8f),new Vector2(-1.2f,4.2f),new Vector2(-1.5f,3f),new Vector2(-0.4f,1.8f),new Vector2(0f,-0.045f),new Vector2(3.79f,-0.045f),new Vector2(3.79f,-0.7f),new Vector2(3.6f,-1.6f),new Vector2(5.57f,-1.6f),new Vector2(5.57f,-2.15f),new Vector2(4.8f,-2.6f),new Vector2(4.2f,-3.2f),new Vector2(7.1f,-3.2f),new Vector2(7.1f,-3.8f),new Vector2(5.4f,-4.4f),new Vector2(5f,-4.8f),new Vector2(8.49f,-4.8f),new Vector2(8.49f,-5.4f),new Vector2(6.8f,-6.1f),new Vector2(6.2f,-6.4f),new Vector2(9.82f,-6.4f),new Vector2(9.82f,-7f),new Vector2(7.7f,-7.8f),new Vector2(7.2f,-8f),new Vector2(10.9f,-8f),new Vector2(10.9f,-8.6f),new Vector2(8.8f,-9.2f),new Vector2(7.8f,-10f),new Vector2(13f,-11f),new Vector2(13f,-16f),new Vector2(-14f,-16f)};
    public static readonly Vector2[] Right=new[]{new Vector2(7f,0f),new Vector2(43f,0f),new Vector2(43f,-18f),new Vector2(27.3f,-18f),new Vector2(27.3f,-10f),new Vector2(27.37f,-9f),new Vector2(26.92f,-8f),new Vector2(27.1f,-7.2f),new Vector2(26.55f,-6.4f),new Vector2(26f,-6.04f),new Vector2(25f,-5.88f),new Vector2(24f,-5.96f),new Vector2(22f,-5.76f),new Vector2(20f,-5.9f),new Vector2(18f,-5.41f),new Vector2(16f,-5.74f),new Vector2(14f,-5.33f),new Vector2(12f,-5.8f),new Vector2(11.53f,-5.5f),new Vector2(10.01f,-4.5f),new Vector2(9.23f,-3f),new Vector2(8.04f,-1.6f),new Vector2(7f,-0.7f)};
    public static readonly Vector2[] FloorLeft=new[]{new Vector2(-1f,-10f),new Vector2(18.06f,-10f),new Vector2(18.07f,-10.3f),new Vector2(17.95f,-10.8f),new Vector2(17.49f,-11.4f),new Vector2(17.77f,-13f),new Vector2(17.27f,-14.5f),new Vector2(17.44f,-16f),new Vector2(17.55f,-18f),new Vector2(17.68f,-20f),new Vector2(-1f,-20f)};
    public static readonly Vector2[] FloorRight=new[]{new Vector2(20.34f,-10f),new Vector2(35f,-10f),new Vector2(35f,-20f),new Vector2(20.55f,-20f),new Vector2(20.4f,-18f),new Vector2(20.36f,-16f),new Vector2(20.15f,-14.5f),new Vector2(20.64f,-13f),new Vector2(20.06f,-11.4f),new Vector2(20.15f,-10.8f),new Vector2(20.28f,-10.3f)};
}
