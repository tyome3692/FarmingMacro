
namespace FarmingMacro
{
    public partial class OnimaiShower : Form
    {
        public OnimaiShower()
        {
            InitializeComponent();
            TopMost = true;
        }

        private void OnimaiShower_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form1.isOnimaiShown = false;
        }
    }
}
