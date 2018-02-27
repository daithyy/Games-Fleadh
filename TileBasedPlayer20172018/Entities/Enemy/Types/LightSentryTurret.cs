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
    class LightSentryTurret : SentryTurret
    {
        private const int FACTOR = 2;
        
        public LightSentryTurret(Game game, Vector2 sentryPosition, List<TileRef> sheetRefs, 
            int frameWidth, int frameHeight, float layerDepth, string nameIn, float angle, 
            SoundEffect turnSound, SoundEffect explosionSound) 
            : base(game, sentryPosition, sheetRefs, 
                  frameWidth, frameHeight, layerDepth, 
                  nameIn, angle, turnSound, explosionSound)
        {
            Health += Health / FACTOR;
            DetectRadius += DetectRadius / FACTOR;
            turnSpeed *= FACTOR;
            
            if (Bullet != null)
            {
                Bullet.OrbLight.Color = Color.Violet;
                Bullet.flyingLifeSpan *= FACTOR;
                Bullet.explosionLifeSpan -= Bullet.explosionLifeSpan / FACTOR;
                Bullet.playerDamageRate *= FACTOR;
            }
        }
    }
}
