﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AnimatedSprite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tiling;
using CameraNS;

namespace Helpers
{
    public class HealthBar : DrawableGameComponent
    {
        public int health;
        private Texture2D txHealthBar; // hold the texture
        public Vector2 position; // Position on the screen
        public Rectangle HealthRect
        {
            get
            {
                return new Rectangle((int)position.X,
                                (int)position.Y, (health / 2), 5);
            }
        }

        public HealthBar(Game game, Vector2 pos) : base(game)
        {
            DrawOrder = 900;
            txHealthBar = new Texture2D(game.GraphicsDevice, 1, 1);
            txHealthBar.SetData(new[] { Color.White });
            position = pos;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();

            spriteBatch.Begin(SpriteSortMode.Immediate,
                    BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera.CurrentCameraTranslation);
            if (health > 60)
                spriteBatch.Draw(txHealthBar, HealthRect, Color.LimeGreen);
            else if (health > 30 && health <= 60)
                spriteBatch.Draw(txHealthBar, HealthRect, Color.Orange);
            else if (health > 0 && health <= 30)
                spriteBatch.Draw(txHealthBar, HealthRect, Color.Red);
            spriteBatch.End();
        }
    }
}