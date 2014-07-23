using EmulatorRobot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace EmulatorRobot
{
    class KeyboardEvent
    {
        private int type;
        private int key;
        public KeyboardEvent(NetworkStream stream, uint playerNum)
        {
            type = stream.ReadByte();
            key = stream.ReadByte();

            if (playerNum == 2)
            {
                switch (key)
                {
                    case 65: //A
                        key = 90;
                        break;
                    case 66: //B
                        key = 86;
                        break;
                    case 83: //S
                        key = 88;
                        break;
                    case 84: //T
                        key = 67;
                        break;
                    case 38: //UP
                        key = 75;
                        break;
                    case 40: //DOWN
                        key = 74;
                        break;
                    case 37: //LEFT
                        key = 72;
                        break;
                    case 39: //RIGHT
                        key = 76;
                        break;
                    default:
                        break;
                }
            }

        }

        public void Execute()
        {
            InputSimulator iss = new InputSimulator();

            if (type == 0)
            {
                Console.WriteLine("Down: " + key);
                //key down
                iss.Keyboard.KeyDown((VirtualKeyCode)key);
            }
            else if (type == 1)
            {
                Console.WriteLine("Up: " + key);
                //key up
                iss.Keyboard.KeyUp((VirtualKeyCode)key);
            }
        }
    }

    class Client
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        private NetworkStream stream;
        private uint playerNum;

        public Client(NetworkStream stream, uint playerNum)
        {
            this.stream = stream;
            this.playerNum = playerNum;
        }

        private void recieveKeyboard()
        {
            while (true)
            {
                KeyboardEvent key = new KeyboardEvent(stream, playerNum);
                key.Execute();
            }
        }

        private Bitmap CaptureWindow(Rectangle bounds)
        {
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }

            return bitmap;
        }

        private void sendScreen()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            while (true)
            {
                try
                {
                    IntPtr window = GetForegroundWindow();
                    RECT active = new RECT();
                    GetWindowRect(window, ref active);

                    Rectangle view = new Rectangle(active.Left, active.Top, active.Right - active.Left, active.Bottom - active.Top);
                    if (view.Width == 0 || view.Height == 0) continue;


                    Bitmap b = CaptureWindow(view);
                    formatter.Serialize(stream, b);
                    stream.Flush();
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("Screen capture error: " + e.Message);
                    continue;
                }
            }

        }

        public void Start()
        {
            Thread keys = new Thread(new ThreadStart(recieveKeyboard));
            keys.Start();

            Thread screen = new Thread(new ThreadStart(sendScreen));
            screen.Start();
        }
    }

    class Server
    {
        private TcpListener tcpListener;
        private uint numPlayers;
        private Boolean[] users;

        public Server(int port)
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            users = new Boolean[2] { false, false };
        }

        public void Run()
        {
            tcpListener.Start();

            Console.WriteLine("Starting server");

            while (true)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (!users[i])
                    {
                        users[i] = true;
                        //blocks until a client has connected to the server
                        TcpClient client = tcpListener.AcceptTcpClient();

                        //create a thread to handle communication 
                        //with connected client
                        Thread clientThread = new Thread(new ParameterizedThreadStart(HandleConnection));
                        clientThread.Start(client);
                        break;
                    }
                }

            }
        }

        private void HandleConnection(object client)
        {
            numPlayers++;
            Console.WriteLine("Client connected!");
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            Client c = new Client(clientStream, numPlayers);
            c.Start();
        }

        static void Main(string[] args)
        {
            Server server = new Server(3000);
            server.Run();
        }
    }
}
