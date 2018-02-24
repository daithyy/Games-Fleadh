using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

using InputManager;
using AnimatedSprite;
using Tiling;
using Helpers;
using CameraNS;

namespace Tiler
{
    public class SentryTurret : RotatingSprite
    {
        #region Properties
        float volumeVelocity = 0;
        public int DetectRadius = 400;
        float turnSpeed = 0.015f;
        const float WIDTH_IN = 11f; // Width in from the left for the sprites origin
        float angleOfRotationPrev;
        public string Name;
        public Projectile Bullet;
        public Vector2 Direction;
        private SoundEffect ExplosionSound;
        private SoundEffect TankTurnSound;
        private SoundEffectInstance TurnSoundInstance;
        HealthBar healthBar;
        public static int Count = 0; // Keeps track of amount of Sentries created
        public bool IsDead = false;
        private float explosionTimer = 0f;
        private Vector2 trueOrigin
        {
            get
            {
                return new Vector2(((FrameWidth / 2) - WIDTH_IN), (FrameHeight / 2));
            }
        }
        private Sentry parentBody;
        private MuzzleFlash muzzleFlash;
        private TankExplosion explosionSprite;
        #endregion

        #region Constructor
        public SentryTurret(Game game, Vector2 sentryPosition,
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth, string nameIn,
            float angle,
            SoundEffect turnSound,
            SoundEffect explosionSound)
                : base(game, sentryPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            Name = nameIn;
            DrawOrder = 60;
            origin = trueOrigin;
            this.angleOfRotation = angle;
            healthBar = new HealthBar(game, PixelPosition);
            Health = 100;
            AddHealthBar(healthBar);
            Interlocked.Increment(ref Count);

            #region Turret Audio
            ExplosionSound = explosionSound;
            TankTurnSound = turnSound;
            TurnSoundInstance = TankTurnSound.CreateInstance();
            TurnSoundInstance.Volume = 0f;
            TurnSoundInstance.Pitch = -0.75f;
            TurnSoundInstance.IsLooped = true;
            TurnSoundInstance.Play();
            #endregion

            #region Muzzleflash Sprite
            muzzleFlash = new MuzzleFlash(Game, PixelPosition, new List<TileRef>()
            {
                new TileRef(11,1,0),
                new TileRef(11,0,0),
            }, 64, 64, 0f);
            #endregion

            #region Explosion Sprite
            explosionSprite = new TankExplosion(Game, PixelPosition, new List<TileRef>()
            {
                new TileRef(0,6,0),
                new TileRef(1,6,0),
                new TileRef(2,6,0),
                new TileRef(3,6,0),
                new TileRef(4,6,0),
                new TileRef(5,6,0),
                new TileRef(6,6,0),
                new TileRef(7,6,0)
            }, 64, 64, 0f)
            { Visible = false };
            #endregion
        }
        #endregion

        #region Methods
        private void FindBody()
        {
            List<Sentry> Sentries = (List<Sentry>)Game.Services.GetService(typeof(List<Sentry>));

            foreach (Sentry sentry in Sentries)
            {
                // Find appropiate body with the same name
                if (Name == sentry.Name && sentry != null)
                {
                    parentBody = sentry;
                }
            }
        }

        public void AddProjectile(Projectile projectileIn)
        {
            Bullet = projectileIn;
        }

        public bool IsInRadius(TilePlayer player)
        {
            float distance = Math.Abs(Vector2.Distance(this.CentrePos, player.CentrePos));

            if (distance <= DetectRadius)
                return true;
            else
                return false;
        }

        private void Detect(GameTime gameTime)
        {
            TilePlayer player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));

            if (IsInRadius(player))
            {
                angleOfRotation = TurnToFace(CentrePos, player.CentrePos, this.angleOfRotation, turnSpeed);

                // Shoot at the player
                FireAt(gameTime, player);
            }
            else
                angleOfRotation = TurnToFace(CentrePos, CentrePos + parentBody.Direction, angleOfRotation, turnSpeed);
        }

        public void FireAt(GameTime gameTime, TilePlayer player)
        {
            if (Bullet != null && Bullet.ProjectileState == Projectile.PROJECTILE_STATUS.Idle)
            {
                Bullet.PixelPosition = (this.PixelPosition - new Vector2(WIDTH_IN, 0));
            }

            if (Bullet != null)
            {
                if (Bullet.ProjectileState == Projectile.PROJECTILE_STATUS.Idle
                    && this.angleOfRotation != 0 && Math.Round(this.angleOfRotationPrev, 2) == Math.Round(this.angleOfRotation, 2))
                {
                    // Send this direction to the projectile
                    Bullet.GetDirection(Direction);
                    // Shoot at the specified position
                    Bullet.Shoot(player.CentrePos);
                    // Draw muzzleflash
                    muzzleFlash.angleOfRotation = this.angleOfRotation;
                    muzzleFlash.PixelPosition = this.PixelPosition - new Vector2(10, 0) + (this.Direction * (FrameWidth - 10));
                    muzzleFlash.Visible = true;
                    muzzleFlash.OrbLight.Enabled = true;
                    muzzleFlash.Draw(gameTime);
                }
                else
                {
                    muzzleFlash.Visible = false;
                    muzzleFlash.OrbLight.Enabled = false;
                }
            }
        }

        public void AddSelfToBody(Vector2 followPos)
        {
            this.PixelPosition = followPos;
        }

        public void PlaySounds()
        {
            TurnSoundInstance.Play();

            volumeVelocity = (turnSpeed * 4); // 0.06
            volumeVelocity = MathHelper.Clamp(volumeVelocity, 0, 0.8f);

            if (Math.Round(this.angleOfRotationPrev, 2) != Math.Round(this.angleOfRotation, 2))
            {
                TurnSoundInstance.Volume = volumeVelocity;
            }
            else
            {
                TurnSoundInstance.Volume = 0f;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Helper.CurrentGameStatus == GameStatus.PLAYING)
            {
                if (Health > 0 && !IsDead)
                {
                    FindBody();

                    #region Share Properties
                    // Props this turret onto the appropiate tank body
                    AddSelfToBody(parentBody.PixelPosition + new Vector2(WIDTH_IN, 0f));

                    // Share properties
                    parentBody.Health = this.Health;
                    parentBody.DetectRadius = (this.DetectRadius + DetectRadius / 4);
                    Alpha = parentBody.Alpha;
                    healthBar.Alpha = this.Alpha;
                    //this.Visible = parentBody.Visible;
                    #endregion

                    angleOfRotationPrev = this.angleOfRotation;

                    Direction = new Vector2((float)Math.Cos(this.angleOfRotation), (float)Math.Sin(this.angleOfRotation));

                    // Face and shoot at the player when player is within radius
                    Detect(gameTime);

                    //PlaySounds();

                    base.Update(gameTime);
                }
                else if (!IsDead)
                {
                    TurnSoundInstance.Stop();
                    Interlocked.Decrement(ref Count);
                    ExplosionSound.Play();
                    IsDead = true;
                    parentBody.IsDead = true;
                }
            }
            else
            {
                TurnSoundInstance.Stop();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (Health > 0)
            {
                base.Draw(gameTime);
            }
            else
            {
                float maxExplosionTime = 0.75f;

                if (explosionTimer < maxExplosionTime)
                {
                    explosionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    explosionSprite.PixelPosition = this.PixelPosition - new Vector2(10, -2);
                    explosionSprite.Visible = true;
                    explosionSprite.OrbLight.Position = CentrePos - Camera.CamPos;
                    if (explosionTimer < maxExplosionTime - maxExplosionTime + 0.3f)
                        explosionSprite.OrbLight.Intensity = 0.25f;
                }
                else
                    explosionSprite.Visible = false;
            }
        }
        #endregion
    }
}
