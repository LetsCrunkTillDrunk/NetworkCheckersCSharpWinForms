using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCheckersWinForms
{
    //Класс доски и клеток. По сути, только инициализирует список шашек в начале игры и предоставляет пару методов для взаимодействия
    //со списком, по типу проверки на наличие, добавление и удаление шашки
    public class Cell
    {
        private bool isActive = true;
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

    }
    [Serializable]
    public class Board
    {

        private Cell[,] board;

        private List<Draught> draughts;
        public List<Draught> Draughts
        {
            get { return draughts; }
            set { draughts = value; }
        }

        public Cell[,] Cells
        {
            get { return board; }
        }

        public Board()
        {
            CreateBoard();
        }

        private void CreateBoard()
        {
            draughts = new List<Draught>();
            board = new Cell[8, 8];

            int counter = 0;

            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (counter % (board.GetLength(0) + 1) == 0)
                    {
                        counter++;
                    }

                    board[i, j] = new Cell();

                    if (counter % 2 != 0)
                    {
                        board[i, j].IsActive = false;
                    }
                    counter++;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i % 2 == 0 && j % 2 != 0) ||
                        (i % 2 != 0 && j % 2 == 0))
                    {
                        draughts.Add(new Draught(false, new Coord(i, j)) { IsSpecial = false});
                    }
                }
            }
            for (int i = 5; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i % 2 == 0 && j % 2 != 0) ||
                        (i % 2 != 0 && j % 2 == 0))
                    {
                        draughts.Add(new Draught(true, new Coord(i, j)) { IsSpecial = false});
                    }
                }
            }
        }
        public bool CheckRange(Coord c)
        {
            return c.X >= 0
                && c.X < 8
                && c.Y >= 0
                && c.Y < 8;
        }

        public void ResetBoard()
        {
            CreateBoard();
        }

        public Draught GetDraught(Coord c)
        {
            return draughts.Find(d => d.Coordinate.X == c.X && d.Coordinate.Y == c.Y);
        }
        public void DeleteDraught(Draught d)
        {
            int index = draughts.FindIndex(i => i.Coordinate.X == d.Coordinate.X&& i.Coordinate.Y==d.Coordinate.Y);
            draughts.RemoveAt(index);
        }

        public void AddDraught(Draught d)
        {
            draughts.Add(d);
        }
    }
}
