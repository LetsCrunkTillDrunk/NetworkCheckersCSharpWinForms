using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace NetworkCheckersWinForms
{
    public partial class Form1 : Form
    {
        private PictureBox[,] box;
        private Board board;
        private List<Coord> possibleMoves = new List<Coord>();
        private List<Coord> possibleKills = new List<Coord>();
        private bool IsPlayWhite = true;
        private bool IsCurrentMove;
        private bool IsKillingTime = false;
        private Draught selectedDraugth = null;
        Network network;

        public Form1()
        {
            InitializeComponent();
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            box = new PictureBox[8, 8];
            board = new Board();

            //создание шахматного поля из PictureBoxов
            bool row = false;
            int cellSize = 50;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    PictureBox pic = new PictureBox();
                    pic.BackColor = row ? Color.BurlyWood : Color.Bisque;
                    pic.Location = new Point(x * (cellSize - 1) + cellSize, y * (cellSize - 1) + cellSize);
                    pic.Size = new Size(cellSize, cellSize);
                    pic.BorderStyle = BorderStyle.FixedSingle;
                    pic.Tag = new Coord(y, x);
                    Draught d = board.GetDraught(new Coord(y, x));

                    if (d == null) pic.Image = Properties.Resources.empty;
                    else if (d.IsWhite) pic.Image = Properties.Resources.white;
                    else pic.Image = Properties.Resources.black;

                    pic.SizeMode = PictureBoxSizeMode.CenterImage;
                    pic.MouseClick += Pic_MouseClick;
                    this.Controls.Add(pic);
                    box[y, x] = pic;

                    row = !row;
                }
                row = !row;
            }
        }
        //Основная логика игры. Обработка клика на PictureBox`ax
        private void Pic_MouseClick(object sender, MouseEventArgs e)
        {
            if (!IsCurrentMove) return;
            Coord coord = (sender as PictureBox).Tag as Coord;
            Draught draught = board.GetDraught(coord);
            Draught test;
            if (selectedDraugth != null && (test = board.GetDraught(selectedDraugth.Coordinate)) != null)
            {
                if (possibleMoves.Where(i => i.X == coord.X && i.Y == coord.Y).FirstOrDefault() != null && IsLegitMove(selectedDraugth.Coordinate, coord))
                {
                    foreach (Coord c in possibleKills)
                    {
                        //Логика удаления 'убитой' шашки
                        if (Math.Abs(coord.X - c.X) == 1 && Math.Abs(coord.Y - c.Y) == 1)
                        {
                            box[c.X, c.Y].Image = Properties.Resources.empty;
                            Draught d = board.GetDraught(c);
                            board.DeleteDraught(d);
                            IsKillingTime = true;
                            break;
                        }
                    }
                    possibleKills.Clear();
                    //Логика перемещения шашек
                    box[selectedDraugth.Coordinate.X, selectedDraugth.Coordinate.Y].Image = Properties.Resources.empty;
                    board.DeleteDraught(selectedDraugth);
                    ClearPossibleMoves();

                    if (IsPlayWhite)
                    {
                        if (coord.X == 0 || selectedDraugth.IsSpecial)
                        {
                            box[coord.X, coord.Y].Image = Properties.Resources.white_queen;
                            board.AddDraught(new Draught(true, new Coord(coord.X, coord.Y)) { IsSpecial = true });
                        }
                        else
                        {
                            box[coord.X, coord.Y].Image = Properties.Resources.white;
                            board.AddDraught(new Draught(true, new Coord(coord.X, coord.Y)));
                        }
                    }
                    else
                    {
                        if (coord.X == 7 || selectedDraugth.IsSpecial)
                        {
                            box[coord.X, coord.Y].Image = Properties.Resources.black_queen;
                            board.AddDraught(new Draught(false, new Coord(coord.X, coord.Y)) { IsSpecial = true });
                        }
                        else
                        {
                            box[coord.X, coord.Y].Image = Properties.Resources.black;
                            board.AddDraught(new Draught(false, new Coord(coord.X, coord.Y)));
                        }
                    }
                    if (IsValidKicks() && IsKillingTime)
                    {
                        DrawPossibleMoves();
                        return;
                    }
                    IsKillingTime = false;
                    SendMessage();
                    selectedDraugth = null;
                    return;
                }
            }

            selectedDraugth = draught;

            if (draught == null || draught.IsWhite != IsPlayWhite)
            {
                ClearPossibleMoves();
                return;
            }
            ClearPossibleMoves();
            IsAbleMoves(draught);
            IsValidKicks();
            DrawPossibleMoves();
        }
        //Отрисовка возможных ходов
        private void DrawPossibleMoves()
        {
            foreach (Coord c in possibleMoves)
            {
                if (IsPlayWhite)
                    box[c.X, c.Y].Image = Properties.Resources.border_green;
                else
                    box[c.X, c.Y].Image = Properties.Resources.border_orange;

            }
        }
        private void ClearPossibleMoves()
        {
            if (possibleMoves.Count > 0)
            {
                foreach (Coord c in possibleMoves)
                {
                    box[c.X, c.Y].Image = Properties.Resources.empty;
                }
                possibleMoves.Clear();
            }
        }
        //Дополнительная проверка на валидность хода
        private bool IsLegitMove(Coord selected, Coord mover)
        {
            if (board.GetDraught(selected) != null && !board.GetDraught(selected).IsSpecial)
            {
                if (possibleKills.Count > 0)
                {
                    Coord c = possibleKills.Where(i => Math.Abs(i.X - selected.X) == 1 & Math.Abs(i.Y - selected.Y) == 1 && Math.Abs(i.X - mover.X) == 1 && Math.Abs(i.Y - mover.Y) == 1).FirstOrDefault();
                    return c != null;
                }
                return Math.Abs(selected.X - mover.X) == 1 && Math.Abs(selected.Y - mover.Y) == 1;
            }
            else
            {
                //Здесь должна быть проерка валидности хода дамки, но тут идеи иссякли. По этому, честность ходов дамкой остается на совести игрока
                return true;
            }
        }
        //Поиск возможных ударов. Если есть возможность забрать шашку противника - остальные ходы не допускаются
        private bool IsValidKicks()
        {
            possibleKills.Clear();
            List<Draught> draughts = board.Draughts.Where(i => i.IsWhite == IsPlayWhite).ToList();
            List<Coord> temp = new List<Coord>();
            foreach (Draught d in draughts)
            {
                if (!d.IsSpecial)
                {
                    Coord[] coords = new Coord[4]
                    {
                    new Coord (d.Coordinate.X - 1, d.Coordinate.Y - 1),
                    new Coord (d.Coordinate.X - 1, d.Coordinate.Y + 1),
                    new Coord (d.Coordinate.X + 1, d.Coordinate.Y - 1),
                    new Coord (d.Coordinate.X + 1, d.Coordinate.Y + 1)
                    };
                    foreach (Coord c in coords)
                    {

                        Draught dr = board.GetDraught(c);
                        if (dr == null) continue;
                        else if (!board.CheckRange(c)) continue;

                        else if (dr.IsWhite == IsPlayWhite) continue;
                        else
                        {
                            if (d.Coordinate.X - c.X > 0 && d.Coordinate.Y - c.Y > 0)
                            {
                                Draught draught = board.GetDraught(new Coord(d.Coordinate.X - 2, d.Coordinate.Y - 2));
                                if (draught == null)
                                {
                                    if (board.CheckRange(new Coord(d.Coordinate.X - 2, d.Coordinate.Y - 2)))
                                    {
                                        temp.Add(new Coord(d.Coordinate.X - 2, d.Coordinate.Y - 2));
                                        possibleKills.Add(new Coord(dr.Coordinate.X, dr.Coordinate.Y));
                                    }
                                }
                            }
                            else if (d.Coordinate.X - c.X > 0 && d.Coordinate.Y - c.Y < 0)
                            {
                                Draught draught = board.GetDraught(new Coord(d.Coordinate.X - 2, d.Coordinate.Y + 2));
                                if (draught == null)
                                {
                                    if (board.CheckRange(new Coord(d.Coordinate.X - 2, d.Coordinate.Y + 2)))
                                    {
                                        temp.Add(new Coord(d.Coordinate.X - 2, d.Coordinate.Y + 2));
                                        possibleKills.Add(new Coord(dr.Coordinate.X, dr.Coordinate.Y));
                                    }
                                }
                            }
                            else if (d.Coordinate.X - c.X < 0 && d.Coordinate.Y - c.Y > 0)
                            {
                                Draught draught = board.GetDraught(new Coord(d.Coordinate.X + 2, d.Coordinate.Y - 2));
                                if (draught == null)
                                {
                                    if (board.CheckRange(new Coord(d.Coordinate.X + 2, d.Coordinate.Y - 2)))
                                    {
                                        temp.Add(new Coord(d.Coordinate.X + 2, d.Coordinate.Y - 2));
                                        possibleKills.Add(new Coord(dr.Coordinate.X, dr.Coordinate.Y));
                                    }
                                }
                            }
                            else if (d.Coordinate.X - c.X < 0 && d.Coordinate.Y - c.Y < 0)
                            {
                                Draught draught = board.GetDraught(new Coord(d.Coordinate.X + 2, d.Coordinate.Y + 2));
                                if (draught == null)
                                {
                                    if (board.CheckRange(new Coord(d.Coordinate.X + 2, d.Coordinate.Y + 2)))
                                    {
                                        temp.Add(new Coord(d.Coordinate.X + 2, d.Coordinate.Y + 2));
                                        possibleKills.Add(new Coord(dr.Coordinate.X, dr.Coordinate.Y));
                                    }
                                }
                            }
                        }
                    }
                }

                else
                {
                    int x = d.Coordinate.X - 1;
                    int y = d.Coordinate.Y - 1;
                    while (board.CheckRange(new Coord(x, y)))
                    {
                        Draught dr = board.GetDraught(new Coord(x, y));
                        if (dr != null && dr.IsWhite == IsPlayWhite)
                        {
                            break;
                        }

                        else if (dr != null && dr.IsWhite != IsPlayWhite)
                        {
                            if (d.Coordinate.X - x > 0 && d.Coordinate.Y - y > 0)
                            {
                                Draught draught = board.GetDraught(new Coord(x - 1, y - 1));
                                if (draught == null)
                                {
                                    if (board.CheckRange(new Coord(x - 1, y - 1)))
                                    {
                                        temp.Add(new Coord(x - 1, y - 1));
                                        possibleKills.Add(new Coord(dr.Coordinate.X, dr.Coordinate.Y));
                                        break;
                                    }
                                }
                            }
                        }
                        x--;
                        y--;

                    }
                    x = d.Coordinate.X + 1;
                    y = d.Coordinate.Y + 1;
                    while (board.CheckRange(new Coord(x, y)))
                    {
                        Draught dr = board.GetDraught(new Coord(x, y));
                        if (dr != null && dr.IsWhite == IsPlayWhite)
                        {
                            break;
                        }
                        else if (dr != null && dr.IsWhite != IsPlayWhite)
                        {
                            if (d.Coordinate.X - x < 0 && d.Coordinate.Y - y < 0)
                            {
                                Draught draught = board.GetDraught(new Coord(x + 1, y + 1));
                                if (draught == null)
                                {
                                    if (board.CheckRange(new Coord(x + 1, y + 1)))
                                    {
                                        temp.Add(new Coord(x + 1, y + 1));
                                        possibleKills.Add(new Coord(dr.Coordinate.X, dr.Coordinate.Y));
                                        break;
                                    }
                                }
                            }
                        }
                        x++;
                        y++;

                    }
                    x = d.Coordinate.X - 1;
                    y = d.Coordinate.Y + 1;
                    while (board.CheckRange(new Coord(x, y)))
                    {
                        Draught dr = board.GetDraught(new Coord(x, y));
                        if (dr != null && dr.IsWhite == IsPlayWhite)
                        {
                            break;
                        }
                        else if (dr != null && dr.IsWhite != IsPlayWhite)
                        {
                            if (d.Coordinate.X - x > 0 && d.Coordinate.Y - y < 0)
                            {
                                Draught draught = board.GetDraught(new Coord(x - 1, y + 1));
                                if (draught == null)
                                {
                                    if (board.CheckRange(new Coord(x - 1, y + 1)))
                                    {
                                        temp.Add(new Coord(x - 1, y + 1));
                                        possibleKills.Add(new Coord(dr.Coordinate.X, dr.Coordinate.Y));
                                        break;
                                    }
                                }
                            }
                        }
                        x--;
                        y++;
                    }
                    x = d.Coordinate.X + 1;
                    y = d.Coordinate.Y - 1;
                    while (board.CheckRange(new Coord(x, y)))
                    {
                        Draught dr = board.GetDraught(new Coord(x, y));
                        if (dr != null && dr.IsWhite == IsPlayWhite)
                        {
                            break;
                        }
                        else if (dr != null && dr.IsWhite != IsPlayWhite)
                        {
                            if (d.Coordinate.X - x < 0 && d.Coordinate.Y - y > 0)
                            {
                                Draught draught = board.GetDraught(new Coord(x + 1, y - 1));
                                if (draught == null)
                                {
                                    if (board.CheckRange(new Coord(x + 1, y - 1)))
                                    {
                                        temp.Add(new Coord(x + 1, y - 1));
                                        possibleKills.Add(new Coord(dr.Coordinate.X, dr.Coordinate.Y));
                                        break;
                                    }
                                }
                            }
                        }
                        x++;
                        y--;
                    }
                }
            }
            if (temp.Count > 0)
            {
                possibleMoves = temp;
                return true;
            }
            return false;

        }
        private void IsAbleMoves(Draught d)
        {
            if (!d.IsSpecial)
            {
                Coord[] coords = new Coord[4]
                {
                new Coord (d.Coordinate.X - 1, d.Coordinate.Y - 1),
                new Coord (d.Coordinate.X - 1, d.Coordinate.Y + 1),
                new Coord (d.Coordinate.X + 1, d.Coordinate.Y - 1),
                new Coord (d.Coordinate.X + 1, d.Coordinate.Y + 1)
                };
                for (int i = 0; i < 4; i++)
                {
                    Draught dr = board.GetDraught(coords[i]);
                    if (!board.CheckRange(coords[i]))
                    {
                        continue;
                    }
                    if (dr != null && dr.IsWhite == IsPlayWhite)
                    {
                        continue;
                    }
                    if (dr == null)
                    {

                        if (IsPlayWhite && coords[i].X < d.Coordinate.X)
                            possibleMoves.Add(coords[i]);
                        if (!IsPlayWhite && coords[i].X > d.Coordinate.X)
                            possibleMoves.Add(coords[i]);

                        continue;
                    }
                }
            }
            else
            {
                int x = d.Coordinate.X - 1;
                int y = d.Coordinate.Y - 1;
                while (board.CheckRange(new Coord(x, y)))
                {
                    Draught dr = board.GetDraught(new Coord(x, y));
                    if (dr != null)
                    {
                        break;
                    }
                    else if (dr == null)
                    {
                        possibleMoves.Add(new Coord(x, y));
                    }
                    x--;
                    y--;

                }
                x = d.Coordinate.X + 1;
                y = d.Coordinate.Y + 1;
                while (board.CheckRange(new Coord(x, y)))
                {
                    Draught dr = board.GetDraught(new Coord(x, y));
                    if (dr != null)
                    {
                        break;
                    }
                    else if (dr == null)
                    {
                        possibleMoves.Add(new Coord(x, y));
                    }
                    x++;
                    y++;

                }
                x = d.Coordinate.X - 1;
                y = d.Coordinate.Y + 1;
                while (board.CheckRange(new Coord(x, y)))
                {
                    Draught dr = board.GetDraught(new Coord(x, y));
                    if (dr != null)
                    {
                        break;
                    }
                    else if (dr == null)
                    {
                        possibleMoves.Add(new Coord(x, y));
                    }
                    x--;
                    y++;

                }
                x = d.Coordinate.X + 1;
                y = d.Coordinate.Y - 1;
                while (board.CheckRange(new Coord(x, y)))
                {
                    Draught dr = board.GetDraught(new Coord(x, y));
                    if (dr != null)
                    {
                        break;
                    }
                    else if (dr == null)
                    {
                        possibleMoves.Add(new Coord(x, y));
                    }
                    x++;
                    y--;

                }
            }
        }

        private void GameOver()
        {
            int black = board.Draughts.Where(i => i.IsWhite == false).Count();
            int white = board.Draughts.Where(i => i.IsWhite == true).Count();

            if (white == 0 || black == 0)
            {
                if (IsPlayWhite && white == 0)
                    MessageBox.Show("Вы проиграли");
                else if (IsPlayWhite && white != 0)
                    MessageBox.Show("Вы победили");
                else if (!IsPlayWhite && white == 0)
                    MessageBox.Show("Вы победили");
                else if (!IsPlayWhite && white != 0)
                    MessageBox.Show("Вы проиграли");

                board.ResetBoard();
                if (IsPlayWhite) IsCurrentMove = true;
                else IsCurrentMove = false;
            }

        }

        //Запуск сервера
        private void tsCreateServer_Click(object sender, EventArgs e)
        {
            Connection connect = new Connection();
            DialogResult result;
            StartServerForm serverForm = new StartServerForm(connect);
            result = serverForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                tsCreateServer.Enabled = false;
                tsConnectToServer.Enabled = false;
                network = new NetworkServer(connect.Port);
                network.Start();
                network.Recv += RecieveMessage;
                network.SendMessageNetwork += SendMessageNet;
                label1.Text = "Ожидание подключений";
                IsPlayWhite = true;
                IsCurrentMove = true;
            }

        }
        //Простой метод, который используется для того, чтобы каждый участник получил достоверную информацию о состоянии игры. Конкретно - чей ход
        private void SendMessageNet(string text)
        {
            label1.Text = text;
        }
        //Сообщения между клиентом и сервером осуществляется отправкой списка шашек. Получатель принимает этот список и отрисовывает у себя на доске
        private void SendMessage()
        {
            GameOver();
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(ms, board.Draughts);
            byte[] buffer = ms.ToArray();
            network.Send(buffer);
            network.SendMessageNetwork("Ход противника");
            IsCurrentMove = false;
        }

        //Собственное прием и десериализация объекта
        private void RecieveMessage(byte[] bytes)
        {
            GameOver();
            MemoryStream ms = new MemoryStream(bytes);
            BinaryFormatter formatter = new BinaryFormatter();
            board.Draughts = (List<Draught>)formatter.Deserialize(ms);
            network.SendMessageNetwork("Ваш ход");
            UpdateView();
            IsCurrentMove = true;
        }
        //Отображение на своей доске полученной информации
        private void UpdateView()
        {
            foreach (PictureBox pb in box)
            {
                pb.Image = Properties.Resources.empty;
            }
            foreach (Draught d in board.Draughts)
            {

                if (d == null) box[d.Coordinate.X, d.Coordinate.Y].Image = Properties.Resources.empty;
                else if (d.IsWhite && !d.IsSpecial) box[d.Coordinate.X, d.Coordinate.Y].Image = Properties.Resources.white;
                else if (d.IsWhite && d.IsSpecial) box[d.Coordinate.X, d.Coordinate.Y].Image = Properties.Resources.white_queen;
                else if (!d.IsWhite && !d.IsSpecial) box[d.Coordinate.X, d.Coordinate.Y].Image = Properties.Resources.black;
                else if (!d.IsWhite && d.IsSpecial) box[d.Coordinate.X, d.Coordinate.Y].Image = Properties.Resources.black_queen;
            }
        }
        //Подключение к серверу
        private void tsConnectToServer_Click(object sender, EventArgs e)
        {
            Connection connect = new Connection();
            DialogResult result;
            StartClientForm clientForm = new StartClientForm(connect);
            result = clientForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                network = new NetworkClient(connect.Address.ToString(), connect.Port);
                network.Start();
                network.Recv += RecieveMessage;
                network.SendMessageNetwork += SendMessageNet;
                IsPlayWhite = false;
                IsCurrentMove = false;
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    public class Connection {
        public IPAddress Address { get; set; }
        public int Port { get; set; }
    }
}
