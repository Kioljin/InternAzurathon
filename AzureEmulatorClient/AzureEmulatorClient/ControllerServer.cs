using System;
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
    public class ControllerServer
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

        public void Close()
        {
            if (!_connected) return;
            _connected = false;
            if (server != null)
                server.Close();
        }
        public bool Connect()
        {
            try
            {
                server = new TcpClient();
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(address), 3000);
                server.Connect(serverEndPoint);

                Thread t = new Thread(new ThreadStart(readBitmap));
                t.Start();

                _connected = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void readBitmap()
        {
            NetworkStream stream = server.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();

            while (true)
            {
                try
                {
                    Bitmap b = (Bitmap)formatter.Deserialize(stream);
                    this.lastImage = b;
                    parent.Invalidate();
                }
                catch (Exception e)
                {
                    return;
                }
            }
        }

        public void SendKeyDown(int key)
        {
            if (!_connected) return;

            try
            {
                NetworkStream stream = server.GetStream();
                stream.WriteByte(0);
                stream.WriteByte((byte)key);
            }
            catch (Exception)
            {
                _connected = false;
            }
        }

        public void SendKeyUp(int key)
        {
            if (!_connected) return;

            try
            {
                NetworkStream stream = server.GetStream();
                stream.WriteByte(1);
                stream.WriteByte((byte)key);
            }
            catch (Exception)
            {
                _connected = false;
            }
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
