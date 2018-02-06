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
            this.Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            TilePlayer p = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));
            Projectile projectile = (Projectile)Game.Services.GetService(typeof(Projectile));

            CollideWithPlayer(p);
            CollideWithProjectile(projectile);

            base.Update(gameTime);
        }

        private void CollideWithPlayer(TilePlayer obj)
        {
            if (obj == null) return;
            else
            {
                if (obj.BoundingRectangle.Intersects(CollisionField))
                {
                    //Rectangle overlap = Rectangle.Intersect(this.CollisionField, obj.BoundingRectangle);
                    obj.PixelPosition = obj.PreviousPosition;
                    //obj.PixelPosition += ;
                }
            }
        }

        private void CollideWithProjectile(Projectile obj)
        {
            if (obj == null) return;
            else
            {
                Rectangle projectileBounds = new Rectangle(
                    new Point(
                    (int)obj.CentrePos.X - (obj.ProjectileWidth), 
                    (int)obj.CentrePos.Y), 
                    new Point(obj.ProjectileWidth, obj.ProjectileHeight));

                if (projectileBounds.Intersects(CollisionField))
                {
                    obj.PixelPosition = obj.PreviousPosition;
                    obj.ProjectileState = Projectile.PROJECTILE_STATUS.Exploding;
                    obj.flyTimer = 0f;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();

            if (Visible)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate,
                        BlendState.AlphaBlend, null, null, null, null, Camera.CurrentCameraTranslation);
                spriteBatch.Draw(texture, CollisionField, Color.White); spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
