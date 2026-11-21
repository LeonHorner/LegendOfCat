using UnityEngine;

public sealed partial class SceneryRenderer
{
    Transform DescentPainting(string asset, float x, float y, float width, float height, float depth)
    {
        var item = Quad();
        item.name = "Permanent descent " + asset;
        item.transform.SetParent(transform, false);
        item.transform.localPosition = new Vector3(UpperRoute.Origin + x, y, depth);
        item.transform.localScale = new Vector3(width, height, 1);
        item.GetComponent<MeshRenderer>().sharedMaterial = terrainMaterial;
        var properties = new MaterialPropertyBlock();
        properties.SetTexture("_MainTex", Resources.Load<Texture2D>("GoldenRiver/Descent/" + asset + (asset.EndsWith("-v2")||asset.EndsWith("-v3")?"":"-v1")));
        properties.SetFloat("_Backdrop", asset == "rear-rock" ? 1 : 0);
        item.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
        return item.transform;
    }

    public void AddDescentTerrain(Vector3 platform)
    {
        bool niche = Mathf.Abs(platform.x - UpperRoute.Platforms[UpperRoute.LookoutIndex].x) < .01f
            && Mathf.Abs(platform.y - UpperRoute.Platforms[UpperRoute.LookoutIndex].y) < .01f;
        string asset = niche ? "floor" : platform.z <= 3 ? "cave-edge-short-v2" : platform.z <= 5 ? "cave-edge-medium-v2" : "cave-edge-wide-v2";
        float contactUV = niche ? .70f : platform.z <= 3 ? .56f : platform.z <= 5 ? .50f : .525f;
        float bottomUV = niche ? .14f : platform.z <= 3 ? .25f : platform.z <= 5 ? .20f : .31f;
        float height = (niche ? 3f : .5f) / (contactUV - bottomUV);
        var visual = DescentPainting(asset, platform.x - UpperRoute.Origin,
            platform.y + .75f + height * (.5f - contactUV), platform.z / .94f, height, 1);
        TerrainVisuals.Add(visual);
    }

    readonly System.Collections.Generic.List<Vector3>[] caveVertices = {
        new System.Collections.Generic.List<Vector3>(),new System.Collections.Generic.List<Vector3>(),
        new System.Collections.Generic.List<Vector3>(),new System.Collections.Generic.List<Vector3>()
    };
    readonly System.Collections.Generic.List<int>[] caveTriangles = {
        new System.Collections.Generic.List<int>(),new System.Collections.Generic.List<int>(),
        new System.Collections.Generic.List<int>(),new System.Collections.Generic.List<int>()
    };
    readonly System.Collections.Generic.List<Vector2[]> caveSolids=new System.Collections.Generic.List<Vector2[]>();
    Texture2D caveDistances;
    void RockPolygon(string name,Vector2[] points,float depth,bool solid,float backdrop=0)
    {
        if(CaveGeometry.Area(points)<.0001f) return;
        int group=backdrop==1?0:depth<0?1:backdrop>0?2:3;
        int start=caveVertices[group].Count;
        foreach(var p in points) caveVertices[group].Add(new Vector3(UpperRoute.Origin+p.x,p.y,depth));
        for(int i=0;i<points.Length-2;i++) {
            caveTriangles[group].Add(start);caveTriangles[group].Add(start+i+1);caveTriangles[group].Add(start+i+2);
        }
        if(solid) {
            caveSolids.Add(points);
            var item=new GameObject(name);item.transform.SetParent(transform,false);
            var collider=item.AddComponent<PolygonCollider2D>();
            var outline=new Vector2[points.Length];for(int i=0;i<points.Length;i++) outline[i]=points[i]+Vector2.right*UpperRoute.Origin;
            collider.points=outline;
        }
    }
    void FinishCaveMeshes()
    {
        caveDistances=CaveShading.Create(caveSolids,UpperRoute.MaskBounds);
        terrainMaterial.SetTexture("_CaveBoundary",caveDistances);
        float[] shade={1,.15f,.1f,0};
        for(int group=0;group<4;group++) {
            if(caveVertices[group].Count==0) continue;
            var mesh=new Mesh {name="Connected cave stone "+group};
            mesh.SetVertices(caveVertices[group]);mesh.SetTriangles(caveTriangles[group],0);
            mesh.uv=new Vector2[caveVertices[group].Count];mesh.RecalculateBounds();terrainMeshes.Add(mesh);
            var item=new GameObject(mesh.name,typeof(MeshFilter),typeof(MeshRenderer));item.transform.SetParent(transform,false);
            item.GetComponent<MeshFilter>().sharedMesh=mesh;item.GetComponent<MeshRenderer>().sharedMaterial=terrainMaterial;
            var properties=new MaterialPropertyBlock();
            properties.SetTexture("_MainTex",Resources.Load<Texture2D>("GoldenRiver/Descent/"+(group==0?"rear-rock-v1":"cliff-stone-v2")));
            properties.SetFloat("_WorldMapped",1);properties.SetFloat("_Backdrop",shade[group]);
            properties.SetFloat("_UseCaveBoundary",1);properties.SetFloat("_CaveLayer",group==0?0:1);
            item.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
            caveVertices[group].Clear();caveTriangles[group].Clear();
        }
        caveSolids.Clear();
    }
    static float WallInset(float y,float top,float seed)
    {
        float depth=top-y;
        return depth<=.001f?0:Mathf.Min(1,depth)*(.24f+.16f*Mathf.Sin(y*1.7f+seed));
    }
    static Vector2[] Rectangle(float left,float bottom,float right,float top) => new[] {
        new Vector2(left,bottom),new Vector2(right,bottom),new Vector2(right,top),new Vector2(left,top)
    };

    public void CreateDescentScenery()
    {
        // A continuous lined rock volume replaces the exposed rectangular background panels.
        for(float x=UpperRoute.MaskBounds.xMin;x<UpperRoute.MaskBounds.xMax;x+=2) {
            float right=Mathf.Min(x+2,UpperRoute.MaskBounds.xMax);
            var backing=new[] {new Vector2(x,UpperRoute.FoundationBottom),new Vector2(right,UpperRoute.FoundationBottom),
                new Vector2(right,UpperRoute.RearBankTop(right)),new Vector2(x,UpperRoute.RearBankTop(x))};
            RockPolygon("Continuous recessed cave lining",backing,1.8f,false,1);
            float a=Mathf.Min(UpperRoute.CeilingAt(x),UpperRoute.RiverBedTop(x)-.6f);
            float b=Mathf.Min(UpperRoute.CeilingAt(right),UpperRoute.RiverBedTop(right)-.6f);
            var roof=new[] {new Vector2(x,a),new Vector2(right,b),new Vector2(right,UpperRoute.RiverBedTop(right)),new Vector2(x,UpperRoute.RiverBedTop(x))};
            foreach(var piece in CaveGeometry.Carve(roof)) RockPolygon("Solid cave roof and riverbed",piece,-2.05f,true,.15f);
        }
        foreach(var p in UpperRoute.Platforms) {
            float x=p.x-UpperRoute.Origin;
            float top=p.y+.25f;
            for(float y=top;y>UpperRoute.FoundationBottom;y-=.65f) {
                float bottom=Mathf.Max(y-.65f,UpperRoute.FoundationBottom);
                var body=new[] {
                    new Vector2(x-p.z/2+WallInset(bottom,top,x),bottom),
                    new Vector2(x+p.z/2-WallInset(bottom,top,x+2),bottom),
                    new Vector2(x+p.z/2-WallInset(y,top,x+2),y),
                    new Vector2(x-p.z/2+WallInset(y,top,x),y)
                };
                foreach(var piece in CaveGeometry.Carve(body)) RockPolygon("Connected cave footing",piece,1.2f,true,.1f);
            }
        }
        // Entry-bank foundation continues through the left side of the camera envelope.
        foreach(var piece in CaveGeometry.Carve(Rectangle(-4,UpperRoute.FoundationBottom,4,0)))
            RockPolygon("Massive entry bank",piece,1.2f,true);
        RockPolygon("Entry bank buried foundation",Rectangle(-24,UpperRoute.FoundationBottom,-4,-4.8f),1.2f,true);
        // Register the painted grass contact, rather than the transparent canvas edge, to main-bank height zero.
        DescentPainting("entry-bank-v3",-.25f,-4.32f,8.3f,12,1.1f);
        foreach(var portal in UpperRoute.Portals) {
            if(portal.yMin>=-3.1f || portal.yMax<=-3.1f) continue;
            float left=portal.xMin,right=portal.xMax;
            RockPolygon("Natural shore left contact",new[] {new Vector2(left-4,-5.3f),new Vector2(left+.12f,-5.3f),
                new Vector2(left+.08f,-3.9f),new Vector2(left-.22f,-2.9f),new Vector2(left-1.1f,-2.65f),new Vector2(left-4,-2.9f)},-2.1f,true);
            RockPolygon("Natural shore right contact",new[] {new Vector2(right-.12f,-5.3f),new Vector2(right+8,-6.8f),
                new Vector2(right+8,-2.2f),new Vector2(right+2,-1.8f),new Vector2(right+.22f,-2.65f),new Vector2(right-.08f,-3.9f)},-2.1f,true);
        }
        RockPolygon("Fountain chamber entrance overhang",new[] {new Vector2(104,-10.6f),new Vector2(106,-10.6f),
            new Vector2(105.7f,-13.5f),new Vector2(104.6f,-13)},-2.05f,true,.15f);
        RockPolygon("Fountain chamber side overhang",new[] {new Vector2(117.8f,-10.8f),new Vector2(120,-10.8f),
            new Vector2(119.5f,-14.5f),new Vector2(118.5f,-14)},-2.05f,true,.15f);
        FinishCaveMeshes();
        var room=UpperRoute.Platforms[UpperRoute.LookoutIndex];
        const float ruinHeight=7.1f,ruinGroundUV=.045f;
        float contact=room.y+.75f;
        DescentPainting("ruin",room.x-UpperRoute.Origin,contact+ruinHeight*(.5f-ruinGroundUV)-.12f,10.5f,ruinHeight,1.4f);
        DescentPainting("vegetation",room.x-UpperRoute.Origin-4,contact+.3f,2.6f,.87f,1.3f);
    }
}
