// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAControls.Test.Helpers
{
    public class TestGameManager : IDisposable
    {
        public Game Game { get; }
        public GraphicsDeviceManager GraphicsDeviceManager { get; }
        
        public TestGameManager()
        {
            Game = new Game();
            GraphicsDeviceManager = new GraphicsDeviceManager(Game);

            //remove the default graphics device service and inject our own
            Game.Services.RemoveService(typeof(IGraphicsDeviceService));

            var gds = GraphicsDeviceService.AddRef(Game.Window.Handle,
                                                   Game.Window.ClientBounds.Width,
                                                   Game.Window.ClientBounds.Height);
            Game.Services.AddService(typeof(IGraphicsDeviceService), gds);
        }

        public void Dispose()
        {
            GraphicsDeviceManager.Dispose();
            Game.Dispose();
        }
    }
}
