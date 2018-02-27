using System;
using System.Collections.Generic;
using System.Windows;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Penumbra;

using Tiler;
using Tiling;
using CameraNS;
using InputManager;
using Screens;
using Helpers;
using AnimatedSprite;
using PowerUps;
using ContentLoader;

namespace TileBasedPlayer20172018
{
    public class GameRoot : Game
    {
        public readonly GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        PenumbraComponent penumbra;

        private int _width = 1280;
        private int _height = 768;

        private Color BackgroundColor = new Color(185, 132, 62);
        private const float WORLD_BRIGHTNESS = 1.0f;

        private Camera CurrentCamera;
        public static SplashScreen MainScreen;

        private List<TileRef> TileRefs = new List<TileRef>();
        private List<Collider> Colliders = new List<Collider>();
        private List<Sentry> Sentries = new List<Sentry>();
        private List<SentryTurret> SentryTurrets = new List<SentryTurret>();

        #region Tile Settings
        private string[] backTileNames = {
            "dirt", "ground", "metal",
            "ground2", "ground3", "ground4",
            "ground5", "ground7", "metal2",
            "metal3", "metal4", "dirt2" };
        public enum TileType {
            DIRT, GROUND, METAL,
            GROUND2, GROUND3, GROUND4, 
            GROUND5, GROUND7, METAL2,
            METAL3, METAL4, DIRT2 };
        
        private int tileWidth = 64;
        private int tileHeight = 64;
        #endregion

        #region Tile Map
        int[,] tileMap = new int[,]
        {
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
        {0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,0,0},
        {0,1,1,1,1,1,5,1,1,1,1,5,5,1,1,1,5,1,0,0,1,1,5,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,0,0},
        {0,1,1,5,1,1,6,1,1,1,6,6,1,1,1,1,1,1,0,0,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,0,0},
        {0,7,6,6,1,1,0,0,0,0,0,7,1,1,1,5,1,1,1,1,1,1,5,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,7,1,1,1,1,1,5,1,1,1,1,1,5,1,1,1,1,1,1,1,5,1,1,1,1,0,0,0,0,0,0,0,0,3,3,3,3,3,3,3,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,7,1,1,1,1,1,2,2,2,2,2,1,1,1,1,1,5,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,3,3,3,0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,7,1,1,2,2,2,2,2,2,2,2,2,2,2,2,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,3,3,3,0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,2,8,9,9,9,9,9,9,9,9,2,2,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,3,3,3,0,0,0,0,0,0,0,0,0,0},
        {0,0,1,1,1,1,1,1,1,5,1,1,1,1,1,1,2,8,9,9,9,9,9,9,9,9,2,2,1,1,1,1,2,2,2,2,1,1,1,1,1,1,1,1,1,1,4,4,4,4,4,1,1,1,1,1,1,1,0,0},
        {0,0,1,1,5,1,1,1,1,1,1,1,5,1,1,1,2,8,9,9,9,9,9,9,9,9,2,2,1,1,1,2,2,9,9,2,2,1,1,1,1,5,1,1,1,1,4,4,4,4,4,1,1,5,1,1,1,1,0,0},
        {0,0,1,1,1,1,1,1,5,1,1,1,1,1,1,1,2,8,9,9,9,9,9,9,9,9,2,1,1,1,1,2,8,9,9,10,2,1,1,5,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0},
        {0,0,1,1,5,1,1,1,1,5,1,1,1,1,1,1,2,8,9,9,9,9,9,9,9,9,2,1,1,1,1,2,8,9,9,10,2,1,1,1,1,1,1,1,1,5,1,1,1,1,1,1,2,2,2,1,1,1,0,0},
        {0,0,7,6,6,1,1,6,1,6,1,1,1,1,1,5,2,8,9,9,9,9,9,9,9,9,2,1,1,5,1,2,8,9,9,10,2,1,1,1,1,5,1,1,1,1,1,1,1,1,5,1,2,2,2,1,1,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,5,2,8,9,9,9,9,9,9,9,10,2,1,1,1,1,2,8,9,9,10,2,1,1,1,1,1,1,2,2,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,2,2,9,9,9,9,9,9,9,10,2,1,1,1,1,2,2,9,9,2,2,1,1,5,1,1,2,2,2,2,1,1,5,1,1,1,1,1,1,1,1,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,2,2,9,9,9,9,9,9,9,2,2,1,1,1,1,2,2,2,2,1,1,1,1,1,2,2,2,2,2,2,1,1,1,1,6,6,1,1,6,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,2,2,9,9,9,9,9,9,9,2,2,5,1,1,5,1,1,1,1,1,1,1,2,2,2,2,2,2,2,1,1,1,1,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,1,1,1,1,1,1,0,0,0,0,1,1,1,1,1,2,2,2,2,2,2,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,1,1,1,0,0,0,0,0,0,0,0,0,0},
        {0,0,1,1,1,1,1,5,1,1,1,0,0,1,1,5,1,1,1,1,1,1,1,5,1,1,1,1,1,1,1,1,5,1,1,1,1,2,2,2,2,2,2,2,2,2,2,1,1,1,0,0,0,0,0,0,0,0,0,0},
        {0,0,1,1,1,1,1,1,1,1,1,1,1,5,1,5,1,5,1,1,1,1,1,1,5,1,1,5,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,2,1,1,1,0,0,0,0,0,0,0,0,0,0},
        {0,0,5,1,1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,5,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,1,1,1,1,0,0,0,0,0,0,0,0,0,0},
        {0,0,1,1,0,0,0,0,1,6,1,6,1,1,5,1,1,1,1,2,2,2,2,2,2,2,2,2,1,1,1,1,1,1,5,1,1,1,2,2,2,2,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0},
        {0,0,1,1,0,0,0,0,0,0,0,0,0,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,5,0,0,0,0,0},
        {0,0,1,1,0,0,0,0,0,0,0,0,0,0,5,1,1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1,5,1,1,5,1,1,1,1,1,1,1,1,1,1,1,5,1,1,1,1,1,1,1,0,0,0,0,0},
        {0,0,1,1,1,0,0,0,0,0,0,0,0,0,1,1,1,2,2,2,2,2,2,2,2,2,2,2,2,2,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,1,1,1,1,5,1,1,1,1,0,0,0,0},
        {0,0,1,1,1,1,1,1,0,0,0,0,0,1,1,1,5,2,2,2,1,1,1,1,1,1,1,1,1,1,5,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,11},
        {0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,6,1,1,1,1,2,2,2,1,1,1,1,1,1,1,5,1,1,1,1,11},
        {0,0,1,6,1,1,1,1,1,6,1,1,1,1,6,1,1,6,1,1,6,1,1,1,1,1,6,1,1,6,1,1,1,1,1,1,1,1,0,0,0,0,0,2,2,2,2,1,1,1,1,1,1,6,6,1,1,1,1,11},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
        };
        #endregion

        public GameRoot()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = _width;
            graphics.PreferredBackBufferHeight = _height;
            //graphics.PreferMultiSampling = false;
            graphics.SynchronizeWithVerticalRetrace = true;
            //graphics.IsFullScreen = true;
            graphics.ApplyChanges();

            IsMouseVisible = false;
            IsFixedTimeStep = true;

            Window.Title = "Steel Wrath";
            Window.AllowAltF4 = false;

            Content.RootDirectory = "Content";

            penumbra = new PenumbraComponent(this)
            {
                //AmbientColor = new Color(new Vector3(WORLD_BRIGHTNESS)) // NO HUE
                //AmbientColor = new Color(22, 30, 38) // LIGHT GRAY BLUE
                //AmbientColor = new Color(15, 27, 38) // DARK GRAY BLUE
                AmbientColor = new Color(19, 19, 38) * WORLD_BRIGHTNESS // DARK PURPLE BLUE
                //AmbientColor = new Color(26, 15, 38) // PURPLE
            };
            Components.Add(penumbra);
            Services.AddService(penumbra);
        }

        protected override void Initialize()
        {
            new InputEngine(this);

            // Add Camera
            CurrentCamera = new Camera(this, Vector2.Zero,
                new Vector2((tileMap.GetLength(1) * tileWidth),
                            (tileMap.GetLength(0) * tileHeight)));
            Services.AddService(CurrentCamera);

            #region Create Player Tank
            TilePlayer tankPlayer = new TilePlayer(this, new Vector2(96, 192), new List<TileRef>()
            {
                new TileRef(10, 0, 0),
            }, 64, 64, 0f,
            Content.Load<SoundEffect>("audio/PlayerTankHum"),
            Content.Load<SoundEffect>("audio/PlayerTankTracks"),
            Content.Load<SoundEffect>("audio/PlayerWarning"),
            Content.Load<SoundEffect>("audio/Heartbeat"));

            TilePlayerTurret tankPlayerTurret = new TilePlayerTurret(this, tankPlayer.PixelPosition, new List<TileRef>()
            {
                new TileRef(10, 1, 0),
            }, 64, 64, 0f,
            Content.Load<SoundEffect>("audio/PlayerTankShoot"),
            Content.Load<SoundEffect>("audio/PlayerTankReload2"),
            Content.Load<SoundEffect>("audio/PlayerTankReload"),
            Content.Load<SoundEffect>("audio/PlayerTurretTurn"),
            Content.Load<SoundEffect>("audio/TankExplosion"));

            // Add Tank Projectile
            const int PLAYER_BULLET_SPD = 8;

            Projectile bullet = new Projectile(this, "PLAYER", tankPlayerTurret.CentrePos, new List<TileRef>()
            {
                new TileRef(10, 2, 0),
            }, 64, 64, 0f, tankPlayerTurret.Direction, PLAYER_BULLET_SPD,
            Content.Load<SoundEffect>("audio/TankShoot"),
            Content.Load<SoundEffect>("audio/TankArmorPierce"));
            tankPlayerTurret.AddProjectile(bullet);

            Ricochet ricochetRound = new Ricochet(this, "PLAYER", tankPlayerTurret.CentrePos, new List<TileRef>()
            {
                new TileRef(11, 2, 0),
            }, 64, 64, 0f, tankPlayerTurret.Direction, PLAYER_BULLET_SPD,
            Content.Load<SoundEffect>("audio/TankShoot"),
            Content.Load<SoundEffect>("audio/TankArmorPierce"),
            Content.Load<SoundEffect>("audio/PlayerTankShootAlt"));

            Services.AddService(ricochetRound);
            Services.AddService(bullet);
            Services.AddService(tankPlayer);
            Services.AddService(tankPlayerTurret);
            #endregion

            // Tank Rotations
            const float ANGLE_DOWN = 1.574f;
            const float ANGLE_RIGHT = 3.15f;
            const float ANGLE_DIAG_LEFT = 2.4f; // Minus for top left
            const float ANGLE_DIAG_RIGHT = 0.75f; // Minus for top right

            #region Create Sentry Tanks
            List<Vector2> SentryPositions = new List<Vector2>
            {
                new Vector2(958, 197),
                new Vector2(200, 768),
                new Vector2(350, 1287),
                new Vector2(2304, 1820),
                new Vector2(3590, 1794),
                new Vector2(3555, 866),
                new Vector2(2580, 180),
                new Vector2(3562, 184),
                new Vector2(2129, 474),
                new Vector2(1374, 251),
            };

            Shuffle(SentryPositions); // Randomize

            Sentry enemyOne = new Sentry(this, SentryPositions[0], new List<TileRef>()
            {
                new TileRef(10, 4, 0),
            }, 64, 64, 0f, "Enemy Tank 1",
            Content.Load<SoundEffect>("audio/SentryTankHum"),
            Content.Load<SoundEffect>("audio/SentryTankTracks"),
            -ANGLE_RIGHT);

            Sentry enemyTwo = new Sentry(this, SentryPositions[1], new List<TileRef>()
            {
                new TileRef(10, 4, 0),
            }, 64, 64, 0f, "Enemy Tank 2",
            Content.Load<SoundEffect>("audio/SentryTankHum"),
            Content.Load<SoundEffect>("audio/SentryTankTracks"),
            0);

            Sentry enemyThree = new Sentry(this, SentryPositions[2], new List<TileRef>()
            {
                new TileRef(10, 4, 0),
            }, 64, 64, 0f, "Enemy Tank 3",
            Content.Load<SoundEffect>("audio/SentryTankHum"),
            Content.Load<SoundEffect>("audio/SentryTankTracks"),
            ANGLE_DIAG_RIGHT);

            Sentry enemyFour = new Sentry(this, SentryPositions[3], new List<TileRef>()
            {
                new TileRef(10, 4, 0),
            }, 64, 64, 0f, "Enemy Tank 4",
            Content.Load<SoundEffect>("audio/SentryTankHum"),
            Content.Load<SoundEffect>("audio/SentryTankTracks"),
            -ANGLE_RIGHT);

            Sentry enemyFive = new Sentry(this, SentryPositions[4], new List<TileRef>()
            {
                new TileRef(10, 4, 0),
            }, 64, 64, 0f, "Enemy Tank 5",
            Content.Load<SoundEffect>("audio/SentryTankHum"),
            Content.Load<SoundEffect>("audio/SentryTankTracks"),
            -ANGLE_RIGHT);

            Sentry enemySix = new Sentry(this, SentryPositions[5], new List<TileRef>()
            {
                new TileRef(10, 4, 0),
            }, 64, 64, 0f, "Enemy Tank 6",
            Content.Load<SoundEffect>("audio/SentryTankHum"),
            Content.Load<SoundEffect>("audio/SentryTankTracks"),
            ANGLE_DOWN);

            Sentry enemySeven = new Sentry(this, SentryPositions[6], new List<TileRef>()
            {
                new TileRef(10, 4, 0),
            }, 64, 64, 0f, "Enemy Tank 7",
            Content.Load<SoundEffect>("audio/SentryTankHum"),
            Content.Load<SoundEffect>("audio/SentryTankTracks"),
            0);

            LightSentry enemyEight = new LightSentry(this, SentryPositions[7], new List<TileRef>()
            {
                new TileRef(10, 6, 0),
            }, 64, 64, 0f, "Enemy Tank 8",
            Content.Load<SoundEffect>("audio/SentryTankHum"),
            Content.Load<SoundEffect>("audio/SentryTankTracks"),
            -ANGLE_RIGHT);

            LightSentry enemyNine = new LightSentry(this, SentryPositions[8], new List<TileRef>()
            {
                new TileRef(10, 6, 0),
            }, 64, 64, 0f, "Enemy Tank 9",
            Content.Load<SoundEffect>("audio/SentryTankHum"),
            Content.Load<SoundEffect>("audio/SentryTankTracks"),
            -ANGLE_RIGHT);

            HeavySentry enemyTen = new HeavySentry(this, SentryPositions[9], new List<TileRef>()
            {
                new TileRef(11, 4, 0),
            }, 64, 64, 0f, "Enemy Tank 10",
            Content.Load<SoundEffect>("audio/SentryTankHum"),
            Content.Load<SoundEffect>("audio/SentryTankTracks"),
            ANGLE_DOWN);

            //Sentry enemyEleven = new Sentry(this, new Vector2(2316, 1345), new List<TileRef>()
            //{
            //    new TileRef(10, 4, 0),
            //}, 64, 64, 0f, "Enemy Tank 11", -ANGLE_HORIZONTAL);

            //Sentry enemyTwelve = new Sentry(this, new Vector2(54, 1916), new List<TileRef>()
            //{
            //    new TileRef(10, 4, 0),
            //}, 64, 64, 0f, "Enemy Tank 12", -ANGLE_DIAG_RIGHT);

            //Sentry enemyThirteen = new Sentry(this, new Vector2(175, 1188), new List<TileRef>()
            //{
            //    new TileRef(10, 4, 0),
            //}, 64, 64, 0f, "Enemy Tank 13", ANGLE_DIAG_RIGHT);

            //Sentry enemyFourteen = new Sentry(this, new Vector2(45, 766), new List<TileRef>()
            //{
            //    new TileRef(10, 4, 0),
            //}, 64, 64, 0f, "Enemy Tank 14", 0f);

            #endregion

            #region Add Sentries to List
            Sentries.Add(enemyOne);
            Sentries.Add(enemyTwo);
            Sentries.Add(enemyThree);
            Sentries.Add(enemyFour);
            Sentries.Add(enemyFive);
            Sentries.Add(enemySix);
            Sentries.Add(enemySeven);
            Sentries.Add(enemyEight);
            Sentries.Add(enemyNine);
            Sentries.Add(enemyTen);
            //Sentries.Add(enemyEleven);
            //Sentries.Add(enemyTwelve);
            //Sentries.Add(enemyThirteen);
            //Sentries.Add(enemyFourteen);

            Services.AddService(Sentries);
            #endregion

            #region Create Sentry Tank Turrets

            SentryTurret enemyTurretOne = new SentryTurret(this, enemyOne.PixelPosition, new List<TileRef>()
            {
                new TileRef(10, 5, 0),
            }, 64, 64, 0f, "Enemy Tank 1", -ANGLE_RIGHT, Content.Load<SoundEffect>("audio/SentryTurretTurn"),
            Content.Load<SoundEffect>("audio/TankExplosion"));

            SentryTurret enemyTurretTwo = new SentryTurret(this, enemyTwo.PixelPosition, new List<TileRef>()
            {
                new TileRef(10, 5, 0),
            }, 64, 64, 0f, "Enemy Tank 2", ANGLE_RIGHT, Content.Load<SoundEffect>("audio/SentryTurretTurn"),
            Content.Load<SoundEffect>("audio/TankExplosion"));

            SentryTurret enemyTurretThree = new SentryTurret(this, enemyThree.PixelPosition, new List<TileRef>()
            {
                new TileRef(10, 5, 0),
            }, 64, 64, 0f, "Enemy Tank 3", -ANGLE_DIAG_RIGHT, Content.Load<SoundEffect>("audio/SentryTurretTurn"),
            Content.Load<SoundEffect>("audio/TankExplosion"));

            SentryTurret enemyTurretFour = new SentryTurret(this, enemyFour.PixelPosition, new List<TileRef>()
            {
                new TileRef(10, 5, 0),
            }, 64, 64, 0f, "Enemy Tank 4", -ANGLE_RIGHT, Content.Load<SoundEffect>("audio/SentryTurretTurn"),
            Content.Load<SoundEffect>("audio/TankExplosion"));

            SentryTurret enemyTurretFive = new SentryTurret(this, enemyFive.PixelPosition, new List<TileRef>()
            {
                new TileRef(10, 5, 0),
            }, 64, 64, 0f, "Enemy Tank 5", -ANGLE_RIGHT, Content.Load<SoundEffect>("audio/SentryTurretTurn"),
            Content.Load<SoundEffect>("audio/TankExplosion"));

            SentryTurret enemyTurretSix = new SentryTurret(this, enemySix.PixelPosition, new List<TileRef>()
            {
                new TileRef(10, 5, 0),
            }, 64, 64, 0f, "Enemy Tank 6", ANGLE_DOWN, Content.Load<SoundEffect>("audio/SentryTurretTurn"),
            Content.Load<SoundEffect>("audio/TankExplosion"));

            SentryTurret enemyTurretSeven = new SentryTurret(this, enemySeven.PixelPosition, new List<TileRef>()
            {
                new TileRef(10, 5, 0),
            }, 64, 64, 0f, "Enemy Tank 7", 0, Content.Load<SoundEffect>("audio/SentryTurretTurn"),
            Content.Load<SoundEffect>("audio/TankExplosion"));

            LightSentryTurret enemyTurretEight = new LightSentryTurret(this, enemyEight.PixelPosition, new List<TileRef>()
            {
                new TileRef(11, 6, 0),
            }, 64, 64, 0f, "Enemy Tank 8", -ANGLE_RIGHT, Content.Load<SoundEffect>("audio/SentryTurretTurn"),
            Content.Load<SoundEffect>("audio/TankExplosion"));

            LightSentryTurret enemyTurretNine = new LightSentryTurret(this, enemyNine.PixelPosition, new List<TileRef>()
            {
                new TileRef(11, 6, 0),
            }, 64, 64, 0f, "Enemy Tank 9", -ANGLE_RIGHT, Content.Load<SoundEffect>("audio/SentryTurretTurn"),
            Content.Load<SoundEffect>("audio/TankExplosion"));

            HeavySentryTurret enemyTurretTen = new HeavySentryTurret(this, enemyTen.PixelPosition, new List<TileRef>()
            {
                new TileRef(11, 5, 0),
            }, 64, 64, 0f, "Enemy Tank 10", ANGLE_DOWN, Content.Load<SoundEffect>("audio/SentryTurretTurn"),
            Content.Load<SoundEffect>("audio/TankExplosion"));

            //SentryTurret enemyTurretEleven = new SentryTurret(this, enemyEleven.PixelPosition, new List<TileRef>()
            //{
            //    new TileRef(10, 5, 0),
            //}, 64, 64, 0f, "Enemy Tank 11", -ANGLE_HORIZONTAL, Content.Load<SoundEffect>("audio/SentryTurretTurn"),
            //Content.Load<SoundEffect>("audio/TankExplosion"));

            //SentryTurret enemyTurretTwelve = new SentryTurret(this, enemyTwelve.PixelPosition, new List<TileRef>()
            //{
            //    new TileRef(10, 5, 0),
            //}, 64, 64, 0f, "Enemy Tank 12", -ANGLE_VERTICAL, Content.Load<SoundEffect>("audio/SentryTurretTurn"),
            //Content.Load<SoundEffect>("audio/TankExplosion"));

            //SentryTurret enemyTurretThirteen = new SentryTurret(this, enemyThirteen.PixelPosition, new List<TileRef>()
            //{
            //    new TileRef(10, 5, 0),
            //}, 64, 64, 0f, "Enemy Tank 13", ANGLE_VERTICAL, Content.Load<SoundEffect>("audio/SentryTurretTurn"),
            //Content.Load<SoundEffect>("audio/TankExplosion"));

            //SentryTurret enemyTurretFourteen = new SentryTurret(this, enemyFourteen.PixelPosition, new List<TileRef>()
            //{
            //    new TileRef(10, 5, 0),
            //}, 64, 64, 0f, "Enemy Tank 14", 0f, Content.Load<SoundEffect>("audio/SentryTurretTurn"),
            //Content.Load<SoundEffect>("audio/TankExplosion"));

            #endregion

            #region Add Sentry Turrets to List
            SentryTurrets.Add(enemyTurretOne);
            SentryTurrets.Add(enemyTurretTwo);
            SentryTurrets.Add(enemyTurretThree);
            SentryTurrets.Add(enemyTurretFour);
            SentryTurrets.Add(enemyTurretFive);
            SentryTurrets.Add(enemyTurretSix);
            SentryTurrets.Add(enemyTurretSeven);
            SentryTurrets.Add(enemyTurretEight);
            SentryTurrets.Add(enemyTurretNine);
            SentryTurrets.Add(enemyTurretTen);
            //SentryTurrets.Add(enemyTurretEleven);
            //SentryTurrets.Add(enemyTurretTwelve);
            //SentryTurrets.Add(enemyTurretThirteen);
            //SentryTurrets.Add(enemyTurretFourteen);

            Services.AddService(SentryTurrets);
            #endregion

            #region Create Sentry Tank Projectiles
            int ENEMY_BULLET_SPD = PLAYER_BULLET_SPD - PLAYER_BULLET_SPD / 2;

            Projectile enemyBulletOne = new Projectile(this, "SENTRY", enemyOne.PixelPosition, new List<TileRef>()
            {
                new TileRef(10, 2, 0),
            }, 64, 64, 0f, enemyTurretOne.Direction, ENEMY_BULLET_SPD,
            Content.Load<SoundEffect>("audio/SentryTankShoot"),
            Content.Load<SoundEffect>("audio/TankArmorPierce"));

            Projectile enemyBulletTwo = new Projectile(this, "SENTRY", enemyTwo.PixelPosition, new List<TileRef>()
            {
                new TileRef(10, 2, 0),
            }, 64, 64, 0f, enemyTurretTwo.Direction, ENEMY_BULLET_SPD,
            Content.Load<SoundEffect>("audio/SentryTankShootAlt"),
            Content.Load<SoundEffect>("audio/TankArmorPierce"));

            Projectile enemyBulletThree = new Projectile(this, "SENTRY", new Vector2(0, 0), new List<TileRef>()
            {
                new TileRef(10, 2, 0),
            }, 64, 64, 0f, enemyTurretThree.Direction, ENEMY_BULLET_SPD,
            Content.Load<SoundEffect>("audio/SentryTankShootAlt"),
            Content.Load<SoundEffect>("audio/TankArmorPierce"));

            Projectile enemyBulletFour = new Projectile(this, "SENTRY", new Vector2(0, 0), new List<TileRef>()
            {
                new TileRef(10, 2, 0),
            }, 64, 64, 0f, enemyTurretFour.Direction, ENEMY_BULLET_SPD,
            Content.Load<SoundEffect>("audio/SentryTankShoot"),
            Content.Load<SoundEffect>("audio/TankArmorPierce"));

            Projectile enemyBulletFive = new Projectile(this, "SENTRY", new Vector2(0, 0), new List<TileRef>()
            {
                new TileRef(10, 2, 0),
            }, 64, 64, 0f, enemyTurretFive.Direction, ENEMY_BULLET_SPD,
            Content.Load<SoundEffect>("audio/SentryTankShootAlt"),
            Content.Load<SoundEffect>("audio/TankArmorPierce"));

            Projectile enemyBulletSix = new Projectile(this, "SENTRY", new Vector2(0, 0), new List<TileRef>()
            {
                new TileRef(10, 2, 0),
            }, 64, 64, 0f, enemyTurretSix.Direction, ENEMY_BULLET_SPD,
            Content.Load<SoundEffect>("audio/SentryTankShoot"),
            Content.Load<SoundEffect>("audio/TankArmorPierce"));

            Projectile enemyBulletSeven = new Projectile(this, "SENTRY", new Vector2(0, 0), new List<TileRef>()
            {
                new TileRef(10, 2, 0),
            }, 64, 64, 0f, enemyTurretSeven.Direction, ENEMY_BULLET_SPD,
            Content.Load<SoundEffect>("audio/SentryTankShoot"),
            Content.Load<SoundEffect>("audio/TankArmorPierce"));

            Ricochet enemyBulletEight = new Ricochet(this, "SENTRY", new Vector2(0, 0), new List<TileRef>()
            {
                new TileRef(11, 7, 0),
            }, 64, 64, 0f, enemyTurretEight.Direction, (ENEMY_BULLET_SPD + ENEMY_BULLET_SPD / 3),
            Content.Load<SoundEffect>("audio/SentryTankShootLight"),
            Content.Load<SoundEffect>("audio/TankArmorPierce"), null);

            Ricochet enemyBulletNine = new Ricochet(this, "SENTRY", new Vector2(0, 0), new List<TileRef>()
            {
                new TileRef(11, 7, 0),
            }, 64, 64, 0f, enemyTurretNine.Direction, (ENEMY_BULLET_SPD + ENEMY_BULLET_SPD / 3),
            Content.Load<SoundEffect>("audio/SentryTankShootLight"),
            Content.Load<SoundEffect>("audio/TankArmorPierce"), null);

            Ricochet enemyBulletTen = new Ricochet(this, "SENTRY", new Vector2(0, 0), new List<TileRef>()
            {
                new TileRef(10, 7, 0),
            }, 64, 64, 0f, enemyTurretTen.Direction, PLAYER_BULLET_SPD,
            Content.Load<SoundEffect>("audio/SentryTankShootHeavy"),
            Content.Load<SoundEffect>("audio/TankArmorPierce"), null);

            //Projectile enemyBulletEleven = new Projectile(this, "SENTRY", new Vector2(0, 0), new List<TileRef>()
            //{
            //    new TileRef(10, 2, 0),
            //}, 64, 64, 0f, enemyTurretEleven.Direction, ENEMY_BULLET_SPD,
            //Content.Load<SoundEffect>("audio/SentryTankShootAlt"),
            //Content.Load<SoundEffect>("audio/TankArmorPierce"));

            //Projectile enemyBulletTwelve = new Projectile(this, "SENTRY", new Vector2(0, 0), new List<TileRef>()
            //{
            //    new TileRef(10, 2, 0),
            //}, 64, 64, 0f, enemyTurretTwelve.Direction, ENEMY_BULLET_SPD,
            //Content.Load<SoundEffect>("audio/SentryTankShootAlt"),
            //Content.Load<SoundEffect>("audio/TankArmorPierce"));

            //Projectile enemyBulletThirteen = new Projectile(this, "SENTRY", new Vector2(0, 0), new List<TileRef>()
            //{
            //    new TileRef(10, 2, 0),
            //}, 64, 64, 0f, enemyTurretThirteen.Direction, ENEMY_BULLET_SPD,
            //Content.Load<SoundEffect>("audio/SentryTankShoot"),
            //Content.Load<SoundEffect>("audio/TankArmorPierce"));

            //Projectile enemyBulletFourteen = new Projectile(this, "SENTRY", new Vector2(0, 0), new List<TileRef>()
            //{
            //    new TileRef(10, 2, 0),
            //}, 64, 64, 0f, enemyTurretFourteen.Direction, ENEMY_BULLET_SPD,
            //Content.Load<SoundEffect>("audio/SentryTankShootAlt"),
            //Content.Load<SoundEffect>("audio/TankArmorPierce"));

            List<Projectile> sentryProjectiles = new List<Projectile>()
            {
              enemyBulletOne, enemyBulletTwo, enemyBulletThree,
              enemyBulletFour, enemyBulletFive, enemyBulletSix,
              enemyBulletSeven, enemyBulletEight, enemyBulletNine,
              enemyBulletTen//, enemyBulletEleven, enemyBulletTwelve,
              //enemyBulletThirteen, enemyBulletFourteen
            };

            for (int i = 0; i < SentryTurrets.Count; i++)
            {
                // Shooting Speed
                sentryProjectiles[i].explosionLifeSpan = sentryProjectiles[i].explosionLifeSpan * 2;
                // Bullet Life Span
                sentryProjectiles[i].flyingLifeSpan = sentryProjectiles[i].flyingLifeSpan + 0.25f;
                SentryTurrets[i].AddProjectile(sentryProjectiles[i]);
            }
            Services.AddService(sentryProjectiles);

            // Heavy Tank Turret Bullet NULL FIX
            enemyTurretTen.UpdateProjectile();
            #endregion

            #region Add Tank Crew
            List <Vector2> CrewmanPositions = new List<Vector2>
            {
                new Vector2(165, 1827),
                new Vector2(194, 863),
                new Vector2(2707, 1758),
                new Vector2(3612, 244),
                new Vector2(1306, 152)
            };

            Shuffle(CrewmanPositions);

            #region Tank Wreck Sprites
            TankWreckSprite[] WreckSprites = new TankWreckSprite[CrewmanPositions.Count];
            for (int i = 0; i < CrewmanPositions.Count; i++)
            {
                WreckSprites[i] = new TankWreckSprite(this, CrewmanPositions[i] + new Vector2(10, 6),
                new List<TileRef>()
                {
                    new TileRef(9,6,0)
                }, 64, 64, 0f);
            }

            WreckSprites[0].angleOfRotation = -ANGLE_DIAG_RIGHT;
            WreckSprites[1].angleOfRotation = -ANGLE_DIAG_RIGHT;
            WreckSprites[2].angleOfRotation = ANGLE_RIGHT;
            WreckSprites[3].angleOfRotation = ANGLE_RIGHT;
            WreckSprites[4].angleOfRotation = ANGLE_DIAG_RIGHT;
            #endregion

            //PowerUp Heal = new PowerUp(this, new Vector2(500, 192), new List<TileRef>()
            //{
            //    new TileRef(12,2,0),
            //}, 64, 64, 0f, 1, PowerUp.PowerUpType.Heal, 50, 1,
            //Content.Load<SoundEffect>(@"audio/Rescue3"));

            PowerUp Speed = new PowerUp(this, CrewmanPositions[0], new List<TileRef>()
            {
                new TileRef(12,3,0)               
            }, 64, 64, 0f, 5, PowerUp.PowerUpType.SpeedBoost, 0, 1.1f,
            Content.Load<SoundEffect>(@"audio/Rescue1"), null, null);

            PowerUp Regen = new PowerUp(this, CrewmanPositions[1], new List<TileRef>()
            {
                new TileRef(12,2,0),
            }, 64, 64, 0f, 1, PowerUp.PowerUpType.Regen, 1, 1,
            Content.Load<SoundEffect>(@"audio/Rescue2"), null, null);

            PowerUp DefenseBoost = new PowerUp(this, CrewmanPositions[2], new List<TileRef>()
            {
                new TileRef(12,1,0),
            }, 64, 64, 0f, 60, PowerUp.PowerUpType.DefenseBoost, 0, 2,
            Content.Load<SoundEffect>(@"audio/Rescue3"), null, null);

            PowerUp ExtraDamage = new PowerUp(this, CrewmanPositions[3], new List<TileRef>()
            {
                new TileRef(12,0,0),
            }, 64, 64, 0f, 60, PowerUp.PowerUpType.ExtraDamage, 0, 2,
            Content.Load<SoundEffect>(@"audio/Rescue4"), null, ricochetRound);

            SoundEffect CamoSound = Content.Load<SoundEffect>(@"audio/PlayerActiveCamo");

            PowerUp Camouflage = new PowerUp(this, CrewmanPositions[4], new List<TileRef>()
            {
                new TileRef(12,4,0),
            }, 64, 64, 0f, (float)CamoSound.Duration.TotalSeconds, PowerUp.PowerUpType.Camouflage, 0, 0,
            Content.Load<SoundEffect>(@"audio/Rescue4"), CamoSound, null);

            List<PowerUp> TankCrewmen = new List<PowerUp>()
            { Speed, Regen, DefenseBoost, ExtraDamage, Camouflage };

            Services.AddService(TankCrewmen);
            #endregion

            #region Add Crosshairs
            new Crosshair(this, new Vector2(0, 0), new List<TileRef>()
            {
                new TileRef(10, 3, 0),
            }, 64, 64, 0f);
            new MouseCrosshair(this, new Vector2(0, 0), new List<TileRef>()
            {
                new TileRef(11, 3, 0),
            }, 64, 64, 0f);
            #endregion

            #region Set Collisions
            Services.AddService(Colliders);
            SetCollider(TileType.DIRT);
            SetCollider(TileType.METAL);
            SetTrigger(TileType.DIRT2); // For WIN condition
            #endregion

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Helper.graphicsDevice = GraphicsDevice;

            penumbra.Transform = Camera.CurrentCameraTranslation;

            // Add SpriteBatch to services, it can be called anywhere.
            Services.AddService(spriteBatch);
            Services.AddService(Content.Load<Texture2D>(@"tiles/tilesheet"));

            // Tile References to be drawn on the Map corresponding to the entries in the defined 
            // Tile Map
            TileRefs.Add(new TileRef(7, 0, 0)); // Dirt
            TileRefs.Add(new TileRef(4, 1, 1)); // Ground
            TileRefs.Add(new TileRef(3, 2, 2)); // Metal Box
            TileRefs.Add(new TileRef(5, 0, 3)); // Ground 2
            TileRefs.Add(new TileRef(5, 1, 4)); // Ground 3
            TileRefs.Add(new TileRef(7, 2, 5)); // Ground 4
            TileRefs.Add(new TileRef(7, 3, 6)); // Ground 5
            TileRefs.Add(new TileRef(6, 2, 7)); // Ground 4
            TileRefs.Add(new TileRef(1, 1, 8)); // Metal 2
            TileRefs.Add(new TileRef(2, 1, 9)); // Metal 3
            TileRefs.Add(new TileRef(3, 1, 10)); // Metal 4
            TileRefs.Add(new TileRef(4, 2, 11)); // Dirt (Trigger)

            SimpleTileLayer Layer = new SimpleTileLayer(this, backTileNames, tileMap, TileRefs, tileWidth, tileHeight);
            Services.AddService(Layer);

            // This code is used to find tiles of a specific type
            //List<Tile> tileFound = SimpleTileLayer.GetNamedTiles(backTileNames[(int)TileType.GREENBOX]);

            #region Splash Screen
            // Load Animated Win Screen Loop
            Queue<Texture2D> txWinQueue = new Queue<Texture2D>();
            Dictionary<string, Texture2D> winFrames = new Dictionary<string, Texture2D>();

            winFrames = Loader.ContentLoad<Texture2D>(Content, "background\\WinFrames");
            foreach (var frame in winFrames)
            {
                txWinQueue.Enqueue(frame.Value);
            }

            MainScreen = new SplashScreen(this, Vector2.Zero, 360000.00f, // 6 mins
                            Content.Load<Texture2D>("background/MainMenu"),
                            Content.Load<Texture2D>("background/Lose"),
                            txWinQueue,
                            Content.Load<Song>("audio/MainMenu"),
                            Content.Load<Song>("audio/Play"),
                            Content.Load<Song>("audio/Pause"),
                            Content.Load<Song>("audio/GameOver"),
                            Content.Load<Song>("audio/Win"),
                            Keys.P, 
                            Keys.Enter,
                            Buttons.A,
                            Buttons.Start,
                            Content.Load<SpriteFont>("fonts/font"),
                            Content.Load<SoundEffect>("audio/BlinkPlay"),
                            Content.Load<SoundEffect>("audio/BlinkPause"));
            #endregion
        }

        protected override void UnloadContent()
        {
            penumbra.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            if (InputEngine.IsButtonPressed(Buttons.Back) || InputEngine.IsKeyPressed(Keys.Escape))
                Exit();

            if (this.IsActive)
                base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            penumbra.BeginDraw();
            GraphicsDevice.Clear(BackgroundColor);

            base.Draw(gameTime);
        }

        #region Methods
        public void Shuffle(List<Vector2> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Camera.random.Next(n + 1);
                Vector2 value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public void SetCollider(TileType t)
        {
            for (int x = 0; x < tileMap.GetLength(1); x++)
            {
                for (int y = 0; y < tileMap.GetLength(0); y++)
                {
                    if (tileMap[y, x] == (int)t)
                    {
                        Colliders.Add(new Collider(this,
                            Content.Load<Texture2D>(@"tiles/collider"),
                            x, y
                            ));
                    }
                }
            }
        }
        public void SetTrigger(TileType t)
        {
            for (int x = 0; x < tileMap.GetLength(1); x++)
            {
                for (int y = 0; y < tileMap.GetLength(0); y++)
                {
                    if (tileMap[y, x] == (int)t)
                    {
                        Colliders.Add(new TileTrigger("EXIT", this,
                            Content.Load<Texture2D>(@"tiles/collider"),
                            x, y
                            ));
                    }
                }
            }
        }
        #endregion
    }
}
