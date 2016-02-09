// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace XNAControls
{
	internal class Win32KeyboardEvents : IKeyboardEvents
	{
		public event CharEnteredHandler CharEntered;

		private readonly IntPtr _prevWndProc;
		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private readonly NativeMethods.WndProc _hookProcDelegate; //This needs to be kept as a member so the GC doesn't clean it up

		public Win32KeyboardEvents(GameWindow window)
		{
			_hookProcDelegate = HookProc;
			_prevWndProc = (IntPtr)NativeMethods.SetWindowLong(window.Handle, NativeMethods.GWL_WNDPROC,
				(int)Marshal.GetFunctionPointerForDelegate(_hookProcDelegate));
		}

		private IntPtr HookProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
		{
			IntPtr returnCode = NativeMethods.CallWindowProc(_prevWndProc, hWnd, msg, wParam, lParam);

			switch (msg)
			{
				case NativeMethods.WM_GETDLGCODE:
					returnCode = (IntPtr)(returnCode.ToInt32() | NativeMethods.DLGC_WANTALLKEYS);
					break;

				case NativeMethods.WM_CHAR:
					if (CharEntered != null)
						CharEntered(null, new Win32CharEnteredEventArgs((char)wParam, lParam.ToInt32()));
					break;
			}

			return returnCode;
		}

		public void Dispose() { }
	}
}
