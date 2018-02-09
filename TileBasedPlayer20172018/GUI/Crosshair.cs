using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using InputEngine;
using Tiling;
using AnimatedSprite;
using CameraNS;

namespace Tiler
{
    public class Crosshair : RotatingSprite
    {
        public static Vector2 Position;
        private Vector2 TankPosition;

        public Crosshair(Game game, Vector2 userPosition,
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth)
                : base(game, userPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            DrawOrder = 100;
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
            Vector2 MousePos = InputManager.MousePosition - new Vector2(FrameWidth / 2, FrameHeight / 2);
            Position = (MousePos + Camera.CamPos);

            Vector2 TransformPosition = ContainTurretAngle(Position);
            TransformPosition += TankPosition;

            PixelPosition = TransformPosition;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
