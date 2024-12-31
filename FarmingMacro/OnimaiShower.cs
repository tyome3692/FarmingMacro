using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FarmingMacro
{
    public partial class OnimaiShower : Form
    {
        public OnimaiShower()
        {
            InitializeComponent();
        }

        private void OnimaiShower_Load(object sender, EventArgs e)
        {

        }

        private void OnimaiShower_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form1.isOnimaiShown = false;
        }
    }
}
