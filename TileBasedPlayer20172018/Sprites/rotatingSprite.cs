using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnimatedSprite;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Tiling;
using Helpers;

namespace AnimatedSprite
{
    public class RotatingSprite : AnimateSheetSprite
    {
        int health = 100;
        protected int DamageRate = 1; // The rate at which this object reduces the health of others
        protected int DamageSustainedRate = 1; // The rate at which this object takes on damage
        private Rectangle range;
        protected int tileRangeDistance = 4;
        protected float rotationSpeed = .5f;
        public Rectangle Range
        {
            get
            {
                return new Rectangle((PixelPosition + origin).ToPoint() - new Point(FrameWidth * tileRangeDistance / 2, FrameHeight * tileRangeDistance / 2),
                    new Point(FrameWidth * tileRangeDistance, FrameHeight * tileRangeDistance));
            }

            set
            {
                range = value;
            }
        }
        public HealthBar Hbar
        {
            get
            {
                return hbar;
            }

            set
            {
                hbar = value;
            }
        }
        public int Health
        {
            get
            {
                return health;
            }

            set
            {
                health = value;
            }
        }
        HealthBar hbar;

        public RotatingSprite(Game game, Vector2 userPosition, List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth)
            : base(game, userPosition, sheetRefs, frameWidth, frameHeight, layerDepth)
        {

        }

        public void AddHealthBar(HealthBar h)
        {
            Hbar = h;
        }

        public virtual void Follow(AnimateSheetSprite followed)
        {
            // Only rotate towards the player if he enters the field of View
            if (followed.BoundingRectangle.Intersects(Range))
                angleOfRotation = TurnToFace(followed.PixelPosition, PixelPosition, angleOfRotation, rotationSpeed);

        }

        public void FollowPosition(Vector2 Pos)
        {

            angleOfRotation = TurnToFace(Pos, PixelPosition, angleOfRotation, rotationSpeed);
        }

        protected static float TurnToFace(Vector2 position, Vector2 faceThis,
            float currentAngle, float turnSpeed)
        {
            // The difference in the two points is 
            float x = faceThis.X - position.X;
            float y = faceThis.Y - position.Y;

            // ArcTan calculates the angle of rotation 
            // relative to a point (the gun turret position)
            // in the positive x plane and 
            float desiredAngle = (float)Math.Atan2(y, x);

            float difference = WrapAngle(desiredAngle - currentAngle);

            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            return WrapAngle(currentAngle + difference);
        }

        public override void Update(GameTime gametime)
        {
            if (Hbar != null)
            {
                Hbar.health = Health;
                Hbar.position = PixelPosition - new Vector2((FrameWidth / 2 - 10), (-FrameHeight - 20));
            }

            base.Update(gametime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (Hbar != null)
                Hbar.Draw(gameTime);
            base.Draw(gameTime);
        }

        // Returns the angle expressed in radians between -Pi and Pi.
        // Angle is always positive
        private static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }
    }
}
