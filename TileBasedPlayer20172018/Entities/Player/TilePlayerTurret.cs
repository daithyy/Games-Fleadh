using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

using InputManager;
using AnimatedSprite;
using Tiling;
using CameraNS;
using Helpers;

namespace Tiler
{
    class TilePlayerTurret : RotatingSprite
    {
        #region Properties
        float volumeVelocity = 0;
        private float turnSpeed = 0.04f;
        private const float WIDTH_IN = 15f; // Width in from the left for the sprites origin
        private float angleOfRotationPrev;
        public Projectile Bullet;
        public Vector2 Direction;
        public Vector2 rotateOrigin
        {
            get
            {
                return new Vector2(((FrameWidth / 2) - WIDTH_IN), (FrameHeight / 2));
            }
        }
        private SoundEffect ExplosionSound;
        private SoundEffect ShellSound;
        private SoundEffect ShellReload;
        private SoundEffect HumReload;
        private SoundEffectInstance HumReloadInstance;
        private SoundEffect TankTurnSound;
        private SoundEffectInstance TurnSoundInstance;
        public bool IsDead = false;
        private MuzzleFlash muzzleFlash;
        private TankExplosion explosionSprite;
        #endregion

        #region Constructor
        public TilePlayerTurret(Game game, Vector2 playerPosition,
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth, 
            SoundEffect shellSound, SoundEffect humReload, SoundEffect shellReload, 
            SoundEffect turnSound, SoundEffect explosionSound)
                : base(game, playerPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            DrawOrder = 70;
            origin = rotateOrigin;

            #region Turret Audio
            this.ExplosionSound = explosionSound;
            this.ShellSound = shellSound;
            this.ShellReload = shellReload;
            this.HumReload = humReload;
            TankTurnSound = turnSound;
            HumReloadInstance = HumReload.CreateInstance();
            HumReloadInstance.Volume = 0.1f;
            HumReloadInstance.Pitch = -0.5f;
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
        public void Destroy()
        {
            TurnSoundInstance.Stop();
            ExplosionSound.Play();
            IsDead = true;
        }
        public void AddProjectile(Projectile loadedBullet)
        {
            Bullet = loadedBullet;
        }

        private void HandleTurns()
        {
            switch (InputEngine.UsingKeyboard)
            {
                case true:
                    #region Handle Mouse Movement
                    this.angleOfRotation = TurnToFace(
                        this.PixelPosition - new Vector2(WIDTH_IN, 0),
                        Crosshair.Position, this.angleOfRotation, turnSpeed);
                    #endregion
                    break;
                case false:
                    #region Handle Stick Movement
                    this.angleOfRotation = TurnToFace(
                        this.PixelPosition - new Vector2(WIDTH_IN, 0),
                        this.PixelPosition + (InputEngine.SmoothThumbStick * this.PixelPosition),
                        this.angleOfRotation, turnSpeed);

                    if (InputEngine.SmoothThumbStick == Vector2.Zero)
                        this.angleOfRotation = angleOfRotationPrev;
                    #endregion
                    break;
            }
        }

        private bool InputShoot()
        {
            if (InputEngine.IsMouseLeftHeld() || InputEngine.IsButtonHeld(Buttons.RightTrigger))
                return true;
            else return false;
        }

        public void Fire(GameTime gameTime)
        {
            Camera thisCamera = (Camera)Game.Services.GetService(typeof(Camera));

            if (Bullet != null && Bullet.ProjectileState == Projectile.PROJECTILE_STATUS.Idle)
            {
                Bullet.PixelPosition = (this.PixelPosition - new Vector2(WIDTH_IN, 0));
            }

            if (Bullet != null)
            {
                if (InputShoot()
                    && Bullet.ProjectileState == Projectile.PROJECTILE_STATUS.Idle
                    && this.angleOfRotation != 0
                    && Math.Round(this.angleOfRotationPrev, 2) == Math.Round(this.angleOfRotation, 2))
                {
                    // Send this direction to the projectile
                    Bullet.GetDirection(Direction);
                    // Shoot at the specified position
                    Bullet.Shoot(Crosshair.Position - new Vector2(FrameWidth / 2, FrameHeight / 2));
                    // Draw muzzleflash
                    muzzleFlash.angleOfRotation = this.angleOfRotation;
                    muzzleFlash.PixelPosition = this.PixelPosition - new Vector2(10, 0) + (this.Direction * (FrameWidth - 10));
                    muzzleFlash.Visible = true;
                    muzzleFlash.OrbLight.Enabled = true;
                    muzzleFlash.Draw(gameTime);
                    // Play sounds
                    ShellSound.Play();
                    ShellReload.Play();
                    HumReloadInstance.Play();
                    // Shake the camera
                    thisCamera.Shake(5f, 0.5f);
                }
                else
                {
                    muzzleFlash.Visible = false;
                    muzzleFlash.OrbLight.Enabled = false;
                }
            }
        }

        public void PlaySounds()
        {
            TurnSoundInstance.Play();

            volumeVelocity = turnSpeed;
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
                TilePlayer player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));

                if (player.Health > 0 && !IsDead)
                {
                    // Props this turret onto the underside tank body if it exists
                    if (player != null)
                    {
                        Track(player.PixelPosition + new Vector2(WIDTH_IN, 0f));
                    }

                    angleOfRotationPrev = this.angleOfRotation;

                    // Alternate controls.
                    HandleTurns();

                    // Get direction for projectiles.
                    Direction = new Vector2((float)Math.Cos(this.angleOfRotation), (float)Math.Sin(this.angleOfRotation));

                    Fire(gameTime);
                    PlaySounds();

                    base.Update(gameTime);
                }
                else if (!IsDead)
                {
                    Destroy();
                }
            }
            else
            {
                TurnSoundInstance.Stop();
            }
        }

        public void Track(Vector2 followPos)
        {
            this.PixelPosition = followPos;
        }

        public override void Draw(GameTime gameTime)
        {
            if (!IsDead)
            {
                base.Draw(gameTime);
            }
            else
            {
                explosionSprite.PixelPosition = this.PixelPosition - new Vector2(WIDTH_IN, -2);
                explosionSprite.Visible = true;
                explosionSprite.OrbLight.Position = CentrePos - Camera.CamPos;
            }
        }
        #endregion
    }
}
