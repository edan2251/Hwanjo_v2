using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Hwanjo.ElementLab.Tests
{
    public class V02ArtTests
    {
        [Test] public void AllDirectionalFamiliesHaveSixDifferentImportedFramesAndRealSword()
        {
            var a=AssetDatabase.LoadAssetAtPath<LabArtLibrary>("Assets/ElementLab/Data/LabArt.asset");
            Assert.AreEqual(7,a.Directional.Length);Assert.NotNull(a.Sword);
            foreach(var strip in a.Directional)
            {
                Assert.AreEqual(6,strip.Frames.Length);Assert.AreEqual(6,strip.Frames.Distinct().Count());
                foreach(var frame in strip.Frames){Assert.AreEqual(new Vector2(48,48),frame.rect.size);Assert.AreEqual(48,frame.pixelsPerUnit);Assert.That(frame.pivot.y,Is.EqualTo(2).Within(.01f));}
            }
            Assert.Greater(a.Sword.rect.width,a.Sword.rect.height*3);
        }
        [Test] public void IndependentBackgroundTexturesAndEightTerrainPiecesAreWired()
        {
            var a=AssetDatabase.LoadAssetAtPath<LabArtLibrary>("Assets/ElementLab/Data/LabArt.asset");
            Assert.NotNull(a.Far);Assert.NotNull(a.Middle);Assert.AreNotSame(a.Far.texture,a.Middle.texture);Assert.AreEqual(8,a.Terrain.Length);
            Assert.IsTrue(a.Terrain.All(t=>t));
        }
    }
}
