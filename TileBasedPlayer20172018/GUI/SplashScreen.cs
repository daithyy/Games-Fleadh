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
using PowerUps;

namespace Screens
{
    public class SplashScreen : DrawableGameComponent
    {
        #region Properties
        Texture2D _txMain;
        Texture2D _txBlack;
        Texture2D _txWhite;
        Texture2D _txGameOver;
        Texture2D _txWin;
        public const float VOLUME = 0.25f;

        public bool Active { get; set; }
        private VideoPlayer vidPlayer;
        private Video vidWin { get; set; }
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
        private Texture2D txBlack
        {
            get { return _txBlack; }
            set { _txBlack = value; }
        }
        private Texture2D txWhite
        {
            get { return _txWhite; }
            set { _txWhite = value; }
        }
        private Rectangle ScreenRect
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
        private float OverlayAlpha = 0f;
        private const float FADE_AMOUNT = 0.01f;
        private bool fadeIn = true;
        private bool fadeOut = false;
        public float TimeRemaining;
        private float TrackPlayCount = 0; // To stop Game Over track loop
        public Color FontColor = new Color(243, 208, 168);
        public Color FontWinColor = new Color(169, 242, 181);
        public Color FontSafeColor = new Color(0, 137, 81);
        public Color FontWarningColor = new Color(255, 86, 86);
        TimeSpan PauseTime;
        #endregion

        #region Constructor
        public SplashScreen(Game game, Vector2 pos, float timeLeft,
            Texture2D txMain, Texture2D txGameOver, Video vidWin,
            Song menuMusic, Song playMusic, Song pauseMusic, Song gameOverMusic, Song winMusic,
            Keys pauseKey, Keys activateKey, Buttons activateButton, Buttons pauseButton, 
            SpriteFont fontIn, SoundEffect blinkPlay, SoundEffect blinkPause) : base(game)
        {
            game.Components.Add(this);
            DrawOrder = 1000;

            _txMain = txMain;
            _txBlack = new Texture2D(game.GraphicsDevice, 1, 1);
            _txBlack.SetData(new[] { new Color(0,0,0,255) });
            _txWhite = new Texture2D(game.GraphicsDevice, 1, 1);
            _txWhite.SetData(new[] { Color.White });
            _txGameOver = txGameOver;
            vidPlayer = new VideoPlayer();
            vidPlayer.IsMuted = true;
            this.vidWin = vidWin;
            Position = pos;
            ActivationKey = activateKey;
            ActivationButton = activateButton;
            PauseKey = pauseKey;
            PauseButton = pauseButton;
            Font = fontIn;
            TimeRemaining = timeLeft;
            CurrentScreen = ActiveScreen.MAIN;
            CurrentGameCondition = GameCondition.LOSE;
            OverlayAlpha = 1;
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
            float deltaTime = gameTime.ElapsedGameTime.Milliseconds;

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

                    if (fadeIn && OverlayAlpha <= 1)
                        OverlayAlpha -= FADE_AMOUNT;

                    // Check Input
                    if (InputEngine.IsKeyPressed(ActivationKey) ||
                        InputEngine.IsButtonPressed(ActivationButton))
                    {
                        OverlayAlpha = 1;
                        BlinkPlay.Play();
                        Active = !Active;
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
                    
                    if (player.Health > 0 && TimeRemaining > 0)
                    {
                        TimeRemaining -= deltaTime;

                        // Check Input
                        if (InputEngine.IsKeyPressed(PauseKey) ||
                            InputEngine.IsButtonPressed(PauseButton))
                        {
                            Active = !Active;
                            BlinkPause.Play();
                            Helper.CurrentGameStatus = GameStatus.PAUSED;
                        }

                        if (OverlayAlpha <= 0)
                            fadeIn = false;
                        if (fadeIn && OverlayAlpha > 0)
                            OverlayAlpha -= FADE_AMOUNT;
                    }
                    else
                    {
                        MediaPlayer.Stop();

                        // Wait for Input
                        if (InputEngine.IsPadInputChanged(true) ||
                            InputEngine.IsKeyInputChanged())
                        {
                            fadeOut = true;
                        }

                        if (fadeOut)
                        {
                            OverlayAlpha += FADE_AMOUNT;
                            if (OverlayAlpha >= 1)
                            {
                                Active = !Active;
                                CurrentGameCondition = GameCondition.LOSE;
                                CurrentScreen = ActiveScreen.LOSE;
                                fadeIn = true;
                            }
                        }
                    }

                    if (CurrentGameCondition == GameCondition.WIN)
                    {
                        OverlayAlpha += FADE_AMOUNT;
                        if (OverlayAlpha >= 1)
                        {
                            MediaPlayer.Stop();
                            Active = !Active;
                            CurrentScreen = ActiveScreen.WIN;
                        }
                    }
                    break;
                case ActiveScreen.PAUSE:
                    if (MediaPlayer.State == MediaState.Stopped)
                    {
                        OverlayAlpha = 0.5f;
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
                        OverlayAlpha = 0;
                        Active = !Active;
                        BlinkPause.Play();
                        Helper.CurrentGameStatus = GameStatus.PLAYING;
                    }
                    break;
                case ActiveScreen.LOSE:
                    if (OverlayAlpha <= 1 || OverlayAlpha > 0)
                        OverlayAlpha -= FADE_AMOUNT;

                    if (MediaPlayer.State == MediaState.Stopped && TrackPlayCount < 1)
                    {
                        MediaPlayer.Play(GameOverTrack);
                        TrackPlayCount++;
                        MediaPlayer.IsRepeating = false;
                    }
                    Helper.CurrentGameStatus = GameStatus.PAUSED;
                    break;
                case ActiveScreen.WIN:
                    if (OverlayAlpha <= 1 || OverlayAlpha > 0)
                        OverlayAlpha -= FADE_AMOUNT;

                    if (MediaPlayer.State == MediaState.Stopped && TrackPlayCount < 1)
                    {
                        MediaPlayer.Play(WinTrack);
                        TrackPlayCount++;
                        if (vidPlayer.State == MediaState.Stopped)
                        {
                            vidPlayer.Play(vidWin);
                        }
                    }

                    if (vidPlayer.State != MediaState.Stopped)
                        _txWin = vidPlayer.GetTexture();
                    else
                        vidPlayer.Dispose();

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
                spriteBatch.DrawString(Font,
                    "STEEL WRATH", new Vector2(Helper.graphicsDevice.Viewport.Width / 2 -
                    Font.MeasureString("STEEL WRATH").X / 2, Helper.graphicsDevice.Viewport.Height / 2 - 50),
                    FontColor);
                spriteBatch.DrawString(Font,
                    "Press enter / A to start", new Vector2(Helper.graphicsDevice.Viewport.Width / 2 -
                    Font.MeasureString("Press enter / A to start").X / 2, Helper.graphicsDevice.Viewport.Height / 2 + 180),
                    FontWinColor);
                spriteBatch.Draw(txWhite, ScreenRect, Color.White * OverlayAlpha);
            }
            else if (Active && CurrentScreen == ActiveScreen.PAUSE)
            {
                spriteBatch.Draw(txBlack, ScreenRect, Color.White * OverlayAlpha);
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
                spriteBatch.DrawString(Font,
                    "GAME OVER", new Vector2(Helper.graphicsDevice.Viewport.Width / 2 -
                    Font.MeasureString("GAME OVER").X / 2, Helper.graphicsDevice.Viewport.Height / 2 + 170),
                    FontColor);
                spriteBatch.Draw(txBlack, ScreenRect, Color.White * OverlayAlpha);
            }
            else if (Active && CurrentScreen == ActiveScreen.WIN)
            {
                if (_txWin != null)
                    spriteBatch.Draw(_txWin, new Rectangle(Position.ToPoint(), new Point(
                        Helper.graphicsDevice.Viewport.Bounds.Width,
                        Helper.graphicsDevice.Viewport.Bounds.Height)), Color.White);
                spriteBatch.DrawString(Font,
                    "You WIN", new Vector2(Helper.graphicsDevice.Viewport.Width / 2 -
                    Font.MeasureString("You WIN").X / 2, Helper.graphicsDevice.Viewport.Height / 2 - 180),
                    FontWinColor);
                spriteBatch.Draw(txWhite, ScreenRect, Color.White * OverlayAlpha);
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
                    Font.MeasureString("Tanks Left: " + String.Format("{0}", Convert.ToInt32(SentryTurret.Count))).X / 2 + 220, 
                    (Helper.graphicsDevice.Viewport.Bounds.Height - 48)),
                    FontColor);

                if (SentryTurret.Count <= 0 && PowerUp.Count >= 5)
                {
                    spriteBatch.DrawString(Font,
                    "ESCAPE !",
                    new Vector2(Helper.graphicsDevice.Viewport.Bounds.Width / 2 -
                    Font.MeasureString("FINISH !").X / 2,
                    (Helper.graphicsDevice.Viewport.Bounds.Height - 48)),
                    FontWinColor);
                }

                if (CurrentGameCondition == GameCondition.LOSE)
                    spriteBatch.Draw(txBlack, ScreenRect, Color.White * OverlayAlpha);
                if (CurrentGameCondition == GameCondition.WIN)
                    spriteBatch.Draw(txWhite, ScreenRect, Color.White * OverlayAlpha);
            }
            spriteBatch.End();
        }
        #endregion
    }
}
