using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ApSetting.Forms
{
    public partial class fCustomNotification : Form
    {
        public fCustomNotification()
        {
            this.InitializeComponent();
            this.TopMost = true;

            // Cấu hình form
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.BackColor = Color.DarkViolet;
            this.Padding = new Padding(0);
            this.ShowInTaskbar = false;

            // Bo góc nhẹ
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 5, 5));

            // Vị trí ở giữa trên màn hình
            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point((screen.Width - this.Width) / 2, 5);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
        }

        /// <summary>
        /// Thread-safe update of the visible message.
        /// Safe to call from any thread.
        /// </summary>
        public void UpdateMessage(string message)
        {
            if (this.IsDisposed) return;
            if (this.InvokeRequired)
            {
                try
                {
                    this.BeginInvoke((Action)(() =>
                    {
                        if (!this.IsDisposed && this.IsHandleCreated) lblMessage.Text = message;
                    }));
                }
                catch { /* ignore if invoke fails during shutdown */ }
            }
            else
            {
                lblMessage.Text = message;
            }
        }

        // Import hàm tạo bo góc
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);
    }

}
