using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using AnimatedSprite;

namespace Tiling
{
    class TankExplosion : AnimateSheetSprite
    {
        public TankExplosion(Game game, Vector2 userPosition, List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth) 
            : base(game, userPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            DrawOrder = 101;
            Visible = false;
        }
    }
}
