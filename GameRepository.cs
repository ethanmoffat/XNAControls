// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;

namespace XNAControls
{
    public class GameRepository
    {
        static GameRepository()
        {
            Singleton<GameRepository>.Map(new GameRepository());
        }

        private Game Game { get; set; }

        public static void SetGame(Game game)
        {
            Singleton<GameRepository>.Instance.Game = game;
        }

        public static Game GetGame()
        {
            return Singleton<GameRepository>.Instance.Game;
        }
    }
}
