using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using InputManager;
using Tiling;
using AnimatedSprite;
using CameraNS;

namespace Tiler
{
    public class Crosshair : RotatingSprite
    {
        public static Vector2 Position;
        private Vector2 TankPosition;
        private const float AIM_RADIUS = 200;

        public Crosshair(Game game, Vector2 userPosition,
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth)
                : base(game, userPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            DrawOrder = 100;
        }

        private void MoveWithRadius()
        {
            TilePlayerTurret playerTurret = (TilePlayerTurret)Game.Services.GetService(typeof(TilePlayerTurret));
            Position = playerTurret.PixelPosition + (playerTurret.Direction * new Vector2(AIM_RADIUS));
        }

        private void MoveWithMouse()
        {
            Vector2 MousePos = InputEngine.MousePosition - new Vector2(FrameWidth / 2, FrameHeight / 2);
            Vector2 TruePosition = (MousePos + Camera.CamPos);

            Vector2 TransformPosition = ContainTurretAngle(TruePosition);
            TransformPosition += TankPosition;
            Position = TransformPosition;
        }

        private Vector2 ContainTurretAngle(Vector2 CrosshairPosition)
        {
            TilePlayerTurret playerTurret = (TilePlayerTurret)Game.Services.GetService(typeof(TilePlayerTurret));
            TilePlayer player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));

            TankPosition = player.PixelPosition;

            return ((playerTurret.Direction * Vector2.Distance(TankPosition, CrosshairPosition)));
        }

        public override void Update(GameTime gameTime)
        {
            if (InputEngine.UsingKeyboard)
            {
                MoveWithMouse();
            }
            else
            {
                MoveWithRadius();
            }

            PixelPosition = Position;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            //if (InputEngine.UsingKeyboard)
                base.Draw(gameTime);
        }
    }
}
