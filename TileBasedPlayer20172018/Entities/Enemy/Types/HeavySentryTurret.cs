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
        private const int FACTOR = 5;

        public HeavySentryTurret(Game game, Vector2 sentryPosition, List<TileRef> sheetRefs, 
            int frameWidth, int frameHeight, float layerDepth, string nameIn, float angle, 
            SoundEffect turnSound, SoundEffect explosionSound) 
            : base(game, sentryPosition, sheetRefs, 
                  frameWidth, frameHeight, layerDepth, 
                  nameIn, angle, turnSound, explosionSound)
        {
            Health *= FACTOR;
            Hbar.Factor = FACTOR;
            DetectRadius += DetectRadius / 2;
            turnSpeed /= 1.5f;
        }

        public void UpdateProjectile()
        {
            if (Bullet != null)
            {
                Bullet.OrbLight.Color = Color.Gold;
                Bullet.flyingLifeSpan += Bullet.flyingLifeSpan * 2;
                Bullet.playerDamageRate *= 3;
            }
        }
    }
}
