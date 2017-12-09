﻿using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers
{
    public enum ActiveScreen { MAIN, PLAY, PAUSE, LOSE, WIN };
    public enum GameCondition { WIN, LOSE }
    public static class Helper
    {

        public static SpriteFont GameFont;
        public static GraphicsDevice graphicsDevice;
    }
}

