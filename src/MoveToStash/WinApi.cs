using System;
using System.Runtime.InteropServices;

namespace MoveToStash
{
    public static class WinApiMouse
    {
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out Point lpMousePoint);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll", EntryPoint = "keybd_event", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern void keybd_event(byte vk, byte scan, int flags, int extrainfo);

        #region Structs/Enums

        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }

        [Flags]
        public enum KeyEventFlags
        {
            KeyEventShiftVirtual = 0x10,
            KeyLControlVirtual = 0x11,
            KeyLeftVirtual = 0x25,
            KeyRightVirtual = 0x27,
            KeyEventKeyDown = 0,
            KeyEventKeyUp = 2,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public Point(System.Drawing.Point pt) : this(pt.X, pt.Y)
            {
            }

            public static implicit operator System.Drawing.Point(Point p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator Point(System.Drawing.Point p)
            {
                return new Point(p.X, p.Y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left { get; private set; }
            public int Top { get; private set; }
            public int Right { get; private set; }
            public int Bottom { get; private set; }

            public Rect ToRectangle(Point point)
            {
                return new Rect { Left = point.X, Top = point.Y, Right = Right - Left, Bottom = Bottom - Top };
            }
        }

        #endregion
    }
}