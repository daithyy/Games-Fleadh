using System;
using System.Collections.Generic;
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
using InputManager;
using CameraNS;

namespace Tiler
{
    public class TilePlayer : RotatingSprite
    {
        #region Properties
        //List<TileRef> images = new List<TileRef>() { new TileRef(15, 2, 0) };
        //TileRef currentFrame;
        float turnSpeed = 0.025f;
        float volumeVelocity = 0;
        float pitchVelocity = -1;
        private Vector2 Velocity = new Vector2(0,0);
        public Vector2 MaxVelocity = new Vector2(2.5f, 2.5f);
        public Vector2 Acceleration = new Vector2(0.1f);
        public Vector2 Deceleration = new Vector2(0.08f);
        public Vector2 Direction;
        public Vector2 PreviousPosition;
        SoundEffect TankHumSound;
        SoundEffect TankTrackSound;
        SoundEffectInstance HumSoundInstance;
        SoundEffectInstance TrackSoundInstance;
        HealthBar healthBar;
        public Light HeadLights { get; } = new Spotlight
        {
            Scale = new Vector2(275,500),
            Radius = 1f,
            Intensity = 0.25f,
            Color = new Color(153,255,255),
            ShadowType = ShadowType.Illuminated
        };
        public Light OrbLight { get; } = new PointLight
        {
            Scale = new Vector2(500),
            Intensity = 0.5f,
            Color = Color.LightCyan,
            ShadowType = ShadowType.Solid
        };
        #endregion

        #region Constructor
        public TilePlayer(Game game, Vector2 startPosition,
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth,
            SoundEffect tankHumSound, SoundEffect tankTrackSound)
                : base(game, startPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            DrawOrder = 45;
            healthBar = new HealthBar(game, PixelPosition);
            Health = 100;
            AddHealthBar(healthBar);

            #region Tank Audio
            TankHumSound = tankHumSound;
            TankTrackSound = tankTrackSound;
            HumSoundInstance = TankHumSound.CreateInstance();
            HumSoundInstance.Volume = 0.05f;
            HumSoundInstance.Pitch = -1f;
            HumSoundInstance.IsLooped = true;
            HumSoundInstance.Play();
            TrackSoundInstance = TankTrackSound.CreateInstance();
            TrackSoundInstance.Volume = 0f;
            TrackSoundInstance.IsLooped = true;
            TrackSoundInstance.Play();
            #endregion

            PenumbraComponent penumbra = Game.Services.GetService<PenumbraComponent>();
            penumbra.Lights.Add(HeadLights);
            penumbra.Lights.Add(OrbLight);
        }
        #endregion

        #region Methods
        public override void Update(GameTime gameTime)
        {
            if (Helper.CurrentGameStatus == GameStatus.PLAYING)
            {
                PreviousPosition = PixelPosition;

                Vector2 PlayerLightPos = new Vector2(CentrePos.X, CentrePos.Y) - Camera.CamPos;
                OrbLight.Position = PlayerLightPos;

                if (Velocity != Vector2.Zero)
                    HeadLights.Enabled = true;
                else
                    HeadLights.Enabled = false;

                HeadLights.Position = PlayerLightPos;
                HeadLights.Rotation = this.angleOfRotation;

                Movement();

                Direction = new Vector2((float)Math.Cos(this.angleOfRotation), (float)Math.Sin(this.angleOfRotation));
                Velocity = Vector2.Clamp(Velocity, -MaxVelocity, MaxVelocity);
                this.PixelPosition += (Direction * Velocity);

                PlaySounds();

                base.Update(gameTime);
            }
            else
            {
                StopSounds();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            TankExplosion Explosion = (TankExplosion)Game.Services.GetService(typeof(TankExplosion));

            Explosion.Visible = false;

            if (Health <= 0)
            {
                Explosion.Visible = true;
                Explosion.PixelPosition = this.CentrePos;
            }

            base.Draw(gameTime);
        }

        public void Movement()
        {
            switch (InputEngine.UsingKeyboard)
            {
                case true:
                    #region Handle Keyboard Movement
                    if (InputEngine.IsKeyHeld(Keys.S))
                    {
                        Velocity -= Acceleration;
                    }
                    else if (Velocity.X < 0)
                    {
                        Velocity += Deceleration;
                    }
                    else if (InputEngine.IsKeyHeld(Keys.W))
                    {
                        Velocity += Acceleration;
                    }
                    else if (Velocity.X > 0)
                    {
                        Velocity -= Deceleration;
                    }

                    if (InputEngine.IsKeyHeld(Keys.A))
                    {
                        this.angleOfRotation -= turnSpeed;
                    }
                    else if (InputEngine.IsKeyHeld(Keys.D))
                    {
                        this.angleOfRotation += turnSpeed;
                    }
                    #endregion
                    break;
                case false:
                    #region Handle Controller Movement
                    Velocity += InputEngine.CurrentPadState.ThumbSticks.Left.Y * Acceleration;

                    if (Velocity.X < 0)
                    {
                        Velocity += Deceleration / 2;
                    }
                    else if (Velocity.X > 0)
                    {
                        Velocity -= Deceleration / 2;
                    }

                    this.angleOfRotation += InputEngine.CurrentPadState.ThumbSticks.Left.X * (turnSpeed * 2);
                    #endregion
                    break;
            }
        }

        public void PlaySounds()
        {
            HumSoundInstance.Play();
            TrackSoundInstance.Play();
            volumeVelocity = (Velocity.X + Velocity.Y) / (MaxVelocity.X + MaxVelocity.Y);
            pitchVelocity = ((Velocity.X + Velocity.Y) / 2) / (MaxVelocity.X + MaxVelocity.Y);
            volumeVelocity = MathHelper.Clamp(volumeVelocity, 0, 0.5f);
            pitchVelocity = MathHelper.Clamp(pitchVelocity, -1, 1);
            HumSoundInstance.Pitch = pitchVelocity;
            TrackSoundInstance.Volume = volumeVelocity;
        }

        public void StopSounds()
        {
            HumSoundInstance.Stop();
            TrackSoundInstance.Stop();
        }
        #endregion
    }
}
