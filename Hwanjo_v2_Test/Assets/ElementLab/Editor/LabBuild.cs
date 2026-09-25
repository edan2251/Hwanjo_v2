using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Hwanjo.ElementLab.Editor
{
    public static class LabBuild
    {
        public const string Root = "Assets/ElementLab/";
        public const string ScenePath = Root + "Scenes/ElementLab.unity";
        [MenuItem("Hwanjo/Prepare Element Lab Scene")]
        public static void Prepare()
        {
            Directory.CreateDirectory(Root + "Scenes"); Directory.CreateDirectory(Root + "Data");
            var tuning = AssetDatabase.LoadAssetAtPath<LabTuning>(Root + "Data/LabTuning.asset");
            if (!tuning) { tuning = ScriptableObject.CreateInstance<LabTuning>(); AssetDatabase.CreateAsset(tuning, Root + "Data/LabTuning.asset"); }
            var art = AssetDatabase.LoadAssetAtPath<LabArtLibrary>(Root + "Data/LabArt.asset");
            if (!art) { art = ScriptableObject.CreateInstance<LabArtLibrary>(); AssetDatabase.CreateAsset(art, Root + "Data/LabArt.asset"); }
            if (!File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var go = new GameObject("Element Lab"); var world = go.AddComponent<LabWorld>(); world.Tuning = tuning; world.Art = art;
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); Debug.Log("ELEMENT_LAB_SCENE_READY " + ScenePath);
        }
        [MenuItem("Hwanjo/Build Windows Element Lab")]
        public static void BuildWindows()
        {
            Prepare();
            string repo = Directory.GetParent(Application.dataPath).Parent.FullName;
            string output = Path.Combine(repo, "Builds/ElementLabV02/HwanjoElementLab.exe"); Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = output, target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
            Debug.Log("ELEMENT_LAB_BUILD " + report.summary.result + " | errors=" + report.summary.totalErrors + " warnings=" + report.summary.totalWarnings + " bytes=" + report.summary.totalSize + " output=" + output);
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Windows build failed: " + report.summary.result);
        }
    }
}
