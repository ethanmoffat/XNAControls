#region File Description
//-----------------------------------------------------------------------------
// GraphicsDeviceService.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

//File modified by Ethan Moffat for use in testing

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
#endregion

#pragma warning disable 67

namespace XNAControls.Test
{
	class GraphicsDeviceService : IGraphicsDeviceService
	{
		#region Fields

		static GraphicsDeviceService singletonInstance;
		static int referenceCount;

		#endregion

		GraphicsDeviceService(IntPtr windowHandle, int width, int height)
		{
			var parameters = new PresentationParameters
			{
				BackBufferWidth = Math.Max(width, 1),
				BackBufferHeight = Math.Max(height, 1),
				BackBufferFormat = SurfaceFormat.Color,
				DepthStencilFormat = DepthFormat.Depth24,
				DeviceWindowHandle = windowHandle,
				PresentationInterval = PresentInterval.Immediate,
				IsFullScreen = false
			};

			GraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, parameters);
		}

		public static GraphicsDeviceService AddRef(IntPtr windowHandle, int width, int height)
		{
			if (Interlocked.Increment(ref referenceCount) == 1)
				singletonInstance = new GraphicsDeviceService(windowHandle, width, height);

			return singletonInstance;
		}

		public GraphicsDevice GraphicsDevice { get; private set; }

		public event EventHandler<EventArgs> DeviceCreated;
		public event EventHandler<EventArgs> DeviceDisposing;
		public event EventHandler<EventArgs> DeviceReset;
		public event EventHandler<EventArgs> DeviceResetting;
	}
}