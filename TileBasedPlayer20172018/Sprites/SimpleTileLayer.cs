using CameraNS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tiling
{
    public class SimpleTileLayer : DrawableGameComponent
    {
        string _layername;
        public int TileWidth;
        public int TileHeight;
        public string Layername
        {
            get { return _layername; }
            set { _layername = value; }
        }
        static Tile[,] _tiles;
        public static Tile[,] Tiles
        {
            get { return _tiles; }
            set { _tiles = value; }
        }
        public int MapWidth
        {
            get { return Tiles.GetLength(1); }
        }
        public int MapHeight
        {
            get { return Tiles.GetLength(0); }
        }
        List<TileRef> _tileRefs;
        private Texture2D _tileSheet;
        public List<TileRef> TileRefs
        {
            get
            {
                return _tileRefs;
            }

            set
            {
                _tileRefs = value;
            }
        }
        public SimpleTileLayer(Game game,
            string[] tileNames, int[,] tileMap,
                        List<TileRef> _tileRefs, int tileWidth, int tileHeight) : base(game)
        {
            _tileSheet = Game.Content.Load<Texture2D>(@"Tiles\tilesheet");
            DrawOrder = 0;
            game.Components.Add(this);
            int tileMapHeight = tileMap.GetLength(0); // row int[row,col]
            int tileMapWidth = tileMap.GetLength(1); // dim 0 = row, dim 1 = col
            Tiles = new Tile[tileMapHeight, tileMapWidth];
            TileRefs = _tileRefs;
            TileWidth = tileWidth;
            TileHeight = tileHeight;

            for (int x = 0; x < tileMapWidth; x++)  // look at columns in row
                for (int y = 0; y < tileMapHeight; y++) // look at rows
                {
                    Tiles[y, x] =
                        new Tile
                        {
                            X = x,
                            Y = y,
                            Id = tileMap[y, x],
                            TileName = tileNames[tileMap[y, x]],
                            TileWidth = TileWidth,
                            TileHeight = TileHeight,
                            Passable = true,
                            TileRef = TileRefs[tileMap[y, x]]
                        };
                }

        }
        public override void Draw(GameTime gameTime)
        {
            if (_tileSheet == null) return;
            SpriteBatch sp = Game.Services.GetService<SpriteBatch>();
            //Texture2D tx = Game.Services.GetService<Texture2D>();
            //SpriteFont font = Game.Services.GetService<SpriteFont>();

            // Draw the tiles
            sp.Begin(SpriteSortMode.Immediate,
                        BlendState.AlphaBlend, null, null, null, null,
                            Camera.CurrentCameraTranslation);
            // Draw the Tiles
            foreach (Tile t in Tiles)
            {
                Vector2 position = new Vector2(t.X * t.TileWidth,
                                                    t.Y * t.TileHeight);
                sp.Draw(_tileSheet,
                    new Rectangle(position.ToPoint(), new Point(t.TileWidth, t.TileHeight)),
                    new Rectangle(t.TileRef._sheetPosX * t.TileWidth, t.TileRef._sheetPosY * t.TileHeight,
                                        t.TileWidth, t.TileHeight)
                    , Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            }
            sp.End();
            base.Draw(gameTime);
        }
        public static List<Tile> GetNamedTiles(string name)
        {
            List<Tile> foundTiles = new List<Tile>();

            int tileMapHeight = Tiles.GetLength(0);
            int tileMapWidth = Tiles.GetLength(1);

            for (int x = 0; x < tileMapWidth; x++) // Look at columns in row
            {
                for (int y = 0; y < tileMapHeight; y++) // Look at rows
                {
                    if (Tiles[y,x].TileName == name)
                    {
                        foundTiles.Add(Tiles[y,x]);
                    }
                }
            }
            return foundTiles;
        }
    }
}
