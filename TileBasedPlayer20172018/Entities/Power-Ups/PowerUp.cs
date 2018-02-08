using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

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
        /// <para>AMOUNT defines a value you wish to give/take away.</para>
        /// <para>FACTOR multiplies this AMOUNT.</para>
        /// 
        /// <para>See below.</para>
        /// <para>1.  Heal</para>
        ///     Gives your AMOUNT x your FACTOR.
        /// <para>2.  Defense Boost</para>
        ///     Takes CURRENT player damage rate / FACTOR.
        /// <para>3.  Speed Boost</para>
        ///     Takes CURRENT player speed x FACTOR.
        /// <para>4.  Extra Damage</para>
        ///     Inflicts your entered AMOUNT x FACTOR.
        /// <para>5.  Camouflage</para>
        ///     Takes AMOUNT x FACTOR as sentry detection radius.
        ///     Leave this at 0 for no detection!
        /// </summary>
        public enum PowerUpType
        {
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

        private PowerUpType Type;
        public PowerUpStatus State;

        private float Duration = 0;
        public float Factor = 0; // This factor will act as the multiplier for player traits.
        public int Amount = 0;
        public float MaxLifeTime;

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
        }
        #endregion

        #region Methods
        public void Activate()
        {
            if (State == PowerUpStatus.Deactivated)
            {
                pickupSndInst.Play();
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

        private void Affect(TilePlayer other, TilePlayerTurret otherTurret, List<SentryTurret> sentryTurrets)
        {
            switch (Type)
            {
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
                    // Accelerate slowly, gain more speed and stop your tracks faster!
                    other.MaxVelocity *= Factor;
                    //other.Acceleration = other.Acceleration * Factor;
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
                            Affect(player, playerTurret, sentryTurrets);

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
