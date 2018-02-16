using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Pathfinding_Demo.Engine.AI
{
    class Node
    {
        #region Node class variables
        /// <summary>
        /// Node Parent.
        /// </summary>
        internal Node Parent;

        /// <summary>
        /// Node position.
        /// </summary>
        internal Vector2 Position;

        /// <summary>
        /// Is this node position is diagonal?
        /// </summary>
        internal bool Diagonal;

        /// <summary>
        /// Can you move through this node?
        /// </summary>
        internal bool Passable = true;

        /// <summary>
        /// Node H cost.
        /// </summary>
        internal int H_Value;

        /// <summary>
        /// Node G cost.
        /// </summary>
        internal int G_Vaule;

        /// <summary>
        /// Node F cost.
        /// </summary>
        internal int F_Vaule;
        #endregion

        /// <summary>
        /// Node constractor.
        /// </summary>
        /// <param name="Position">Node position.</param>
        public Node(Vector2 Position)
        {
            this.Position = Position;
        }
    }
}