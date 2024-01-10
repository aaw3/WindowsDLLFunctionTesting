using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Windows_DLL_Function_Testing
{
    public partial class ColorViewForm : Form
    {
        public string pposition;
        public ColorViewForm(string position)
        {
            InitializeComponent();

            pposition = position;
        }

        protected override CreateParams CreateParams //Hide in Alt-Tab (If pressing alt-tab while it's showing, it won't work until hiding again)
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        private void ColorViewForm_Load(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Size = new Size(125 + 3, 125 + 3);
            if (pposition == "TLeft")
            {
                this.Location = new Point((int)(Screen.PrimaryScreen.Bounds.Width * 1.06f) - Screen.PrimaryScreen.Bounds.Width - 1, (int)(Screen.PrimaryScreen.Bounds.Height * 1.10f) - Screen.PrimaryScreen.Bounds.Height - 1);
            }
            else if (pposition == "TRight")
            {
                this.Location = new Point((int)(Screen.PrimaryScreen.Bounds.Width / 1.06f) - this.Size.Width - 1, (int)(Screen.PrimaryScreen.Bounds.Height * 1.10f) - Screen.PrimaryScreen.Bounds.Height - 1);
            }
            else if (pposition == "BLeft")
            {
                this.Location = new Point((int)(Screen.PrimaryScreen.Bounds.Width * 1.06f) - Screen.PrimaryScreen.Bounds.Width - 1, (int)(Screen.PrimaryScreen.Bounds.Height / 1.10f - this.Size.Height - 1));
            }
            else if (pposition == "BRight")
            {
                this.Location = new Point((int)(Screen.PrimaryScreen.Bounds.Width / 1.06f) - this.Size.Width - 1, (int)(Screen.PrimaryScreen.Bounds.Height / 1.10f) - this.Size.Height - 1);
            }
        }

        int Width = 3;
        private void ColorViewForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(Color.Black, Width), new Rectangle(new Point(this.DisplayRectangle.X + 1, this.DisplayRectangle.Y + 1), new Size(this.DisplayRectangle.Width - Width, this.DisplayRectangle.Height - Width)));
            //e.Graphics.DrawRectangle(new Pen(Color.Black, 3), new Rectangle(this.Location, new Size(this.Width - 5, this.Height - 5)));
        }
    }
}
