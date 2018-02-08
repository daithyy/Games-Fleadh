using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using AnimatedSprite;

namespace Tiling
{
    class BulletExplosion : Explosion
    {
        public BulletExplosion(Game game, Vector2 userPosition, List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth)
            : base(game, userPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            Scale = 0.5f;
        }
    }
}
