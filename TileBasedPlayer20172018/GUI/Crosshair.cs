using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Engine.Engines;
using Tiling;
using AnimatedSprite;

namespace Tiler
{
    public class Crosshair : RotatingSprite
    {
        public Crosshair(Game game, Vector2 userPosition,
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth)
                : base(game, userPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            DrawOrder = 100;
        }

        public override void Update(GameTime gameTime)
        {
            Vector2 MouseTrackPos = (InputEngine.MousePosition - new Vector2(FrameWidth / 2, FrameHeight / 2) + CameraNS.Camera.CamPos);
            PixelPosition = MouseTrackPos;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
