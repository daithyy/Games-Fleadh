﻿using CameraNS;
using Engine.Engines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Tiler;
using Tiling;

namespace TileBasedPlayer20172018
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        int tileWidth = 64;
        int tileHeight = 64;
        List<TileRef> TileRefs = new List<TileRef>();
        List<Collider> colliders = new List<Collider>();
        string[] backTileNames = { "blueBox", "pavement", "blueSteel", "greenBox", "home" };
        public enum TileType { BLUEBOX, PAVEMENT, BLUESTEEL, GREENBOX, HOME };
        int[,] tileMap = new int[,]
        {
        {1,2,2,2,2,2,1,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {1,2,3,2,2,2,1,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {1,1,1,1,1,1,1,1,1,1,1,3,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,2,2,2},
        {2,1,1,2,2,2,0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,2,0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,2,0,0,0,0,0,2,0,0,0,0,2,0,0,0,2,0,0,0,2,2,2,2,3,2,2,2,2},
        {2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0,3,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        {2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,3,2,1,1,2,2,2,1,1,2,2,2,2,1,1,2,2,2,2,2,2,2,2,2,2,2,2,3,2,2,2,2,2,2},
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1,1,2,2,2,1,1,2,2,2,2,1,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,2,3,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,3,2,1,1,2,2,2,4,1,2,2,2,2,1,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1,1,2,2,2,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,3,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,3,2,2,2,2,2,2,2,2,3,2,2,2,2,2,2,2,2,2,2,2,3,2,2,2,2,2,3,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        };

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            //graphics.PreferredBackBufferWidth = 1280;
            //graphics.PreferredBackBufferHeight = 720;
            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.ApplyChanges();

            IsMouseVisible = true;
            IsFixedTimeStep = true;

            Window.Title = "Tile Based Tank Game - 2D Game Programming Assignment";
            Window.AllowAltF4 = false;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            new Camera(this, Vector2.Zero,
                new Vector2(tileMap.GetLength(1) * tileWidth, tileMap.GetLength(0) * tileHeight));

            new InputEngine(this);
            Services.AddService(new TilePlayer(this, new Vector2(64, 128), new List<TileRef>()
            {
                new TileRef(15, 2, 0),
                new TileRef(15, 3, 0),
                new TileRef(15, 4, 0),
                new TileRef(15, 5, 0),
                new TileRef(15, 6, 0),
                new TileRef(15, 7, 0),
                new TileRef(15, 8, 0),
            }, 64, 64, 0f));

            SetColliders(TileType.BLUESTEEL);
            SetColliders(TileType.BLUEBOX);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(spriteBatch);
            Services.AddService(Content.Load<Texture2D>(@"Tiles/tank tiles 64 x 64"));

            // Tile References to be drawn on the Map corresponding to the entries in the defined 
            // Tile Map
            // "free", "pavement", "ground", "blue", "home" 
            TileRefs.Add(new TileRef(4, 2, 0));
            TileRefs.Add(new TileRef(3, 3, 1));
            TileRefs.Add(new TileRef(6, 3, 2));
            TileRefs.Add(new TileRef(6, 2, 3));
            TileRefs.Add(new TileRef(0, 2, 4));
            // Names fo the Tiles

            new SimpleTileLayer(this, backTileNames, tileMap, TileRefs, tileWidth, tileHeight);

            // This code is used to find tiles of a specific type
            //List<Tile> tileFound = SimpleTileLayer.GetNamedTiles(backTileNames[(int)TileType.GREENBOX]);
        }

        public void SetColliders(TileType t)
        {
            for (int x = 0; x < tileMap.GetLength(1); x++)
                for (int y = 0; y < tileMap.GetLength(0); y++)
                {
                    if (tileMap[y, x] == (int)t)
                    {
                        colliders.Add(new Collider(this,
                            Content.Load<Texture2D>(@"Tiles/collider"),
                            x, y
                            ));
                    }

                }
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}
