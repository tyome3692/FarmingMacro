
namespace FarmingMacro
{
    public partial class OnimaiShower : Form
    {
        public OnimaiShower()
        {
            InitializeComponent();
            TopMost = true;
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
