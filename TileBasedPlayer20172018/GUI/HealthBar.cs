using Microsoft.Xna.Framework;
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
        #region Properties
        public string Name;
        public int health;
        public int Factor = 2;
        public Texture2D txHealthBar; // hold the texture
        public Vector2 position; // Position on the screen
        public Color HealthyColor = new Color(243, 208, 168);
        public Color WarningColor = new Color(255, 142, 86);
        public Color CriticalColor = new Color(255, 86, 86);
        public static int WarningLevel = 60;
        public static int CriticalLevel = 30;
        public Rectangle HealthRect
        {
            get
            {
                return new Rectangle((int)position.X,
                                (int)position.Y, (health / Factor), 5);
            }
        }
        public float Alpha = 0f;
        #endregion

        #region Constructor
        public HealthBar(Game game, Vector2 pos) : base(game)
        {
            DrawOrder = 900;
            txHealthBar = new Texture2D(game.GraphicsDevice, 1, 1);
            txHealthBar.SetData(new[] { Color.White });
            position = pos;
        }
        #endregion

        #region Methods
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();

            spriteBatch.Begin(SpriteSortMode.Immediate,
                    BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera.CurrentCameraTranslation);
            if (health > WarningLevel)
                spriteBatch.Draw(txHealthBar, HealthRect, HealthyColor * Alpha);
            else if (health > CriticalLevel && health <= WarningLevel)
                spriteBatch.Draw(txHealthBar, HealthRect, WarningColor * Alpha);
            else if (health > 0 && health <= CriticalLevel)
                spriteBatch.Draw(txHealthBar, HealthRect, CriticalColor * Alpha);
            spriteBatch.End();
        }
        #endregion
    }
}
