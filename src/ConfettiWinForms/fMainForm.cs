using ConfettiWinForms.ConfettiWinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfettiWinForms
{
    public partial class fMainForm : Form
    {
        public fMainForm()
        {
            InitializeComponent();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ConfettiEffect.Run(8000, 8); // chạy 8 giây
        }
    }
}
