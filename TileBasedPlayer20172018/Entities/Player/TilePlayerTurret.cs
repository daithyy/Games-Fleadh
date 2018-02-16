﻿using System;
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
        private const float WIDTH_IN = 11f; // Width in from the left for the sprites origin
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
        private SoundEffect TankTurnSound;
        private SoundEffectInstance TurnSoundInstance;
        public bool IsDead = false;
        private float explosionTimer = 0f;
        #endregion

        #region Constructor
        public TilePlayerTurret(Game game, Vector2 playerPosition,
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth, 
            SoundEffect shellSound, SoundEffect shellReload, SoundEffect turnSound,
            SoundEffect explosionSound)
                : base(game, playerPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            DrawOrder = 70;
            origin = rotateOrigin;

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
                MuzzleFlash muzzleFlash = (MuzzleFlash)Game.Services.GetService(typeof(MuzzleFlash));

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
                TankExplosion Explosion = (TankExplosion)Game.Services.GetService(typeof(TankExplosion));

                float maxExplosionTime = 0.75f;
                if (explosionTimer < maxExplosionTime)
                {
                    explosionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    Explosion.PixelPosition = this.PixelPosition - new Vector2(10, -2);
                    Explosion.Visible = true;
                    Vector2 LightPosition = new Vector2(CentrePos.X, CentrePos.Y) - Camera.CamPos;
                    Explosion.OrbLight.Position = LightPosition;
                    if (explosionTimer < maxExplosionTime - maxExplosionTime + 0.3f)
                        Explosion.OrbLight.Intensity = 0.25f;
                }
                else
                    Explosion.Visible = false;
            }
        }
        #endregion
    }
}
