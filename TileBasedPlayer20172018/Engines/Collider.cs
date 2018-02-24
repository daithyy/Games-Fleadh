using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;

using AnimatedSprite;
using CameraNS;
using Helpers;

namespace Tiler
{
    public class Collider : DrawableGameComponent
    {
        #region Properties
        public enum CollisionDirection
        {
            Top,
            Bottom,
            Left,
            Right
        }
        public CollisionDirection Direction;
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
        public Hull Shadow;
        private List<Sentry> sentries;
        private TilePlayer player;
        private Ricochet projectile;
        private List<Projectile> sentryProjectiles;

        #region Minkowski Sum Variables
        private Rectangle overlap;
        private Rectangle projectileBounds;
        float w;
        float h;
        float dx;
        float dy;
        float wy;
        float hx;
        #endregion
        #endregion

        #region Constructor
        public Collider(Game game, Texture2D tx, int tlx, int tly) : base(game)
        {
            game.Components.Add(this);
            texture = tx;
            tileX = tlx;
            tileY = tly;
            DrawOrder = 2;
            this.Visible = false;

            Shadow = new Hull(
                new Vector2(1.0f),
                new Vector2(-1.0f, 1.0f),
                new Vector2(-1.0f),
                new Vector2(1.0f, -1.0f));
            Shadow.Scale = new Vector2(texture.Width / 2, texture.Height / 2);

            PenumbraComponent penumbra = Game.Services.GetService<PenumbraComponent>();
            penumbra.Hulls.Add(Shadow);
        }
        #endregion

        #region Methods
        private void MinkowskiSum(Tank obj)
        {
            overlap = Rectangle.Intersect(this.CollisionField, obj.BoundingRectangle);

            #region Minkowski Sum of B and A
            w = 0.5f * (obj.BoundingRectangle.Width + CollisionField.Width);
            h = 0.5f * (obj.BoundingRectangle.Height + CollisionField.Height);
            dx = obj.BoundingRectangle.Center.X - CollisionField.Center.X;
            dy = obj.BoundingRectangle.Center.Y - CollisionField.Center.Y;

            if (Math.Abs(dx) <= w && Math.Abs(dy) <= h)
            {
                /* collision! */
                wy = w * dy;
                hx = h * dx;

                if (wy > hx)
                    if (wy > -hx)
                    {
                        /* collision at the top */
                        obj.PixelPosition += new Vector2(0, overlap.Height);
                        Direction = CollisionDirection.Top;
                    }
                    else
                    {
                        /* on the left */
                        obj.PixelPosition -= new Vector2(overlap.Width, 0);
                        Direction = CollisionDirection.Left;
                    }
                else
                {
                    if (wy > -hx)
                    {
                        /* on the right */
                        obj.PixelPosition += new Vector2(overlap.Width, 0);
                        Direction = CollisionDirection.Right;
                    }
                    else
                    {
                        /* at the bottom */
                        obj.PixelPosition -= new Vector2(0, overlap.Height);
                        Direction = CollisionDirection.Bottom;
                    }
                }
            }
            #endregion
        }
        private void MinkowskiSum(Ricochet obj)
        {
            w = 0.5f * (obj.BoundingRectangle.Width + CollisionField.Width);
            h = 0.5f * (obj.BoundingRectangle.Height + CollisionField.Height);
            dx = obj.BoundingRectangle.Center.X - CollisionField.Center.X;
            dy = obj.BoundingRectangle.Center.Y - CollisionField.Center.Y;

            if (Math.Abs(dx) <= w && Math.Abs(dy) <= h)
            {
                /* collision! */
                wy = w * dy;
                hx = h * dx;

                if (wy > hx)
                {
                    if (wy > -hx)
                    {
                        /* collision at the top */
                        obj.Direction.Y = -obj.Direction.Y;
                        return;
                    }
                    else
                    {
                        /* on the left */
                        obj.Direction.X = -obj.Direction.X;
                        obj.Scale = -obj.Scale;
                        return;
                    }
                }
                else
                {
                    if (wy > -hx)
                    {
                        /* on the right */
                        obj.Direction.X = -obj.Direction.X;
                        obj.Scale = -obj.Scale;
                        return;
                    }
                    else
                    {
                        /* at the bottom */
                        obj.Direction.Y = -obj.Direction.Y;
                        return;
                    }
                }
            }
        }

        private void UpdateCollisions()
        {
            sentries = (List<Sentry>)Game.Services.GetService(typeof(List<Sentry>));
            player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));
            projectile = (Ricochet)Game.Services.GetService(typeof(Ricochet));
            sentryProjectiles = (List<Projectile>)Game.Services.GetService(typeof(List<Projectile>));

            CollideWithProjectile(projectile);
            CollideWithTank(player);

            for (int i = 0; i < sentries.Count; i++)
            {
                if (!sentries[i].IsDead)
                    CollideWithTank(sentries[i]);
            }

            for (int i = 0; i < sentryProjectiles.Count; i++)
            {
                CollideWithProjectile(sentryProjectiles[i]);
            }
        }

        private void UpdateShadow()
        {
            Shadow.Position = (WorldPosition / 2) +
            new Vector2(
                WorldPosition.X / 2,
                WorldPosition.Y / 2) +
            new Vector2(
                texture.Width / 2,
                texture.Height / 2) -
                Camera.CamPos;
        }

        private void CollideWithTank(Tank obj)
        {
            if (obj != null && obj.PixelPosition != obj.PreviousPosition)
            {
                float distance = Math.Abs(Vector2.Distance(WorldPosition, obj.CentrePos));
                if (distance <= 100)
                {
                    MinkowskiSum(obj);
                    //Visible = true;
                }
                //else
                //    Visible = false;
            }
        }

        private void CollideWithProjectile(Projectile obj)
        {
            if (obj != null && obj.ProjectileState != Projectile.PROJECTILE_STATUS.Idle)
            {
                projectileBounds = new Rectangle(
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
        private void CollideWithProjectile(Ricochet obj)
        {
            if (obj != null && obj.ProjectileState != Projectile.PROJECTILE_STATUS.Idle)
            {
                projectileBounds = new Rectangle(
                    new Point(
                    (int)obj.CentrePos.X - (obj.ProjectileWidth),
                    (int)obj.CentrePos.Y),
                    new Point(obj.ProjectileWidth, obj.ProjectileHeight));

                if (projectileBounds.Intersects(CollisionField))
                {
                    obj.PixelPosition = obj.PreviousPosition;
                    obj.angleOfRotation = -obj.angleOfRotation;
                    MinkowskiSum(obj);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            UpdateShadow();

            if (Helper.CurrentGameStatus != GameStatus.PAUSED)
            {
                UpdateCollisions();
            }

            base.Update(gameTime);
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
        #endregion
    }
}
