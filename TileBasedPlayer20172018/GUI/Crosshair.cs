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
using Helpers;

namespace Tiler
{
    public class Crosshair : RotatingSprite
    {
        public static Vector2 Position;
        private Vector2 TankPosition;
        private const float AIM_RADIUS = 200;
        public bool Disabled;

        public Crosshair(Game game, Vector2 userPosition,
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth)
                : base(game, userPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            DrawOrder = 100;
        }

        private void MoveWithRadius(TilePlayerTurret playerTurret)
        {
            Position = playerTurret.PixelPosition + (playerTurret.Direction * new Vector2(AIM_RADIUS));
            PixelPosition = Position;
        }

        private void MoveWithMouse(TilePlayer player, TilePlayerTurret playerTurret)
        {
            Vector2 MousePos = InputEngine.MousePosition - new Vector2(FrameWidth / 2, FrameHeight / 2);
            Position = (MousePos + Camera.CamPos);
            Vector2 TransformPosition = ContainTurretAngle(player, playerTurret, Position);

            TransformPosition += TankPosition;
            PixelPosition = TransformPosition;
        }

        private Vector2 ContainTurretAngle(TilePlayer player, TilePlayerTurret playerTurret, Vector2 CrosshairPosition)
        {
            TankPosition = player.PixelPosition;

            return ((playerTurret.Direction * Vector2.Distance(TankPosition, CrosshairPosition)));
        }

        public override void Update(GameTime gameTime)
        {
            if (!Disabled && Helper.CurrentGameStatus == GameStatus.PLAYING)
            {
                TilePlayer player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));
                TilePlayerTurret playerTurret = (TilePlayerTurret)Game.Services.GetService(typeof(TilePlayerTurret));

                if (InputEngine.UsingKeyboard)
                {
                    MoveWithMouse(player, playerTurret);
                }
                else
                {
                    MoveWithRadius(playerTurret);
                }

                Disabled = playerTurret.IsDead;

                base.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            //if (InputEngine.UsingKeyboard)
            if (!Disabled)
                base.Draw(gameTime);
        }
    }
}
