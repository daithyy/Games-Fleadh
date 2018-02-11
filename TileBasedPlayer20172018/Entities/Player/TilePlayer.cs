using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Penumbra;

using Tiling;
using Helpers;
using InputManager;

namespace Tiler
{
    public class TilePlayer : Tank
    {
        #region Properties
        public Light HeadLights { get; } = new Spotlight
        {
            Scale = new Vector2(275,500),
            Radius = 1f,
            Intensity = 0.25f,
            Color = new Color(153,255,255),
            ShadowType = ShadowType.Illuminated
        };
        public HealthBar healthBar;
        private const int MAX_HEALTH = 100;
        private const float FADE_AMOUNT = 0.075f;
        #endregion

        #region Constructor
        public TilePlayer(Game game, Vector2 startPosition,
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth,
            SoundEffect tankHumSound, SoundEffect tankTrackSound)
                : base(game, startPosition, sheetRefs, frameWidth, frameHeight, layerDepth,
                      tankHumSound, tankTrackSound)
        {
            Health = MAX_HEALTH;
            DrawOrder = 45;

            OrbLight.Color = Color.LightCyan;

            healthBar = new HealthBar(game, PixelPosition);
            AddHealthBar(healthBar);

            PenumbraComponent penumbra = Game.Services.GetService<PenumbraComponent>();
            penumbra.Lights.Add(HeadLights);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Fade in the health bar when health is lower than max health.
        /// Fade out when health is at max.
        /// </summary>
        private void ToggleHealthBar()
        {
            if (Health < MAX_HEALTH)
            {
                healthBar.Alpha += FADE_AMOUNT;
            }
            else if (Health > 0 && healthBar.Alpha > 0)
            {
                healthBar.Alpha -= FADE_AMOUNT;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Velocity != Vector2.Zero)
                HeadLights.Enabled = true;
            else
                HeadLights.Enabled = false;

            HeadLights.Position = TankLightPos;
            HeadLights.Rotation = this.angleOfRotation;

            Movement();

            PlaySounds();

            ToggleHealthBar();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
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
        #endregion
    }
}
