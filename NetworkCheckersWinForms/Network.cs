using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Windows.Forms;

namespace NetworkCheckersWinForms
{
    //Класс реализующий подключение по сети
    public delegate void deRecv(byte[] data);
    public delegate void deSend(string t);
    abstract class Network
    {
        protected NetworkStream ns;
        protected string host;
        protected int port;
        public deRecv Recv;
        public deSend SendMessageNetwork;
        public void Start()
        {
            Thread thread = new Thread(Waiter);
            thread.IsBackground = true;
            thread.Start();
        }

        public void Waiter()
        {
            while (true)
            {
                Connect();
                while (true)
                {
                    try
                    {
                        byte[] bytes = new byte[1024*1024*50];
                        ns.Read(bytes, 0, bytes.Length);
                        Recv(bytes);
                    }
                    catch
                    {
                        Thread.Sleep(100);
                        break;
                    }
                }
            }
        }

        abstract public void Connect();

        public bool Send(byte[] data)
        {
            try
            {
                ns.Write(data, 0, data.Length);
                return true;
            }
            catch
            {
                Thread.Sleep(100);
                return false;
            }
        }

    }

    class NetworkServer : Network
    {
        public NetworkServer(int port)
        {
            this.port = port;
        }

        override public void Connect()
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                TcpClient client = listener.AcceptTcpClient();
                ns = client.GetStream();
                SendMessageNetwork("Ваш ход");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


    }

    class NetworkClient : Network
    {
        public NetworkClient(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        override public void Connect()
        {
            try
            {
                TcpClient client = new TcpClient(host, port);
                ns = client.GetStream();
                SendMessageNetwork("Ход противника");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
