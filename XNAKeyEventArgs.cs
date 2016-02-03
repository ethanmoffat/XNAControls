// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using Microsoft.Xna.Framework.Input;

namespace XNAControls
{
	internal delegate void KeyEventHandler(object sender, XNAKeyEventArgs e);

	public class XNAKeyEventArgs : EventArgs
	{
		private readonly Keys keyCode;

		public XNAKeyEventArgs(Keys keyCode)
		{
			this.keyCode = keyCode;
		}

		public Keys KeyCode
		{
			get { return keyCode; }
		}
	}
}
