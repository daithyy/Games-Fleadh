using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using AnimatedSprite;

namespace Tiling
{
    class Explosion : AnimateSheetSprite
    {
        public enum Effect { Idle, Exploding };
        public Effect State;
        public int FrameCount { get; set; }
        public Explosion(Game game, Vector2 userPosition, List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth)
            : base(game, userPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            DrawOrder = 100;
            Visible = false;
            FrameCount = sheetRefs.Count;
            State = Effect.Idle;
        }
    }
}
