// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using Microsoft.Xna.Framework;

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
            Game.RunOneFrame(); //creates necessary graphics device so tests will pass
        }

        public void Dispose()
        {
            GraphicsDeviceManager.Dispose();
            Game.Dispose();
        }
    }
}
