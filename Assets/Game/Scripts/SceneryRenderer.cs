using UnityEngine;

// Painted parallax, terrain and water renderer for the playable course.
[DefaultExecutionOrder(100)]
public sealed class SceneryRenderer : MonoBehaviour
{
    public const float SkyTravel = 0.05f, ValleyTravel = 0.14f, CanopyTravel = 0.08f;
    public const float FinaleRuinTravel = .22f;
    public Camera View { get; private set; }
    public const float CameraFloor = 1f;
    public static bool IsFloating(int index) => index == 3 || index == 4 || index == 8 || index == 12;
    public CatController Cat { get; private set; }
    // Optional course-owned styling; null retains the original Movement Lab selection.
    public int[] FloatingPlatforms, RootPlatforms;
    public bool RouteLandmarks;
    readonly Transform[] landmarks = new Transform[3];
    public static readonly float[] LandmarkAnchors = { 84, 216, 342 };
    // Travel is the fraction of the camera motion seen on screen.
    static readonly float[] LandmarkTravel = { 0.48f, 0.19f, 0.32f };
    Material landmarkMaterial, fortressMaterial, finaleMaterial;
    Transform finaleSite;
    readonly Transform[] easterEggs = new Transform[3];
    float fishClock, waterCaptureTime = -1;
    readonly Transform[] layers = new Transform[3];
    readonly Material[] materials = new Material[3];
    readonly System.Collections.Generic.List<Mesh> terrainMeshes = new System.Collections.Generic.List<Mesh>();
    Material terrainMaterial, cornerMaterial, riverFrontMaterial;
    Transform riverFront, hillside;
    Mesh quadMesh;
    bool initialized;
    Vector3[] coursePlatforms;
    static readonly float[] TravelFactors = { SkyTravel, ValleyTravel, CanopyTravel };

    public void InitializeForCourse(Camera camera, CatController cat, Vector3[] platforms)
    {
        View = camera;
        Cat = cat;
        coursePlatforms = platforms;
        Initialize();
    }

    static Material PaintedMaterial(string name, string shader)
    {
        var source = Resources.Load<Material>("BackgroundStudy/" + name);
        return source ? new Material(source) : new Material(Shader.Find(shader));
    }

    // Camera-relative travel, calculated absolutely: reversals and teleports cannot accumulate drift.
    public static float Offset(float cameraX, float speed) => cameraX * (1 - speed);
    public static float TileCenter(float cameraX, float speed, float width) =>
        Mathf.Floor(cameraX * speed / width + 0.5f) * width;

    void Initialize()
    {
        if (initialized) return;
        initialized = true;
        // Render-only unit quad: never create a primitive's temporary MeshCollider.
        quadMesh = new Mesh { name = "Scenery unit quad" };
        quadMesh.vertices = new[] { new Vector3(-0.5f, -0.5f), new Vector3(0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f), new Vector3(0.5f, 0.5f) };
        quadMesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
        quadMesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
        quadMesh.RecalculateBounds();
        var atlas = Resources.Load<Texture2D>("BackgroundStudy/painted-atlas");
        var textures = new[] { atlas, Resources.Load<Texture2D>("BackgroundStudy/recessed-valley-v4"),
            Resources.Load<Texture2D>("BackgroundStudy/distant-canopy-v3") };
        for (int i = 0; i < 3; i++) {
            layers[i] = new GameObject("Painted depth " + i).transform;
            layers[i].SetParent(transform, false);
            materials[i] = PaintedMaterial("background-material", "LegendOfCat/BackgroundStudy");
            materials[i].mainTexture = textures[i];
            materials[i].SetFloat("_Cutout", i == 0 ? 0 : 1);
            if (i == 1) {
                materials[i].SetFloat("_Fog", RouteLandmarks ? 0.40f : 0.32f);
                materials[i].SetFloat("_Saturation", RouteLandmarks ? 0.72f : 0.8f);
                if (RouteLandmarks) materials[i].SetColor("_FogTint", new Color(0.72f, 0.73f, 0.81f));
            }
            if (i == 2) {
                materials[i].SetFloat("_Fog", 0.42f);
                materials[i].SetFloat("_Saturation", 0.75f);
                if (RouteLandmarks) materials[i].SetColor("_FogTint", new Color(0.62f, 0.65f, 0.81f));
            }
            if (RouteLandmarks && i > 0) {
                // One authored strip covers the whole route's parallax travel, without recycling.
                materials[i].mainTexture = Resources.Load<Texture2D>("GoldenRiver/" + (i == 1 ? "near-extended-panorama-v1" : "far-extended-panorama-v1"));
                float panoramaHeight = i==1?208f/6:160f/6;
                var panoramaTexture=(Texture2D)materials[i].mainTexture;
                float panoramaWidth=panoramaHeight*panoramaTexture.width/panoramaTexture.height;
                float panoramaLeft=i==1?-17:-20;
                float panoramaCenter=panoramaLeft+panoramaWidth/2;
                float crop = i == 1 ? 0.10f : 0.12f;
                var panorama = Quad();
                panorama.name = i == 1 ? "Continuous valley panorama" : "Continuous canopy panorama";
                panorama.transform.SetParent(layers[i], false);
                panorama.transform.localPosition = new Vector3(panoramaCenter, i == 1 ? 8.2f : 11.8f, i == 1 ? 16 : 24);
                panorama.transform.localScale = new Vector3(panoramaWidth, panoramaHeight, 1);
                // Crop lower terrain below the camera while retaining the painting's native aspect.
                var mesh = Instantiate(quadMesh);
                mesh.name = panorama.name + " mesh";
                if (i == 1) {
                    // Preserve the original introduction/ascent mapping while appending new pixels.
                    float retained=8688f/panoramaTexture.width;
                    float[] columns={-17,45.4f,87,191,panoramaLeft+panoramaWidth};
                    float[] u={0,.25f*retained,.5f*retained,retained,1};
                    var positions=new Vector3[10]; var coordinates=new Vector2[10]; var triangles=new int[24];
                    for(int j=0;j<5;j++) {
                        float local=(columns[j]-panoramaCenter)/panoramaWidth;
                        positions[j]=new Vector3(local,-.5f-crop); positions[j+5]=new Vector3(local,.5f);
                        coordinates[j]=new Vector2(u[j],0); coordinates[j+5]=new Vector2(u[j],1+crop);
                        if(j==4) continue;
                        int t=j*6; triangles[t]=j;triangles[t+1]=j+5;triangles[t+2]=j+1;
                        triangles[t+3]=j+1;triangles[t+4]=j+5;triangles[t+5]=j+6;
                    }
                    mesh.vertices=positions;mesh.uv=coordinates;mesh.triangles=triangles;mesh.RecalculateBounds();
                } else {
                    mesh.vertices = new[] { new Vector3(-0.5f, -0.5f - crop), new Vector3(0.5f, -0.5f - crop),
                        new Vector3(-0.5f, 0.5f), new Vector3(0.5f, 0.5f) };
                    mesh.RecalculateBounds();
                    mesh.uv = new[] { new Vector2(0, 0), new Vector2(1, 0),
                        new Vector2(0, 1 + crop), new Vector2(1, 1 + crop) };
                }
                terrainMeshes.Add(mesh);
                panorama.GetComponent<MeshFilter>().sharedMesh = mesh;
                panorama.GetComponent<MeshRenderer>().sharedMaterial = materials[i];
                continue;
            }
            float width = LayerWidth(i);
            for (int tile = -1; tile <= 1; tile++) {
                var panel = Quad();
                panel.name = "Mirrored scenery tile " + tile;
                panel.transform.SetParent(layers[i], false);
                panel.transform.localPosition = new Vector3(tile * width, i == 0 ? 0 : -1,
                    i == 0 ? 30 : i == 1 ? 16 : 24);
                // Preserve the painting's proportions instead of stretching thin atlas strips.
                panel.transform.localScale = new Vector3(width, i == 0 ? width / 4.5f : width / 3, 1);
                panel.GetComponent<MeshRenderer>().sharedMaterial = materials[i];
            }
        }
        // Independent foreground water: no texture, geometry or perspective join to the valley.
        riverFront = new GameObject("Foreground water").transform;
        riverFront.SetParent(transform, false);
        riverFrontMaterial = PaintedMaterial("water-material", "LegendOfCat/StudyWater");
        riverFrontMaterial.mainTexture = Resources.Load<Texture2D>(RouteLandmarks ? "GoldenRiver/river-water-v2" : "BackgroundStudy/foreground-river");
        riverFrontMaterial.SetFloat("_Fog", 0.20f);
        riverFrontMaterial.SetFloat("_Saturation", 0.70f);
        // Keep transparent headroom: clipping at the old wave height cuts new crests into a rectangle.
        // Retain the original world/UV mapping for both the lab and route paintings.
        var waterMesh = new Mesh { name = "Visible foreground water" };
        waterMesh.vertices = new[] { new Vector3(-0.5f, -0.5f), new Vector3(0.5f, -0.5f),
            new Vector3(-0.5f, 0), new Vector3(0.5f, 0) };
        waterMesh.uv = new[] { Vector2.zero, Vector2.right, new Vector2(0, 0.5f), new Vector2(1, 0.5f) };
        waterMesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
        waterMesh.RecalculateBounds(); terrainMeshes.Add(waterMesh);
        for (int tile = 0; tile < 3; tile++) {
            var front = Quad();
            front.name = "Foreground water tile " + tile;
            front.transform.SetParent(riverFront, false);
            front.transform.localPosition = new Vector3((tile - 1) * 16, RouteLandmarks ? -0.64f : -0.32f, -2);
            front.transform.localScale = new Vector3(16, 16 / 1.5f, 1);
            front.GetComponent<MeshRenderer>().sharedMaterial = riverFrontMaterial;
            front.GetComponent<MeshFilter>().sharedMesh = waterMesh;
        }
        terrainMaterial = PaintedMaterial("terrain-material", "LegendOfCat/StudyTerrain");
        terrainMaterial.mainTexture = Resources.Load<Texture2D>("BackgroundStudy/painted-pillar-v2");
        var platforms = coursePlatforms;
        for (int index = 0; index < platforms.Length; index++) {
            var p = platforms[index];
            Terrain(p, FloatingAt(index), index);
        }
        if (RouteLandmarks) {
            CreateLandmarks();
            var fortress = Quad();
            fortress.name = "Unique hillside fortress";
            hillside = fortress.transform;
            hillside.SetParent(transform, false);
            hillside.localPosition = new Vector3(800, 7, 10);
            hillside.localScale = new Vector3(70, 70f / 1.5f, 1);
            fortressMaterial = new Material(landmarkMaterial);
            fortressMaterial.mainTexture = Resources.Load<Texture2D>("GoldenRiver/landmark-hillside-fortress-v1");
            fortressMaterial.SetFloat("_Fog", 0.35f);
            fortressMaterial.SetFloat("_Saturation", 0.72f);
            fortress.GetComponent<MeshRenderer>().sharedMaterial = fortressMaterial;
            CreateFinaleScenery();
            CreateEasterEggs();
            CreateRiverRocks();
        }
        cornerMaterial = PaintedMaterial("background-material", "LegendOfCat/BackgroundStudy");
        cornerMaterial.mainTexture = Resources.Load<Texture2D>("BackgroundStudy/foreground-corners");
        cornerMaterial.SetFloat("_Cutout", 1); cornerMaterial.SetFloat("_Fog", 0);
        cornerMaterial.SetFloat("_Corner", 1);
        float halfView = View.orthographicSize * 16 / 9;
        for (int side = 0; side < 2; side++) {
            var corners = Quad();
            corners.name = "Foreground corner foliage " + side;
            corners.transform.SetParent(View.transform, false);
            // Each half of the source retains its aspect ratio. Scaling the entire
            // painting leaves natural leaf contours instead of clipping a straight top.
            corners.transform.localPosition = new Vector3((side == 0 ? -1 : 1) * (halfView - 2.25f), -2.5f, 1);
            corners.transform.localScale = new Vector3(4.5f, 6, 1);
            var renderer = corners.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = cornerMaterial; renderer.sortingOrder = 10;
            var parameters = new MaterialPropertyBlock(); parameters.SetFloat("_CornerSide", side);
            renderer.SetPropertyBlock(parameters);
        }
        ApplyViewport(Screen.width, Screen.height);
        ApplyView(6, CameraFloor);
    }

    public readonly System.Collections.Generic.List<Transform> TerrainVisuals = new System.Collections.Generic.List<Transform>();
    public bool FloatingAt(int index)
    {
        int finaleIndex=index-MovementCourse.LevelOnePlatforms.Length-MovementCourse.AscentPlatforms.Length;
        if(RouteLandmarks && finaleIndex>=0)
            return FinaleRoute.Forms[finaleIndex]!=FinaleRoute.Form.Garden || coursePlatforms[index].z<14;
        return RouteLandmarks && index>=MovementCourse.LevelOnePlatforms.Length && coursePlatforms[index].z<10 ||
            (FloatingPlatforms==null?IsFloating(index):System.Array.IndexOf(FloatingPlatforms,index)>=0);
    }


    public static void MatchFloatingCollider(GameObject platform, Transform visual)
    {
        var properties = new MaterialPropertyBlock(); visual.GetComponent<MeshRenderer>().GetPropertyBlock(properties);
        string name = properties.GetTexture("_MainTex").name;
        Vector2[] underside;
        switch (name) {
            // Flat stone treads avoid converting horizontal input into upward impulses or idle sliding.
            case "finale-rubble-v1": underside=new[]{new Vector2(.64f,.50f),new Vector2(.71f,.50f),new Vector2(.71f,.40f),new Vector2(.87f,.40f),new Vector2(.87f,.34f),new Vector2(.94f,.34f),new Vector2(.94f,.17f),new Vector2(.06f,.17f),new Vector2(.06f,.32f),new Vector2(.15f,.32f),new Vector2(.15f,.42f),new Vector2(.25f,.42f),new Vector2(.25f,.53f),new Vector2(.37f,.53f)};break;
            case "finale-wall-v1": underside = new[] { new Vector2(.97f,.51f),new Vector2(.90f,.32f),new Vector2(.80f,.25f),new Vector2(.67f,.23f),new Vector2(.50f,.22f),new Vector2(.32f,.24f),new Vector2(.17f,.24f),new Vector2(.05f,.46f) }; break;
            case "finale-garden-v1": underside = new[] {new Vector2(.97f,.54f),new Vector2(.90f,.29f),new Vector2(.75f,.22f),new Vector2(.58f,.20f),new Vector2(.40f,.22f),new Vector2(.23f,.25f),new Vector2(.06f,.47f)}; break;
            case "finale-pier-v1": underside = new[] {new Vector2(.95f,.58f),new Vector2(.86f,.28f),new Vector2(.70f,.18f),new Vector2(.51f,.16f),new Vector2(.30f,.19f),new Vector2(.14f,.28f),new Vector2(.05f,.56f)}; break;
            case "shelf-root": underside = new[] { new Vector2(.96f,.60f), new Vector2(.89f,.48f), new Vector2(.73f,.32f), new Vector2(.58f,.20f), new Vector2(.38f,.35f), new Vector2(.19f,.50f), new Vector2(.06f,.55f) }; break;
            case "shelf-root-compact-v3": underside = new[] { new Vector2(.95f,.58f), new Vector2(.81f,.40f), new Vector2(.63f,.25f), new Vector2(.42f,.13f), new Vector2(.24f,.24f), new Vector2(.10f,.43f), new Vector2(.05f,.57f) }; break;
            case "shelf-masonry-compact-v3": underside = new[] { new Vector2(.95f,.53f), new Vector2(.87f,.35f), new Vector2(.76f,.22f), new Vector2(.63f,.23f), new Vector2(.53f,.29f), new Vector2(.40f,.20f), new Vector2(.30f,.18f), new Vector2(.13f,.32f), new Vector2(.05f,.53f) }; break;
            case "shelf-ascent-masonry-v1": underside = new[] { new Vector2(.97f,.49f), new Vector2(.91f,.37f), new Vector2(.76f,.31f), new Vector2(.61f,.24f), new Vector2(.42f,.22f), new Vector2(.24f,.22f), new Vector2(.10f,.23f), new Vector2(.04f,.46f) }; break;
            case "shelf-masonry": underside = new[] { new Vector2(.98f,.55f), new Vector2(.92f,.43f), new Vector2(.85f,.29f), new Vector2(.66f,.31f), new Vector2(.58f,.36f), new Vector2(.42f,.36f), new Vector2(.34f,.39f), new Vector2(.22f,.30f), new Vector2(.12f,.35f), new Vector2(.02f,.55f) }; break;
            default: return;
        }
        var old = platform.GetComponent<BoxCollider2D>(); old.enabled = false;
        var polygon = platform.AddComponent<PolygonCollider2D>();
        var points = new Vector2[underside.Length + 2];
        points[0] = new Vector2(-.5f,.5f); points[1] = new Vector2(.5f,.5f);
        var bounds = visual.GetComponent<MeshFilter>().sharedMesh.bounds;
        for (int i = 0; i < underside.Length; i++) {
            Vector2 uv = underside[i];
            var painted = new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, uv.x), Mathf.Lerp(bounds.min.y, bounds.max.y, uv.y), 0);
            points[i + 2] = platform.transform.InverseTransformPoint(visual.TransformPoint(painted));
        }
        if(name=="finale-rubble-v1") {
            for(int i=0;i<2;i++) {
                var uv=new Vector2(i==0?.37f:.64f,.665f);
                var painted=new Vector3(Mathf.Lerp(bounds.min.x,bounds.max.x,uv.x),Mathf.Lerp(bounds.min.y,bounds.max.y,uv.y),0);
                points[i]=platform.transform.InverseTransformPoint(visual.TransformPoint(painted));
            }
        }
        polygon.points = points;
        Object.Destroy(old);
    }

    public void AddFinaleObstacle(Vector3 platform) => Terrain(platform,true,-2,true);
    public void GroundFinaleObstacle(int index) => RegisteredFinaleGrass(TerrainVisuals[MovementCourse.LevelOnePlatforms.Length+MovementCourse.AscentPlatforms.Length+index],"Rubble footing grass");
    public void AddRecoveryTerrain(Vector3 platform) => Terrain(platform, true, 0);

    void Terrain(Vector3 platform, bool floating, int variant, bool rubble=false)
    {
        // Artwork owns its silhouette; the collider owns the exact standable span.
        bool bank = platform.z >= 6;
        float contact = platform.y + 0.75f;
        // Register contact independently from the authored grass tufts above it.
        bool variedCourse = FloatingPlatforms != null;
        bool distinctShelf = floating && variedCourse;
        bool rootShelf = distinctShelf && (System.Array.IndexOf(RootPlatforms, variant) >= 0 ||
            variant >= MovementCourse.LevelOnePlatforms.Length + 83 && variant < MovementCourse.LevelOnePlatforms.Length + MovementCourse.OverlookIndex);
        int finaleIndex = variant - MovementCourse.LevelOnePlatforms.Length - MovementCourse.AscentPlatforms.Length;
        if (RouteLandmarks && finaleIndex >= 0 && finaleIndex < FinaleRoute.Forms.Count)
            rootShelf = distinctShelf && FinaleRoute.Forms[finaleIndex] == FinaleRoute.Form.Root;
        bool compactShelf = distinctShelf && platform.z <= (rootShelf ? 4.25f : 4.5f);
        string texturePath;
        float contactUV, landingFraction;
        if (distinctShelf) {
            texturePath = "GoldenRiver/" + (rootShelf ? "shelf-root" : "shelf-masonry") + (compactShelf ? "-compact-v3" : "");
            contactUV = rootShelf ? (compactShelf ? 0.70f : 0.68f) : 0.66f;
            landingFraction = rootShelf ? 0.90f : 0.96f;
        } else if (variedCourse) {
            string form = platform.z >= 18 ? "wide" : platform.z >= 10 ? "medium" : "narrow";
            texturePath = "GoldenRiver/bank-" + form + "-v3";
            contactUV = form == "narrow" ? 0.66f : 0.587f;
            landingFraction = form == "narrow" ? 0.94f : 0.89f;
        } else {
            texturePath = floating ? "BackgroundStudy/painted-shelf-v3" : bank ? "BackgroundStudy/painted-bank-v2" : "BackgroundStudy/painted-pillar-v2";
            contactUV = floating ? 0.66f : bank ? 0.587f : 0.748f;
            landingFraction = floating ? 0.90f : bank ? 0.89f : 0.76f;
        }
        bool ascentShelf = distinctShelf && variant >= MovementCourse.LevelOnePlatforms.Length && !compactShelf && !rootShelf;
        if (ascentShelf) { texturePath = "GoldenRiver/shelf-ascent-masonry-v1"; contactUV = 0.62f; landingFraction = 0.90f; }
        bool tallBank = variedCourse && !floating && platform.x > 425 && contact >= 1.5f;
        if (tallBank) { texturePath = "GoldenRiver/bank-tall-ascent-v1"; contactUV = 0.914f; landingFraction = 0.86f; }
        bool finaleArt = RouteLandmarks && finaleIndex >= 0 && finaleIndex < FinaleRoute.Forms.Count && distinctShelf && !rootShelf;
        if(finaleArt) {
            var form=FinaleRoute.Forms[finaleIndex];
            texturePath="GoldenRiver/finale-"+(form==FinaleRoute.Form.Garden?"garden":form==FinaleRoute.Form.Pier?"pier":"wall")+"-v1";
            contactUV=form==FinaleRoute.Form.Garden?.655f:form==FinaleRoute.Form.Pier?.675f:.60f;
            landingFraction=form==FinaleRoute.Form.Pier?.92f:.94f;
        }
        if(rubble) { texturePath="GoldenRiver/finale-rubble-v1"; contactUV=.665f; landingFraction=.90f; }
        float width = platform.z / landingFraction;
        float sourceAspect = compactShelf ? 2 : 3;
        if (tallBank || finaleArt || rubble) {
            var source = Resources.Load<Texture2D>(texturePath);
            sourceAspect = (float)source.width / source.height;
        }
        float bottom = distinctShelf || tallBank ? contact - width / sourceAspect * contactUV : floating ? contact - 0.90f : -4.5f;
        float crown = distinctShelf || tallBank ? contact + width / sourceAspect * (1 - contactUV) : contact + (floating ? 0.35f : 0.60f);
        float half = width * 0.5f;
        var vertices = new[] {
            new Vector3(-half, bottom, 2), new Vector3(half, bottom, 2),
            new Vector3(-half, contact, 2), new Vector3(half, contact, 2),
            new Vector3(-half, crown, 2), new Vector3(half, crown, 2)
        };
        bool mirror = !variedCourse && variant % 3 == 1;
        float left = mirror ? 1 : 0, right = 1 - left;
        var mesh = new Mesh { name = distinctShelf ? (rootShelf ? "Root island" : "Masonry shelf") + (compactShelf ? " compact" : " long") : variedCourse ? "Painted grounded bank" : floating ? "Painted floating rock" : "Painted grounded cliff" };
        mesh.vertices = vertices;
        mesh.uv = new[] { new Vector2(left, 0), new Vector2(right, 0),
            new Vector2(left, contactUV), new Vector2(right, contactUV),
            new Vector2(left, 1), new Vector2(right, 1) };
        mesh.triangles = new[] { 0, 2, 1, 1, 2, 3, 2, 4, 3, 3, 4, 5 };
        mesh.RecalculateBounds(); terrainMeshes.Add(mesh);
        if(finaleArt) mesh.name="Finale " + FinaleRoute.Forms[finaleIndex] + " terrain";
        if(rubble) mesh.name="Grounded rubble";
        var visual = new GameObject(mesh.name); visual.transform.SetParent(transform, false);
        visual.transform.localPosition = new Vector3(platform.x, 0, 0);
        visual.AddComponent<MeshFilter>().sharedMesh = mesh;
        TerrainVisuals.Add(visual.transform);
        var renderer = visual.AddComponent<MeshRenderer>(); renderer.sharedMaterial = terrainMaterial;
        var parameters = new MaterialPropertyBlock();
        parameters.SetTexture("_MainTex", Resources.Load<Texture2D>(texturePath));
        renderer.SetPropertyBlock(parameters);
        if (!variedCourse && !floating && (variant == 0 || variant == 2 || variant == 6 || variant == 10 || variant == 13))
            ShoreFoot(platform, variant == 0 || variant == 10 ? -1 : 1);
    }

    void CreateFinaleScenery()
    {
        finaleMaterial=PaintedMaterial("background-material","LegendOfCat/BackgroundStudy");
        finaleMaterial.SetFloat("_Cutout",1);
        finaleMaterial.SetFloat("_Fog",.28f); finaleMaterial.SetFloat("_Saturation",.86f);
        Transform Painting(string name,string texture,Vector3 center,float width,float fog) {
            var image=Resources.Load<Texture2D>("GoldenRiver/"+texture);
            var item=Quad(); item.name=name; item.transform.SetParent(transform,false);
            item.transform.localPosition=center; item.transform.localScale=new Vector3(width,width*image.height/image.width,1);
            item.GetComponent<MeshRenderer>().sharedMaterial=finaleMaterial;
            var block=new MaterialPropertyBlock();block.SetTexture("_MainTex",image);block.SetFloat("_Fog",fog);block.SetFloat("_Brightness",.96f);
            item.GetComponent<MeshRenderer>().SetPropertyBlock(block);return item.transform;
        }
        float anchor=(FinaleRoute.StartX+FinaleRoute.Last.x)/2;
        finaleSite=Painting("Unique crown hillside","finale-hillside-v1",new Vector3(anchor,7,9),70,.28f);
        var tower=FinaleRoute.Platforms[FinaleRoute.TowerTurn];
        // Full wall foundations continue behind the supported bank, never end in open air.
        Painting("Collapsed watchtower", "finale-watchtower-v1",new Vector3(tower.x+3,8,3),50,.14f);
        var last=FinaleRoute.Last;
        Painting("Sunlit final gateway","finale-gate-v1",new Vector3(last.x+1,last.y+.75f+10f/3-.22f,1),10,.03f);
        var diagonal=FinaleRoute.Platforms[FinaleRoute.DiagonalIndex];
        Painting("Inclined counterweight bearings","finale-guide-v1",new Vector3(diagonal.x,diagonal.y+.08f,3),9,.12f);
        var lift=FinaleRoute.Platforms[FinaleRoute.LiftIndex];
        Painting("Vertical counterweight tower","finale-vertical-guide-v1",new Vector3(lift.x,lift.y+.85f,3),4,.12f);
        // The registered bank crown covers gateway foot seams, using the original pixels.
        var ground=TerrainVisuals[MovementCourse.CompletePlatforms.Length-1];
        RegisteredFinaleGrass(ground,"Gateway ground overlap");
    }

    public void GroundFinaleMechanisms()
    {
        foreach(var p in FinaleRoute.Recoveries) {
            foreach(var ground in TerrainVisuals)
                if(Mathf.Abs(ground.localPosition.x-p.x)<.01f) RegisteredFinaleGrass(ground,"Counterweight footing grass");
        }
    }

    void RegisteredFinaleGrass(Transform ground,string name)
    {
        var source=ground.GetComponent<MeshFilter>().sharedMesh;
        var vertices=source.vertices;var uv=source.uv;
        float bottom=vertices[2].y-.5f;
        float bottomUV=Mathf.Lerp(uv[0].y,uv[2].y,Mathf.InverseLerp(vertices[0].y,vertices[2].y,bottom));
        for(int j=0;j<vertices.Length;j++) vertices[j].z=.5f;
        vertices[0].y=vertices[1].y=bottom;uv[0].y=uv[1].y=bottomUV;
        var mesh=new Mesh{name="Registered gate grass"};mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=source.triangles;mesh.RecalculateBounds();terrainMeshes.Add(mesh);
        var grass=Quad();grass.name=name;grass.transform.SetParent(ground,false);
        grass.GetComponent<MeshFilter>().sharedMesh=mesh;grass.GetComponent<MeshRenderer>().sharedMaterial=terrainMaterial;
        var parameters=new MaterialPropertyBlock();ground.GetComponent<MeshRenderer>().GetPropertyBlock(parameters);grass.GetComponent<MeshRenderer>().SetPropertyBlock(parameters);
    }

    void CreateLandmarks()
    {
        landmarkMaterial = PaintedMaterial("background-material", "LegendOfCat/BackgroundStudy");
        landmarkMaterial.SetFloat("_Cutout", 1);
        landmarkMaterial.SetFloat("_Fog", 0.24f);
        landmarkMaterial.SetFloat("_Saturation", 0.72f);
        var names = new[] { "waterwheel", "viaduct", "willow" };
        var brightness = new[] { 0.91f, 0.91f, 0.80f };
        var fog = new[] { 0.15f, 0.30f, 0.20f };
        var saturation = new[] { 0.83f, 0.85f, 0.88f };
        var depth = new[] { 6f, 13f, 9f };
        var width = new[] { 30f, 34f, 29f };
        var fogTint = new[] {
            new Color(0.80f, 0.79f, 0.73f),
            new Color(0.60f, 0.65f, 0.80f),
            new Color(0.71f, 0.74f, 0.77f)
        };
        for (int i = 0; i < names.Length; i++) {
            var item = Quad();
            item.name = "Unique landmark " + names[i];
            item.transform.SetParent(transform, false);
            item.transform.localPosition = new Vector3(LandmarkAnchors[i], -0.5f, depth[i]);
            // The opaque hillside continues below the viewport; its sides descend under the river.
            item.transform.localScale = new Vector3(width[i], width[i] * 2 / 3, 1);
            item.GetComponent<MeshRenderer>().sharedMaterial = landmarkMaterial;
            var properties = new MaterialPropertyBlock();
            properties.SetTexture("_MainTex", Resources.Load<Texture2D>("GoldenRiver/landmark-" + names[i] + "-hill-v2"));
            properties.SetFloat("_Brightness", brightness[i]);
            properties.SetFloat("_Fog", fog[i]);
            properties.SetColor("_FogTint", fogTint[i]);
            properties.SetFloat("_Saturation", saturation[i]);
            properties.SetFloat("_MillMotion", i == 0 ? 1 : 0);
            properties.SetFloat("_WillowMotion", i == 2 ? 1 : 0);
            item.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
            landmarks[i] = item.transform;
        }
    }

    void CreateEasterEggs()
    {
        // Review anchor only: the statue is painted into the valley so it shares its depth and masonry.
        var king = new GameObject("Easter egg cat king");
        king.transform.SetParent(transform, false);
        easterEggs[0] = king.transform;
        var fish = new GameObject("Easter egg watching fish");
        fish.transform.SetParent(transform, false);
        fish.transform.localPosition = new Vector3(96, -3.30f, -2.2f);
        easterEggs[1] = fish.transform;
        for (int i = 0; i < 2; i++) {
            var item = Quad();
            item.name = i == 0 ? "Drifting fish left" : "Drifting fish right";
            item.transform.SetParent(fish.transform, false);
            item.transform.localScale = new Vector3(0.95f, 0.95f, 1);
            var mesh = Instantiate(quadMesh);
            mesh.name = item.name + " mesh";
            float left = i * 0.5f, right = left + 0.5f;
            mesh.uv = new[] { new Vector2(left, 0), new Vector2(right, 0), new Vector2(left, 1), new Vector2(right, 1) };
            terrainMeshes.Add(mesh);
            item.GetComponent<MeshFilter>().sharedMesh = mesh;
            item.GetComponent<MeshRenderer>().sharedMaterial = landmarkMaterial;
            var properties = new MaterialPropertyBlock();
            properties.SetTexture("_MainTex", Resources.Load<Texture2D>("GoldenRiver/egg-watching-fish"));
            properties.SetFloat("_Fog", 0.02f); properties.SetFloat("_Brightness", 0.95f);
            properties.SetFloat("_Submerged", 1);
            item.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
        }
        // These two belong after the introduction, without blocking the playable plane.
        var signBank = MovementCourse.AscentPlatforms[44];
        easterEggs[2] = Egg("no cats sign", "egg-no-cats-sign", new Vector3(signBank.x + 1, signBank.y + 1.79f, 3), new Vector2(2.2f, 2.2f), true);
        UpdateFish(fishClock);
    }

    Transform Egg(string name, string source, Vector3 position, Vector2 size, bool foreground)
    {
        var item = Quad();
        item.name = "Easter egg " + name;
        item.transform.SetParent(transform, false);
        item.transform.localPosition = position;
        item.transform.localScale = new Vector3(size.x, size.y, 1);
        item.GetComponent<MeshRenderer>().sharedMaterial = foreground ? terrainMaterial : landmarkMaterial;
        var properties = new MaterialPropertyBlock();
        properties.SetTexture("_MainTex", Resources.Load<Texture2D>("GoldenRiver/" + source));
        properties.SetFloat("_Fog", 0.28f); properties.SetFloat("_Brightness", 0.90f);
        item.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
        return item.transform;
    }

    void UpdateFish(float seconds)
    {
        if (!easterEggs[1]) return;
        easterEggs[1].localPosition = new Vector3(96 + 0.18f * Mathf.Sin(seconds * 0.7f), -3.30f, -2.2f);
        for (int i = 0; i < 2; i++) {
            float phase = seconds * 1.6f + i * 1.8f;
            var fish = easterEggs[1].GetChild(i);
            // The current slowly wins, then a short swimming stroke recovers the lost ground.
            float cycle = Mathf.Repeat(seconds / 2.4f + i * 0.43f, 1);
            float recovery = Mathf.SmoothStep(0, 1, Mathf.Clamp01((cycle - 0.76f) / 0.24f));
            float drift = 0.16f * (cycle - recovery);
            fish.localPosition = new Vector3((i == 0 ? -0.48f : 0.48f) + drift,
                0.055f * Mathf.Sin(phase), 0);
            Vector3 target = Cat ? transform.InverseTransformPoint(Cat.transform.position) : Vector3.zero;
            Vector3 position = easterEggs[1].localPosition + fish.localPosition;
            bool watching = Cat && Mathf.Abs(target.x - 96) < 9 && Vector2.Distance(target, position) < 11;
            float facing = watching ? (target.x >= position.x ? 1 : -1) : -1;
            // Both painted heads look upward; turn the silhouette and incline the head toward Mika.
            float nativeFacing = -1;
            fish.localScale = new Vector3(0.95f * facing * nativeFacing, 0.95f, 1);
            float gaze = watching ? Mathf.Clamp((target.x - position.x) * -2, -12, 12) : 0;
            float stroke = Mathf.Sin(recovery * Mathf.PI) * 6;
            fish.localRotation = Quaternion.Euler(0, 0, gaze + facing * stroke + 3 * Mathf.Sin(phase * 0.8f));
        }
    }

    void CreateRiverRocks()
    {
        float[] anchors = { 22, 78, 163, 273, 394, 487, 43, 120, 212, 327, 425, 511, 55, 138, 235, 303, 365, 455 };
        var atlases = new[] {
            Resources.Load<Texture2D>("GoldenRiver/river-rock-groups-v1"),
            Resources.Load<Texture2D>("GoldenRiver/river-rock-groups-v2"),
            Resources.Load<Texture2D>("GoldenRiver/river-rock-groups-v3")
        };
        float[] extraWidths = { 2.6f, 1.85f, 2.4f, 2.8f, 2.1f, 2.65f };
        float[] extraColumns = { 0, 490f / 1536, 1018f / 1536, 1 };
        for (int i = 0; i < anchors.Length; i++) {
            var rock = Quad();
            rock.name = "Unique river rock group " + i;
            rock.transform.SetParent(transform, false);
            int cell = i % 6;
            bool extra = i >= 12;
            rock.transform.localPosition = new Vector3(anchors[i], extra ? -3.82f : -4.47f, -2.3f);
            rock.transform.localScale = extra ? new Vector3(extraWidths[cell], 1.65f, 1) : new Vector3(2.2f, 2.2f, 1);
            var mesh = Instantiate(quadMesh);
            mesh.name = rock.name + " mesh";
            int column = cell % 3;
            float left = extra ? extraColumns[column] : column / 3f;
            float right = extra ? extraColumns[column + 1] : (column + 1) / 3f;
            float bottom = cell < 3 ? 0.5f : 0, top = bottom + 0.5f;
            mesh.uv = new[] { new Vector2(left, bottom), new Vector2(right, bottom),
                new Vector2(left, top), new Vector2(right, top) };
            terrainMeshes.Add(mesh);
            rock.GetComponent<MeshFilter>().sharedMesh = mesh;
            rock.GetComponent<MeshRenderer>().sharedMaterial = landmarkMaterial;
            var properties = new MaterialPropertyBlock();
            properties.SetTexture("_MainTex", atlases[i / 6]);
            if (i >= 6 && i < 12) properties.SetFloat("_AlphaFloor", 0.85f);
            properties.SetFloat("_Fog", 0.08f);
            properties.SetFloat("_Brightness", 0.9f);
            rock.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
        }
    }

    void ShoreFoot(Vector3 platform, int side)
    {
        // A low decorative buttress joins the cliff to its bank below the landing plane.
        var foot = Quad();
        foot.name = "Painted shore footing"; foot.transform.SetParent(transform, false);
        foot.transform.localPosition = new Vector3(platform.x + side * (platform.z * 0.5f + 1.25f), -2.65f, 3);
        foot.transform.localScale = new Vector3(4.8f * side, 2.6f, 1);
        var renderer = foot.GetComponent<MeshRenderer>(); renderer.sharedMaterial = terrainMaterial;
        var parameters = new MaterialPropertyBlock();
        parameters.SetTexture("_MainTex", Resources.Load<Texture2D>("BackgroundStudy/painted-shore-v2"));
        renderer.SetPropertyBlock(parameters);
    }
    GameObject Quad()
    {
        var item = new GameObject("Scenery quad", typeof(MeshFilter), typeof(MeshRenderer));
        item.GetComponent<MeshFilter>().sharedMesh = quadMesh;
        return item;
    }

    static float LayerWidth(int layer) => layer == 0 ? 64 : layer == 1 ? 40 : 48;

    public void ApplyView(float x, float y)
    {
        // Keep the damp-foot tint registered with the placed shore/water grid too.
        terrainMaterial.SetFloat("_CourseOriginY", transform.position.y);
        terrainMaterial.SetFloat("_CourseScale", transform.lossyScale.y);
        View.transform.position = new Vector3(x, y, -10);
        var localView = transform.InverseTransformPoint(new Vector3(x, y, 0));
        // Distant scenery has no physical join to the world-space terrain or foreground water.
        for (int i = 0; i < 3; i++) {
            if (RouteLandmarks && i > 0) {
                layers[i].localPosition = new Vector3(Offset(localView.x, TravelFactors[i]), localView.x <= 425 ? localView.y : 1 + (localView.y - 1) * 0.75f, 0);
                continue;
            }
            float width = LayerWidth(i);
            float center = TileCenter(localView.x, TravelFactors[i], width);
            layers[i].localPosition = new Vector3(Offset(localView.x, TravelFactors[i]) + center, localView.y, 0);
            int tileIndex = Mathf.RoundToInt(center / width);
            for (int j = 0; j < 3; j++) {
                var panel = layers[i].GetChild(j);
                var scale = panel.localScale;
                scale.x = ((tileIndex + j - 1) & 1) == 0 ? width : -width;
                panel.localScale = scale;
            }
        }
        if (RouteLandmarks) {
            for (int i = 0; i < landmarks.Length; i++) {
                var position = landmarks[i].localPosition;
                // Each landmark has one fixed anchor, never participates in tile recycling.
                position.x = LandmarkAnchors[i] + (localView.x - LandmarkAnchors[i]) * (1 - LandmarkTravel[i]);
                landmarks[i].localPosition = position;
            }
            // Register the small statue with the right-hand battlement in the valley painting.
            easterEggs[0].localPosition = new Vector3(Offset(localView.x, ValleyTravel) + 45.28f, localView.y + 2.57f, 12);
            float finaleAnchor=(FinaleRoute.StartX+FinaleRoute.Last.x)/2;
            finaleSite.localPosition=new Vector3(finaleAnchor+(localView.x-finaleAnchor)*(1-FinaleRuinTravel),7,9);
            hillside.localPosition = new Vector3(800 + (localView.x - 800) * 0.75f, 7, 10);
            UpdateFish(waterCaptureTime >= 0 ? waterCaptureTime : fishClock);
        }
        PositionForegroundWater(localView.x);
    }

    void PositionForegroundWater(float cameraX)
    {
        float center = TileCenter(cameraX, 1, 16);
        riverFront.localPosition = new Vector3(center, 0, 0);
        int tileIndex = Mathf.RoundToInt(center / 16);
        for (int tile = 0; tile < 3; tile++) {
            var scale = riverFront.GetChild(tile).localScale;
            scale.x = ((tileIndex + tile - 1) & 1) == 0 ? 16 : -16;
            riverFront.GetChild(tile).localScale = scale;
        }
    }

    public void SetWaterTime(float seconds)
    {
        waterCaptureTime = seconds;
        UpdateFish(seconds >= 0 ? seconds : fishClock);
        riverFrontMaterial.SetFloat("_WaterTime", seconds);
        riverFrontMaterial.SetFloat("_UseCaptureTime", seconds >= 0 ? 1 : 0);
    }

    public void ApplyViewport(int width, int height)
    {
        View.rect = MenuLayout.Letterbox16By9(width, height);
        View.aspect = 16f / 9f;
    }

    void FixedUpdate()
    {
        if (!Cat || Cat.Paused) return;
        if (RouteLandmarks && !Cat.Paused) {
            fishClock += Time.fixedDeltaTime;
            if (waterCaptureTime < 0) UpdateFish(fishClock);
        }

    }

    void LateUpdate()
    {
        if (!View) return;
        ApplyViewport(Screen.width, Screen.height);
        ApplyView(View.transform.position.x, View.transform.position.y);
    }

    void OnDestroy()
    {
        foreach (var material in materials) Release(material);
        foreach (var mesh in terrainMeshes) Release(mesh);
        Release(quadMesh);
        Release(terrainMaterial); Release(cornerMaterial); Release(riverFrontMaterial); Release(landmarkMaterial); Release(fortressMaterial); Release(finaleMaterial);
    }

    static void Release(Object owned)
    {
        if (!owned) return;
        if (Application.isPlaying) Destroy(owned);
        else DestroyImmediate(owned); // Edit-mode previews have no deferred destruction loop.
    }
}
