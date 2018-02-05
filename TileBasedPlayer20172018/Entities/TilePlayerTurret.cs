using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

using Engine.Engines;
using AnimatedSprite;
using Tiling;
using CameraNS;
using Helpers;

namespace Tiler
{
    class TilePlayerTurret : RotatingSprite
    {
        float volumeVelocity = 0;
        private float turnSpeed = 0.04f;
        private const float WIDTH_IN = 11f; // Width in from the left for the sprites origin
        private float angleOfRotationPrev;
        public Projectile Bullet;
        public Vector2 CrosshairPosition;
        public Vector2 Direction;
        public Vector2 CentrePos
        {
            get
            {
                return PixelPosition + new Vector2(FrameWidth / 2, FrameHeight / 2);
            }
        }
        private Vector2 originToRotate
        {
            get
            {
                return new Vector2(((FrameWidth / 2) - WIDTH_IN), (FrameHeight / 2));
            }
        }
        private SoundEffect ExplosionSound;
        private SoundEffect ShellSound;
        private SoundEffect ShellReload;
        private SoundEffect TankTurnSound;
        private SoundEffectInstance TurnSoundInstance;
        private bool IsDead = false;

        public TilePlayerTurret(Game game, Vector2 playerPosition,
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth, 
            SoundEffect shellSound, SoundEffect shellReload, SoundEffect turnSound,
            SoundEffect explosionSound)
                : base(game, playerPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            DrawOrder = 70;
            origin = originToRotate;

            #region Turret Audio
            this.ExplosionSound = explosionSound;
            this.ShellSound = shellSound;
            this.ShellReload = shellReload;
            TankTurnSound = turnSound;
            TurnSoundInstance = TankTurnSound.CreateInstance();
            TurnSoundInstance.Volume = 0f;
            TurnSoundInstance.Pitch = -0.75f;
            TurnSoundInstance.IsLooped = true;
            TurnSoundInstance.Play();
            #endregion
        }

        public override void Update(GameTime gameTime)
        {
            if (Helper.CurrentGameStatus == GameStatus.PLAYING)
            {
                if (!IsDead)
                {
                    TilePlayer player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));

                    // Props this turret onto the underside tank body if it exists
                    if (player != null)
                    {
                        Track(player.PixelPosition + new Vector2(WIDTH_IN, 0f));
                    }

                    CrosshairPosition = ((InputEngine.MousePosition) + Camera.CamPos);

                    angleOfRotationPrev = this.angleOfRotation;

                    this.angleOfRotation = TurnToFace(this.CentrePos - new Vector2(WIDTH_IN, 0f), CrosshairPosition, this.angleOfRotation, turnSpeed);

                    Direction = new Vector2((float)Math.Cos(this.angleOfRotation), (float)Math.Sin(this.angleOfRotation));

                    Fire(gameTime);
                    PlaySounds();

                    base.Update(gameTime);
                }
                else
                {
                    ExplosionSound.Play();
                    IsDead = true;
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
            base.Draw(gameTime);
        }

        public void AddProjectile(Projectile loadedBullet)
        {
            Bullet = loadedBullet;
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
                MuzzleFlash muzzleFlash = (MuzzleFlash)Game.Services.GetService(typeof(MuzzleFlash));

                if (Mouse.GetState().LeftButton == ButtonState.Pressed
                    && Bullet.ProjectileState == Projectile.PROJECTILE_STATUS.Idle
                    && this.angleOfRotation != 0 && Math.Round(this.angleOfRotationPrev,2) == Math.Round(this.angleOfRotation,2))
                {
                    // Send this direction to the projectile
                    Bullet.GetDirection(Direction);
                    // Shoot at the specified position
                    Bullet.Shoot(CrosshairPosition - new Vector2(FrameWidth / 2, FrameHeight / 2));
                    // Draw muzzleflash
                    muzzleFlash.angleOfRotation = this.angleOfRotation;
                    muzzleFlash.PixelPosition = this.PixelPosition - new Vector2(10,0) + (this.Direction * (FrameWidth - 10));
                    muzzleFlash.Visible = true;
                    muzzleFlash.Draw(gameTime);
                    // Play sounds
                    ShellSound.Play();
                    ShellReload.Play();
                    // Shake the camera
                    thisCamera.Shake(5f, 0.5f);
                }
                else
                {
                    muzzleFlash.Visible = false;
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
    }
}
