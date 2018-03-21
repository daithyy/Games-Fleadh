using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Penumbra;

using CameraNS;
using AnimatedSprite;
using Tiling;
using Helpers;
using Tiler;
using InputManager;

namespace PowerUps
{
    class PowerUp : RotatingSprite
    {
        #region Properties

        /// <summary>
        /// <para>1.  Regen</para>
        ///     Adds your AMOUNT x FACTOR over durations of one second.
        /// <para>2.  Health</para>
        ///     Adds your AMOUNT x FACTOR.
        /// <para>3.  DefenseBoost</para>
        ///     Subtracts CURRENT player damage rate / FACTOR.
        /// <para>4.  SpeedBoost</para>
        ///     Subtracts CURRENT player speed x FACTOR.
        /// <para>5.  ExtraDamage</para>
        ///     Adds your entered AMOUNT x FACTOR.
        /// <para>6.  Camouflage</para>
        ///     Reduces AMOUNT x FACTOR sentry detection radius.
        ///     Leave this at 0 for no detection!
        /// </summary>
        public enum PowerUpType
        {
            Heal,
            Regen,
            DefenseBoost,
            SpeedBoost,
            ExtraDamage,
            Camouflage
        }

        public enum PowerUpStatus
        {
            Activated,
            Deactivated,
            ExecuteOnce,
            Depleted
        }

        public Light OrbLight { get; } = new PointLight
        {
            Scale = new Vector2(100),
            Intensity = 0.5f,
            Color = Color.White,
            ShadowType = ShadowType.Illuminated,
            CastsShadows = false,
        };

        private PowerUpType Type;
        public PowerUpStatus State;
        public static int Count = 0;

        private float Duration = 0; // Effect life time
        private const int MAX_COOLDOWN = 10; // Cool down time
        private float coolDownTime = 0;
        public float Factor = 0; // Double, Triple amount.etc
        public int Amount = 0;
        public float MaxLifeTime;
        public float RegenTimer;
        private bool attachToHUD = false;
        private float distance;
        private const int RUN_RADIUS = 100;
        private const float RUN_SPEED = 0.01f;

        #region Tank Crewman Colors

        #endregion

        private Color durationBarColor = new Color(169, 169, 242);
        private HealthBar durationBar;
        private const float ALPHA_SPD = 0.05f;
        private Ricochet newRound;

        private SoundEffect camoSnd;
        private SoundEffectInstance camoSndInst;
        private SoundEffect pickupSnd;
        private SoundEffectInstance pickupSndInst;

        #region Store Default values
        private Projectile defaultBullet;
        private SoundEffect defaultShellSnd; // Store original shoot sound.
        private float defaultTurnSpeed; // Store original turret turning speed.
        private Vector2 defaultMaxVelocity; // Default player speed.
        private Vector2 defaultPlayerAcceleration;
        private Vector2 defaultPlayerDeceleration;
        private int defaultPlayerDamageRate; // Default player damage rate.
        private int defaultSentryDamageRate; // Default sentry damage rate.
        private int defaultRadius; // Player spotted radius.
        #endregion
        #endregion

        #region Constructors
        public PowerUp(Game game, Vector2 position, List<TileRef>sheetRefs, 
            int frameWidth, int frameHeight, float layerDepth,
            float maxLifeTime, PowerUpType type, int amount, float multiplier,
            SoundEffect pickupSnd, SoundEffect camoSnd, Ricochet newRound)
            : base(game, position, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            Visible = true;
            DrawOrder = 100;
            this.Type = type;
            this.MaxLifeTime = maxLifeTime;
            this.Factor = multiplier;
            this.Amount = amount;
            this.State = PowerUpStatus.Deactivated;
            this.newRound = newRound;
            BoundingRectangle = new Rectangle(
                PixelPosition.ToPoint(), 
                new Point(FrameWidth / 8, FrameHeight / 8));

            #region Get Default Values
            TilePlayer player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));
            TilePlayerTurret playerTurret = (TilePlayerTurret)Game.Services.GetService(typeof(TilePlayerTurret));
            List<SentryTurret> sentryTurrets = (List<SentryTurret>)Game.Services.GetService(typeof(List<SentryTurret>));

            if (player != null)
            {
                defaultMaxVelocity = player.MaxVelocity;
                defaultPlayerAcceleration = player.Acceleration;
                defaultPlayerDeceleration = player.Deceleration;
                defaultTurnSpeed = player.turnSpeed;
            }

            foreach (SentryTurret turret in sentryTurrets)
            {
                if (turret != null)
                {
                    // Finds first enemy tank in list that's not NULL and takes those as default values.
                    defaultPlayerDamageRate = turret.Bullet.playerDamageRate;
                    defaultRadius = turret.DetectRadius;
                    break;
                }
            }

            if (playerTurret != null)
            {
                defaultSentryDamageRate = playerTurret.Bullet.sentryDamageRate;
                defaultBullet = playerTurret.Bullet;
                defaultShellSnd = playerTurret.ShellSound;
            }
            #endregion

            #region Handle Audio
            this.pickupSnd = pickupSnd;
            pickupSndInst = this.pickupSnd.CreateInstance();
            #endregion

            #region Setup OrbLight
            PenumbraComponent penumbra = Game.Services.GetService<PenumbraComponent>();
            penumbra.Lights.Add(OrbLight);

            switch (type)
            {
                case PowerUpType.Regen:
                case PowerUpType.Heal:
                    OrbLight.Color = Color.LimeGreen;
                    break;
                case PowerUpType.DefenseBoost:
                    OrbLight.Color = Color.LightBlue;
                    break;
                case PowerUpType.SpeedBoost:
                    OrbLight.Color = Color.Orange;
                    break;
                case PowerUpType.ExtraDamage:
                    OrbLight.Color = Color.Red;
                    break;
                case PowerUpType.Camouflage:
                    OrbLight.Color = Color.Purple;
                    break;
                default:
                    OrbLight.Color = Color.White;
                    break;
            }
            #endregion

            #region Setup Duration Bar
            if (Type == PowerUpType.Camouflage)
            {
                durationBar = new HealthBar(game, CentrePos);
                durationBar.Name = Type.ToString();
                durationBar.txHealthBar.SetData(new[] { durationBarColor });
                AddHealthBar(durationBar);
                Health = (int)Duration;

                this.camoSnd = camoSnd;
                camoSndInst = this.camoSnd.CreateInstance();
                camoSndInst.Volume = 0.8f;
            }
            #endregion
        }
        #endregion

        #region Methods
        public void Activate()
        {
            if (State == PowerUpStatus.Deactivated)
            {
                Interlocked.Increment(ref Count);
                pickupSndInst.Play();
                OrbLight.Enabled = false;
                Frames.Clear();
                State = PowerUpStatus.Activated;
            }
        }

        public void Deactivate(TilePlayer other, TilePlayerTurret otherTurret, List<SentryTurret> sentryTurrets)
        {
            switch (Type)
            {
                case PowerUpType.Heal:
                    // Do nothing on deactivation
                    break;
                case PowerUpType.DefenseBoost:
                    foreach (SentryTurret turret in sentryTurrets)
                    {
                        if (turret != null)
                            turret.Bullet.playerDamageRate = defaultPlayerDamageRate;
                    }
                    break;
                case PowerUpType.SpeedBoost:
                    other.MaxVelocity = defaultMaxVelocity;
                    other.Acceleration = defaultPlayerAcceleration;
                    other.Deceleration = defaultPlayerDeceleration;
                    other.turnSpeed = defaultTurnSpeed;
                    break;
                case PowerUpType.ExtraDamage:
                    otherTurret.Bullet = defaultBullet;
                    otherTurret.Bullet.sentryDamageRate = defaultSentryDamageRate;
                    otherTurret.ShellSound = defaultShellSnd;
                    break;
                case PowerUpType.Camouflage:
                    other.Alpha = 1;
                    otherTurret.Alpha = 1;

                    List<Sentry> otherTanks = (List<Sentry>)Game.Services.GetService(typeof(List<Sentry>));

                    foreach (Sentry tank in otherTanks)
                    {
                        if (tank != null)
                            tank.StayVisible = false;
                    }

                    foreach (SentryTurret turret in sentryTurrets)
                    {
                        if (turret != null)
                            turret.DetectRadius = defaultRadius;
                    }
                    break;
            }
        }

        private void Affect(GameTime gameTime, TilePlayer other, TilePlayerTurret otherTurret, List<SentryTurret> sentryTurrets)
        {
            switch (Type)
            {
                case PowerUpType.Heal:
                    Frames.Add(new TileRef(13, 2, 0));
                    other.Health += Amount * (int)Factor;
                    State = PowerUpStatus.Depleted;
                    break;
                case PowerUpType.Regen:
                    Frames.Add(new TileRef(13, 2, 0));
                    //Duration = 0;
                    RegenTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (other.Health < 100 && other.Health > 0 && RegenTimer > 1)
                    {
                        other.Health += Amount * (int)Factor;
                        RegenTimer = 0;
                    }
                    break;
                case PowerUpType.DefenseBoost:
                    Frames.Add(new TileRef(13, 1, 0));
                    foreach (SentryTurret turret in sentryTurrets)
                    {
                        if (turret != null)
                            turret.Bullet.playerDamageRate /= (int)Factor;
                    }
                    State = PowerUpStatus.ExecuteOnce;
                    break;
                case PowerUpType.SpeedBoost:
                    Frames.Add(new TileRef(13, 3, 0));
                    other.MaxVelocity *= Factor;
                    //other.Acceleration = other.Acceleration * Factor; // Immediately gain speed
                    other.Deceleration *= Factor;
                    other.turnSpeed *= (Factor + (Factor / 2));
                    State = PowerUpStatus.ExecuteOnce;
                    break;
                case PowerUpType.ExtraDamage:
                    Frames.Add(new TileRef(13, 0, 0));
                    if (newRound != null)
                        otherTurret.Bullet = newRound;
                    otherTurret.Bullet.sentryDamageRate *= (int)Factor;
                    otherTurret.ShellSound = newRound.shellSoundAlt;
                    State = PowerUpStatus.ExecuteOnce;
                    break;
                case PowerUpType.Camouflage:
                    Frames.Add(new TileRef(13, 4, 0));

                    other.Alpha = 0.5f;
                    otherTurret.Alpha = 0.5f;
                    
                    foreach (SentryTurret turret in sentryTurrets)
                    {
                        if (turret != null)
                        {
                            turret.DetectRadius = Amount * (int)Factor;
                        }
                    }

                    List<Sentry> otherTanks = (List<Sentry>)Game.Services.GetService(typeof(List<Sentry>));
                    foreach (Sentry tank in otherTanks)
                    {
                        if (tank != null)
                            tank.StayVisible = true;
                    }

                    State = PowerUpStatus.ExecuteOnce;
                    break;
            }
        }

        private void UpdateHUD(GameTime gametime)
        {
            if (attachToHUD)
            {
                PixelPosition = new Vector2(
                    (Helper.graphicsDevice.Viewport.Bounds.Width / 2 - 450) +
                    (FrameWidth * (int)Type),
                    (Helper.graphicsDevice.Viewport.Bounds.Height - 64)) +
                    Camera.CamPos;

                if (Type == PowerUpType.Camouflage)
                {
                    Duration += (float)gametime.ElapsedGameTime.TotalSeconds;
                    CheckCamoState(gametime);
                }
            }
        }

        private void RunToPlayer(TilePlayer player)
        {
            distance = Math.Abs(Vector2.Distance(this.CentrePos, player.CentrePos));

            if (distance <= RUN_RADIUS)
                PixelPosition = Vector2.Lerp(
                    this.PixelPosition,
                    player.PixelPosition,
                    RUN_SPEED);
        }

        private void DisplayDurationBar(GameTime gameTime)
        {
            Health = (int)coolDownTime * ((int)coolDownTime);

            if (Health < 100)
            {
                durationBar.Alpha += ALPHA_SPD;
            }
            else
                durationBar.Alpha -= ALPHA_SPD;
        }

        private void CheckCamoState(GameTime gameTime)
        {
            DisplayDurationBar(gameTime);

            if (InputEngine.IsKeyPressed(Keys.C) || InputEngine.IsButtonPressed(Buttons.X)
                && State == PowerUpStatus.Depleted)
            {
                if (coolDownTime >= MAX_COOLDOWN)
                {
                    State = PowerUpStatus.Activated;
                    Duration = 0;
                    coolDownTime = 0;
                    camoSndInst.Play();
                }
            }
        }

        public override void Update(GameTime gametime)
        {
            if (Helper.CurrentGameStatus == GameStatus.PLAYING)
            {
                OrbLight.Position = (CentrePos - Camera.CamPos);

                TilePlayer player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));
                TilePlayerTurret playerTurret = (TilePlayerTurret)Game.Services.GetService(typeof(TilePlayerTurret));
                List<SentryTurret> sentryTurrets = (List<SentryTurret>)Game.Services.GetService(typeof(List<SentryTurret>));

                if (player != null && playerTurret != null)
                {
                    if (State == PowerUpStatus.Deactivated && CollisionDetect(player))
                    {
                        Activate();
                    }

                    if (State == PowerUpStatus.Activated || State == PowerUpStatus.ExecuteOnce)
                    {
                        // Comment out to make effects last infinitely.
                        //Duration += (float)gametime.ElapsedGameTime.TotalSeconds;

                        attachToHUD = true;

                        if (State != PowerUpStatus.ExecuteOnce)
                        {
                            Affect(gametime, player, playerTurret, sentryTurrets);
                        }

                        if (Duration >= MaxLifeTime)
                        {
                            State = PowerUpStatus.Depleted;
                            Deactivate(player, playerTurret, sentryTurrets);
                        }
                    }

                    RunToPlayer(player);

                    // Cool Down Timer
                    if (State == PowerUpStatus.Depleted && coolDownTime <= MAX_COOLDOWN)
                    {
                        coolDownTime += (float)gametime.ElapsedGameTime.TotalSeconds;
                    }

                    UpdateHUD(gametime);
                }

                base.Update(gametime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            TilePlayer player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));

            if (attachToHUD && Helper.CurrentGameStatus == GameStatus.PAUSED || player.Health <= 0)
            {
                // Don't draw over pause screen.
            }
            else
                base.Draw(gameTime);
        }
        #endregion
    }
}
