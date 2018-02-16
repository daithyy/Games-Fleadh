using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Tiler;

namespace Pathfinding_Demo.Engine.AI
{
    class WayPoint
    {
        public int WayPointIndex;
        public bool ReachedDestination;

        public void MoveTo(GameTime gameTime, Sentry tank, List<Vector2> DestinationWaypoint, float Speed)
        {
            if (DestinationWaypoint.Count > 0)
            {
                if (!ReachedDestination)
                {
                    float Distance = Vector2.Distance(tank.PixelPosition, DestinationWaypoint[WayPointIndex]);
                    Vector2 Direction = DestinationWaypoint[WayPointIndex] - tank.PixelPosition;
                    Direction.Normalize();

                    if (Distance > Direction.Length())
                        tank.PixelPosition += Direction * (float)(Speed * gameTime.ElapsedGameTime.TotalMilliseconds);
                    else
                    {
                        if (WayPointIndex >= DestinationWaypoint.Count - 1)
                        {
                            tank.PixelPosition += Direction;
                            ReachedDestination = true;
                        }
                        else
                            WayPointIndex++;
                    }
                }
            }
        }
    }
}