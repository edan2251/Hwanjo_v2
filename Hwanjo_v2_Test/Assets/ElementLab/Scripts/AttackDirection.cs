using UnityEngine;
using UnityEngine.InputSystem;

namespace Hwanjo.ElementLab
{
    public enum SlashDirection { Right, Left, Up, Down }
    public static class AttackDirections
    {
        public static SlashDirection Resolve(int facing, bool up, bool down) => up != down
            ? (up ? SlashDirection.Up : SlashDirection.Down) : facing < 0 ? SlashDirection.Left : SlashDirection.Right;
        public static SlashDirection Read(int facing)
        {
            var k = Keyboard.current;
            return Resolve(facing, k != null && (k.wKey.isPressed || k.upArrowKey.isPressed),
                k != null && (k.sKey.isPressed || k.downArrowKey.isPressed));
        }
        public static Vector2 Vector(this SlashDirection direction) => direction == SlashDirection.Up ? Vector2.up
            : direction == SlashDirection.Down ? Vector2.down : direction == SlashDirection.Left ? Vector2.left : Vector2.right;
        public static string Korean(this SlashDirection direction) => direction == SlashDirection.Up ? "위" : direction == SlashDirection.Down ? "아래" : direction == SlashDirection.Left ? "왼쪽" : "오른쪽";
        public static Vector2 AreaSize(Vector2 direction, float length, float width) => Mathf.Abs(direction.x) > .5f ? new Vector2(length, width) : new Vector2(width, length);
    }
}
