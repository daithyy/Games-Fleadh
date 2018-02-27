using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Penumbra;
using AnimatedSprite;
using CameraNS;

namespace Tiling
{
    class MuzzleFlash : RotatingSprite
    {
        public Light OrbLight { get; } = new PointLight
        {
            Scale = new Vector2(500),
            Intensity = 0.4f,
            Color = Color.Gold,
            ShadowType = ShadowType.Illuminated
        };

        public MuzzleFlash(Game game, Vector2 userPosition, List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth)
            : base(game, userPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            DrawOrder = 100;
            Visible = false;
            Scale = 2;

            PenumbraComponent penumbra = Game.Services.GetService<PenumbraComponent>();
            penumbra.Lights.Add(OrbLight);
            OrbLight.CastsShadows = false;
            OrbLight.Enabled = false;
        }

        public override void Update(GameTime gametime)
        {
            OrbLight.Position = PixelPosition - Camera.CamPos;

            base.Update(gametime);
        }
    }
}
