using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Penumbra;

using AnimatedSprite;
using Tiling;
using CameraNS;

namespace Tiler
{
    public class Tank : RotatingSprite
    {
        #region Properties
        public float turnSpeed = 0.025f;
        float volumeVelocity = 0;
        float pitchVelocity = -1;
        public Vector2 Velocity = new Vector2(0, 0);
        public Vector2 MaxVelocity = new Vector2(2.5f, 2.5f);
        public Vector2 Acceleration = new Vector2(0.1f);
        public Vector2 Deceleration = new Vector2(0.08f);
        public Vector2 Direction;
        public Vector2 PreviousPosition;
        public Vector2 TankLightPos;
        SoundEffect TankHumSound;
        SoundEffect TankTrackSound;
        SoundEffectInstance HumSoundInstance;
        SoundEffectInstance TrackSoundInstance;

        public Light OrbLight { get; } = new PointLight
        {
            Scale = new Vector2(500),
            Intensity = 0.5f,
            Color = Color.White,
            ShadowType = ShadowType.Solid
        };
        #endregion

        #region Constructor
        public Tank(Game game, Vector2 userPosition, 
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth,
            SoundEffect tankHumSound, SoundEffect tankTrackSound) 
            : base(game, userPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            DrawOrder = 30;

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

            //AddHealthBar(healthBar);

            PenumbraComponent penumbra = Game.Services.GetService<PenumbraComponent>();
            penumbra.Lights.Add(OrbLight);
        }
        #endregion

        #region Methods
        public override void Update(GameTime gameTime)
        {
            PreviousPosition = PixelPosition;

            TankLightPos = new Vector2(CentrePos.X, CentrePos.Y) - Camera.CamPos;
            OrbLight.Position = TankLightPos;

            Direction = new Vector2((float)Math.Cos(this.angleOfRotation), (float)Math.Sin(this.angleOfRotation));
            Velocity = Vector2.Clamp(Velocity, -MaxVelocity, MaxVelocity);
            this.PixelPosition += (Direction * Velocity);

            base.Update(gameTime);
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

