using AnimatedSprite;
using CameraNS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tiler
{
    public class Collider : DrawableGameComponent
    {
        public int tileX;
        public int tileY;
        public Texture2D texture;
        bool visible = false;

        public Vector2 WorldPosition
        {
            get
            {
                return new Vector2(tileX * texture.Width, tileY * texture.Height);
            }

        }

        public Rectangle CollisionField
        {
            get
            {
                return new Rectangle(WorldPosition.ToPoint(), new Point(texture.Width, texture.Height));
            }

        }

        public Collider(Game game, Texture2D tx, int tlx, int tly) : base(game)
        {
            game.Components.Add(this);
            texture = tx;
            tileX = tlx;
            tileY = tly;
            DrawOrder = 2;
        }

        public override void Update(GameTime gameTime)
        {
            TilePlayer p = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));
            Projectile projectile = (Projectile)Game.Services.GetService(typeof(Projectile));

            if (p == null) return;
            else
            {
                if (p.BoundingRectangle.Intersects(CollisionField))
                {
                    p.PixelPosition = p.PreviousPosition;
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();
            if (visible)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate,
                        BlendState.AlphaBlend, null, null, null, null, Camera.CurrentCameraTranslation);
                spriteBatch.Draw(texture, CollisionField, Color.White); spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
