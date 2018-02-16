using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Pathfinding_Demo.Engine.AI
{
    /// <summary>
    /// This class is responsible for running the A* algorithm in a new thread.
    /// This class will be called by AstarManager where AstarManager will create a new thread in a ThreadPool which this class will run on.
    /// After this class finds the path the thread will be destroyed and the results will be stored in a ConcurrentQueue in the AstartManager.
    /// </summary>
    class AstarThreadWorker
    {
        /// <summary>
        /// A* class.
        /// </summary>
        public Astar astar;
        /// <summary>
        /// ID number for this worker thread so you can get the results back.
        /// </summary>
        public string WorkerIDNumber;

        /// <summary>
        /// This function will run the astar algorithem and tries to find the shortest path.
        /// </summary>
        /// <param name="StartingNode">The starting position in (Array coordinates) of the search path.</param>
        /// <param name="TargetNode">The target or destination position in (Array coordinates) where the search for the path will end at.</param>
        /// <param name="map">Map class.</param>
        /// <param name="DisableDiagonalPathfinding">If true, the A* algorithm will not search the path in diagonal direction.</param>
        /// <param name="WorkerIDNumber">ID number for this worker thread so you can get the results back.</param>
        public AstarThreadWorker(Node StartingNode, Node TargetNode, Game game, int[,] map, bool DisableDiagonalPathfinding, string WorkerIDNumber)
        {
            if (StartingNode.Position.X > map.GetLength(1) || StartingNode.Position.Y > map.GetLength(0))
                throw new Exception("StartingNode size cannot be bigger than map array size. Please make sure the StartingNode position is in array coordinates not pixel coordinates.");

            if (TargetNode.Position.X > map.GetLength(1) || TargetNode.Position.Y > map.GetLength(0))
                throw new Exception("TargetNode size cannot be bigger than map array size. Please make sure the TargetNode position is in array coordinates not pixel coordinates.");

            this.WorkerIDNumber = WorkerIDNumber;
            astar = new Astar(StartingNode, TargetNode, game, map, DisableDiagonalPathfinding);
            astar.FindPath(game);
        }
    }
}