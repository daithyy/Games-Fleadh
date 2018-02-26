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
    class HeavySentryTurret : SentryTurret
    {
        public HeavySentryTurret(Game game, Vector2 sentryPosition, List<TileRef> sheetRefs, 
            int frameWidth, int frameHeight, float layerDepth, string nameIn, float angle, 
            SoundEffect turnSound, SoundEffect explosionSound) 
            : base(game, sentryPosition, sheetRefs, 
                  frameWidth, frameHeight, layerDepth, 
                  nameIn, angle, turnSound, explosionSound)
        {
            Health *= 4;
            Hbar.HealthRect.Size /= new Point(2,1);
            DetectRadius += DetectRadius / 2;
            turnSpeed /= 2;

            if (Bullet != null)
            {
                Bullet.OrbLight.Color = Color.Yellow;
                Bullet.flyingLifeSpan += Bullet.flyingLifeSpan * 2;
                Bullet.playerDamageRate += (Bullet.playerDamageRate * 2) + (Bullet.playerDamageRate / 2);
            }
        }
    }
}
