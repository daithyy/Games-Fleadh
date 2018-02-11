using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Penumbra;

namespace Tiling
{
    class TankExplosion : Explosion
    {
        public Light OrbLight { get; } = new PointLight
        {
            Scale = new Vector2(500),
            Intensity = 0.25f,
            Color = Color.Gold,
            ShadowType = ShadowType.Illuminated
        };

        public TankExplosion(Game game, Vector2 userPosition, List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth) 
            : base(game, userPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            DrawOrder = 101;

            PenumbraComponent penumbra = Game.Services.GetService<PenumbraComponent>();
            penumbra.Lights.Add(OrbLight);
        }

        public override void Update(GameTime gameTime)
        {
            if (OrbLight.Intensity > 0)
                OrbLight.Intensity -= 0.01f;

            base.Update(gameTime);
        }
    }
}
