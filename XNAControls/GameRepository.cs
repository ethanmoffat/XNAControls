using Microsoft.Xna.Framework;

namespace XNAControls
{
    public class GameRepository
    {
        static GameRepository()
        {
            Singleton<GameRepository>.MapIfMissing(new GameRepository());
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
