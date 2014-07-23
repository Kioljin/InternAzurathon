using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AzureEmulatorClient
{
    public partial class ControllerForm : Form
    {
        private ControllerServer server;
        private KeyboardState keyboard;

        private Bitmap screen;

        public ControllerForm()
        {
            InitializeComponent();

            this.keyboard = new KeyboardState();

            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.KeyDown += Control_KeyDown;
            this.KeyUp += Control_KeyUp;
            this.Paint += ControllerForm_Paint;

            this.Width = 300;
            this.Height = 300;
        }

        void ControllerForm_Paint(object sender, PaintEventArgs e)
        {
            if (server == null) return;
            if (server.Image == null) return;

            e.Graphics.DrawImage(server.Image, 0, 50);
        }

        private void Control_KeyUp(object sender, KeyEventArgs e)
        {
            this.screen = CaptureWindow();
            this.Invalidate();

            if (server == null)
            {
                return;
            }

            if (e.KeyData == Keys.ShiftKey || e.KeyData == Keys.Shift)
            {
                server.SendKeyUp((int)Keys.Shift);
                server.SendKeyUp((int)Keys.ShiftKey);
                server.SendKeyUp((int)(Keys.Shift | Keys.ShiftKey));
            }
            else
            {
                server.SendKeyUp((int)e.KeyData);
            }
        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            if (server == null)
            {
                return;
            }

            server.SendKeyDown((int)e.KeyData);
        }

        private void UpdateServer(object sender, EventArgs e)
        {
            server.SendKeyboardState(keyboard);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            server = new ControllerServer(this, txtServer.Text);
            server.Connect();
        }

        private Bitmap CaptureWindow()
        {
            Rectangle bounds = this.Bounds;
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                 g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }

            return bitmap;
        }
    }
}
