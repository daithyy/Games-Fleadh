using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tiling
{
    public class Tile
    {
        int _tileWidth;
        int _tileHeight;
        int scale = 1;
        int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        string _tileName;

        bool _passable;

        public bool Passable
        {
            get { return _passable; }
            set { _passable = value; }
        }

        public string TileName
        {
            get { return _tileName; }
            set { _tileName = value; }
        }
        int _x;

        public int X
        {
            get { return _x; }
            set { _x = value; }
        }
        int _y;

        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public int TileWidth
        {
            get
            {
                return _tileWidth;
            }

            set
            {
                _tileWidth = value;
            }
        }

        public int TileHeight
        {
            get
            {
                return _tileHeight;
            }

            set
            {
                _tileHeight = value;
            }
        }


        // The tiles reference in the Tile sheet for drawing puposes
        public TileRef TileRef
        {
            get
            {
                return _tileRef;
            }

            set
            {
                _tileRef = value;
            }
        }

        private TileRef _tileRef;

    }
}
