using Microsoft.Xna.Framework;

namespace XNAControls
{
	public static class RectangleExtensions
	{
		public static bool ContainsPoint(this Rectangle rect, int x, int y)
		{
			return x >= rect.Left && x <= rect.Right && y >= rect.Top && y <= rect.Bottom;
		}
	}
}
