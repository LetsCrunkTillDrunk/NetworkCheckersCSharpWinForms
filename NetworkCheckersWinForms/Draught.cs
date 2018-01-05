using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCheckersWinForms
{
    //Класс представляющий шашку
    [Serializable]
    public class Coord
    {
        private int x;
        private int y;

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void changeCoord(Coord c)
        {
            x = c.X;
            y = c.Y;
        }
    }
    [Serializable]
    public class Draught
    {
        private bool special;
        private bool isWhite;
        private Coord coord;
        public bool IsWhite
        {
            get { return isWhite; }
            set { isWhite = value; }
        }
        public bool IsSpecial
        {
            get { return special; }
            set { special = value; }
        }
        public Coord Coordinate
        {
            get { return coord; }
            set { coord = value; }
        }

        public Draught(bool col, Coord c)
        {
            IsWhite = col;
            coord = c;
        }
    }
}
