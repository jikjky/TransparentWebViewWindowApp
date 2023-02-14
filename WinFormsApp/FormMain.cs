using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System.Windows.Forms;
using WinFormsApp.Hook;

namespace WinFormsApp
{
    public partial class FormMain : Form
    {
        FormWebView webView;

        bool isClose = false;

        public KeyboardHook keyboardHook = new KeyboardHook();

        #region RegistryKey
        static RegistryKey clsoeRegistrykey = Registry.CurrentUser.CreateSubKey("ThisIsAToyApp").CreateSubKey("Close Button");
        static RegistryKey minimizeRegistrykey = Registry.CurrentUser.CreateSubKey("ThisIsAToyApp").CreateSubKey("Minimize Button");

        RegistryKeyConfig closeConfig = new RegistryKeyConfig(clsoeRegistrykey);
        RegistryKeyConfig minimizeConfig = new RegistryKeyConfig(minimizeRegistrykey);
        public class RegistryKeyConfig
        {
            RegistryKey myRegistryKey;

            public RegistryKeyConfig(RegistryKey registryKey)
            {
                myRegistryKey = registryKey;
            }

            public bool bCtrl
            {
                get
                {
                    return Convert.ToBoolean(myRegistryKey.GetValue("Ctrl"));
                }
                set
                {
                    myRegistryKey.SetValue("Ctrl", value);
                }
            }
            public bool bAlt
            {
                get
                {
                    return Convert.ToBoolean(myRegistryKey.GetValue("Alt"));
                }
                set
                {
                    myRegistryKey.SetValue("Alt", value);
                }
            }
            public bool bShift
            {
                get
                {
                    return Convert.ToBoolean(myRegistryKey.GetValue("Shift"));
                }
                set
                {
                    myRegistryKey.SetValue("Shift", value);
                }
            }
            public Keys key
            {
                get
                {
                    try
                    {
                        var returnValue = (Keys)Enum.Parse(typeof(Keys), Convert.ToString(myRegistryKey.GetValue("Key")));
                        return returnValue;
                    }
                    catch
                    {
                        return Keys.None;
                    }
                }
                set
                {
                    myRegistryKey.SetValue("Key", value.ToString());
                }
            }
        }
        #endregion

        private bool IsFormAlreadyOpen(Type FormType)
        {
            foreach (Form OpenForm in Application.OpenForms)
            {
                if (OpenForm.GetType() == FormType)
                    return true;
            }

            return false;
        }

        public FormMain()
        {
            InitializeComponent();
            SetHotKey(textBox2, closeConfig, closeConfig.bCtrl, closeConfig.bAlt, closeConfig.bShift, closeConfig.key);
            SetHotKey(textBox3, minimizeConfig, minimizeConfig.bCtrl, minimizeConfig.bAlt, minimizeConfig.bShift, minimizeConfig.key);
            keyboardHook.KeyDown += new KeyEventHandler(keyboardHook_KeyDown);
            keyboardHook.KeyUp += new KeyEventHandler(keyboardHook_KeyUp);

            keyboardHook.Start();
            for (int i = 0; i < 10; i++)
            {
                var item = new ToolStripMenuItem();
                item.Name = i.ToString();
                item.Text = ((i + 1) * 10).ToString() + "%";
                item.Tag = i;
                item.Click += (s, e) =>
                {
                    trackBar1.Value = ((((int)item.Tag) + 1) * 10);
                    if (IsFormAlreadyOpen(typeof(FormWebView)))
                        webView.Opacity = (double)(((int)item.Tag) + 1) / (double)10;
                };
                opacityToolStripMenuItem.DropDownItems.Add(item);

            }
        }

        void keyboardHook_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (IsFormAlreadyOpen(typeof(FormWebView)))
                {
                    webView.IsSizeChage = false;
                }
            }
        }

        void keyboardHook_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.LControlKey)
            {
                if (webView != null)
                {
                    webView.IsSizeChage = true;
                }
            }
            if (closeConfig.bCtrl == e.Control && closeConfig.bAlt == e.Alt && closeConfig.bShift == e.Shift && closeConfig.key == e.KeyCode)
            {
                isClose = true;
                this.Close();
            }
            if (minimizeConfig.bCtrl == e.Control && minimizeConfig.bAlt == e.Alt && minimizeConfig.bShift == e.Shift && minimizeConfig.key == e.KeyCode)
            {
                if (IsFormAlreadyOpen(typeof(FormWebView)))
                {
                    if (webView.Opacity == 0)
                    {
                        webView.Opacity = (double)trackBar1.Value / (double)100;
                        webView.Activate();
                    }
                    else
                        webView.Opacity = 0;
                }
            }
            if (e.Alt && ((int)Keys.D0 <= (int)e.KeyCode && (int)e.KeyCode <= (int)Keys.D9))
            {
                if (IsFormAlreadyOpen(typeof(FormWebView)))
                {
                    int value = 0;
                    if (e.KeyCode == Keys.D0)
                        value = 10;
                    else
                        value = (int)e.KeyCode - 0x30;
                    if (0 <= value && value <= 10)
                    {
                        trackBar1.Value = value * 10;
                        webView.Opacity = (double)trackBar1.Value / (double)100;
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void AppOpen()
        {
            this.Opacity = 1;
            this.ShowInTaskbar = true;
        }

        private void AppClose()
        {
            this.Opacity = 0;
            this.ShowInTaskbar = false;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (!IsFormAlreadyOpen(typeof(FormWebView)))
            {
                webView = new FormWebView();
                webView.FormBorderStyle = FormBorderStyle.Sizable;
                webView.Opacity = (double)trackBar1.Value / (double)100;
                webView.IsBorderLess = checkBox1.Checked;
                webView.TopMost = checkBox2.Checked;
                await webView.Init();
                webView.Show();
            }
            if (string.IsNullOrWhiteSpace(textBox1.Text))
                return;
            if (!(textBox1.Text.IndexOf("http://") == 0 || textBox1.Text.IndexOf("https://") == 0))
            {
                textBox1.Text = "https://" + textBox1.Text;
            }
            webView.Navigate(textBox1.Text);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (IsFormAlreadyOpen(typeof(FormWebView)))
                webView.IsBorderLess = checkBox1.Checked;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (IsFormAlreadyOpen(typeof(FormWebView)))
                webView.Opacity = (double)trackBar1.Value / (double)100;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (IsFormAlreadyOpen(typeof(FormWebView)))
                webView.TopMost = checkBox2.Checked;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isClose = true;
            this.Close();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isClose)
            {
                e.Cancel = true;
                AppClose();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.Opacity == 0)
                AppOpen();
            else
                AppClose();
        }

        private void SetHotKey(TextBox textBox, RegistryKeyConfig registryKeyConfig, bool bCtrl, bool bAlt, bool bShift, Keys keys)
        {
            string text = "";
            Keys tempKeys = keys;

            if (bCtrl == true)
            {
                text += "Ctrl";
            }
            if (bAlt == true)
            {
                if (text.Length != 0)
                {
                    text += " + ";
                }
                text += "Alt";
            }
            if (bShift == true)
            {
                if (text.Length != 0)
                {
                    text += " + ";
                }
                text += "Shift";
            }
            if ((int)tempKeys > 0)
            {
                if (false == (tempKeys == Keys.Control || tempKeys == Keys.Shift || tempKeys == Keys.Alt || tempKeys == Keys.Menu || tempKeys == Keys.ControlKey || tempKeys == Keys.ShiftKey))
                {
                    if (text.Length != 0)
                    {
                        text += " + ";
                    }
                    text += tempKeys.ToString();
                }
            }
            registryKeyConfig.bCtrl = bCtrl;
            registryKeyConfig.bShift = bShift;
            registryKeyConfig.bAlt = bAlt;
            registryKeyConfig.key = keys;
            textBox.Text = text;
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            SetHotKey((TextBox)sender, closeConfig, e.Control, e.Alt, e.Shift, e.KeyCode);
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            if (this.Opacity == 0)
                AppOpen();
            else if (IsFormAlreadyOpen(typeof(FormWebView)))
                webView.Activate();
        }

        private void borderLessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkBox1.Checked = !checkBox1.Checked;
        }

        private void topMostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkBox2.Checked = !checkBox2.Checked;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripTextBox1.ReadOnly= true;
            toolStripTextBox1.Text = Clipboard.GetText();
            toolStripMenuNoneClickItem.Text = (checkBox3.Checked ? "* " : "") + "None Click";
            borderLessToolStripMenuItem.Text = (checkBox1.Checked ? "* " : "") + "Border Less";
            topMostToolStripMenuItem.Text = (checkBox2.Checked ? "* " : "") + "Top Most";
            openToolStripMenuItem.Text = this.Opacity == 0 ? "Menu Show" : "Menu Hide";
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            SetHotKey((TextBox)sender, minimizeConfig, e.Control, e.Alt, e.Shift, e.KeyCode);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (IsFormAlreadyOpen(typeof(FormWebView)))
                webView.NoneClickForm(checkBox3.Checked);
        }

        private void toolStripMenuNoneClickItem_Click(object sender, EventArgs e)
        {
            checkBox3.Checked = !checkBox3.Checked;
        }

        private async void toolStripMenuOpenItem_Click(object sender, EventArgs e)
        {
            if (!IsFormAlreadyOpen(typeof(FormWebView)))
            {
                webView = new FormWebView();
                webView.FormBorderStyle = FormBorderStyle.Sizable;
                webView.Opacity = (double)trackBar1.Value / (double)100;
                webView.IsBorderLess = checkBox1.Checked;
                webView.TopMost = checkBox2.Checked;
                await webView.Init();
                webView.Show();
            }
            if (string.IsNullOrWhiteSpace(toolStripTextBox1.Text))
                return;
            if (!(toolStripTextBox1.Text.IndexOf("http://") == 0 || toolStripTextBox1.Text.IndexOf("https://") == 0))
            {
                toolStripTextBox1.Text = "https://" + toolStripTextBox1.Text;
            }
            webView.Navigate(toolStripTextBox1.Text);
            toolStripTextBox1.Text = "";
        }

        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                toolStripMenuOpenItem_Click(null, null);
            }
        }
    }
}