namespace FarmingMacro
{
    public partial class Form1 : Form
    {
        static KeyboardHook keyboardHook = new KeyboardHook();
        static bool isReady;
        public Form1()
        {
            InitializeComponent();

            keyboardHook.KeyDownEvent += KeyboardHook_KeyDownEvent;
            keyboardHook.Hook();

            Size = new Size(250, 160);
            MaximumSize = Size;
            MinimumSize = Size;
            Location = new Point(-7, 505);
            label1.Text = "収穫する作物";
            button1.Text = "起動";
            Text = "自動作物回収機";
            string imageDirectoryPath = Environment.CurrentDirectory + @"\images\farm";
            if (!Directory.Exists(imageDirectoryPath))
            {
                MessageBox.Show("imagesディレクトリが見つかりませんでした。ファイル構成を確認してください", "FarmingMacro");
                Close();
            }
            foreach (string d in Directory.GetDirectories(imageDirectoryPath))
            {
                comboBox1.Items.Add(d[(d.LastIndexOf('\\') + 1)..]);
            }
            comboBox1.SelectedIndex = Properties.Settings.Default.cropIndex;
        }

        internal static bool isOnimaiShown;
        private void KeyboardHook_KeyDownEvent(object sender, KeyEventArgs e)
        {
            if (!isReady)
                return;
            switch ((Keys)e.KeyCode)
            {
                case Keys end when end == (Keys)Properties.Settings.Default.end:
                    if (isOnimaiShown)
                        return;
                    OnimaiChallenge();
                    WindowState = FormWindowState.Normal;
                    MacroBase.StopMacro();
                    keyboardHook.UnHook();
                    Close();
                    break;
                case Keys stop when stop == (Keys)Properties.Settings.Default.stop:
                case Keys inventory when inventory == (Keys)Properties.Settings.Default.inv && Properties.Settings.Default.invStop:
                case Keys chat when chat == (Keys)Properties.Settings.Default.chat && Properties.Settings.Default.chatStop:
                case Keys esc when esc == Keys.Escape && Properties.Settings.Default.escStop:
                    MacroBase.StopMacro();
                    Console.Clear();
                    Console.WriteLine("一時停止中");
                    break;
                case Keys start when start == (Keys)Properties.Settings.Default.start:
                    MacroBase.FullAutomaticalyCropCollecter(comboBox1.SelectedItem?.ToString() ?? "wheat");                    
                    break;
            }
        }

        private static void OnimaiChallenge()
        {
            if (!isOnimaiShown && new Random().Next(99) < 1)
            {
                isOnimaiShown = true;
                OnimaiShower os = new OnimaiShower();
                os.ShowDialog();
                os.Dispose();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MacroBase.StopMacro();
            keyboardHook.UnHook();
        }

        private void SettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingForm sf = new SettingForm();
            sf.ShowDialog();
            sf.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            isReady = !isReady;
            if (isReady)
            {
                button1.Text = "停止";
                NativeMethods.AttachConsole();
            }
            else
            {
                button1.Text = "起動";
                NativeMethods.FreeConsole();
            }
            menuStrip1.Enabled = !isReady;
            comboBox1.Enabled = !isReady;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.cropIndex = comboBox1.SelectedIndex;
            Properties.Settings.Default.Save();
        }
    }
}
