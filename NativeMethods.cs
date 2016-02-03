// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Runtime.InteropServices;

namespace XNAControls
{
	internal static class NativeMethods
	{
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		public const int GWL_WNDPROC = -4;
		public const int WM_KEYDOWN = 0x100;
		public const int WM_KEYUP = 0x101;
		public const int WM_CHAR = 0x102;
		public const int WM_IME_SETCONTEXT = 0x0281;
		public const int WM_INPUTLANGCHANGE = 0x51;
		public const int WM_GETDLGCODE = 0x87;
		public const int WM_IME_COMPOSITION = 0x10f;
		public const int DLGC_WANTALLKEYS = 4;
	}
}
