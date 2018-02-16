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
        public float speed = 0.1f;

        AstarThreadWorker astarThreadWorkerTemp, astarThreadWorker;
        List<Vector2> WayPointsList;

        WayPoint wayPoint;
        #endregion

        #region Constructor
        public Sentry(Game game, Vector2 startPosition,
            List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth,
            string nameIn, SoundEffect tankHumSound, SoundEffect tankTrackSound, float angle)
                : base(game, startPosition, sheetRefs, frameWidth, frameHeight, layerDepth,
                      tankHumSound, tankTrackSound)
        {
            Health = 100;
            Name = nameIn;
            this.angleOfRotation = angle;

            WayPointsList = new List<Vector2>();

            wayPoint = new WayPoint();

            OrbLight.Scale = new Vector2(120);
            OrbLight.Color = Color.HotPink;

            Alpha = 1f;
        }
        #endregion

        #region Methods
        void Astar(GameTime gameTime, SimpleTileLayer layer, string UnitID, List<Sentry> Units)
        {
            if (InputEngine.IsMouseRightClick())
            {
                astarThreadWorker = null;
                AstarManager.AddNewThreadWorker(
                    new Node(new Vector2((int)PixelPosition.X / FrameWidth, (int)PixelPosition.Y / FrameHeight)), 
                    new Node(new Vector2(
                        (int)(InputEngine.MousePosition.X + Camera.CamPos.X) / FrameWidth, 
                        (int)(InputEngine.MousePosition.Y + Camera.CamPos.Y) / FrameHeight)), Game, layer, false, UnitID);
            }

            AstarManager.AstarThreadWorkerResults.TryPeek(out astarThreadWorkerTemp);

            if (astarThreadWorkerTemp != null)
                if (astarThreadWorkerTemp.WorkerIDNumber == UnitID)
                {
                    AstarManager.AstarThreadWorkerResults.TryDequeue(out astarThreadWorker);

                    if (astarThreadWorker != null)
                    {
                        wayPoint = new WayPoint();

                        WayPointsList = astarThreadWorker.astar.GetFinalPath();

                        for (int i = 0; i < WayPointsList.Count; i++)
                            WayPointsList[i] = new Vector2(WayPointsList[i].X * FrameWidth, WayPointsList[i].Y * FrameHeight);
                    }
                }

            if (WayPointsList.Count > 0)
            {
                List<Sentry> Sentries = (List<Sentry>)Game.Services.GetService(typeof(List<Sentry>));
                Avoidance(gameTime, Sentries, UnitID);
                wayPoint.MoveTo(gameTime, this, WayPointsList, speed);
            }
        }

        void Avoidance(GameTime gameTime, List<Sentry> Units, string UnitID)
        {
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
                        PixelPosition -= OppositeDirection * (float)(speed * gameTime.ElapsedGameTime.TotalMilliseconds);
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
            SimpleTileLayer tileMap = (SimpleTileLayer)Game.Services.GetService(typeof(SimpleTileLayer));
            List<Sentry> Sentries = (List<Sentry>)Game.Services.GetService(typeof(List<Sentry>));

            #region Handle Movement Logic
            Astar(gameTime, tileMap, Name, Sentries);
            #endregion

            //if (IsSpotted())
            //{
            //    Alpha += fadeAmount;
            //    OrbLight.Enabled = true;
            //}
            //else
            //{
            //    if (Alpha > 0)
            //        Alpha -= fadeAmount;
            //    OrbLight.Enabled = false;
            //}

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
