using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Tiler;
using AnimatedSprite;

namespace Pathfinding_Demo.Engine.AI
{
    class WayPoint
    {
        public int WayPointIndex;
        public bool ReachedDestination; // This is NOT updated !
        public static int freezeRadius = 200; // Stay outside this radius from target

        public void MoveTo(GameTime gameTime, Sentry tank, List<Vector2> DestinationWaypoint)
        {
            if (DestinationWaypoint.Count > 0)
            {
                if (!ReachedDestination)
                {
                    #region Get distance to Target
                    float Distance = Vector2.Distance(tank.PixelPosition, DestinationWaypoint[WayPointIndex]);
                    Vector2 Direction = DestinationWaypoint[WayPointIndex] - tank.PixelPosition;
                    Direction.Normalize();
                    #endregion

                    #region Get distance away from Target
                    float DistanceAway = Vector2.Distance(-tank.PixelPosition, -DestinationWaypoint[WayPointIndex]);
                    Vector2 DirectionAway = -DestinationWaypoint[WayPointIndex] + tank.PixelPosition;
                    DirectionAway.Normalize();
                    #endregion

                    switch (tank.SentryState)
                    {
                        case Sentry.State.Patrol:
                            #region Move to Exact Location
                            if (Distance > Direction.Length())
                            {
                                tank.angleOfRotation = RotatingSprite.TurnToFace(
                                    tank.PixelPosition,
                                    (tank.PixelPosition + Direction),
                                    tank.angleOfRotation, tank.turnSpeed);

                                tank.Velocity += tank.Acceleration;
                            }
                            else
                            {
                                if (WayPointIndex >= DestinationWaypoint.Count - 1)
                                {
                                    tank.Velocity = Vector2.Zero;
                                    tank.PixelPosition += Direction;
                                    tank.CanMove = false;
                                    tank.DestinationReached = true;
                                    tank.SentryState = Sentry.State.Idle;
                                    ReachedDestination = true;
                                }
                                else
                                    WayPointIndex++;
                            }
                            #endregion
                            break;
                        case Sentry.State.Attack:
                            #region Move around outside Detection Radius
                            if (Distance > Direction.Length())
                            {
                                tank.angleOfRotation = RotatingSprite.TurnToFace(
                                    tank.PixelPosition,
                                    (tank.PixelPosition + Direction),
                                    tank.angleOfRotation, tank.turnSpeed);

                                if (Vector2.Distance(tank.PixelPosition, tank.Target) > freezeRadius)
                                    tank.Velocity += tank.Acceleration;
                                else
                                {
                                    if (tank.Velocity.X < Vector2.Zero.X)
                                    {
                                        tank.Velocity += tank.Deceleration;
                                    }
                                    else if (tank.Velocity.X > Vector2.Zero.X)
                                    {
                                        tank.Velocity -= tank.Deceleration;
                                    }

                                    if (tank.Velocity.Y < Vector2.Zero.Y)
                                    {
                                        tank.Velocity += tank.Deceleration;
                                    }
                                    else if (tank.Velocity.Y > Vector2.Zero.Y)
                                    {
                                        tank.Velocity -= tank.Deceleration;
                                    }
                                    //tank.Velocity -= tank.Deceleration * 4; // Tank moves back when you advance!
                                }
                            }
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
                            #endregion
                            break;
                        case Sentry.State.Flee:
                            #region Move away from Target
                            if (DistanceAway > DirectionAway.Length())
                            {
                                tank.angleOfRotation = RotatingSprite.TurnToFace(
                                    tank.PixelPosition,
                                    (tank.PixelPosition + DirectionAway),
                                    tank.angleOfRotation, tank.turnSpeed);

                                if (Vector2.Distance(tank.PixelPosition, tank.Target) > 0)
                                    tank.Velocity += tank.Acceleration;
                            }
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
                            #endregion
                            break;
                    }
                }
            }
        }
    }
}