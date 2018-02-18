using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Penumbra;

using AnimatedSprite;
using Tiling;
using Helpers;
using CameraNS;

namespace Tiler
{
    public class Projectile : RotatingSprite
    {
        #region Properties
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
        public Vector2 PreviousPosition;
        public int Velocity = 25; // Default bullet speed
        public Vector2 Direction;
        private Random damageRate = new Random();
        public int sentryDamageRate = 35;
        public int playerDamageRate = 10;
        public float explosionLifeSpan = 2f; // Default explosion life in seconds
        private const float FLYING_LIFE_SPAN = 1f; // Default flight life in seconds
        private float timer = 0;
        public float flyTimer = 0;
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
        public int ProjectileWidth = 12;
        public int ProjectileHeight = 2;
        public Light OrbLight { get; } = new PointLight
        {
            Scale = new Vector2(200),
            Intensity = 0.25f,
            Color = Color.Orange,
            ShadowType = ShadowType.Illuminated
        };
        private Vector2 defaultScale = new Vector2(200f);
        #endregion

        #region Constructor
        public Projectile(Game game, string ParentName, Vector2 projectilePosition, List<TileRef> sheetRefs, 
            int frameWidth, int frameHeight, float layerDepth, Vector2 direction, 
            int speed, SoundEffect sndShoot, SoundEffect sndPierce)
            : base(game, projectilePosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            Parent = ParentName;
            Target = Vector2.Zero;
            Direction = direction;
            DrawOrder = 50;
            StartPosition = projectilePosition;
            _sndShoot = sndShoot;
            _sndPierce = sndPierce;
            Velocity = speed;

            PenumbraComponent penumbra = Game.Services.GetService<PenumbraComponent>();
            penumbra.Lights.Add(OrbLight);
            OrbLight.Enabled = false;
        }
        #endregion

        #region Methods
        public override void Update(GameTime gameTime)
        {
            if (Helper.CurrentGameStatus == GameStatus.PLAYING)
            {
                PreviousPosition = PixelPosition;
                OrbLight.Position = new Vector2(CentrePos.X, CentrePos.Y) - Camera.CamPos;

                switch (projectileState)
                {
                    case PROJECTILE_STATUS.Idle:
                        this.Visible = false;
                        OrbLight.Enabled = false;
                        break;

                    case PROJECTILE_STATUS.Firing:
                        flyTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        this.Visible = true;
                        this.gotDirection = false;
                        this.PixelPosition += (Direction * Velocity);
                        OrbLight.Enabled = true;
                        OrbLight.Scale = defaultScale;

                        FaceThis(gameTime);

                        CollisionCheck();

                        // Play sounds
                        if (!ShootSoundPlayed)
                        {
                            ShootSound.Play();
                            ShootSoundPlayed = true;
                        }
                        break;

                    case PROJECTILE_STATUS.Exploding:
                        this.Visible = false;
                        ShootSoundPlayed = false;
                        
                        OrbLight.Scale = defaultScale * 2;

                        timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (timer > explosionLifeSpan - explosionLifeSpan + 0.1f)
                            OrbLight.Enabled = false;

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

        private void CollisionCheck()
        {
            // Projectile is out of tile map bounds
            if (this.PixelPosition.X < 0 || this.PixelPosition.Y < 0
                || this.PixelPosition.X > Camera.WorldBound.X
                || this.PixelPosition.Y > Camera.WorldBound.Y
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

                if (CollideWith(player))
                {
                    //playerDamageRate = damageRate.Next(5, 15);
                    flyTimer = 0f;
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
                    if (CollideWith(otherSentry))
                    {
                        //sentryDamageRate = damageRate.Next(30, 40);
                        flyTimer = 0f;
                        projectileState = PROJECTILE_STATUS.Exploding;
                        otherSentry.Health -= sentryDamageRate;
                        _sndPierce.Play();
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            BulletExplosion Explosion = (BulletExplosion)Game.Services.GetService(typeof(BulletExplosion));

            if (ProjectileState == PROJECTILE_STATUS.Exploding && Explosion.State == BulletExplosion.Effect.Idle)
            {
                Explosion.PixelPosition = this.PixelPosition;
                Explosion.Visible = true;

                if (Explosion.CurrentFrame >= Explosion.FrameCount - 1)
                {
                    Explosion.Visible = false;
                    Explosion.State = BulletExplosion.Effect.Exploding;
                }
            }
            else if (ProjectileState == PROJECTILE_STATUS.Firing)
            {
                Explosion.State = BulletExplosion.Effect.Idle;
            }

            base.Draw(gameTime);
        }

        private bool CollideWith(AnimateSheetSprite other)
        {
            Rectangle myBound = new Rectangle((int)CentrePos.X - this.ProjectileWidth, (int)CentrePos.Y, ProjectileWidth, ProjectileHeight);
            Rectangle otherBound = new Rectangle((int)other.PixelPosition.X, (int)other.PixelPosition.Y, other.FrameWidth, other.FrameHeight);

            return myBound.Intersects(otherBound);
        }
        #endregion
    }
}
