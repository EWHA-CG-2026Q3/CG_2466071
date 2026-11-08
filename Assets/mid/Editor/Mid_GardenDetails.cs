using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

public static class Mid_GardenDetails
{
    const string Root="Assets/mid";
    static GameObject Find(string name)=>GameObject.Find(name)??throw new Exception("Missing "+name);
    static Material Mat(string name)
    {
        string path=Root+"/Materials/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};AssetDatabase.CreateAsset(m,path);}
        Undo.RecordObject(m,"Garden details");return m;
    }
    static Texture2D Texture(string file,bool normal=false,bool linear=false)
    {
        string path=Root+"/Textures/"+file;var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;
        importer.sRGBTexture=!linear&&!normal;importer.wrapMode=TextureWrapMode.Repeat;
        importer.filterMode=FilterMode.Bilinear;importer.mipmapEnabled=true;importer.anisoLevel=4;importer.maxTextureSize=1024;importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static GameObject MeshObject(string name,Mesh mesh,Material material,Transform parent)
    {
        var go=GameObject.Find(name);string path=Root+"/Meshes/"+name+".asset";
        var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(saved==null){saved=mesh;AssetDatabase.CreateAsset(saved,path);}
        else
        {
            Undo.RegisterCompleteObjectUndo(saved,"Garden details");saved.Clear();saved.indexFormat=mesh.indexFormat;
            saved.vertices=mesh.vertices;saved.triangles=mesh.triangles;saved.uv=mesh.uv;saved.normals=mesh.normals;saved.tangents=mesh.tangents;saved.bounds=mesh.bounds;
            UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(saved);
        }
        if(go==null){go=new GameObject(name);go.AddComponent<MeshFilter>();go.AddComponent<MeshRenderer>();Undo.RegisterCreatedObjectUndo(go,"Garden details");}
        go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=saved;go.GetComponent<Renderer>().sharedMaterial=material;
        return go;
    }
    [MenuItem("Tools/Mid Garden/Add star holder soil and candle fire")]
    public static void Apply()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(EditorApplication.isPlaying||scene.path!=Root+"/Mid_DarkChristmas.unity")throw new Exception("Open mid scene outside Play mode.");
        var decor=Find("03_ChristmasDecorations").transform;
        var gold=Find("OPT02_SwingingOrnament_0").GetComponent<Renderer>().sharedMaterial;
        var mount=new Mid_GardenBuilder.Draft();
        // 줄기 끝 고리, 촛대 모양 받침, 별 안쪽까지 이어지는 고정 봉 생성
        mount.Tube(new[]{new Vector3(.236f,6.65f,-.24f),new Vector3(.23f,6.91f,-.08f)},.145f,.09f,16);
        mount.Tube(new[]{new Vector3(.23f,6.9f,-.08f),new Vector3(.23f,7.13f,-.08f)},.09f,.22f,24);
        mount.Tube(new[]{new Vector3(.23f,6.9f,-.08f),new Vector3(.23f,7.43f,-.08f)},.045f,.045f,12);
        MeshObject("Star_CandleholderSocket",mount.Mesh("Star_CandleholderSocket"),gold,decor);
        var lip=Mid_RingMesh.Build(32,.23f,.045f,.045f);
        var rim=MeshObject("Star_HolderRim",lip,gold,decor);rim.transform.localPosition=new Vector3(.23f,7.115f,-.08f);
        var star=Find("REQ04_OPT01_RotatingStar");Undo.RecordObject(star.transform,"Seat star in holder");star.transform.localPosition=new Vector3(.23f,7.62f,-.08f);
        Find("StarWarmLight").transform.localPosition=new Vector3(.23f,7.05f,-.48f);

        var soil=Mat("Mat_Mid_PlantingSoil");
        soil.SetTexture("_BaseMap",Texture("Mid_Soil_Color.jpg"));soil.SetColor("_BaseColor",new Color(.9f,.85f,.78f));
        soil.SetTexture("_BumpMap",Texture("Mid_Soil_NormalGL.jpg",true));soil.EnableKeyword("_NORMALMAP");soil.SetFloat("_BumpScale",1.2f);
        soil.SetTexture("_OcclusionMap",Texture("Mid_Soil_Occlusion.jpg",false,true));soil.EnableKeyword("_OCCLUSIONMAP");soil.SetFloat("_OcclusionStrength",.75f);
        soil.SetFloat("_Smoothness",.07f);soil.SetFloat("_Metallic",0);EditorUtility.SetDirty(soil);
        var soilObject=Find("CentralSoil");soilObject.GetComponent<Renderer>().sharedMaterial=soil;
        var soilMesh=soilObject.GetComponent<MeshFilter>().sharedMesh;Undo.RegisterCompleteObjectUndo(soilMesh,"Planar soil UV");
        soilMesh.uv=soilMesh.vertices.Select(v=>new Vector2(v.x,v.z)/2.3f).ToArray();soilMesh.RecalculateTangents();EditorUtility.SetDirty(soilMesh);
        var bark=Find("TwistedTrunkAndBranches").GetComponent<Renderer>().sharedMaterial;
        Undo.RecordObject(bark,"Rough bark");bark.SetFloat("_BumpScale",1.35f);bark.SetFloat("_Smoothness",.065f);bark.SetFloat("_OcclusionStrength",.75f);EditorUtility.SetDirty(bark);

        var candles=Find("CandlePillars");var vertices=candles.GetComponent<MeshFilter>().sharedMesh.vertices;
        if(vertices.Length!=42*32)throw new Exception("Unexpected candle mesh layout.");
        var fireRoot=GameObject.Find("Candle_Fires");
        if(fireRoot==null){fireRoot=new GameObject("Candle_Fires");Undo.RegisterCreatedObjectUndo(fireRoot,"Candle fires");}
        fireRoot.transform.SetParent(decor,false);
        string firePath=Root+"/Materials/Mat_Mid_CandleFire.mat";
        var fire=AssetDatabase.LoadAssetAtPath<Material>(firePath);
        if(fire==null)
        {
            fire=new Material(AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat")){name="Mat_Mid_CandleFire"};
            AssetDatabase.CreateAsset(fire,firePath);
        }
        fire.SetFloat("_Surface",1);fire.SetFloat("_Blend",2);fire.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);fire.SetFloat("_DstBlend",(float)BlendMode.One);
        fire.SetFloat("_ZWrite",0);fire.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");fire.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        fire.SetColor("_BaseColor",Color.white);fire.EnableKeyword("_EMISSION");fire.SetTexture("_EmissionMap",fire.GetTexture("_BaseMap"));fire.SetColor("_EmissionColor",new Color(2.6f,1.15f,.12f));
        fire.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;
        fire.SetOverrideTag("RenderType","Transparent");fire.renderQueue=3000;EditorUtility.SetDirty(fire);
        var flameCore=new Mid_GardenBuilder.Draft();var wicks=new Mid_GardenBuilder.Draft();
        for(int i=0;i<42;i++)
        {
            Vector3 center=Vector3.zero;float top=float.MinValue;
            for(int j=0;j<32;j++){var p=vertices[i*32+j];center+=p;top=Mathf.Max(top,p.y);}center/=32;center.y=top;
            Vector3 world=candles.transform.TransformPoint(center);
            wicks.Tube(new[]{world,world+Vector3.up*.055f},.012f,.008f,6);
            // 작은 불꽃 중심과 위로 흩어지는 반투명 파티클 구성
            flameCore.Sphere(center+Vector3.up*.105f,new Vector3(.042f,.13f,.042f),8,7);
            string name="CandleFire_"+i.ToString("00");var go=GameObject.Find(name);
            if(go==null){go=new GameObject(name);go.AddComponent<ParticleSystem>();Undo.RegisterCreatedObjectUndo(go,"Candle fire");}
            go.transform.SetParent(fireRoot.transform,false);go.transform.position=world+Vector3.up*.035f;
            var ps=go.GetComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);ps.useAutoRandomSeed=false;ps.randomSeed=(uint)(1225+i);
            var main=ps.main;main.duration=1;main.loop=true;main.prewarm=true;main.playOnAwake=true;main.maxParticles=24;
            main.simulationSpace=ParticleSystemSimulationSpace.Local;main.startLifetime=new ParticleSystem.MinMaxCurve(.24f,.42f);main.startSpeed=new ParticleSystem.MinMaxCurve(.18f,.3f);
            main.startSize3D=true;main.startSizeX=new ParticleSystem.MinMaxCurve(.07f,.12f);main.startSizeY=new ParticleSystem.MinMaxCurve(.14f,.23f);main.startSizeZ=.08f;
            main.startColor=new Color(1,.75f,.25f,1);
            var emission=ps.emission;emission.rateOverTime=24;
            var shape=ps.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=6;shape.radius=.012f;shape.rotation=new Vector3(-90,0,0);
            var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.6f),new Keyframe(.25f,1),new Keyframe(1,.05f)));
            var colors=ps.colorOverLifetime;colors.enabled=true;var gradient=new Gradient();
            gradient.SetKeys(new[]{new GradientColorKey(new Color(1,.95f,.6f),0),new GradientColorKey(new Color(1,.42f,.055f),.5f),new GradientColorKey(new Color(.6f,.06f,.01f),1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.9f,.15f),new GradientAlphaKey(.75f,.65f),new GradientAlphaKey(0,1)});colors.color=gradient;
            var noise=ps.noise;noise.enabled=true;noise.strength=.025f;noise.frequency=2;noise.scrollSpeed=.45f;
            var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=fire;renderer.renderMode=ParticleSystemRenderMode.Billboard;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            ps.Simulate(1,true,true,true);
        }
        var wickMat=Mat("Mat_Mid_CandleWick");wickMat.color=new Color(.025f,.018f,.012f);wickMat.SetFloat("_Smoothness",0);EditorUtility.SetDirty(wickMat);
        MeshObject("Candle_Wicks",wicks.Mesh("Candle_Wicks"),wickMat,decor);
        var flameObj=Find("CandleFlames");var coreMesh=flameCore.Mesh("CandleFlames");var existing=flameObj.GetComponent<MeshFilter>().sharedMesh;
        var flameMaterial=flameObj.GetComponent<Renderer>().sharedMaterial;
        Undo.RecordObject(flameMaterial,"Enable candle emission");flameMaterial.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;flameMaterial.EnableKeyword("_EMISSION");EditorUtility.SetDirty(flameMaterial);
        Undo.RegisterCompleteObjectUndo(existing,"Visible candle flames");existing.Clear();existing.indexFormat=coreMesh.indexFormat;existing.vertices=coreMesh.vertices;existing.triangles=coreMesh.triangles;existing.uv=coreMesh.uv;existing.normals=coreMesh.normals;existing.RecalculateBounds();EditorUtility.SetDirty(existing);UnityEngine.Object.DestroyImmediate(coreMesh);
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);EditorApplication.QueuePlayerLoopUpdate();SceneView.RepaintAll();
    }
}
