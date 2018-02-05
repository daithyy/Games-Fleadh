using Engine.Engines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers;
using Tiler;

namespace Screens
{
    public class SplashScreen : DrawableGameComponent
    {
        Texture2D _txMain;
        Texture2D _txPause;
        Texture2D _txGameOver;
        Texture2D _txWin;
        public const float VOLUME = 0.6f;
        public bool Active { get; set; }
        public Texture2D txMain
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
        public Texture2D txPause
        {
            get
            {
                return _txPause;
            }

            set
            {
                _txPause = value;
            }
        }
        public Texture2D txGameOver
        {
            get { return _txGameOver; }
            set { _txGameOver = value; }
        }
        public Texture2D txWin
        {
            get { return _txGameOver; }
            set { _txGameOver = value; }
        }
        public Song MenuTrack { get; set; }
        public Song BackingTrack { get; set; }
        public Song PauseTrack { get; set; }
        public Song GameOverTrack { get; set; }
        public Song WinTrack { get; set; }
        public SoundEffect BlinkPlay { get; set; }
        public SoundEffect BlinkPause { get; set; }
        public Vector2 Position { get; set; }
        public Keys PauseKey;
        public Keys ActivationKey;
        public ActiveScreen CurrentScreen;
        public GameCondition CurrentGameCondition;
        public SpriteFont Font;
        public float TimeRemaining;
        private float TrackPlayCount = 0; // To stop Game Over track loop
        public Color FontColor = new Color(243, 208, 168);
        public Color FontSafeColor = new Color(0, 137, 81);
        public Color FontWarningColor = new Color(255, 86, 86);
        TimeSpan PauseTime;

        public SplashScreen(Game game, Vector2 pos, float timeLeft,
            Texture2D txMain, Texture2D txPause, Texture2D txGameOver, Texture2D txWin,
            Song menuMusic, Song playMusic, Song pauseMusic, Song gameOverMusic, Song winMusic,
            Keys pauseKey, Keys activateKey, SpriteFont fontIn, SoundEffect blinkPlay, SoundEffect blinkPause) : base(game)
        {
            game.Components.Add(this);
            DrawOrder = 1000;
            #region Load Audio
            _txMain = txMain;
            _txPause = txPause;
            _txGameOver = txGameOver;
            _txWin = txWin;
            MenuTrack = menuMusic;
            BackingTrack = playMusic;
            PauseTrack = pauseMusic;
            GameOverTrack = gameOverMusic;
            WinTrack = winMusic;
            BlinkPlay = blinkPlay;
            BlinkPause = blinkPause;
            #endregion
            Position = pos;
            ActivationKey = activateKey;
            PauseKey = pauseKey;
            Font = fontIn;
            TimeRemaining = timeLeft;
            CurrentScreen = ActiveScreen.MAIN;
            CurrentGameCondition = GameCondition.LOSE;
            Active = true;
            MediaPlayer.Volume = VOLUME;
            MediaPlayer.IsRepeating = true;
        }

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
                    if (InputEngine.IsKeyPressed(ActivationKey))
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
                    if (InputEngine.IsKeyPressed(PauseKey))
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
                        Active = !Active;
                        CurrentGameCondition = GameCondition.LOSE;
                        CurrentScreen = ActiveScreen.LOSE;
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
                    if (InputEngine.IsKeyPressed(PauseKey))
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
                spriteBatch.Draw(_txPause, new Rectangle(Position.ToPoint(), new Point(
                    Helper.graphicsDevice.Viewport.Bounds.Width,
                    Helper.graphicsDevice.Viewport.Bounds.Height)), Color.White);
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
                spriteBatch.DrawString(Font,
                    "Time Remaining: ",
                    new Vector2(Game.Window.ClientBounds.Width / 2 -
                    Font.MeasureString("Time Remaining: " + String.Format("{0}",
                    Convert.ToInt32(TimeRemaining / 1000)) + " seconds").X / 2, 24),
                    FontColor);

                if (TimeRemaining / 1000 <= 10)
                {

                    spriteBatch.DrawString(Font,
                    String.Format("{0}", Convert.ToInt32(TimeRemaining / 1000)) + " seconds",
                    new Vector2(Game.Window.ClientBounds.Width / 2 -
                    Font.MeasureString(String.Format("{0}",
                    Convert.ToInt32(TimeRemaining / 1000)) + " seconds").X / 2 + Font.MeasureString("Time Remaining: ").X / 2, 24),
                    FontWarningColor);
                }
                else
                {
                    spriteBatch.DrawString(Font,
                    String.Format("{0}", Convert.ToInt32(TimeRemaining / 1000)) + " seconds",
                    new Vector2(Game.Window.ClientBounds.Width / 2 -
                    Font.MeasureString(String.Format("{0}",
                    Convert.ToInt32(TimeRemaining / 1000)) + " seconds").X / 2 + Font.MeasureString("Time Remaining: ").X / 2, 24),
                    FontColor);
                }

                spriteBatch.DrawString(Font,
                    "Tanks Remaining: " + String.Format("{0}", Convert.ToInt32(SentryTurret.Count)),
                    new Vector2(Game.Window.ClientBounds.Width / 2 -
                    Font.MeasureString("Tanks Remaining: " + String.Format("{0}", Convert.ToInt32(SentryTurret.Count))).X / 2 - 220, 
                    (Game.Window.ClientBounds.Height - 48)),
                    FontColor);

                if (SentryTurret.Count <= 0)
                {
                    spriteBatch.DrawString(Font,
                    "ESCAPE !",
                    new Vector2(Game.Window.ClientBounds.Width / 2 -
                    Font.MeasureString("FINISH !").X / 2 + 24,
                    (Game.Window.ClientBounds.Height - 48)),
                    FontSafeColor);
                }
            }
            spriteBatch.End();
        }
    }
}
