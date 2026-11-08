using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class Mid_GardenPolish
{
    const string Root="Assets/mid";
    static GameObject Find(string name) => GameObject.Find(name) ?? throw new Exception("Missing object: "+name);
    static Material MaterialOf(string name) => Find(name).GetComponent<Renderer>().sharedMaterial;
    static void CopyMesh(Mesh target,Mesh source)
    {
        Undo.RegisterCompleteObjectUndo(target,"Polish garden");
        target.Clear();target.indexFormat=source.indexFormat;
        target.vertices=source.vertices;target.triangles=source.triangles;target.uv=source.uv;
        target.normals=source.normals;target.tangents=source.tangents;target.bounds=source.bounds;
        EditorUtility.SetDirty(target);
    }
    static Material MakeMaterial(string name,string shader)
    {
        string path=Root+"/Materials/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find(shader)){name=name};AssetDatabase.CreateAsset(m,path);}
        Undo.RecordObject(m,"Polish material");return m;
    }
    static Texture2D ConfigureTexture(string file,bool normal=false,bool linear=false)
    {
        string path=Root+"/Textures/"+file;
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;
        importer.sRGBTexture=!linear&&!normal;
        importer.wrapMode=TextureWrapMode.Repeat;
        importer.filterMode=FilterMode.Bilinear;
        importer.mipmapEnabled=true;importer.anisoLevel=4;importer.maxTextureSize=1024;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static void Emission(Material m,Color c)
    {m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;EditorUtility.SetDirty(m);}

    [MenuItem("Tools/Mid Garden/Apply twilight garden polish")]
    public static void Apply()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(EditorApplication.isPlaying||scene.path!=Root+"/Mid_DarkChristmas.unity")throw new Exception("Open the mid scene outside Play mode.");
        var camera=Camera.main;
        var moonlight=(GameObject.Find("Moonlight")??Find("Directional Light")).GetComponent<Light>();
        var volume=UnityEngine.Object.FindFirstObjectByType<Volume>();
        Undo.RecordObjects(new UnityEngine.Object[]{camera.gameObject,camera,camera.transform,moonlight.gameObject,volume.gameObject},"Name scene essentials");
        camera.name="Main Camera";
        moonlight.name="Directional Light";
        volume.name="Global Volume";
        // Moonlight는 방향광, Moon은 하늘에 보이는 달 오브젝트
        camera.transform.position=new Vector3(12f,9.6f,-21f);
        camera.transform.LookAt(new Vector3(0,3.5f,0));camera.fieldOfView=48;
        camera.clearFlags=CameraClearFlags.Skybox;camera.allowHDR=true;
        var sky=MakeMaterial("Mat_Mid_TwilightSky","CG/Mid_TwilightSky");
        sky.SetColor("_TopColor",new Color(.025f,.055f,.13f));
        sky.SetColor("_HorizonColor",new Color(.24f,.34f,.46f));
        sky.SetColor("_CloudColor",new Color(.19f,.25f,.34f));
        RenderSettings.skybox=sky;
        RenderSettings.fogColor=new Color(.12f,.18f,.26f);RenderSettings.fogDensity=.009f;
        var moon=GameObject.Find("Moon");
        if(moon==null){moon=GameObject.CreatePrimitive(PrimitiveType.Sphere);moon.name="Moon";UnityEngine.Object.DestroyImmediate(moon.GetComponent<Collider>());Undo.RegisterCreatedObjectUndo(moon,"Add moon");}
        moon.transform.SetParent(Find("04_Environment").transform,true);
        moon.transform.position=camera.ViewportToWorldPoint(new Vector3(.23f,.81f,72));
        moon.transform.localScale=Vector3.one*4.4f;
        var moonMat=MakeMaterial("Mat_Mid_Moon","CG/Mid_Moon");
        moonMat.SetColor("_BaseColor",new Color(.7f,.81f,.95f));
        var moonRenderer=moon.GetComponent<Renderer>();moonRenderer.sharedMaterial=moonMat;
        moonRenderer.shadowCastingMode=ShadowCastingMode.Off;moonRenderer.receiveShadows=false;

        // S18: 회백색 나무껍질에 노멀·오클루전 적용, 반복·필터링·밉맵 설정
        var bark=MaterialOf("TwistedTrunkAndBranches");Undo.RecordObject(bark,"Bark textures");
        bark.SetTexture("_BaseMap",ConfigureTexture("Mid_FrostBark_Base.jpg"));
        bark.SetTexture("_BumpMap",ConfigureTexture("Mid_Bark_NormalGL.jpg",true));
        bark.SetTexture("_OcclusionMap",ConfigureTexture("Mid_Bark_Occlusion.jpg",false,true));
        bark.SetTextureScale("_BaseMap",Vector2.one);
        bark.SetColor("_BaseColor",new Color(.84f,.88f,.92f));
        bark.EnableKeyword("_NORMALMAP");bark.EnableKeyword("_OCCLUSIONMAP");
        bark.SetFloat("_BumpScale",.75f);bark.SetFloat("_OcclusionStrength",.55f);
        bark.SetFloat("_Metallic",0);bark.SetFloat("_Smoothness",.2f);EditorUtility.SetDirty(bark);

        // 어두운 곳에서도 빨강과 금색 장식이 보이도록 밝기 조절
        var red=MaterialOf("OPT02_SwingingOrnament_1");Undo.RecordObject(red,"Brighten ornaments");
        red.SetColor("_BaseColor",new Color(.83f,.075f,.13f));red.SetFloat("_Metallic",.22f);red.SetFloat("_Smoothness",.62f);
        Emission(red,new Color(.11f,.006f,.013f));
        var gold=MaterialOf("OPT02_SwingingOrnament_0");Undo.RecordObject(gold,"Brighten ornaments");
        gold.SetColor("_BaseColor",new Color(1f,.66f,.22f));gold.SetFloat("_Metallic",.38f);gold.SetFloat("_Smoothness",.65f);
        Emission(gold,new Color(.09f,.042f,.006f));
        var fairy=MakeMaterial("Mat_Mid_FairyLightEmission","Universal Render Pipeline/Lit");
        fairy.SetColor("_BaseColor",new Color(1,.74f,.28f));Emission(fairy,new Color(12,6,1.8f));
        Find("ChristmasFairyLights").GetComponent<Renderer>().sharedMaterial=fairy;
        var wireMat=MakeMaterial("Mat_Mid_GarlandEmission","Universal Render Pipeline/Lit");
        wireMat.SetColor("_BaseColor",new Color(1,.55f,.13f));Emission(wireMat,new Color(9f,4f,.8f));
        Find("GarlandWire").GetComponent<Renderer>().sharedMaterial=wireMat;
        var wire=new Mid_GardenBuilder.Draft();var spiral=new Vector3[151];
        for(int i=0;i<spiral.Length;i++){float t=i/(float)(spiral.Length-1),a=t*Mathf.PI*6,r=3.1f*(1-t)+.35f;spiral[i]=new Vector3(r*Mathf.Cos(a),2.5f+t*4.15f,r*Mathf.Sin(a));}
        wire.Tube(spiral,.032f,.032f,6);
        var wireMesh=wire.Mesh("GarlandWire");CopyMesh(Find("GarlandWire").GetComponent<MeshFilter>().sharedMesh,wireMesh);UnityEngine.Object.DestroyImmediate(wireMesh);
        var cameraData=camera.GetUniversalAdditionalCameraData();cameraData.renderPostProcessing=true;cameraData.volumeLayerMask|=1<<volume.gameObject.layer;
        if(!volume.sharedProfile.TryGet<Bloom>(out var bloom))bloom=volume.sharedProfile.Add<Bloom>(true);
        Undo.RecordObject(bloom,"Visible warm glow");bloom.intensity.Override(.85f);bloom.threshold.Override(.9f);bloom.scatter.Override(.65f);
        EditorUtility.SetDirty(bloom);EditorUtility.SetDirty(volume.sharedProfile);

        // 다리 양 끝이 이어지도록 바깥 원과 안쪽 원을 같은 비율로 확장
        var architecture=Find("01_StoneGarden").transform;Undo.RecordObject(architecture,"Widen garden");
        architecture.localScale=new Vector3(1.16f,1,1.16f);
        foreach(string name in new[]{"CandlePillars","CandleFlames","CandleHolders","WaterChannel"})
        {var tr=Find(name).transform;Undo.RecordObject(tr,"Match widened garden");tr.localScale=new Vector3(1.16f,1,1.16f);}
        for(int i=0;i<14;i+=2)
        {var tr=Find("CandlePool_"+i).transform;float a=2*Mathf.PI*i/14;tr.localPosition=new Vector3(3.53f*Mathf.Cos(a)*1.16f,1.57f,3.53f*Mathf.Sin(a)*1.16f);}
        Directory.CreateDirectory(Root+"/Editor/Baselines");AssetDatabase.Refresh();
        foreach(string name in new[]{"ArchedBridgeDeck","BridgeRailCore","BridgeMasonry"})
        {
            var filter=Find(name).GetComponent<MeshFilter>();string path=Root+"/Editor/Baselines/"+name+".asset";
            var original=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(original==null){original=UnityEngine.Object.Instantiate(filter.sharedMesh);AssetDatabase.CreateAsset(original,path);}
            var mesh=UnityEngine.Object.Instantiate(original);var vertices=mesh.vertices;
            for(int i=0;i<vertices.Length;i++){float t=Mathf.Clamp01((vertices[i].z+7.6f)/4.2f);vertices[i].y-=.13f*Mathf.Sin(t*Mathf.PI);}
            mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();CopyMesh(filter.sharedMesh,mesh);UnityEngine.Object.DestroyImmediate(mesh);
            var collider=filter.GetComponent<MeshCollider>();if(collider!=null){collider.sharedMesh=null;collider.sharedMesh=filter.sharedMesh;}
        }

        // S15 셰이더 그래프 복제, 시간 × 흐름 방향 → UV 이동 → 물 텍스처 두 장 혼합
        var flow=MakeMaterial("Mat_Mid_FlowWater","CG/Mid_WaterFlow");
        flow.SetTexture("_MainTex1",ConfigureTexture("Mid_Water1_Seamless.png"));
        flow.SetTexture("_MainTex2",ConfigureTexture("Mid_Water2_Seamless.png"));
        flow.SetVector("_Flow1",new Vector4(.018f,.028f,0,0));
        flow.SetVector("_Flow2",new Vector4(-.014f,.019f,0,0));
        flow.SetVector("_Tiling",new Vector4(3,3,0,0));flow.SetFloat("_Blend",.45f);
        flow.SetVector("_WaterTint",new Vector4(.22f,.34f,.42f,1));
        Find("WaterChannel").GetComponent<Renderer>().sharedMaterial=flow;
        // 부채꼴 경계에서 물 무늬가 끊기지 않도록 x/z 좌표를 평면 UV로 사용
        const int segments=192;var waterMesh=new Mesh{name="Mid_ContinuousWaterSurface"};
        var positions=new Vector3[segments+1];var uv=new Vector2[segments+1];var triangles=new int[segments*3];
        positions[0]=new Vector3(0,.2f,0);uv[0]=Vector2.one*.5f;
        for(int i=0;i<segments;i++)
        {float a=i*Mathf.PI*2/segments;positions[i+1]=new Vector3(6.12f*Mathf.Cos(a),.2f,6.12f*Mathf.Sin(a));uv[i+1]=new Vector2(positions[i+1].x,positions[i+1].z)/12.24f+Vector2.one*.5f;triangles[i*3]=0;triangles[i*3+1]=(i+1)%segments+1;triangles[i*3+2]=i+1;}
        waterMesh.vertices=positions;waterMesh.uv=uv;waterMesh.triangles=triangles;waterMesh.RecalculateNormals();waterMesh.RecalculateTangents();waterMesh.RecalculateBounds();
        CopyMesh(Find("WaterChannel").GetComponent<MeshFilter>().sharedMesh,waterMesh);UnityEngine.Object.DestroyImmediate(waterMesh);
        foreach(var m in new[]{sky,moonMat,flow,fairy,wireMat})EditorUtility.SetDirty(m);
        EditorSceneManager.MarkSceneDirty(scene);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene);
        EditorApplication.QueuePlayerLoopUpdate();SceneView.RepaintAll();
    }
}
