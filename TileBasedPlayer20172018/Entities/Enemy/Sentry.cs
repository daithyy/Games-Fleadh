using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using Pathfinding_Demo.Engine.AI;

using InputManager;
using Tiling;
using CameraNS;
using AnimatedSprite;
using Helpers;

namespace Tiler
{
    class Sentry : Tank
    {
        #region Properties
        public string Name;
        public int DetectRadius;
        private const float fadeAmount = 0.05f;
        private float knockBack = 0.1f;
        private float updateTime = 0f;
        private const float UPDATE_TIME_MAX = 0.1f;
        private float patrolTime = 0f;
        private const float PATROL_TIME_MAX = 10f; // 10 seconds
        private float waitTime = 0f;
        private const int WAIT_TIME_MAX = 5; // 5 seconds
        public bool IsDead = false;

        public enum State
        {
            Wait,
            Idle,
            Patrol,
            Attack,
            Flee
        }
        public State SentryState;

        AstarThreadWorker astarThreadWorkerTemp, astarThreadWorker;
        List<Vector2> WayPointsList;
        public Vector2 Target;
        WayPoint wayPoint;
        #endregion

        #region Constructor
        public Sentry(Game game, Vector2 startPosition,
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth,
            string nameIn, SoundEffect tankHumSound, SoundEffect tankTrackSound, float angle)
                : base(game, startPosition, sheetRefs, frameWidth, frameHeight, layerDepth,
                      tankHumSound, tankTrackSound)
        {
            Name = nameIn;
            SentryState = State.Wait;
            this.angleOfRotation = angle;
            MaxVelocity /= 2;

            WayPointsList = new List<Vector2>();

            wayPoint = new WayPoint();

            OrbLight.Scale = new Vector2(120);
            OrbLight.Color = Color.HotPink;

            Alpha = 0f;
        }
        #endregion

        #region Methods
        private void SlowDown()
        {
            if (Velocity.X < 0)
            {
                Velocity += Deceleration;
            }
            else if (Velocity.X > 0)
            {
                Velocity -= Deceleration;
            }

            if (Velocity.Y < 0)
            {
                Velocity += Deceleration;
            }
            else if (Velocity.Y > 0)
            {
                Velocity -= Deceleration;
            }
        }

        private Vector2 ChooseRandomLocation()
        {
            List<Tile> groundTiles = SimpleTileLayer.GetNamedTiles("ground");

            return new Vector2(
                groundTiles[Camera.random.Next(0, groundTiles.Count)].X * FrameWidth,
                groundTiles[Camera.random.Next(0, groundTiles.Count)].Y * FrameHeight);
        }

        private void React()
        {
            if (Health > HealthBar.WarningLevel)
            {
                SentryState = State.Attack;
                WayPoint.freezeRadius = 100; // Get closer
            }
            else if (Health > HealthBar.CriticalLevel && Health <= HealthBar.WarningLevel)
            {
                WayPoint.freezeRadius = 200; // Keep distance
            }
            else if (Health > 0 && Health <= HealthBar.CriticalLevel)
            {
                SentryState = State.Flee; // Escape
            }
        }

        private void ChooseState(GameTime gameTime)
        {
            switch (SentryState)
            {
                case State.Wait:
                    SlowDown();
                    break;
                case State.Idle:
                    if (waitTime > Camera.random.Next(1, WAIT_TIME_MAX))
                    {
                        SentryState = State.Patrol;
                        waitTime = 0f;
                    }
                    else
                        waitTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    break;
                case State.Patrol:
                    if (patrolTime > PATROL_TIME_MAX)
                    {
                        Target = ChooseRandomLocation();
                        patrolTime = 0f;
                    }
                    else
                        patrolTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    HandleMovement(gameTime);
                    break;
                case State.Attack:
                    HandleMovement(gameTime);
                    break;
                case State.Flee:
                    HandleMovement(gameTime);
                    break;
            }
        }

        private void HandleMovement(GameTime gameTime)
        {
            SimpleTileLayer tileMap = (SimpleTileLayer)Game.Services.GetService(typeof(SimpleTileLayer));
            List<Sentry> Sentries = (List<Sentry>)Game.Services.GetService(typeof(List<Sentry>));

            if (!IsDead)
            {
                PathFinding(gameTime, tileMap, Name, Sentries);
            }
            else
            {
                if (Velocity.X > 0 && Velocity.Y > 0)
                    Velocity -= Deceleration;
                else
                    Velocity = Vector2.Zero;
            }
        }

        private void PathFinding(GameTime gameTime, SimpleTileLayer layer, string UnitID, List<Sentry> Units)
        {
            #region Calculate Location
            if (updateTime > UPDATE_TIME_MAX) // Check every millisecond.
            {
                astarThreadWorker = null;
                AstarManager.AddNewThreadWorker(
                    new Node(new Vector2(
                        (int)(PixelPosition.X / FrameWidth), 
                        (int)(PixelPosition.Y / FrameHeight))), 
                    new Node(new Vector2(
                        (int)(Target.X) / FrameWidth, 
                        (int)(Target.Y) / FrameHeight)), 
                    Game, layer, false, UnitID);
                updateTime = 0f;
            }
            else
            {
                updateTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            #endregion

            AstarManager.AstarThreadWorkerResults.TryPeek(out astarThreadWorkerTemp);

            #region Add Location to WayPoints
            if (astarThreadWorkerTemp != null)
                if (astarThreadWorkerTemp.WorkerIDNumber == UnitID)
                {
                    AstarManager.AstarThreadWorkerResults.TryDequeue(out astarThreadWorker);

                    if (astarThreadWorker != null)
                    {
                        wayPoint = new WayPoint();

                        WayPointsList = astarThreadWorker.astar.GetFinalPath();

                        for (int i = 0; i < WayPointsList.Count; i++)
                            WayPointsList[i] = new Vector2(
                                WayPointsList[i].X * FrameWidth, 
                                WayPointsList[i].Y * FrameHeight);
                    }
                }
            #endregion

            #region Avoid Obstacles and Move to Target
            if (WayPointsList.Count > 0)
            {
                {
                    Avoidance(gameTime, UnitID);
                    wayPoint.MoveTo(gameTime, this, WayPointsList);
                }
            }
            #endregion
        }

        private void Avoidance(GameTime gameTime, string UnitID)
        {
            List<Sentry> Units = (List<Sentry>)Game.Services.GetService(typeof(List<Sentry>));

            for (int i = 0; i < Units.Count; i++)
            {
                if (Units[i].BoundingRectangle.Intersects(BoundingRectangle))
                {
                    float Distance1 = Vector2.Distance(PixelPosition, WayPointsList[WayPointsList.Count - 1]);
                    float Distance2 = Vector2.Distance(Units[i].PixelPosition, WayPointsList[WayPointsList.Count - 1]);

                    if (Distance1 > Distance2)
                    {
                        Vector2 OppositeDirection = Units[i].PixelPosition - PixelPosition;
                        OppositeDirection.Normalize();
                        PixelPosition -= OppositeDirection * (float)(knockBack * gameTime.ElapsedGameTime.TotalMilliseconds);
                    }
                }
            }
        }

        private bool IsSpotted()
        {
            TilePlayer player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));

            float distance = Math.Abs(Vector2.Distance(this.CentrePos, player.CentrePos));

            if (distance <= DetectRadius)
                return true;
            else
                return false;
        }

        public override void Update(GameTime gameTime)
        {
            ChooseState(gameTime);

            if (IsSpotted())
            {
                TilePlayer player = (TilePlayer)Game.Services.GetService(typeof(TilePlayer));

                Target = player.CentrePos;
                React();

                // Show self
                Alpha += fadeAmount;
                OrbLight.Enabled = true;
            }
            else
            {
                SentryState = State.Wait;

                // Hide
                if (Alpha > 0)
                    Alpha -= fadeAmount;
                OrbLight.Enabled = false;
            }

            Alpha = MathHelper.Clamp(Alpha, 0, 2);

            //PlaySounds();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
        #endregion
    }
}
