using Microsoft.Xna.Framework;

namespace XNAControls
{
    /// <summary>
    /// Holds a reference to a Game object. Used as a default for constructing all controls
    /// </summary>
    public class GameRepository
    {
        static GameRepository()
        {
            Singleton<GameRepository>.MapIfMissing(new GameRepository());
        }

        private Game Game { get; set; }

        /// <summary>
        /// Set the game to use as the default game
        /// </summary>
        public static void SetGame(Game game)
        {
            Singleton<GameRepository>.Instance.Game = game;
        }

        /// <summary>
        /// Get the game used as the default game
        /// </summary>
        public static Game GetGame()
        {
            return Singleton<GameRepository>.Instance.Game;
        }
    }
}
