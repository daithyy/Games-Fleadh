using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Tiling;
using AnimatedSprite;

namespace Tiler
{
    class Sentry : RotatingSprite
    {
        #region Properties
        public Vector2 Direction;
        public Vector2 PreviousPosition;
        public string Name;
        #endregion

        #region Constructor
        public Sentry(Game game, Vector2 startPosition,
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth, string nameIn, float angle)
                : base(game, startPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            Name = nameIn;
            DrawOrder = 30;
            this.angleOfRotation = angle;
        }
        #endregion

        #region Methods
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            PreviousPosition = PixelPosition;

            Direction = new Vector2((float)Math.Cos(this.angleOfRotation), (float)Math.Sin(this.angleOfRotation));

            base.Update(gameTime);
        }
        #endregion
    }
}
