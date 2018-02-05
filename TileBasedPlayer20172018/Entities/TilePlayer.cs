using AnimatedSprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tiling;
using Helpers;

namespace Tiler
{
    public class TilePlayer : RotatingSprite
    {
        //List<TileRef> images = new List<TileRef>() { new TileRef(15, 2, 0) };
        //TileRef currentFrame;

        float turnSpeed = 0.025f;
        float volumeVelocity = 0;
        float pitchVelocity = -1;
        Vector2 Velocity = new Vector2(0,0);
        Vector2 MaxVelocity = new Vector2(2.5f, 2.5f);
        Vector2 Acceleration = new Vector2(0.1f);
        public Vector2 Deceleration = new Vector2(0.08f);
        public Vector2 Direction;
        public Vector2 PreviousPosition;
        public Vector2 CentrePos
        {
            get
            {
                return PixelPosition + new Vector2(FrameWidth / 2, FrameHeight / 2);
            }
        }
        SoundEffect TankHumSound;
        SoundEffect TankTrackSound;
        SoundEffectInstance HumSoundInstance;
        SoundEffectInstance TrackSoundInstance;
        HealthBar healthBar;

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
        }

        public void Collision(Collider c)
        {
            if (BoundingRectangle.Intersects(c.CollisionField))
                PixelPosition = PreviousPosition;
        }

        public override void Update(GameTime gameTime)
        {
            if (Helper.CurrentGameStatus == GameStatus.PLAYING)
            {
                PreviousPosition = PixelPosition;

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
                Explosion.Draw(gameTime);
            }

            base.Draw(gameTime);
        }

        public void Movement()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                Velocity -= Acceleration;
            }
            else if (Velocity.X < 0)
            {
                Velocity += Deceleration;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                Velocity += Acceleration;
            }
            else if (Velocity.X > 0)
            {
                Velocity -= Deceleration;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                this.angleOfRotation -= turnSpeed;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                this.angleOfRotation += turnSpeed;
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
    }
}
