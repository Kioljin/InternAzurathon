using EmulatorRobot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
        public KeyboardEvent(NetworkStream stream)
        {
            type = stream.ReadByte();
            key = stream.ReadByte();
        }

        public void Execute()
        {
            try
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
            catch (Exception e)
            {
                    System.Console.WriteLine("Unable to display keypress.");
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
        private bool connected = true;    

        public Client(NetworkStream stream)
        {
            this.stream = stream;
        }

        private void recieveKeyboard()
        {
            while (connected)
            {
                try
                {
                    KeyboardEvent key = new KeyboardEvent(stream);
                    key.Execute();
                }
                catch (IOException)
                {
                    this.connected = false;
                    System.Console.WriteLine("Client closed connection");
                    return;
                }
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

            while (connected)
            {
                try
                {
                    IntPtr window = GetForegroundWindow();
                    RECT active = new RECT();
                    GetWindowRect(window, ref active);

                    Rectangle view = new Rectangle(active.Left + 10, active.Top + 52, active.Right - active.Left - 20, active.Bottom - active.Top - 52 - 10);

                    if (view.Width <= 0 || view.Height <= 0) continue;

                    Bitmap b = CaptureWindow(view);
                    formatter.Serialize(stream, b);
                    stream.Flush();

                    Thread.Sleep(100);
                }
                catch (IOException)
                {
                    this.connected = false;
                    System.Console.WriteLine("Client closed connection");
                    return;
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
        private Process game;

        public Server(int port)
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
        }

        public void Run()
        {
            tcpListener.Start();

            Console.WriteLine("Starting server");

            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = tcpListener.AcceptTcpClient();

                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleConnection));
                clientThread.Start(client);
            }
        }

        private void HandleConnection(object client)
        {
            Console.WriteLine("Client connected!");
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            Client c = new Client(clientStream);
            c.Start();
        }

        static void Main(string[] args)
        {
            Server server = new Server(3000);
            server.Run();
        }
    }
}
