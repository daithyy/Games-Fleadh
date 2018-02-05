using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Helpers;

namespace Tiler
{
    class TileTrigger : Collider
    {
        string Name;
        public TileTrigger(string name, Game game, Texture2D tx, int tlx, int tly) : base(game, tx, tlx, tly)
        {
            Name = name;
        }
        public override void Update(GameTime gameTime)
        {
            TilePlayer p = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));

            if (p == null) return;
            else
            {
                switch (Name.ToUpper())
                {
                    case "EXIT":
                        if (p.BoundingRectangle.Intersects(CollisionField) && SentryTurret.Count <= 0)
                        {
                            TileBasedPlayer20172018.Game1.MainScreen.CurrentGameCondition = GameCondition.WIN;
                        }
                        break;
                    default:
                        break;
                }
            }

            base.Update(gameTime);
        }
    }
}
