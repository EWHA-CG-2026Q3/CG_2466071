using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// 수양버들 가지 모양과 서리 색상 수정
public static class Mid_WillowRefinement
{
    // S17: 면마다 끊기지 않고 나무를 따라 이어지는 UV 설정
    static void WoodTube(Mid_GardenBuilder.Draft draft, Vector3[] path, float start, float end, int sides)
    {
        int first=draft.v.Count;float distance=0;
        Vector3 right=Vector3.right;
        for(int j=0;j<path.Length;j++)
        {
            Vector3 tangent=(path[Mathf.Min(j+1,path.Length-1)]-path[Mathf.Max(0,j-1)]).normalized;
            right=Vector3.ProjectOnPlane(right,tangent).normalized;
            if(right.sqrMagnitude<.1f)right=Vector3.Cross(tangent,Vector3.forward).normalized;
            Vector3 up=Vector3.Cross(right,tangent).normalized;
            float r=Mathf.Lerp(start,end,j/(float)(path.Length-1));
            if(j>0)distance+=Vector3.Distance(path[j],path[j-1]);
            for(int k=0;k<=sides;k++)
            {
                float a=2*Mathf.PI*k/sides;
                float rough=1+.065f*Mathf.Sin(a*5+distance*.8f)+.035f*Mathf.Sin(a*11-distance*3.1f);
                draft.v.Add(path[j]+r*rough*(right*Mathf.Cos(a)+up*Mathf.Sin(a)));
                draft.uv.Add(new Vector2(k/(float)sides*Mathf.Max(.5f,2*Mathf.PI*start/1.4f),distance/1.4f));
            }
        }
        for(int j=0;j<path.Length-1;j++)for(int k=0;k<sides;k++)
        {
            int a=first+j*(sides+1)+k,b=a+sides+1,c=b+1,d=a+1;
            if(Vector3.Dot(Vector3.Cross(draft.v[b]-draft.v[a],draft.v[c]-draft.v[a]),draft.v[a]-path[j])<0)
            {int temp=b;b=d;d=temp;}
            draft.t.AddRange(new[]{a,b,c,a,c,d});
        }
    }
    static Vector3 Polar(float r, float a, float y) => new Vector3(r*Mathf.Cos(a), y, r*Mathf.Sin(a));
    static float Range(System.Random rng, float min, float max) => Mathf.Lerp(min,max,(float)rng.NextDouble());
    static Vector3 Branch(float t, float reach, float angle, float height)
    {
        // 줄기에서 위로 뻗은 뒤 끝이 내려오는 주가지 생성
        return Polar(reach*t,angle+.12f*Mathf.Sin(t*3),height+3.5f*t-2.15f*t*t);
    }
    static void Replace(string name, Mid_GardenBuilder.Draft draft)
    {
        var filter=GameObject.Find(name).GetComponent<MeshFilter>();
        var mesh=filter.sharedMesh;
        if(!AssetDatabase.GetAssetPath(mesh).StartsWith("Assets/mid/"))
            throw new InvalidOperationException("Only mid mesh assets can be changed: "+name);
        var replacement=draft.Mesh(mesh.name);
        Undo.RegisterCompleteObjectUndo(mesh,"Curve and frost willow");
        mesh.Clear();
        mesh.indexFormat=replacement.indexFormat;
        mesh.vertices=replacement.vertices;
        mesh.triangles=replacement.triangles;
        mesh.uv=replacement.uv;
        mesh.normals=replacement.normals;
        mesh.tangents=replacement.tangents;
        mesh.bounds=replacement.bounds;
        UnityEngine.Object.DestroyImmediate(replacement);
        EditorUtility.SetDirty(mesh);
    }
    [MenuItem("Tools/Mid Garden/Refine white weeping willow")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play mode first.");
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/mid/Mid_DarkChristmas.unity")
            throw new InvalidOperationException("Open the mid garden scene first.");
        foreach(string name in new[]{"TwistedTrunkAndBranches","CascadingSilverTwigs","SilverWillowLeaves","ChristmasFairyLights"})
            if(GameObject.Find(name)==null)throw new InvalidOperationException("Missing willow part: "+name);
        Undo.IncrementCurrentGroup();
        int undoGroup=Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Curve and frost willow");
        var rng=new System.Random(1225);
        var wood=new Mid_GardenBuilder.Draft();
        var twigs=new Mid_GardenBuilder.Draft();
        var leaves=new Mid_GardenBuilder.Draft();
        var lights=new Mid_GardenBuilder.Draft();
        var main=new Vector3[24];
        for(int i=0;i<main.Length;i++)
        {
            float t=i/(float)(main.Length-1);
            main[i]=new Vector3(.36f*Mathf.Sin(t*7),.7f+t*6.2f,.25f*Mathf.Sin(t*5));
        }
        WoodTube(wood,main,.83f,.10f,20);
        for(int r=0;r<10;r++)
        {
            var path=new Vector3[12];float a=r*Mathf.PI*2/10;
            for(int j=0;j<path.Length;j++){float t=j/(float)(path.Length-1);path[j]=Polar(2.9f*t,a+.16f*Mathf.Sin(t*4),.72f+1.6f*(1-t)*(1-t));}
            WoodTube(wood,path,.36f,.025f,12);
        }
        for(int arm=0;arm<11;arm++)
        {
            float angle=arm*2.39996f, reach=Range(rng,2.5f,3.6f), height=3.05f+(arm%4)*.62f;
            var path=new Vector3[25];
            for(int j=0;j<path.Length;j++)path[j]=Branch(j/(float)(path.Length-1),reach,angle,height);
            WoodTube(wood,path,.27f,.025f,14);
            for(int hang=0;hang<14;hang++)
            {
                float attachment=.27f+.72f*hang/13f;
                Vector3 start=Branch(attachment,reach,angle,height);
                float direction=angle+Range(rng,-.55f,.55f), spread=Range(rng,.8f,1.75f);
                float length=Mathf.Min(Range(rng,2.2f,4.0f),start.y-1.18f);
                float lift=Range(rng,.45f,.9f), drift=Range(rng,-.18f,.18f);
                var trail=new Vector3[25];
                for(int j=0;j<trail.Length;j++)
                {
                    float u=j/(float)(trail.Length-1);
                    // 끝으로 갈수록 바깥쪽 이동은 줄이고 아래로 처지는 정도는 증가
                    // 레퍼런스처럼 둥글게 뻗다가 아래로 내려오는 모양
                    trail[j]=start+Polar(spread*Mathf.Sin(u*Mathf.PI*.5f),direction+drift*u,
                        lift*Mathf.Sin(u*Mathf.PI)-length*u*u);
                }
                twigs.Tube(trail,.027f,.004f,6);
                for(int k=2;k<trail.Length-1;k++)
                {
                    Vector3 c=trail[k], tangent=(trail[k+1]-trail[k-1]).normalized;
                    Vector3 side=Vector3.Cross(tangent,Vector3.up).normalized;
                    if(side.sqrMagnitude<.1f)side=Vector3.right;
                    side*=k%2==0?1:-1;
                    Vector3 tip=c+tangent*Range(rng,.15f,.24f)+side*.045f;
                    Vector3 mid=(c+tip)*.5f;
                    Vector3 normal=Vector3.Cross(tip-c,side).normalized;
                    leaves.Quad(c,mid+side*.031f,tip,mid-side*.024f,normal);
                    leaves.Quad(c,mid+side*.031f,tip,mid-side*.024f,-normal);
                }
                if(hang%4==0)for(int k=3;k<24;k+=3)lights.Sphere(trail[k],Vector3.one*.035f,6,4);
            }
            var ornament=GameObject.Find("OPT02_SwingingOrnament_"+arm);
            if(ornament!=null)
            {
                Undo.RecordObject(ornament.transform,"Follow curved branch");
                ornament.transform.localPosition=Branch(.74f,reach,angle,height)+Vector3.down*.08f;
            }
        }
        for(int i=0;i<=100;i+=2)
        {
            float t=i/100f;lights.Sphere(Polar(3.1f*(1-t)+.35f,t*Mathf.PI*6,2.5f+t*4.15f),Vector3.one*.045f,6,4);
        }
        Replace("TwistedTrunkAndBranches",wood);
        Replace("CascadingSilverTwigs",twigs);
        Replace("SilverWillowLeaves",leaves);
        Replace("ChristmasFairyLights",lights);
        var bark=GameObject.Find("TwistedTrunkAndBranches").GetComponent<Renderer>().sharedMaterial;
        Undo.RecordObject(bark,"Frost white bark");
        bark.SetColor("_BaseColor",new Color(.85f,.88f,.91f));
        bark.SetFloat("_Metallic",0);bark.SetFloat("_Smoothness",.24f);
        EditorUtility.SetDirty(bark);
        var leaf=GameObject.Find("SilverWillowLeaves").GetComponent<Renderer>().sharedMaterial;
        Undo.RecordObject(leaf,"Frost white leaves");
        leaf.SetColor("_BaseColor",new Color(.88f,.92f,.95f));
        leaf.SetFloat("_Metallic",.04f);leaf.SetFloat("_Smoothness",.3f);
        leaf.SetColor("_EmissionColor",new Color(.018f,.022f,.028f));
        EditorUtility.SetDirty(leaf);
        Undo.CollapseUndoOperations(undoGroup);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        SceneView.RepaintAll();
    }
}
