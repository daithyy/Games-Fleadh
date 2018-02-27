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
        private Color RoundColor = new Color(255, 0, 80);
        public SoundEffect shellSoundAlt;

        public Ricochet(Game game, string ParentName, Vector2 projectilePosition, 
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth, 
            Vector2 direction, int speed, SoundEffect sndShoot, SoundEffect sndPierce,
            SoundEffect shellSoundAlt) 
            : base(game, ParentName, projectilePosition, sheetRefs, 
                  frameWidth, frameHeight, layerDepth, 
                  direction, speed, sndShoot, sndPierce)
        {
            OrbLight.Color = RoundColor;
            this.shellSoundAlt = shellSoundAlt;
        }
    }
}
