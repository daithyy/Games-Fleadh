using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Tiling;

namespace AnimatedSprite
{
    class TankWreckSprite : RotatingSprite
    {
        public TankWreckSprite(Game game, Vector2 userPosition, List<TileRef> sheetRefs, 
            int frameWidth, int frameHeight, float layerDepth) 
            : base(game, userPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            DrawOrder = 29;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
