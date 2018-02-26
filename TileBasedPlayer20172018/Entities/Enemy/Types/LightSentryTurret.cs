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
        public LightSentryTurret(Game game, Vector2 sentryPosition, List<TileRef> sheetRefs, 
            int frameWidth, int frameHeight, float layerDepth, string nameIn, float angle, 
            SoundEffect turnSound, SoundEffect explosionSound) 
            : base(game, sentryPosition, sheetRefs, 
                  frameWidth, frameHeight, layerDepth, 
                  nameIn, angle, turnSound, explosionSound)
        {
            Health += Health / 2;
            DetectRadius += DetectRadius / 2;
            turnSpeed *= 2;
            
            if (Bullet != null)
            {
                Bullet.OrbLight.Color = Color.Violet;
                Bullet.flyingLifeSpan += Bullet.flyingLifeSpan;
                Bullet.explosionLifeSpan -= Bullet.explosionLifeSpan / 2;
                Bullet.playerDamageRate *= 2;
            }
        }
    }
}
