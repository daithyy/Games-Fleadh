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
    class HeavySentry : Sentry
    {
        public HeavySentry(Game game, Vector2 startPosition, List<TileRef> sheetRefs, 
            int frameWidth, int frameHeight, float layerDepth, string nameIn, 
            SoundEffect tankHumSound, SoundEffect tankTrackSound, float angle) 
            : base(game, startPosition, sheetRefs, frameWidth, frameHeight, 
                  layerDepth, nameIn, tankHumSound, tankTrackSound, angle)
        {
            MaxVelocity /= 2;
            turnSpeed /= 2;
        }
    }
}
