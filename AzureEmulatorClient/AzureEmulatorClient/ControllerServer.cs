﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AzureEmulatorClient
{
    class ControllerServer
    {
        private string address;
        private TcpClient server;

        private bool _connected = false;
        private Bitmap lastImage;

        private Form parent;
 
        public Bitmap Image { get { return lastImage; } }

        public ControllerServer(Form parent, string address)
        {
            this.parent = parent;
            this.address = address;
        }

        public void Connect()
        {
            server = new TcpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(address), 3000);
            server.Connect(serverEndPoint);

            Thread t = new Thread(new ThreadStart(readBitmap));
            t.Start();

            _connected = true;
        }

        private void readBitmap()
        {
            NetworkStream stream = server.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();

            while (true)
            {
                Bitmap b = (Bitmap)formatter.Deserialize(stream);
                this.lastImage = b;
                parent.Invalidate();
            }
        }

        public void SendKeyDown(int key)
        {
            if (!_connected) return;

            NetworkStream stream = server.GetStream();
            stream.WriteByte(0);
            stream.WriteByte((byte)key);
        }

        public void SendKeyUp(int key)
        {
            if (!_connected) return;

            NetworkStream stream = server.GetStream();
            stream.WriteByte(1);
            stream.WriteByte((byte)key);
        }

        public void SendKeyboardState(KeyboardState state)
        {
            if (server == null)
                Connect();

            NetworkStream stream = server.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, state);
        }
    }
}
