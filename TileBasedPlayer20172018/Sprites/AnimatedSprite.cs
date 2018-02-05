using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tiling;
using CameraNS;

namespace AnimatedSprite
{
    public class AnimateSheetSprite : DrawableGameComponent
    {
        private bool visible = true;
        protected Vector2 origin;
        public float angleOfRotation;
        protected float spriteDepth = 6f;
        private float scale = 1f;
        private Vector2 _pixelPosition;
        private Rectangle boundingRectangle;

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        private Vector2 _tileposition;
        public Vector2 TilePosition
        {
            get
            {
                return _tileposition;
            }

            set
            {
                _tileposition = value;
            }
        }

        // The number of frames in the sprite sheet
        // The current fram in the animation
        // The time between frames
        int mililsecondsBetweenFrames = 100;
        float timer = 0f;

        // The width and height of our texture
        public int FrameWidth = 0;
        public int FrameHeight = 0;

        // The source of our image within the sprite sheet to draw
        Rectangle sourceRectangle;

        public Rectangle SourceRectangle
        {
            get { return sourceRectangle; }
            set { sourceRectangle = value; }
        }

        public Vector2 PixelPosition
        {
            get
            {
                //return new Vector2(TilePosition.X * FrameWidth, 
                //                    TilePosition.Y * FrameHeight) ;
                return _pixelPosition;
            }

            set { _pixelPosition = value; }

        }

        public int CurrentFrame
        {
            get
            {
                return _currentFrame;
            }

            set
            {
                _currentFrame = value;
            }
        }

        public float Scale
        {
            get
            {
                return scale;
            }

            set
            {
                scale = value;
            }
        }

        public Rectangle BoundingRectangle
        {
            get
            {
                return new Rectangle(PixelPosition.ToPoint(),
                    new Point(FrameWidth, FrameHeight));
            }

            set
            {
                boundingRectangle = value;
            }
        }

        protected List<TileRef> Frames = new List<TileRef>();
        private int _currentFrame;

        public AnimateSheetSprite(Game g, Vector2 userPosition, List<TileRef> sheetRefs, int frameWidth, int frameHeight, float layerDepth) : base(g)
        {
            spriteDepth = layerDepth;
            _pixelPosition = userPosition;
            visible = true;
            FrameHeight = frameHeight;
            FrameWidth = frameWidth;
            Frames = sheetRefs;
            // added to allow sprites to rotate
            origin = new Vector2(FrameWidth / 2, FrameHeight / 2);
            angleOfRotation = 0;
            CurrentFrame = 0;
            g.Components.Add(this);
        }

        public override void Update(GameTime gameTime)
        {
            if (Visible)
            {
                timer += (float)gameTime.ElapsedGameTime.Milliseconds;

                //if the timer is greater then the time between frames, then animate
                if (timer > mililsecondsBetweenFrames)
                {
                    _currentFrame++;
                    //if we have exceed the number of frames
                    if (_currentFrame > Frames.Count - 1)
                    {
                        _currentFrame = 0;
                    }
                    //reset our timer
                    timer = 0f;
                }
                //set the source to be the current frame in our animation
                sourceRectangle = new Rectangle(Frames[CurrentFrame]._sheetPosX * FrameWidth,
                        Frames[CurrentFrame]._sheetPosY * FrameHeight,
                        FrameWidth, FrameHeight);
            }
            else
            {
                _currentFrame = 0;
            }
        }

        public bool collisionDetect(AnimateSheetSprite other)
        {
            Rectangle myBound = new Rectangle((int)(this.PixelPosition.X + FrameWidth / 2), (int)(this.PixelPosition.Y + FrameHeight / 2), 12, 2);
            Rectangle otherBound = new Rectangle((int)other.PixelPosition.X, (int)other.PixelPosition.Y, other.FrameWidth, other.FrameHeight);

            return myBound.Intersects(otherBound);
        }

        public bool TileCollision(Tile other)
        {
            Rectangle myBound = new Rectangle((int)this.PixelPosition.X, (int)this.PixelPosition.Y, this.FrameWidth, this.FrameHeight);
            Rectangle otherBound = new Rectangle((int)other.X * other.TileWidth, (int)other.Y * other.TileHeight, other.TileWidth, other.TileHeight);

            return myBound.Intersects(otherBound);
        }

        public bool InTile(Tile other)
        {
            Rectangle myBound = new Rectangle((int)this.PixelPosition.X, (int)this.PixelPosition.Y, this.FrameWidth, this.FrameHeight);
            Rectangle otherBound = new Rectangle((int)other.X * other.TileWidth, (int)other.Y * other.TileHeight, other.TileWidth, other.TileHeight);

            return otherBound.Contains(myBound);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = Game.Services.GetService<SpriteBatch>();

            // This means you can only use one spritesheet in the entire game, to save on resources
            Texture2D SpriteSheet = Game.Services.GetService<Texture2D>();

            if (visible)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate,
                        BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera.CurrentCameraTranslation);
                spriteBatch.Draw(SpriteSheet,
                    PixelPosition + origin,
                    sourceRectangle,
                    Color.White, angleOfRotation, origin,
                    Scale, SpriteEffects.None, spriteDepth);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
