using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hwanjo.ElementLab.Editor
{
    public static class LabArtImport
    {
        [MenuItem("Hwanjo/Import Element Lab Art")]
        public static void Import()
        {
            LabBuild.Prepare();
            Sprite[] run = Sheet("HeroRunSlash", 6, 2);
            Sprite[] other = Sheet("HeroOtherPoses", 6, 4);
            string bgPath = LabBuild.Root + "Art/ForestRuins.png";
            var bg = (TextureImporter)AssetImporter.GetAtPath(bgPath);
            Configure(bg); bg.spriteImportMode = SpriteImportMode.Single; bg.SaveAndReimport();
            var art = AssetDatabase.LoadAssetAtPath<LabArtLibrary>(LabBuild.Root + "Data/LabArt.asset");
            art.Background = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
            art.Poses = new[] {
                Strip("Run",run,0,1,2,3,4,5), Strip("Slash",run,6,7,8,9,10,11),
                Strip("Idle",other,0,1,2,3), Strip("Jump",other,4), Strip("Fall",other,5),
                Strip("Dash",other,6,7), Strip("Charge",other,8,9), Strip("Hit",other,10,11),
                Strip("ChargeRelease",other,8,9,13,15,16,17), Strip("Death",other,18,19,20,21,22,23)
            };
            EditorUtility.SetDirty(art); AssetDatabase.SaveAssets();
            Debug.Log("ELEMENT_LAB_ART_READY | 10 poses | 36 source frames | 48x48 | PPU=48 | feet pivot=(24,2) | Point | Uncompressed");
        }
        static PoseStrip Strip(string name, Sprite[] source, params int[] frames) => new PoseStrip { Name = name, Frames = frames.Select(i => source[i]).ToArray() };
        static void Configure(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite; importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.spritePixelsPerUnit = 48; importer.maxTextureSize = 4096;
            importer.npotScale = TextureImporterNPOTScale.None;
        }
        static Sprite[] Sheet(string name, int columns, int rows)
        {
            string path = LabBuild.Root + "Art/" + name + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path); Configure(importer);
            importer.spriteImportMode = SpriteImportMode.Multiple;
            var frames = new SpriteMetaData[columns * rows];
            for (int i = 0; i < frames.Length; i++) frames[i] = new SpriteMetaData {
                name = name + "_" + i.ToString("D2"), rect = new Rect(i % columns * 48, (rows - 1 - i / columns) * 48, 48, 48),
                alignment = (int)SpriteAlignment.Custom, pivot = new Vector2(.5f, 2f/48)
            };
            // Legacy metadata API remains supported in the project's pinned Unity 6000.3.
#pragma warning disable 618
            importer.spritesheet = frames;
#pragma warning restore 618
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name, StringComparer.Ordinal).ToArray();
        }
    }
}
