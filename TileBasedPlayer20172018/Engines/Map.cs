using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathfinding_Demo.Engine.MapContent.Graphics
{
    class Map
    {
        GraphicsDeviceManager Graphics;
        SpriteBatch spriteBatch;

        public List<Rectangle> AStarCollisionObjects;
        public List<Rectangle> CollisionObjects;

        public Vector2 ArraySize;
        public Vector2 Size;
        public string Name;

        public Map()
        {
            AStarCollisionObjects = new List<Rectangle>();
            CollisionObjects = new List<Rectangle>();
        }

        public void LoadContent()
        {
            AStarCollisionObjects = CollisionObjects;
        }
    }
}