using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using EcoGuardian.Core;
namespace EcoGuardian.Editor
{
    [InitializeOnLoad]
    public static class ProjectBuilder
    {
        const string Root="Assets/EcoGuardian/";
        static ProjectBuilder(){EditorApplication.delayCall+=EnsureProject;}
        static void EnsureProject(){if(!File.Exists(Root+"Scenes/MainMenu.unity")||!File.Exists(Root+"Resources/Levels/Level01.asset"))Generate();}
        [MenuItem("Eco Guardian/Generate Scenes and Level Assets")]
        public static void Generate()
        {
            Directory.CreateDirectory(Root+"Resources/Levels");Directory.CreateDirectory(Root+"Scenes");
            for(int i=1;i<=5;i++){
                string path=Root+"Resources/Levels/Level"+i.ToString("00")+".asset";
                if(AssetDatabase.LoadAssetAtPath<LevelData>(path)!=null)continue;
                var data=ScriptableObject.CreateInstance<LevelData>();data.configuration=Campaign.Create(i);AssetDatabase.CreateAsset(data,path);
            }
            bool createdScenes=false;var names=new[]{"MainMenu","LevelSelect","Gameplay"};
            for(int i=0;i<names.Length;i++){
                string path=Root+"Scenes/"+names[i]+".unity";
                if(File.Exists(path))continue;
                createdScenes=true;var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var camera=new GameObject("Main Camera");camera.tag="MainCamera";camera.AddComponent<Camera>().orthographic=true;camera.AddComponent<AudioListener>();
                var root=new GameObject("Eco Guardian");var app=root.AddComponent<GuardianGame>();app.entryScreen=i==1?"levels":"menu";
                EditorSceneManager.SaveScene(scene,path);
            }
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Root+"Scenes/MainMenu.unity",true),new EditorBuildSettingsScene(Root+"Scenes/LevelSelect.unity",true),new EditorBuildSettingsScene(Root+"Scenes/Gameplay.unity",true)};
            PlayerSettings.companyName="Da Game";PlayerSettings.productName="Eco Guardian";PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=1000;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.runInBackground=false;PlayerSettings.colorSpace=ColorSpace.Gamma;
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            if(createdScenes)EditorSceneManager.OpenScene(Root+"Scenes/MainMenu.unity");Debug.Log("Eco Guardian: scenes and editable LevelData assets are ready.");
        }
        [MenuItem("Eco Guardian/Build Windows Demo")]
        public static void BuildWindows()
        {
            Generate();Directory.CreateDirectory("Builds/Windows");
            var report=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Builds/Windows/EcoGuardian.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Windows build failed: "+report.summary.result);
        }
    }
    public sealed class ArtImporter:AssetPostprocessor
    {
        void OnPreprocessTexture(){if(!assetPath.Contains("EcoGuardian/Resources/Art/"))return;var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.filterMode=FilterMode.Bilinear;t.textureCompression=TextureImporterCompression.Uncompressed;}
    }
}
