using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xAuto.Overlay
{
    public partial class fHover : Form
    {
        private Point _local;
        private int _width;
        private int _height;
        public fHover(Point local, int width, int height)
        {
            InitializeComponent();
            _local = local;
            _width = width;
            _height = height;
            this.BackColor = Color.Yellow;
            this.Opacity = 1;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;

        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (Pen p = new Pen(Color.Green, 5))
            {
                e.Graphics.DrawRectangle(p, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

        private void fHover_Load(object sender, EventArgs e)
        {
            this.Width = _width;
            this.Height = _height;
            this.Location = _local;
        }
    }
}
