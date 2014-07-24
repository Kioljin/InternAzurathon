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
        
        public ControllerForm()
        {
            InitializeComponent();

            this.keyboard = new KeyboardState();

            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.KeyDown += Control_KeyDown;
            this.KeyUp += Control_KeyUp;
            this.Paint += ControllerForm_Paint;

            this.FormClosed += ControllerForm_FormClosed;

            this.Width = 300;
            this.Height = 300;
        }

        void ControllerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (server != null)
                server.Close();
        }
        
        void ControllerForm_Paint(object sender, PaintEventArgs e)
        {
            if (server == null) return;
            if (server.Image == null) return;

            this.Width = server.Image.Width + 10 + 5;
            this.Height = server.Image.Height + 5 + 32;
            e.Graphics.DrawImage(server.Image, 0, 0);
        }

        private void Control_KeyUp(object sender, KeyEventArgs e)
        {
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
            if (server.Connect())
            {
                txtServer.Visible = false;
                txtServer.Enabled = true;
                btnConnect.Visible = false;
                btnConnect.Enabled = true;
            }
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
