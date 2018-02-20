using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Penumbra;

using AnimatedSprite;
using Tiling;
using Helpers;
using Tiler;

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
            Regen,
            Heal,
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

        private float Duration = 0;
        public float Factor = 0; // This factor will act as the multiplier for player traits.
        public int Amount = 0;
        public float MaxLifeTime;
        public float RegenTimer;

        private SoundEffect pickupSnd;
        private SoundEffectInstance pickupSndInst;

        // Store important variables.
        private Vector2 defaultMaxVelocity; // Default player speed.
        private Vector2 defaultPlayerAcceleration;
        private Vector2 defaultPlayerDeceleration;
        private int defaultPlayerDamageRate; // Default player damage rate.
        private int defaultSentryDamageRate; // Default sentry damage rate.
        private int defaultRadius; // Player spotted radius.
        #endregion

        #region Constructors
        public PowerUp(Game game, Vector2 position, List<TileRef>sheetRefs, 
            int frameWidth, int frameHeight, float layerDepth,
            float maxLifeTime, PowerUpType type, int amount, float multiplier,
            SoundEffect pickupSnd)
            : base(game, position, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            Visible = true;
            DrawOrder = 100;
            this.Type = type;
            this.MaxLifeTime = maxLifeTime;
            this.Factor = multiplier;
            this.Amount = amount;
            this.State = PowerUpStatus.Deactivated;

            #region Get Default Values
            TilePlayer player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));
            TilePlayerTurret playerTurret = (TilePlayerTurret)Game.Services.GetService(typeof(TilePlayerTurret));
            List<SentryTurret> sentryTurrets = (List<SentryTurret>)Game.Services.GetService(typeof(List<SentryTurret>));

            if (player != null)
            {
                defaultMaxVelocity = player.MaxVelocity;
                defaultPlayerAcceleration = player.Acceleration;
                defaultPlayerDeceleration = player.Deceleration;
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

            if (playerTurret != null) defaultSentryDamageRate = playerTurret.Bullet.sentryDamageRate;
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
        }
        #endregion

        #region Methods
        public void Activate()
        {
            if (State == PowerUpStatus.Deactivated)
            {
                pickupSndInst.Play();
                State = PowerUpStatus.Activated;
                OrbLight.Enabled = false;
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
                    break;
                case PowerUpType.ExtraDamage:
                    otherTurret.Bullet.sentryDamageRate = defaultSentryDamageRate;
                    break;
                case PowerUpType.Camouflage:
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
                case PowerUpType.Regen:
                    Duration = 0;
                    RegenTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (other.Health < 100 && other.Health > 0 && RegenTimer > 1)
                    {
                        other.Health += Amount * (int)Factor;
                        RegenTimer = 0;
                    }
                    break;
                case PowerUpType.Heal:
                    other.Health += Amount * (int)Factor;
                    State = PowerUpStatus.Depleted;
                    break;
                case PowerUpType.DefenseBoost:
                    foreach (SentryTurret turret in sentryTurrets)
                    {
                        if (turret != null)
                            turret.Bullet.playerDamageRate /= (int)Factor;
                    }
                    State = PowerUpStatus.ExecuteOnce;
                    break;
                case PowerUpType.SpeedBoost:
                    other.MaxVelocity *= Factor;
                    //other.Acceleration = other.Acceleration * Factor; // Immediately gain speed
                    other.Deceleration *= Factor;
                    State = PowerUpStatus.ExecuteOnce;
                    break;
                case PowerUpType.ExtraDamage:
                    otherTurret.Bullet.sentryDamageRate *= (int)Factor;
                    State = PowerUpStatus.ExecuteOnce;
                    break;
                case PowerUpType.Camouflage:
                    foreach (SentryTurret turret in sentryTurrets)
                    {
                        if (turret != null)
                            turret.DetectRadius = Amount * (int)Factor;
                    }
                    State = PowerUpStatus.ExecuteOnce;
                    break;
            }
        }

        public override void Update(GameTime gametime)
        {
            if (Helper.CurrentGameStatus == GameStatus.PLAYING)
            {
                OrbLight.Position = CentrePos - CameraNS.Camera.CamPos;

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
                        Duration += (float)gametime.ElapsedGameTime.TotalSeconds;

                        if (State != PowerUpStatus.ExecuteOnce)
                            Affect(gametime, player, playerTurret, sentryTurrets);

                        if (Duration >= MaxLifeTime)
                        {
                            State = PowerUpStatus.Depleted;
                            Deactivate(player, playerTurret, sentryTurrets);
                        }
                    }
                }

                base.Update(gametime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (State == PowerUpStatus.Deactivated)
            {
                base.Draw(gameTime);
            }  
        }
        #endregion
    }
}
