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
