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
            Deactivated
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
        private int defaultPlayerDamageRate; // Default player damage rate.
        private int defaultSentryDamageRate; // Default sentry damage rate.
        private int defaultRadius; // Player spotted radius.
        #endregion

        #region Constructors
        public PowerUp(Game game, Vector2 position, List<TileRef>sheetRefs, 
            int frameWidth, int frameHeight, float layerDepth,
            float maxLifeTime, PowerUpType type, float multiplier, int amount,
            SoundEffect pickupSnd)
            : base(game, position, sheetRefs, frameWidth, frameHeight, layerDepth)
        {
            Visible = true;
            DrawOrder = 100;
            this.Type = type;
            this.MaxLifeTime = maxLifeTime;
            this.Factor = multiplier;
            this.Amount = amount;

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

        public void Deactivate(TilePlayer other, TilePlayerTurret otherTurret, SentryTurret sentryTurret)
        {
            switch (Type)
            {
                case PowerUpType.Heal:
                    // Do nothing on deactivation
                    break;
                case PowerUpType.DefenseBoost:
                    sentryTurret.Bullet.playerDamageRate = defaultPlayerDamageRate;
                    break;
                case PowerUpType.SpeedBoost:
                    other.MaxVelocity = defaultMaxVelocity;
                    break;
                case PowerUpType.ExtraDamage:
                    otherTurret.Bullet.sentryDamageRate = defaultSentryDamageRate;
                    break;
                case PowerUpType.Camouflage:
                    sentryTurret.DetectRadius = defaultRadius;
                    break;
            }
        }

        private void Affect(TilePlayer other, TilePlayerTurret otherTurret, SentryTurret sentryTurret)
        {
            switch (Type)
            {
                case PowerUpType.Heal:
                    other.Health += Amount * (int)Factor;
                    State = PowerUpStatus.Deactivated;
                    break;
                case PowerUpType.DefenseBoost:
                    sentryTurret.Bullet.playerDamageRate = defaultPlayerDamageRate;
                    sentryTurret.Bullet.playerDamageRate = sentryTurret.Bullet.playerDamageRate / (int)Factor;
                    break;
                case PowerUpType.SpeedBoost:
                    defaultMaxVelocity = other.MaxVelocity;
                    other.MaxVelocity = new Vector2(other.MaxVelocity.X * Factor, other.MaxVelocity.Y * Factor);
                    break;
                case PowerUpType.ExtraDamage:
                    otherTurret.Bullet.sentryDamageRate = Amount * (int)Factor;
                    break;
                case PowerUpType.Camouflage:
                    defaultRadius = sentryTurret.DetectRadius;
                    sentryTurret.DetectRadius = Amount * (int)Factor;
                    break;
            }
        }

        public override void Update(GameTime gametime)
        {
            if (Helper.CurrentGameStatus == GameStatus.PLAYING)
            {
                TilePlayer player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));
                TilePlayerTurret playerTurret = (TilePlayerTurret)Game.Services.GetService(typeof(TilePlayerTurret));
                SentryTurret sentryTurret = (SentryTurret)Game.Services.GetService(typeof(SentryTurret));

                if (player != null && playerTurret != null && sentryTurret != null) return;

                if (State == PowerUpStatus.Deactivated)
                {
                    Deactivate(player, playerTurret, sentryTurret);
                    return; // SKIP !
                }

                if (player != null && CollisionDetect(player))
                {
                    Activate();
                }

                if (State == PowerUpStatus.Activated)
                {
                    Duration += (float)gametime.ElapsedGameTime.TotalSeconds;

                    if (Duration >= MaxLifeTime)
                    {
                        State = PowerUpStatus.Deactivated;
                    }
                }

                Affect(player, playerTurret, sentryTurret);

                base.Update(gametime);
            }
        }

        public override void Draw(GameTime gameTime)
        {


            base.Draw(gameTime);
        }
        #endregion
    }
}
