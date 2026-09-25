using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hwanjo.ElementLab.Editor
{
    public static class LabV02ArtImport
    {
        const string Folder="Assets/ElementLab/Art/V02/";
        static TextureImporter Configure(string name)
        {
            var t=(TextureImporter)AssetImporter.GetAtPath(Folder+name+".png");
            t.textureType=TextureImporterType.Sprite;t.spritePixelsPerUnit=48;t.filterMode=FilterMode.Point;
            t.textureCompression=TextureImporterCompression.Uncompressed;t.mipmapEnabled=false;t.alphaIsTransparency=true;
            t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=4096;return t;
        }
        static Sprite Single(string name,Vector2 pivot)
        {
            var t=Configure(name);t.spriteImportMode=SpriteImportMode.Single;
            var settings=new TextureImporterSettings();t.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=pivot;t.SetTextureSettings(settings);t.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(Folder+name+".png");
        }
        [MenuItem("Hwanjo/Import v02 additions")]
        public static void Import()
        {
            var art=AssetDatabase.LoadAssetAtPath<LabArtLibrary>(LabBuild.Root+"Data/LabArt.asset");
            var sheet=Configure("HeroDirections");sheet.spriteImportMode=SpriteImportMode.Multiple;
            var frames=new SpriteMetaData[36];
            for(int i=0;i<36;i++)frames[i]=new SpriteMetaData {name="Direction_"+i.ToString("D2"),rect=new Rect(i%6*48,(5-i/6)*48,48,48),alignment=(int)SpriteAlignment.Custom,pivot=new Vector2(.5f,2f/48)};
#pragma warning disable 618
            sheet.spritesheet=frames;
#pragma warning restore 618
            sheet.SaveAndReimport();
            var sprites=AssetDatabase.LoadAllAssetsAtPath(Folder+"HeroDirections.png").OfType<Sprite>().OrderBy(s=>s.name,StringComparer.Ordinal).ToArray();
            string[] names={"SlashHorizontal","SlashUp","SlashDown","ChargeHorizontal","ChargeUp","ChargeDown"};
            art.Directional=Enumerable.Range(0,6).Select(i=>new PoseStrip{Name=names[i],Frames=sprites.Skip(i*6).Take(6).ToArray()}).ToArray();
            var air=Configure("AirDown");air.spriteImportMode=SpriteImportMode.Multiple;
            var airMeta=Enumerable.Range(0,6).Select(i=>new SpriteMetaData{name="AirDown_"+i,rect=new Rect(i*48,0,48,48),alignment=(int)SpriteAlignment.Custom,pivot=new Vector2(.5f,2f/48)}).ToArray();
#pragma warning disable 618
            air.spritesheet=airMeta;
#pragma warning restore 618
            air.SaveAndReimport();
            art.Directional=art.Directional.Concat(new[]{new PoseStrip{Name="AirDown",Frames=AssetDatabase.LoadAllAssetsAtPath(Folder+"AirDown.png").OfType<Sprite>().OrderBy(s=>s.name).ToArray()}}).ToArray();
            // Cropped generated sword: grip center is pixel 8, blade axis is pixel 7.5 above lower edge.
            art.Sword=Single("Sword",new Vector2(8f/56,7.5f/11));
            art.Far=Single("FarRuins",Vector2.one*.5f);art.Middle=Single("MiddleForest",Vector2.one*.5f);
            art.Terrain=Enumerable.Range(0,8).Select(i=>Single("Terrain"+i,Vector2.one*.5f)).ToArray();
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();Debug.Log("V02_ART_IMPORTED six directional families / 36 distinct frames / sword / 8 terrain / 2 independent background textures");
        }
    }
}
