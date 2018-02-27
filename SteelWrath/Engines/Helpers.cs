using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using InputManager;

namespace Helpers
{
    public enum ActiveScreen { MAIN, PLAY, PAUSE, LOSE, WIN };
    public enum GameCondition { WIN, LOSE }
    public enum GameStatus { PAUSED, PLAYING }

    public static class Helper
    {
        public static GameStatus CurrentGameStatus = GameStatus.PAUSED;
        public static SpriteFont GameFont;
        public static GraphicsDevice graphicsDevice;
    }
}

