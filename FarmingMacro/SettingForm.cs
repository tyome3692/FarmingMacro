using System.Text.RegularExpressions;

namespace FarmingMacro
{
    public partial class SettingForm : Form
    {
        private static Dictionary<int, int> minecraftKeyList = new Dictionary<int, int>()
        {
                {1,27},
                {2,49},
                {3,50},
                {4,51},
                {5,52},
                {6,53},
                {7,54},
                {8,55},
                {9,56},
                {10,57},
                {11,48},
                {12,189},
                {13,187},
                {14,8},
                {15,9},
                {16,81},
                {17,87},
                {18,69},
                {19,82},
                {20,84},
                {21,89},
                {22,85},
                {23,73},
                {24,79},
                {25,80},
                {26,219},
                {27,221},
                {28,13},
                {29,17},
                {30,65},
                {31,83},
                {32,68},
                {33,70},
                {34,71},
                {35,72},
                {36,74},
                {37,75},
                {38,76},
                {39,186},
                {40,222},
                {41,192},
                {42,16},
                {43,220},
                {44,90},
                {45,88},
                {46,67},
                {47,86},
                {48,66},
                {49,78},
                {50,77},
                {51,188},
                {52,190},
                {53,191},
                {54,16},
                {55,106},
                {56,18},
                {57,32},
                {58,20},
                {59,112},
                {60,113},
                {61,114},
                {62,115},
                {63,116},
                {64,117},
                {65,118},
                {66,119},
                {67,120},
                {68,121},
                {69,144},
                {70,145},
                {71,103},
                {72,104},
                {73,105},
                {74,109},
                {75,100},
                {76,101},
                {77,102},
                {78,107},
                {79,97},
                {80,98},
                {81,99},
                {82,96},
                {83,110},
                {87,122},
                {88,123},
                {121,28},
                {123,29},
                {156,13},
                {181,111},
                {183,44},
                {184,18},
                {197,19},
                {199,36},
                {200,38},
                {201,33},
                {203,37},
                {205,39},
                {207,35},
                {208,40},
                {209,34},
                {210,45},
                {211,46},
                {219,91},
                {220,92},
                {224,226}
        };
        private static bool isMCOptionLoad;
        public SettingForm()
        {
            InitializeComponent();

            textBox1.KeyDown += TextBoxKeyDown;
            textBox2.KeyDown += TextBoxKeyDown;
            textBox3.KeyDown += TextBoxKeyDown;
            textBox4.KeyDown += TextBoxKeyDown;
            textBox5.KeyDown += TextBoxKeyDown;

            textBox1.Tag = label6;
            textBox2.Tag = label7;
            textBox3.Tag = label8;
            textBox4.Tag = label9;
            textBox5.Tag = label10;

            textBox1.Text = Properties.Settings.Default.start.ToString();
            textBox2.Text = Properties.Settings.Default.end.ToString();
            textBox3.Text = Properties.Settings.Default.stop.ToString();
            textBox4.Text = Properties.Settings.Default.inv.ToString();
            textBox5.Text = Properties.Settings.Default.chat.ToString();

            label6.Text = ((Keys)Properties.Settings.Default.start).ToString();
            label7.Text = ((Keys)Properties.Settings.Default.end).ToString();
            label8.Text = ((Keys)Properties.Settings.Default.stop).ToString();
            label9.Text = ((Keys)Properties.Settings.Default.inv).ToString();
            label10.Text = ((Keys)Properties.Settings.Default.chat).ToString();

            button1.Click += Button1_Click;

            checkBox1.Checked = Properties.Settings.Default.chatStop;
            checkBox2.Checked = Properties.Settings.Default.invStop;
            checkBox3.Checked = Properties.Settings.Default.escStop;

            MaximumSize = Size;
            MinimumSize = Size;
            Text = "環境設定";

            isMCOptionLoad = false;
        }

        private void Button1_Click(object? sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.FileName = "マインクラフトの設定ファイル(option.txt)を選択してください";
            ofd.InitialDirectory = $@"C:\Users\{Environment.UserName}\AppData\Roaming\.minecraft";
            ofd.Filter = "|options.txt";
            ofd.FilterIndex = 1;
            ofd.Title = "マインクラフトの設定ファイル(option.txt)を選択してください";
            ofd.RestoreDirectory = true;
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                isMCOptionLoad = true;
                SetKeysFromOptionFile(ofd.FileName);
            }
            ofd.Dispose();
        }
        private void SetKeysFromOptionFile(string filePath)
        {
            string fileText = File.ReadAllText(filePath);
            string inventoryKey = Regex.Match(fileText, @"(?<=key_key\.inventory:)\d+").Value;
            if (int.TryParse(inventoryKey, out int invKeyCode))
            {
                label9.Text = ((Keys)minecraftKeyList[invKeyCode]).ToString();
            }
            textBox4.Text = inventoryKey;

            string chatKey = Regex.Match(fileText, @"(?<=key_key\.chat:)\d+").Value;
            if (int.TryParse(chatKey, out int chatKeyCode))
            {
                label10.Text = ((Keys)minecraftKeyList[chatKeyCode]).ToString();
            }
            textBox5.Text = chatKey;
        }
        private void TextBoxKeyDown(object? sender, System.Windows.Forms.KeyEventArgs e)
        {
            TextBox tb = (TextBox)(sender ?? throw new ArgumentNullException(nameof(sender)));
            Label lb = tb.Tag as Label ?? throw new ArgumentNullException(nameof(sender));
            tb.Text = e.KeyValue.ToString();
            lb.Text = e.KeyCode.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string tb1 = textBox1.Text;
            string tb2 = textBox2.Text;
            string tb3 = textBox3.Text;
            string tb4 = textBox4.Text;
            string tb5 = textBox5.Text;

            if (int.TryParse(tb1, out int startKeyCode))
                Properties.Settings.Default.start = startKeyCode;
            if (int.TryParse(tb2, out int endKeyCode))
                Properties.Settings.Default.end = endKeyCode;
            if (int.TryParse(tb3, out int stopKeyCode))
                Properties.Settings.Default.stop = stopKeyCode;
            if (int.TryParse(tb4, out int invKeyCode))
                Properties.Settings.Default.inv = isMCOptionLoad ? minecraftKeyList[invKeyCode] : invKeyCode;
            if (int.TryParse(tb5, out int chatKeyCode))
                Properties.Settings.Default.chat = isMCOptionLoad ? minecraftKeyList[chatKeyCode] : chatKeyCode;
            string[] strs = [tb1, tb2, tb3, tb4, tb5];

            Properties.Settings.Default.invStop = checkBox1.Checked;
            Properties.Settings.Default.chatStop = checkBox2.Checked;
            Properties.Settings.Default.escStop = checkBox3.Checked;

            if (hasDist(strs))
            {
                MessageBox.Show("同じキーは選択できません。");
            }
            else
            {
                Properties.Settings.Default.Save();
                Close();
            }
        }
        private static bool hasDist(string[] arr)
        {
            for (int i = 0; i < arr.Length - 1; i++)
            {
                for (int j = i + 1; j < arr.Length; j++)
                {
                    if (arr[i] == arr[j])
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
