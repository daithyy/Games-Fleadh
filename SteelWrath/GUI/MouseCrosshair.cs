using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using AnimatedSprite;
using Tiling;
using Helpers;
using InputManager;
using CameraNS;

namespace Tiler
{
    class MouseCrosshair : RotatingSprite
    {
        public bool Disabled;

        public MouseCrosshair(Game game, Vector2 userPosition, List<TileRef> sheetRefs, 
            int frameWidth, int frameHeight, float layerDepth) 
            : base(game, userPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            DrawOrder = 100;
            Scale = 2;
            Alpha = 0.5f;
        }

        public override void Update(GameTime gametime)
        {
            if (!Disabled && Helper.CurrentGameStatus == GameStatus.PLAYING)
            {
                if (InputEngine.UsingKeyboard)
                {
                    PixelPosition = InputEngine.MousePosition - new Vector2(FrameWidth / 2, FrameHeight / 2);
                    PixelPosition += Camera.CamPos;
                }

                TilePlayerTurret playerTurret = (TilePlayerTurret)Game.Services.GetService(typeof(TilePlayerTurret));
                Disabled = playerTurret.IsDead;

                base.Update(gametime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Disabled && InputEngine.UsingKeyboard)
                base.Draw(gameTime);
        }
    }
}
