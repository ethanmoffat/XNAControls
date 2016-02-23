// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;

namespace XNAControls
{
	public static class ExtensionMethods
	{
		public static bool ContainsPoint(this Rectangle rect, int x, int y)
		{
			return x >= rect.Left && x <= rect.Right && y >= rect.Top && y <= rect.Bottom;
		}
	}
}
