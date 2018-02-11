using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using Tiling;
using AnimatedSprite;
using Helpers;

namespace Tiler
{
    class Sentry : Tank
    {
        #region Properties
        public string Name;
        public int DetectRadius;
        private const float fadeAmount = 0.05f;
        #endregion

        #region Constructor
        public Sentry(Game game, Vector2 startPosition,
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth,
            string nameIn, SoundEffect tankHumSound, SoundEffect tankTrackSound, float angle)
                : base(game, startPosition, sheetRefs, frameWidth, frameHeight, layerDepth,
                      tankHumSound, tankTrackSound)
        {
            Health = 100;
            Name = nameIn;
            this.angleOfRotation = angle;

            OrbLight.Scale = new Vector2(120);
            OrbLight.Color = Color.HotPink;

            Alpha = 0f;
        }
        #endregion
        
        #region Methods
        private bool IsSpotted()
        {
            TilePlayer player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));

            float distance = Math.Abs(Vector2.Distance(this.CentrePos, player.CentrePos));

            if (distance <= DetectRadius)
                return true;
            else
                return false;
        }
        public override void Update(GameTime gameTime)
        {
            if (IsSpotted())
            {
                Alpha += fadeAmount;
                OrbLight.Enabled = true;
            }
            else
            {
                if (Alpha > 0)
                    Alpha -= fadeAmount;
                OrbLight.Enabled = false;
            }

            Alpha = MathHelper.Clamp(Alpha, 0, 2);

            //PlaySounds();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
        #endregion
    }
}
