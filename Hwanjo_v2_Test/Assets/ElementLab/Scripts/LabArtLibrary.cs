using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hwanjo.ElementLab
{
    [Serializable] public class PoseStrip { public string Name; public Sprite[] Frames; }
    [CreateAssetMenu(menuName = "Hwanjo/Element Lab Art")]
    public sealed class LabArtLibrary : ScriptableObject
    {
        public Sprite Background;
        public PoseStrip[] Poses;
        public Sprite Frame(string pose, float phase)
        {
            if (Poses == null) return null;
            foreach (var strip in Poses)
                if (strip.Name == pose && strip.Frames != null && strip.Frames.Length > 0)
                    return strip.Frames[Mathf.Clamp(Mathf.FloorToInt(phase * strip.Frames.Length), 0, strip.Frames.Length - 1)];
            return null;
        }
    }

    // Code-native environmental symbols and pre-art functional placeholders.
    // Generated hero sprites replace the articulated placeholder when the art library is populated.
    public static class LabSprites
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        static Sprite pixel;
        public static Sprite Pixel
        {
            get
            {
                if (pixel) return pixel;
                var tex = new Texture2D(1, 1); tex.SetPixel(0, 0, Color.white); tex.Apply(); tex.filterMode = FilterMode.Point;
                pixel = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1); return pixel;
            }
        }
        public static Color ElementColor(Element e) => e == Element.Fire ? new Color(1, .40f, .18f) : e == Element.Water ? new Color(.2f, .64f, 1) : e == Element.Wind ? new Color(.58f, .96f, .79f) : new Color(.73f, .9f, 1);
        public static Sprite Prop(string kind)
        {
            if (cache.TryGetValue(kind, out var sprite)) return sprite;
            var p = new PixelCanvas(48, 48);
            Color ink = Hex("17292d"), stone = Hex("597e79"), moss = Hex("83a661"), gold = Hex("d5b578"), wood = Hex("795643");
            if (kind == "box" || kind == "dummy")
            {
                p.Rect(6, 2, 36, 36, ink); p.Rect(8, 4, 32, 32, wood); p.Rect(10, 6, 28, 28, Hex("b78659"));
                for (int y = 9; y < 33; y += 8) p.Rect(10, y, 28, 2, wood);
                p.Line(11, 7, 37, 32, gold, 3); p.Line(11, 32, 37, 7, wood, 2);
                if (kind == "dummy") { p.Circle(24, 22, 10, ink); p.Circle(24, 22, 7, gold); p.Circle(24, 22, 3, Hex("a54648")); }
            }
            else if (kind == "vine")
            {
                for (int x = 10; x <= 38; x += 12)
                {
                    p.Line(x, 1, x - 3, 46, Hex("27493b"), 3);
                    for (int y = 7; y < 44; y += 9) { p.Line(x, y, x - 9, y + 5, moss, 3); p.Line(x, y + 4, x + 7, y + 9, Hex("548957"), 4); }
                }
            }
            else if (kind == "rope")
            { p.Rect(3, 41, 42, 5, wood); for (int y = 2; y < 42; y += 4) { p.Rect(21, y, 6, 4, y % 8 == 0 ? gold : wood); } p.Rect(13, 1, 23, 4, stone); }
            else if (kind == "water")
            { for (int y = 3; y < 18; y += 3) p.Rect(1 + y % 5, y, 45 - y % 6, 2, y > 12 ? Hex("9bd9d6") : Hex("367f92")); }
            else if (kind == "wind" || kind == "device")
            {
                p.Rect(20, 2, 8, 30, stone); p.Rect(11, 0, 26, 5, ink);
                p.Circle(24, 28, 7, gold); p.Circle(24, 28, 4, ink);
                for (int i = 0; i < 4; i++) { float a = i * Mathf.PI / 2; p.Line(24 + (int)(Mathf.Cos(a) * 6), 28 + (int)(Mathf.Sin(a) * 6), 24 + (int)(Mathf.Cos(a) * 20), 28 + (int)(Mathf.Sin(a) * 17), kind == "wind" ? moss : gold, 5); }
            }
            else if (kind == "enemy")
            {
                p.Line(12, 3, 19, 12, ink, 6); p.Line(35, 3, 30, 12, ink, 6);
                p.Circle(24, 20, 15, ink); p.Circle(24, 22, 12, Hex("6b6555"));
                p.Rect(12, 25, 25, 8, Hex("304c45")); p.Rect(16, 26, 5, 3, Hex("ffd394")); p.Rect(28, 26, 5, 3, Hex("ffd394"));
                p.Line(15, 34, 10, 43, wood, 5); p.Line(33, 34, 38, 43, wood, 5); p.Rect(20, 12, 10, 4, ink);
            }
            else { p.Rect(2, 2, 44, 42, stone); p.Rect(2, 39, 44, 5, moss); }
            sprite = p.Sprite(kind); cache[kind] = sprite; return sprite;
        }
        public static Sprite HeroPlaceholder(string pose, float phase)
        {
            int f = Mathf.Clamp((int)(phase * 6), 0, 5); string key = "placeholder-" + pose + f;
            if (cache.TryGetValue(key, out var sprite)) return sprite;
            var p = new PixelCanvas(48, 48); Color ink = Hex("152234"), navy = Hex("344d70"), ivory = Hex("e3d8bb"), red = Hex("ac3b49"), skin = Hex("e8b694");
            int lean = pose == "Slash" || pose == "ChargeRelease" ? (f >= 2 && f <= 3 ? 4 : -2) : pose == "Dash" ? 5 : 0;
            int stride = pose == "Run" ? (int)(Mathf.Sin(phase * Mathf.PI * 2) * 7) : pose == "Jump" ? 4 : 2;
            int bodyY = pose == "Dash" ? 15 : pose == "Death" ? 8 : 18;
            p.Line(22, bodyY, 19 + stride, 4, ink, 6); p.Line(27, bodyY, 29 - stride, 4, navy, 6);
            p.Rect(19 + lean, bodyY - 1, 12, 15, navy); p.Rect(24 + lean, bodyY + 1, 5, 12, ivory);
            p.Rect(18 + lean, bodyY + 11, 14, 5, red); p.Line(18 + lean, bodyY + 13, 5, bodyY + 7 + f % 2 * 3, red, 4);
            p.Circle(26 + lean, bodyY + 23, 8, ink); p.Rect(24 + lean, bodyY + 17, 10, 8, skin); p.Rect(18 + lean, bodyY + 23, 15, 6, ink);
            p.Circle(18 + lean, bodyY + 28, 4, ink); p.Line(17 + lean, bodyY + 25, 9, bodyY + 19, ink, 3);
            int handX = pose == "Charge" ? 19 : pose.Contains("Release") || pose == "Slash" ? 35 + (f == 2 ? 5 : 0) : 32;
            int handY = pose == "Charge" ? bodyY + 19 : pose == "Slash" && f < 2 ? bodyY + 23 : bodyY + 9;
            p.Line(29 + lean, bodyY + 11, handX, handY, navy, 4); p.Rect(handX - 1, handY - 1, 3, 3, skin);
            if (pose == "Death") { p.Rect(4, 2, 35, 9, ink); p.Rect(8, 8, 20, 5, navy); }
            sprite = p.Sprite(key); cache[key] = sprite; return sprite;
        }
        public static SpriteRenderer Quad(string name, Transform parent, Vector2 pos, Vector2 size, Color color, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localScale = size;
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Pixel; sr.color = color; sr.sortingOrder = order; return sr;
        }
        public static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var c); return c; }
    }

    public sealed class PixelCanvas
    {
        readonly int w, h; readonly Color[] pixels;
        public PixelCanvas(int width, int height) { w = width; h = height; pixels = new Color[w * h]; }
        public void Rect(int x, int y, int width, int height, Color c)
        { for (int yy = y; yy < y + height; yy++) for (int xx = x; xx < x + width; xx++) if (xx >= 0 && yy >= 0 && xx < w && yy < h) pixels[yy * w + xx] = c; }
        public void Circle(int x, int y, int radius, Color c)
        { for (int yy = -radius; yy <= radius; yy++) for (int xx = -radius; xx <= radius; xx++) if (xx * xx + yy * yy <= radius * radius) Rect(x + xx, y + yy, 1, 1, c); }
        public void Line(int x, int y, int xx, int yy, Color c, int thickness)
        { int steps = Mathf.Max(Mathf.Abs(xx - x), Mathf.Abs(yy - y)); for (int i = 0; i <= steps; i++) Rect(Mathf.RoundToInt(Mathf.Lerp(x, xx, steps == 0 ? 0 : i / (float)steps)), Mathf.RoundToInt(Mathf.Lerp(y, yy, steps == 0 ? 0 : i / (float)steps)), thickness, thickness, c); }
        public Sprite Sprite(string name)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { name = name, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            tex.SetPixels(pixels); tex.Apply(); return UnityEngine.Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(.5f, 0), 48);
        }
    }
}
