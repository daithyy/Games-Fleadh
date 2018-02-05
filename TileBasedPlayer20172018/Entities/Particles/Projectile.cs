using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

using Engine.Engines;
using AnimatedSprite;
using Tiling;
using Helpers;
using CameraNS;

namespace Tiler
{
    public class Projectile : RotatingSprite
    {
        public enum PROJECTILE_STATUS
        {
            Idle,
            Firing,
            Exploding
        }
        private PROJECTILE_STATUS projectileState = PROJECTILE_STATUS.Idle;
        public PROJECTILE_STATUS ProjectileState
        {
            get { return projectileState; }
            set { projectileState = value; }
        }
        private Vector2 Target;
        private Vector2 StartPosition;
        private Vector2 PreviousPosition;
        public Vector2 CentrePos
        {
            get
            {
                return PixelPosition + new Vector2(FrameWidth / 2, FrameHeight / 2);
            }
        }
        public int Velocity = 15;
        public Vector2 Direction;
        private Random damageRate = new Random();
        private int sentryDamageRate = 35;
        private int playerDamageRate = 10;
        private float explosionLifeSpan = 2f; // Default explosion life in seconds
        private const float FLYING_LIFE_SPAN = 1f; // Default flight life in seconds
        private float timer = 0;
        private float flyTimer = 0;
        private string Parent;
        private bool isPastTarget = false;
        private bool gotDirection = false;
        private SoundEffect _sndShoot;
        public SoundEffect ShootSound
        {
            get
            { return _sndShoot; }
            set
            { _sndShoot = value; }
        }
        private SoundEffect _sndPierce;
        public bool ShootSoundPlayed = false;

        public Projectile(Game game, string ParentName, Vector2 projectilePosition, List<TileRef> sheetRefs, 
            int frameWidth, int frameHeight, float layerDepth, Vector2 direction, SoundEffect sndShoot, SoundEffect sndPierce)
            : base(game, projectilePosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            Parent = ParentName;
            Target = Vector2.Zero;
            Direction = direction;
            DrawOrder = 50;
            StartPosition = projectilePosition;
            _sndShoot = sndShoot;
            _sndPierce = sndPierce;
            if (Parent.ToUpper() == "SENTRY") Velocity /= 3;
        }

        #region LifeSpan Overload Method
        public Projectile(Game game, string ParentName, Vector2 projectilePosition, List<TileRef> sheetRefs,
            int frameWidth, int frameHeight, float layerDepth, Vector2 direction, SoundEffect sndShoot, SoundEffect sndPierce, 
            float lifeSpanIn)
            : base(game, projectilePosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            Parent = ParentName;
            Target = Vector2.Zero;
            Direction = direction;
            DrawOrder = 50;
            explosionLifeSpan = lifeSpanIn;
            _sndShoot = sndShoot;
            _sndPierce = sndPierce;
            if (Parent.ToUpper() == "SENTRY") Velocity /= 3;
        }
        #endregion

        public override void Update(GameTime gameTime)
        {
            if (Helper.CurrentGameStatus == GameStatus.PLAYING)
            {
                PreviousPosition = PixelPosition;

                switch (projectileState)
                {
                    case PROJECTILE_STATUS.Idle:
                        this.Visible = false;
                        break;

                    case PROJECTILE_STATUS.Firing:
                        flyTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        this.Visible = true;
                        this.gotDirection = false;
                        this.PixelPosition += (Direction * Velocity);

                        FaceThis(gameTime);

                        #region Collision Checking

                        // Projectile is out of tile map bounds
                        if (this.PixelPosition.X < 0 || this.PixelPosition.Y < 0
                            || this.PixelPosition.X > CameraNS.Camera._worldBound.X
                            || this.PixelPosition.Y > CameraNS.Camera._worldBound.Y
                            || flyTimer > FLYING_LIFE_SPAN)
                        {
                            flyTimer = 0f;
                            projectileState = PROJECTILE_STATUS.Exploding;
                        }

                        // Ensure the sentry doesn't shoot itself !
                        if (Parent != "PLAYER")
                        {
                            Camera thisCamera = (Camera)Game.Services.GetService(typeof(Camera));
                            TilePlayer player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));

                            if (collisionDetect(player))
                            {
                                playerDamageRate = damageRate.Next(5, 15);
                                projectileState = PROJECTILE_STATUS.Exploding;
                                player.Health -= playerDamageRate;
                                thisCamera.Shake(7.5f, 0.25f);
                                _sndPierce.Play();
                            }
                        }
                        else
                        {
                            // Reference Collision Objects
                            List<SentryTurret> SentryTurretList = (List<SentryTurret>)Game.Services.GetService(typeof(List<SentryTurret>));

                            // Check collision with Sentry
                            foreach (SentryTurret otherSentry in SentryTurretList)
                            {
                                if (collisionDetect(otherSentry))
                                {
                                    sentryDamageRate = damageRate.Next(30, 40);
                                    projectileState = PROJECTILE_STATUS.Exploding;
                                    otherSentry.Health -= sentryDamageRate;
                                    _sndPierce.Play();
                                }
                            }
                        }
                        #endregion

                        if (!ShootSoundPlayed)
                        {
                            ShootSound.Play();
                            ShootSoundPlayed = true;
                        }

                        //// Old smooth point to point projectile movement, doesn't move beyond target
                        //PixelPosition = Vector2.Lerp(PixelPosition, Target, Velocity);

                        //if (Vector2.Distance(PixelPosition, Target) < Velocity * 2)
                        //{
                        //    projectileState = PROJECTILE_STATUS.Exploding;
                        //}
                        break;

                    case PROJECTILE_STATUS.Exploding:
                        this.Visible = false;
                        ShootSoundPlayed = false;

                        timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (timer > explosionLifeSpan)
                        {
                            timer = 0f;
                            // Reload Projectile
                            projectileState = PROJECTILE_STATUS.Idle;
                        }
                        break;
                }
                base.Update(gameTime);
            }
        }

        public void GetDirection(Vector2 TurretDirection)
        {
            if (!gotDirection)
            {
                Direction = TurretDirection;
                gotDirection = true;
            }
        }

        public void Shoot(Vector2 TargetDirection)
        {
            projectileState = PROJECTILE_STATUS.Firing;
            Target = TargetDirection;
            isPastTarget = false;
        }

        public void FaceThis(GameTime gameTime)
        {

            if (Parent.ToUpper() == "PLAYER" && Vector2.Distance(this.PixelPosition, Target) > 2
                && !isPastTarget)
            {
                this.angleOfRotation = TurnToFace(PixelPosition, Target, angleOfRotation, 10f);
                isPastTarget = true;
            }
            else if (!isPastTarget)
            {
                this.angleOfRotation = TurnToFace(CentrePos, Target, angleOfRotation, 10f);
                isPastTarget = true;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            BulletExplosion Explosion = (BulletExplosion)Game.Services.GetService(typeof(BulletExplosion));

            if (ProjectileState != Projectile.PROJECTILE_STATUS.Idle)
            {
                Explosion.Visible = false;

                if (ProjectileState == Projectile.PROJECTILE_STATUS.Exploding && timer < 0.75f)
                {
                    Explosion.Visible = true;
                    Explosion.PixelPosition = this.PixelPosition;
                    Explosion.Draw(gameTime);
                }
                base.Draw(gameTime);
            }
        }
    }
}
