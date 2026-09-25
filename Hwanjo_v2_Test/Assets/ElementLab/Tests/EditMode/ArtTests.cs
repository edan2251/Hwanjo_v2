using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Hwanjo.ElementLab.Tests
{
    public class ArtTests
    {
        [Test] public void EveryRequiredPoseUsesRealImported48PixelFrames()
        {
            var art = AssetDatabase.LoadAssetAtPath<LabArtLibrary>("Assets/ElementLab/Data/LabArt.asset");
            Assert.IsNotNull(art); Assert.IsNotNull(art.Background);
            foreach (string pose in new[] { "Idle", "Run", "Jump", "Fall", "Dash", "Slash", "Charge", "ChargeRelease", "Hit", "Death" })
            {
                Sprite previous = null;
                foreach (float phase in new[] { 0f, .3f, .6f, .99f })
                {
                    var frame = art.Frame(pose, phase); Assert.IsNotNull(frame, pose);
                    Assert.AreEqual(new Vector2(48, 48), frame.rect.size); Assert.AreEqual(48, frame.pixelsPerUnit);
                    Assert.That(frame.pivot.x, Is.EqualTo(24).Within(.01)); Assert.That(frame.pivot.y, Is.EqualTo(2).Within(.01));
                    if (phase == .99f && pose != "Jump" && pose != "Fall") Assert.AreNotEqual(previous, frame, pose);
                    if (phase == 0) previous = frame;
                }
            }
        }
        [TestCase("HeroRunSlash",288,96)] [TestCase("HeroOtherPoses",288,192)]
        public void AtlasesHaveActualAlphaPointFilterAndNoCompression(string name, int w, int h)
        {
            string path = "Assets/ElementLab/Art/" + name + ".png";
            var texture = new Texture2D(2,2); texture.LoadImage(File.ReadAllBytes(path));
            Assert.AreEqual(w, texture.width); Assert.AreEqual(h, texture.height);
            bool transparent = false, opaque = false;
            foreach (var pixel in texture.GetPixels32()) { transparent |= pixel.a == 0; opaque |= pixel.a > 240; }
            Assert.IsTrue(transparent && opaque); Object.DestroyImmediate(texture);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Assert.AreEqual(FilterMode.Point, importer.filterMode); Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression);
        }
    }
}
