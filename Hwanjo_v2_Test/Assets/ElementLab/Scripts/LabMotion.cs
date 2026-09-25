using UnityEngine;

namespace Hwanjo.ElementLab
{
    public static class LabMotion
    {
        // Measured grip positions in the imported 48px atlas. Coordinates: left/bottom pixels,
        // same feet pivot (24,2) as the existing hero. No rotation of body/collider.
        static readonly Vector2[,] grips = {
            {new Vector2(34,15),new Vector2(31,29),new Vector2(41,20),new Vector2(42,20),new Vector2(38,14),new Vector2(39,14)},
            {new Vector2(30,5),new Vector2(31,6),new Vector2(29,39),new Vector2(31,37),new Vector2(31,34),new Vector2(38,16)},
            {new Vector2(16,39),new Vector2(36,23),new Vector2(39,6),new Vector2(39,5),new Vector2(36,12),new Vector2(39,21)},
            {new Vector2(35,14),new Vector2(39,16),new Vector2(44,21),new Vector2(43,22),new Vector2(38,11),new Vector2(38,12)},
            {new Vector2(35,12),new Vector2(32,7),new Vector2(26,40),new Vector2(27,35),new Vector2(38,19),new Vector2(38,16)},
            {new Vector2(23,39),new Vector2(39,22),new Vector2(39,5),new Vector2(40,4),new Vector2(40,6),new Vector2(42,21)},
            {new Vector2(24,34),new Vector2(27,40),new Vector2(37,8),new Vector2(37,5),new Vector2(23,24),new Vector2(39,23)}
        };
        static readonly float[,] angles = {
            {125,110,12,-8,-34,-24}, {15,30,88,100,125,-24}, {80,45,-87,-96,-50,-24},
            {135,110,5,-12,-40,-24}, {15,30,90,100,135,-24}, {85,55,-90,-100,-55,-24}, {90,90,-90,-92,-38,-24}
        };
        public static int Row(SlashDirection direction,bool charged) => (charged?3:0)+(direction==SlashDirection.Up?1:direction==SlashDirection.Down?2:0);
        public static Vector2 Grip(int row,int frame) => (grips[row,frame]-new Vector2(24,2))/48;
        public static float Angle(int row,int frame)=>angles[row,frame];
    }
}
