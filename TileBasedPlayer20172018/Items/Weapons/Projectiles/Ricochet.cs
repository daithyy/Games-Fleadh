using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

using Tiling;

namespace Tiler
{
    class Ricochet : Projectile
    {
        public Ricochet(Game game, string ParentName, Vector2 projectilePosition, 
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth, 
            Vector2 direction, int speed, SoundEffect sndShoot, SoundEffect sndPierce) 
            : base(game, ParentName, projectilePosition, sheetRefs, 
                  frameWidth, frameHeight, layerDepth, 
                  direction, speed, sndShoot, sndPierce)
        {

        }
    }
}
