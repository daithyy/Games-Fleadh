using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Penumbra;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Helpers;
using Tiler;
using InputManager;
using CameraNS;

namespace Screens
{
    public class SplashScreen : DrawableGameComponent
    {
        #region Properties
        Texture2D _txMain;
        Texture2D _txPause;
        Texture2D _txGameOver;
        Texture2D _txWin;
        public const float VOLUME = 0.25f;

        public bool Active { get; set; }
        private Texture2D txMain
        {
            get
            {
                return _txMain;
            }

            set
            {
                _txMain = value;
            }
        }
        private Texture2D txPause
        {
            get { return _txPause; }
            set { _txPause = value; }
        }
        private Rectangle txPauseRect
        {
            get { return new Rectangle(0, 0, 
                GraphicsDevice.Viewport.Width, 
                GraphicsDevice.Viewport.Height); }
        }
        private Texture2D txGameOver
        {
            get { return _txGameOver; }
            set { _txGameOver = value; }
        }
        private Texture2D txWin
        {
            get { return _txGameOver; }
            set { _txGameOver = value; }
        }
        private Song MenuTrack { get; set; }
        private Song BackingTrack { get; set; }
        private Song PauseTrack { get; set; }
        private Song GameOverTrack { get; set; }
        private Song WinTrack { get; set; }
        private SoundEffect BlinkPlay { get; set; }
        private SoundEffect BlinkPause { get; set; }
        public Vector2 Position { get; set; }
        private Keys PauseKey;
        private Buttons PauseButton;
        private Keys ActivationKey;
        private Buttons ActivationButton;
        public ActiveScreen CurrentScreen;
        public GameCondition CurrentGameCondition;
        private SpriteFont Font;
        public float TimeRemaining;
        private float TrackPlayCount = 0; // To stop Game Over track loop
        public Color FontColor = new Color(243, 208, 168);
        public Color FontSafeColor = new Color(0, 137, 81);
        public Color FontWarningColor = new Color(255, 86, 86);
        TimeSpan PauseTime;
        #endregion

        #region Constructor
        public SplashScreen(Game game, Vector2 pos, float timeLeft,
            Texture2D txMain, Texture2D txGameOver, Texture2D txWin,
            Song menuMusic, Song playMusic, Song pauseMusic, Song gameOverMusic, Song winMusic,
            Keys pauseKey, Keys activateKey, Buttons activateButton, Buttons pauseButton, 
            SpriteFont fontIn, SoundEffect blinkPlay, SoundEffect blinkPause) : base(game)
        {
            game.Components.Add(this);
            DrawOrder = 1000;

            _txMain = txMain;
            txPause = new Texture2D(game.GraphicsDevice, 1, 1);
            txPause.SetData(new[] { new Color(0,0,0,127) });
            _txGameOver = txGameOver;
            _txWin = txWin;
            Position = pos;
            ActivationKey = activateKey;
            ActivationButton = activateButton;
            PauseKey = pauseKey;
            PauseButton = pauseButton;
            Font = fontIn;
            TimeRemaining = timeLeft;
            CurrentScreen = ActiveScreen.MAIN;
            CurrentGameCondition = GameCondition.LOSE;
            Active = true;

            #region Load Audio
            MenuTrack = menuMusic;
            BackingTrack = playMusic;
            PauseTrack = pauseMusic;
            GameOverTrack = gameOverMusic;
            WinTrack = winMusic;
            BlinkPlay = blinkPlay;
            BlinkPause = blinkPause;
            #endregion

            MediaPlayer.Volume = VOLUME;
            MediaPlayer.IsRepeating = true;
        }
        #endregion

        #region Methods
        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.Milliseconds;

            switch (CurrentScreen)
            {
                case ActiveScreen.MAIN:
                    if (Active)
                    {
                        if (MediaPlayer.State == MediaState.Stopped)
                            MediaPlayer.Play(MenuTrack);
                    }
                    else
                    {
                        MediaPlayer.Stop();
                        MediaPlayer.Play(BackingTrack);
                        CurrentScreen = ActiveScreen.PLAY;
                    }

                    // Check Input
                    if (InputEngine.IsKeyPressed(ActivationKey) ||
                        InputEngine.IsButtonPressed(ActivationButton))
                    {
                        Active = !Active;
                        BlinkPlay.Play();
                        Helper.CurrentGameStatus = GameStatus.PLAYING;
                    }
                    break;
                case ActiveScreen.PLAY:
                    TilePlayer player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));

                    if (Active)
                    {
                        MediaPlayer.Stop();
                        PauseTime = gameTime.TotalGameTime;
                        CurrentScreen = ActiveScreen.PAUSE;
                    }

                    // Check Input
                    if (InputEngine.IsKeyPressed(PauseKey) ||
                        InputEngine.IsButtonPressed(PauseButton))
                    {
                        Active = !Active;
                        BlinkPause.Play();
                        Helper.CurrentGameStatus = GameStatus.PAUSED;
                        
                    }
                    
                    if (player.Health > 0 && TimeRemaining > 0)
                    {
                        TimeRemaining -= deltaTime;
                    }
                    else
                    {
                        MediaPlayer.Stop();
                        //Active = !Active;
                        //CurrentGameCondition = GameCondition.LOSE;
                        //CurrentScreen = ActiveScreen.LOSE;
                    }

                    if (CurrentGameCondition == GameCondition.WIN)
                    {
                        MediaPlayer.Stop();
                        Active = !Active;
                        CurrentScreen = ActiveScreen.WIN;
                    }
                    break;
                case ActiveScreen.PAUSE:
                    if (MediaPlayer.State == MediaState.Stopped)
                    {
                        MediaPlayer.Play(PauseTrack);
                    }

                    if (!Active)
                    {
                        MediaPlayer.Stop();
                        MediaPlayer.Play(BackingTrack);
                        CurrentScreen = ActiveScreen.PLAY;
                        gameTime.TotalGameTime = PauseTime;
                    }

                    // Check Input
                    if (InputEngine.IsKeyPressed(PauseKey) ||
                        InputEngine.IsButtonPressed(PauseButton))
                    {
                        Active = !Active;
                        BlinkPause.Play();
                        Helper.CurrentGameStatus = GameStatus.PLAYING;
                    }
                    break;
                case ActiveScreen.LOSE:
                    
                    if (MediaPlayer.State == MediaState.Stopped && TrackPlayCount < 1)
                    {
                        MediaPlayer.Play(GameOverTrack);
                        TrackPlayCount++;
                        MediaPlayer.IsRepeating = false;
                    }
                    Helper.CurrentGameStatus = GameStatus.PAUSED;
                    break;
                case ActiveScreen.WIN:
                    if (MediaPlayer.State == MediaState.Stopped && TrackPlayCount < 1)
                    {
                        MediaPlayer.Play(WinTrack);
                        TrackPlayCount++;
                    }
                    Helper.CurrentGameStatus = GameStatus.PAUSED;
                    break;
                default:
                    break;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();

            spriteBatch.Begin(SpriteSortMode.Immediate,
                        BlendState.AlphaBlend, SamplerState.PointClamp);
            if (Active && CurrentScreen == ActiveScreen.MAIN)
            {
                spriteBatch.Draw(_txMain, new Rectangle(Position.ToPoint(), new Point(
                    Helper.graphicsDevice.Viewport.Bounds.Width,
                    Helper.graphicsDevice.Viewport.Bounds.Height)), Color.White);
            }
            else if (Active && CurrentScreen == ActiveScreen.PAUSE)
            {
                spriteBatch.Draw(txPause, txPauseRect, Color.White);
                spriteBatch.DrawString(Font,
                    "Paused", new Vector2(Helper.graphicsDevice.Viewport.Width / 2 -
                    Font.MeasureString("Paused").X / 2, Helper.graphicsDevice.Viewport.Height / 2),
                    FontColor);
            }
            else if (Active && CurrentScreen == ActiveScreen.LOSE)
            {
                spriteBatch.Draw(_txGameOver, new Rectangle(Position.ToPoint(), new Point(
                    Helper.graphicsDevice.Viewport.Bounds.Width,
                    Helper.graphicsDevice.Viewport.Bounds.Height)), Color.White);
            }
            else if (Active && CurrentScreen == ActiveScreen.WIN)
            {
                spriteBatch.Draw(_txWin, new Rectangle(Position.ToPoint(), new Point(
                    Helper.graphicsDevice.Viewport.Bounds.Width,
                    Helper.graphicsDevice.Viewport.Bounds.Height)), Color.White);
            }
            else if (!Active && CurrentScreen == ActiveScreen.PLAY)
            {
                #region Get Minutes and Seconds Format
                int minutes = (int)Math.Floor((TimeRemaining / 1000) / 60f);
                int seconds = (int)Math.Floor((TimeRemaining / 1000) - minutes * 60);
                string timeLeft = string.Format("{0:00}:{1:00}", minutes, seconds);
                #endregion

                if ((TimeRemaining / 1000) <= 10)
                {
                    spriteBatch.DrawString(Font, timeLeft,
                    new Vector2(Helper.graphicsDevice.Viewport.Width / 2 -
                    Font.MeasureString(timeLeft).X / 2, 24),
                    FontWarningColor);
                }
                else
                {
                    spriteBatch.DrawString(Font, timeLeft,
                    new Vector2(Helper.graphicsDevice.Viewport.Width / 2 -
                    Font.MeasureString(timeLeft).X / 2, 24),
                    FontColor);
                }

                spriteBatch.DrawString(Font,
                    "Tanks Left: " + String.Format("{0}", Convert.ToInt32(SentryTurret.Count)),
                    new Vector2(Helper.graphicsDevice.Viewport.Bounds.Width / 2 -
                    Font.MeasureString("Tanks Left: " + String.Format("{0}", Convert.ToInt32(SentryTurret.Count))).X / 2 - 220, 
                    (Helper.graphicsDevice.Viewport.Bounds.Height - 48)),
                    FontColor);

                if (SentryTurret.Count <= 0)
                {
                    spriteBatch.DrawString(Font,
                    "ESCAPE !",
                    new Vector2(Helper.graphicsDevice.Viewport.Bounds.Width / 2 -
                    Font.MeasureString("FINISH !").X / 2 + 24,
                    (Helper.graphicsDevice.Viewport.Bounds.Height - 48)),
                    FontSafeColor);
                }
            }
            spriteBatch.End();
        }
        #endregion
    }
}
