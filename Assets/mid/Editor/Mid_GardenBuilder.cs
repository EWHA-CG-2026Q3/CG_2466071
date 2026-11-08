using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// 장면 생성용 에디터 코드, 기존 씬 덮어쓰기 방지
public static class Mid_GardenBuilder
{
    const string Root="Assets/mid";
    static Material stone,stoneLight,brick,mortar,ground,bark,leaf,red,gold,glow,wax,water,soil;
    static Transform architecture,tree,decor,environment;
    static System.Random random;
    static int serial;
    static float R(float a,float b){return a+(b-a)*(float)random.NextDouble();}
    static Vector3 Polar(float radius,float angle,float y){return new Vector3(Mathf.Cos(angle)*radius,y,Mathf.Sin(angle)*radius);}

    public class Draft
    {
        public List<Vector3> v=new List<Vector3>(); public List<int> t=new List<int>(); public List<Vector2> uv=new List<Vector2>();
        public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector3 normal)
        {
            if(Vector3.Dot(Vector3.Cross(b-a,c-a),normal)<0){Vector3 tmp=b;b=d;d=tmp;}
            int n=v.Count;v.Add(a);v.Add(b);v.Add(c);v.Add(d);
            uv.Add(new Vector2(0,0));uv.Add(new Vector2(0,1));uv.Add(new Vector2(1,1));uv.Add(new Vector2(1,0));
            t.Add(n);t.Add(n+1);t.Add(n+2);t.Add(n);t.Add(n+2);t.Add(n+3);
        }
        public void Box(Vector3 c,Vector3 s)
        {
            Vector3 x=Vector3.right*s.x/2,y=Vector3.up*s.y/2,z=Vector3.forward*s.z/2;
            Quad(c-x+y-z,c+x+y-z,c+x+y+z,c-x+y+z,Vector3.up);
            Quad(c-x-y-z,c+x-y-z,c+x-y+z,c-x-y+z,Vector3.down);
            Quad(c-x-y-z,c+x-y-z,c+x+y-z,c-x+y-z,Vector3.back);
            Quad(c-x-y+z,c+x-y+z,c+x+y+z,c-x+y+z,Vector3.forward);
            Quad(c-x-y-z,c-x-y+z,c-x+y+z,c-x+y-z,Vector3.left);
            Quad(c+x-y-z,c+x-y+z,c+x+y+z,c+x+y-z,Vector3.right);
        }
        public void Wedge(float inner,float outer,float y,float h,float a,float b)
        {
            Vector3 ai=Polar(inner,a,y),bi=Polar(inner,b,y),ao=Polar(outer,a,y),bo=Polar(outer,b,y),up=Vector3.up*h;
            Vector3 radial=Polar(1,(a+b)/2,0);
            Quad(ao+up,ai+up,bi+up,bo+up,Vector3.up);Quad(ao,bo,bi,ai,Vector3.down);
            Quad(ao,ao+up,bo+up,bo,radial);Quad(ai,bi,bi+up,ai+up,-radial);
            Quad(ai,ai+up,ao+up,ao,new Vector3(Mathf.Sin(a),0,-Mathf.Cos(a)));
            Quad(bi,bo,bo+up,bi+up,new Vector3(-Mathf.Sin(b),0,Mathf.Cos(b)));
        }
        public void Sphere(Vector3 center,Vector3 size,int sides=10,int rings=6)
        {
            for(int r=0;r<rings;r++)for(int i=0;i<sides;i++)
            {
                Vector3[] p=new Vector3[4];int[] rr={r,r+1,r+1,r};int[] ii={i,i,i+1,i+1};
                for(int k=0;k<4;k++){float a=2*Mathf.PI*ii[k]/sides,b=Mathf.PI*rr[k]/rings;p[k]=center+Vector3.Scale(new Vector3(Mathf.Cos(a)*Mathf.Sin(b),Mathf.Cos(b),Mathf.Sin(a)*Mathf.Sin(b)),size);}
                Quad(p[0],p[1],p[2],p[3],(p[0]+p[1]+p[2]+p[3])/4-center);
            }
        }
        public void Tube(Vector3[] path,float start,float end,int sides=7)
        {
            Vector3[][] circles=new Vector3[path.Length][];
            for(int j=0;j<path.Length;j++)
            {
                Vector3 tangent=(path[Mathf.Min(j+1,path.Length-1)]-path[Mathf.Max(j-1,0)]).normalized;
                Vector3 right=Vector3.Cross(tangent,Mathf.Abs(tangent.y)>.95f?Vector3.forward:Vector3.up).normalized;
                Vector3 up=Vector3.Cross(right,tangent).normalized;
                float radius=Mathf.Lerp(start,end,(float)j/(path.Length-1));circles[j]=new Vector3[sides];
                for(int k=0;k<sides;k++){float a=2*Mathf.PI*k/sides;circles[j][k]=path[j]+radius*(right*Mathf.Cos(a)+up*Mathf.Sin(a));}
            }
            for(int j=0;j<path.Length-1;j++)for(int k=0;k<sides;k++)
            {int next=(k+1)%sides;Quad(circles[j][k],circles[j+1][k],circles[j+1][next],circles[j][next],circles[j][k]-path[j]);}
        }
        public Mesh Mesh(string name)
        {
            var m=new Mesh{name=name,indexFormat=IndexFormat.UInt32};m.SetVertices(v);m.SetTriangles(t,0);m.SetUVs(0,uv);m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();return m;
        }
    }

    static GameObject MeshObject(string name,Mesh mesh,Material mat,Transform parent,bool collider=false)
    {
        string path=Root+"/Meshes/"+name+"_"+(serial++)+".asset";
        AssetDatabase.CreateAsset(mesh,path);
        var go=new GameObject(name);go.transform.SetParent(parent,false);
        go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mat;
        if(collider)go.AddComponent<MeshCollider>().sharedMesh=mesh;
        return go;
    }
    static GameObject Object(string name,Draft draft,Material mat,Transform parent,bool collider=false){return MeshObject(name,draft.Mesh(name),mat,parent,collider);}
    static Transform Group(string name,Transform parent=null){var g=new GameObject(name);g.transform.SetParent(parent,false);return g.transform;}
    static Material Mat(string name,Color color,float metallic=0,float smooth=.35f,Color? emission=null)
    {
        var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Mat_Mid_"+name};
        m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smooth);
        if(emission.HasValue){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",emission.Value);}
        AssetDatabase.CreateAsset(m,Root+"/Materials/"+m.name+".mat");return m;
    }
    static void BrickRing(string name,float inner,float outer,float baseY,int rows,int count,float rowHeight,bool entrance=false)
    {
        var d=new[]{new Draft(),new Draft(),new Draft()};
        for(int r=0;r<rows;r++)for(int i=0;i<count;i++)
        {
            float a=2*Mathf.PI*(i+(r%2)*.5f)/count,b=a+2*Mathf.PI/count;
            if(entrance&&Mathf.Abs(Mathf.DeltaAngle((a+b)*Mathf.Rad2Deg/2,270))<10)continue;
            d[(r+i)%3].Wedge(inner,outer,baseY+r*rowHeight,rowHeight-.025f,a+.009f,b-.009f);
        }
        Material[] mats={stone,stoneLight,brick};for(int i=0;i<3;i++)Object(name+"_"+i,d[i],mats[i],architecture,true);
    }
    static void Disk(string name,float radius,float y,Material mat,Transform parent)
    {
        var d=new Draft();for(int i=0;i<96;i++)d.Wedge(.001f,radius,y,.1f,2*Mathf.PI*i/96,2*Mathf.PI*(i+1)/96);
        Object(name,d,mat,parent);
    }
    static void PointLight(string name,Vector3 pos,Color color,float power,float range,Transform parent)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;
        var l=g.AddComponent<Light>();l.type=LightType.Point;l.color=color;l.intensity=power;l.range=range;l.shadows=LightShadows.None;
    }

    [MenuItem("Tools/Mid Garden/Create new scene")]
    public static string Build()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play mode first.");
        var existing=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(existing.isDirty)throw new Exception("Current scene has unsaved edits. Save it first.");
        if(File.Exists(Root+"/Mid_DarkChristmas.unity"))throw new Exception("Mid scene already exists; construction will not overwrite it.");
        random=new System.Random(1225);serial=0;
        foreach(string f in new[]{"Scenes","Materials","Meshes","Settings"})Directory.CreateDirectory(Root+"/"+f);
        AssetDatabase.Refresh();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        architecture=Group("01_StoneGarden");tree=Group("02_WeepingWillow");decor=Group("03_ChristmasDecorations");environment=Group("04_Environment");
        stone=Mat("BlueStone",new Color(.24f,.27f,.33f),.08f,.38f);
        stoneLight=Mat("StoneCaps",new Color(.37f,.39f,.43f),.08f,.4f);
        brick=Mat("OldBrick",new Color(.28f,.22f,.25f),0,.25f);
        mortar=Mat("Mortar",new Color(.12f,.14f,.18f));
        ground=Mat("Paving",new Color(.23f,.26f,.31f),.05f,.5f);
        bark=Mat("BlackBark",new Color(.10f,.075f,.09f),.05f,.28f);
        leaf=Mat("SilverWillow",new Color(.72f,.68f,.78f),.15f,.42f,new Color(.025f,.02f,.035f));
        red=Mat("Burgundy",new Color(.30f,.018f,.05f),.55f,.8f);
        gold=Mat("AntiqueGold",new Color(.67f,.36f,.09f),.7f,.6f);
        glow=Mat("WarmGlow",new Color(1,.65f,.23f),0,.4f,new Color(5,2.4f,.6f));
        wax=Mat("IvoryWax",new Color(.72f,.62f,.42f),0,.35f);
        water=Mat("MidnightWater",new Color(.035f,.095f,.13f),.65f,.92f);
        soil=Mat("DarkSoil",new Color(.055f,.045f,.065f),0,.1f);

        // 반복문으로 만든 원형 화단 생성
        var ring=MeshObject("REQ01_LoopRingPlanter",Mid_RingMesh.Build(64,3.75f,3.2f,.65f),mortar,architecture,true);
        ring.transform.localPosition=Vector3.up*.25f;ring.AddComponent<Mid_RingMesh>();
        BrickRing("CentralPlanterBricks",3.19f,3.77f,.27f,3,44,.21f);
        BrickRing("CentralCapstones",3.14f,3.84f,.9f,1,44,.12f);
        Disk("CentralSoil",3.2f,.65f,soil,architecture);
        var terrace=Mid_RingMesh.Build(96,11.8f,6.1f,.45f);
        MeshObject("CircularTerrace",terrace,mortar,architecture,true);
        BrickRing("CanalEdge",6.02f,6.35f,.25f,2,72,.18f);
        // 줄눈이 보이도록 작은 부채꼴 포장석 반복 배치
        var paving=new[]{new Draft(),new Draft()};
        for(int row=0;row<8;row++)for(int i=0;i<84;i++)
        {float a=2*Mathf.PI*(i+(row%2)*.5f)/84; paving[(row+i)%2].Wedge(6.4f+row*.65f,7.02f+row*.65f,.45f,.07f,a+.006f,a+2*Mathf.PI/84-.006f);}
        Object("TerracePaversA",paving[0],ground,architecture);Object("TerracePaversB",paving[1],stone,architecture);
        BrickRing("BoundaryWall",11.65f,12.1f,.45f,5,96,.26f,true);
        BrickRing("BoundaryCoping",11.55f,12.18f,1.75f,1,96,.16f,true);
        Disk("WaterChannel",6.12f,.1f,water,environment);
        Disk("GroundUnderGarden",26,-.35f,soil,environment);
        BuildBridge();BuildWillow();BuildCandles();BuildGifts();BuildLighting();
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,Root+"/Mid_DarkChristmas.unity");
        Selection.activeGameObject=GameObject.Find("REQ01_LoopRingPlanter");
        if(SceneView.lastActiveSceneView!=null)SceneView.lastActiveSceneView.LookAt(new Vector3(0,3,0),Quaternion.Euler(22,0,0),18);
        return "Created Assets/mid/Mid_DarkChristmas.unity";
    }

    static float BridgeY(float t){return .56f+.83f*Mathf.Sin(Mathf.PI*t);}
    static void BuildBridge()
    {
        var deck=new Draft();var rails=new Draft();var caps=new Draft();
        for(int i=0;i<24;i++)
        {
            float t=i/24f,u=(i+1)/24f,z=Mathf.Lerp(-7.6f,-3.4f,t),zz=Mathf.Lerp(-7.6f,-3.4f,u);
            float y=BridgeY(t),yy=BridgeY(u);
            deck.Quad(new Vector3(-1.05f,y,z),new Vector3(1.05f,y,z),new Vector3(1.05f,yy,zz),new Vector3(-1.05f,yy,zz),Vector3.up);
            for(int s=-1;s<=1;s+=2)
            {
                rails.Quad(new Vector3(s*1.1f,y-.2f,z),new Vector3(s*1.1f,yy-.2f,zz),new Vector3(s*1.1f,yy+.65f,zz),new Vector3(s*1.1f,y+.65f,z),Vector3.right*s);
                for(int r=0;r<3;r++)caps.Box(new Vector3(s*1.13f,(y+yy)/2+r*.22f+.12f,(z+zz)/2),new Vector3(.33f,.2f,(zz-z)*.92f));
                caps.Box(new Vector3(s*1.13f,(y+yy)/2+.78f,(z+zz)/2),new Vector3(.44f,.13f,(zz-z)*.96f));
            }
            // 다리 상판에 가로 방향 돌 배치
            for(int c=0;c<4;c++)caps.Box(new Vector3(-.78f+c*.52f,(y+yy)/2+.025f,(z+zz)/2),new Vector3(.49f,.045f,(zz-z)*.92f));
        }
        Object("ArchedBridgeDeck",deck,stone,architecture,true);Object("BridgeRailCore",rails,mortar,architecture);
        Object("BridgeMasonry",caps,stoneLight,architecture,true);
    }

    static void BuildWillow()
    {
        var wood=new Draft();var strands=new Draft();var leaves=new Draft();var bulbs=new Draft();var wire=new Draft();
        var main=new Vector3[18];
        for(int i=0;i<main.Length;i++){float t=i/(float)(main.Length-1);main[i]=new Vector3(.36f*Mathf.Sin(t*7),.7f+t*6.2f,.25f*Mathf.Sin(t*5));}
        wood.Tube(main,.83f,.12f,12);
        for(int root=0;root<10;root++)
        {
            float a=root*2*Mathf.PI/10;var points=new Vector3[8];
            for(int j=0;j<8;j++){float t=j/7f;points[j]=Polar(2.9f*t,a+.16f*Mathf.Sin(t*4),.72f+1.6f*(1-t)*(1-t));}
            wood.Tube(points,.36f,.025f,8);
        }
        for(int arm=0;arm<11;arm++)
        {
            float a=arm*2.39996f, reach=R(2.7f,4.25f),baseHeight=3.1f+(arm%4)*.62f;
            var points=new Vector3[12];
            for(int j=0;j<12;j++){float t=j/11f;points[j]=Polar(reach*t,a+.16f*Mathf.Sin(t*4),baseHeight+2.1f*t-.85f*t*t);}
            wood.Tube(points,.27f,.035f,8);
            for(int hang=0;hang<12;hang++)
            {
                float t=R(.55f,1.02f);Vector3 start=Polar(reach*t,a+R(-.30f,.30f),baseHeight+2.1f*t-.85f*t*t+R(-.08f,.25f));
                float length=R(1.8f,3.5f);var trail=new Vector3[12];
                for(int j=0;j<12;j++){float u=j/11f;trail[j]=start+new Vector3(.16f*Mathf.Sin(u*4+hang)*u,-length*u,.13f*Mathf.Sin(u*5+arm)*u);}
                strands.Tube(trail,.019f,.006f,4);
                for(int k=0;k<10;k++)
                {
                    Vector3 c=Vector3.Lerp(trail[k],trail[k+1],.5f);float side=k%2==0?1:-1;
                    leaves.Quad(c,c+new Vector3((.085f*.43f)*side,-.07f,(.018f*.43f)),c+new Vector3((.02f*.43f)*side,-.22f,0),c+new Vector3((-.025f*.43f)*side,-.065f,-(.018f*.43f)),Vector3.forward);
                    leaves.Quad(c,c+new Vector3((.085f*.43f)*side,-.07f,(.018f*.43f)),c+new Vector3((.02f*.43f)*side,-.22f,0),c+new Vector3((-.025f*.43f)*side,-.065f,-(.018f*.43f)),Vector3.back);
                }
                if(hang%4==0)for(int k=0;k<7;k++)bulbs.Sphere(Vector3.Lerp(trail[0],trail[10],k/7f),Vector3.one*.035f,6,4);
            }
            // 끈과 공을 한 메시로 묶어 별과 다른 흔들림 적용
            Vector3 anchor=points[8]+Vector3.down*.08f;
            var ornament=new Draft();float len=R(.65f,1.2f);
            ornament.Tube(new[]{Vector3.zero,Vector3.down*len},.012f,.012f,5);
            ornament.Sphere(Vector3.down*(len+.16f),Vector3.one*R(.19f,.27f),14,9);
            var go=Object("OPT02_SwingingOrnament_"+arm,ornament,arm%3==0?gold:red,decor);
            go.transform.localPosition=anchor;var swing=go.AddComponent<Mid_OrnamentSwing>();swing.phase=arm*.8f;swing.frequency=.24f+arm*.008f;swing.amplitude=10+arm%4*2;
        }
        // 나무 둘레에 전구 가랜드 배치
        var garland=new Vector3[101];
        for(int i=0;i<=100;i++){float t=i/100f;garland[i]=Polar(3.1f*(1-t)+.35f,t*Mathf.PI*6,2.5f+t*4.15f);if(i%2==0)bulbs.Sphere(garland[i],Vector3.one*.045f,6,4);}
        wire.Tube(garland,.012f,.012f,4);
        Object("TwistedTrunkAndBranches",wood,bark,tree);Object("CascadingSilverTwigs",strands,leaf,tree);Object("SilverWillowLeaves",leaves,leaf,tree);
        Object("ChristmasFairyLights",bulbs,glow,decor);Object("GarlandWire",wire,gold,decor);
        var star=MeshObject("REQ04_OPT01_RotatingStar",Mid_StarMesh.Build(.68f,.29f,.16f),gold,decor);
        star.transform.localPosition=new Vector3(.23f,7.45f,-.08f);star.AddComponent<Mid_StarMesh>();star.AddComponent<Mid_StarRotation>();
        PointLight("StarWarmLight",new Vector3(.1f,7.2f,-.6f),new Color(1,.66f,.26f),3,5,environment);
    }

    static void BuildCandles()
    {
        var candles=new Draft();var flames=new Draft();var holders=new Draft();
        for(int i=0;i<14;i++)
        {
            float a=2*Mathf.PI*i/14;
            Vector3 basePos=Polar(i%2==0?3.53f:6.2f,a,i%2==0?1.02f:.65f);
            for(int j=0;j<3;j++)
            {
                Vector3 p=basePos+new Vector3((j-1)*.16f,0,j%2*.12f);float h=R(.24f,.65f);
                candles.Tube(new[]{p,p+Vector3.up*h},.055f,.055f,8);
                holders.Sphere(p, new Vector3(.08f,.025f,.08f),8,4);
                flames.Sphere(p+Vector3.up*(h+.055f),new Vector3(.035f,.08f,.035f),7,5);
            }
            if(i%2==0)PointLight("CandlePool_"+i,basePos+Vector3.up*.55f,new Color(1,.58f,.23f),2.3f,4,environment);
        }
        Object("CandlePillars",candles,wax,decor);Object("CandleFlames",flames,glow,decor);Object("CandleHolders",holders,gold,decor);
    }
    static void BuildGifts()
    {
        var boxes=new Draft();var ribbons=new Draft();
        for(int i=0;i<7;i++)
        {
            Vector3 size=new Vector3(R(.35f,.65f),R(.3f,.6f),R(.35f,.65f));Vector3 c=Polar(R(1.4f,2.5f),R(3.6f,5.9f),.76f+size.y/2);
            boxes.Box(c,size);ribbons.Box(c,new Vector3(size.x+.012f,size.y+.012f,.065f));ribbons.Box(c,new Vector3(.065f,size.y+.02f,size.z+.012f));
        }
        Object("BurgundyGiftBoxes",boxes,red,decor);Object("GoldGiftRibbons",ribbons,gold,decor);
    }
    static void BuildLighting()
    {
        var lightGo=new GameObject("Moonlight");lightGo.transform.SetParent(environment);lightGo.transform.rotation=Quaternion.Euler(43,-28,0);
        var moon=lightGo.AddComponent<Light>();moon.type=LightType.Directional;moon.color=new Color(.73f,.8f,1);moon.intensity=1.2f;moon.shadows=LightShadows.Soft;
        RenderSettings.sun=moon;RenderSettings.skybox=null;RenderSettings.ambientMode=AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(.19f,.22f,.32f);RenderSettings.ambientEquatorColor=new Color(.12f,.13f,.18f);RenderSettings.ambientGroundColor=new Color(.055f,.05f,.07f);
        var ambientProbe=new SphericalHarmonicsL2();ambientProbe.AddAmbientLight(new Color(.24f,.26f,.33f));RenderSettings.ambientProbe=ambientProbe;
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.055f,.075f,.13f);RenderSettings.fogDensity=.012f;
        PointLight("SilverCanopyFill",new Vector3(0,5,-4),new Color(.78f,.76f,1),3,12,environment);
        var cg=new GameObject("ShotCamera");cg.tag="MainCamera";cg.transform.position=new Vector3(10.8f,8.2f,-17);cg.transform.LookAt(new Vector3(0,3.1f,0));
        var camera=cg.AddComponent<Camera>();camera.fieldOfView=46;camera.nearClipPlane=.1f;camera.farClipPlane=150;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.055f,.075f,.13f);camera.allowHDR=true;
        cg.AddComponent<AudioListener>();cg.AddComponent<Mid_GardenCamera>();var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.volumeLayerMask=1;data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        var profile=ScriptableObject.CreateInstance<VolumeProfile>();profile.name="Mid_NightProfile";
        var bloom=profile.Add<Bloom>(true);bloom.intensity.value=.38f;bloom.threshold.value=1;bloom.scatter.value=.6f;
        var tone=profile.Add<Tonemapping>(true);tone.mode.value=TonemappingMode.ACES;
        var color=profile.Add<ColorAdjustments>(true);color.postExposure.value=.5f;color.contrast.value=8;color.saturation.value=-8;
        var vignette=profile.Add<Vignette>(true);vignette.intensity.value=.18f;vignette.smoothness.value=.55f;
        AssetDatabase.CreateAsset(profile,Root+"/Settings/Mid_NightProfile.asset");
        foreach(var comp in profile.components)AssetDatabase.AddObjectToAsset(comp,profile);
        var vol=Group("Mid_GlobalVolume",environment).gameObject.AddComponent<Volume>();vol.isGlobal=true;vol.sharedProfile=profile;
    }
}
