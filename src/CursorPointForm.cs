using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Windows_DLL_Function_Testing
{
    public partial class CursorPointForm : Form
    {
        public int pwidth;
        public int pheight;
        public CursorPointForm(int width, int height)
        {
            InitializeComponent();

            pwidth = width;
            pheight = height;
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

        private void CursorPointForm_Load(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Size = new Size(pwidth, pheight);
        }
    }
}
