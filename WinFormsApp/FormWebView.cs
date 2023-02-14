using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp
{
    
    public partial class FormWebView : Form
    {
        public class TransparentPanel : System.Windows.Forms.Panel
        {
            public TransparentPanel()
            {

            }

            public TransparentPanel(IContainer container)
            {
                container.Add(this);
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x20;
                    return cp;
                }
            }

            protected override void OnPaintBackground(PaintEventArgs e) { }

        }
        private Panel noneClickPanel;

        public int ClickX;
        public int ClickY;

        public int LastX;
        public int LastY;

        public bool IsMove;
        public bool IsSizeChage { get; set; }

        public bool IsNoneClick;

        public bool IsBorderLess
        {
            get
            {
                if (this.FormBorderStyle == FormBorderStyle.Sizable)
                    return false;
                else
                    return true;
            }
            set
            {
                int xMove = 8;
                int yMove = 31;
                int x = this.Location.X;
                int y = this.Location.Y;
                if (value == true)
                {
                    x = x + xMove;
                    y = y + yMove;
                    this.FormBorderStyle = FormBorderStyle.None;
                }
                else
                {
                    x = x - xMove;
                    y = y - yMove;
                    this.FormBorderStyle |= FormBorderStyle.Sizable;
                }
                this.Location = new Point(x, y);
            }
        }
        public FormWebView()
        {
            InitializeComponent();
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Icon = null;
            this.ShowIcon = false;
        }
        public async Task Init()
        {
            webView21.NavigationCompleted += WebView2_NavigationCompleted;
            await this.webView21.EnsureCoreWebView2Async(null);
        }

        private void WebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                ((WebView2)sender).ExecuteScriptAsync("document.querySelector('body').style.overflow='scroll';var style=document.createElement('style');style.type='text/css';style.innerHTML='::-webkit-scrollbar{display:none}';document.getElementsByTagName('body')[0].appendChild(style)");
            }
        }

        public void Navigate(string url)
        {
            webView21.CoreWebView2.Navigate(url);
        }

        public void NoneClickForm(bool bHide)
        {
            IsNoneClick = bHide;
            if (noneClickPanel == null)
            {
                noneClickPanel = new TransparentPanel();
                noneClickPanel.Dock = DockStyle.Fill;

                noneClickPanel.MouseDown += (o, e) =>
                {
                    IsMove = true;
                    ClickX = e.Location.X;
                    ClickY = e.Location.Y;
                    LastX = e.Location.X;
                    LastY = e.Location.Y;
                };

                noneClickPanel.MouseMove += (o, e) =>
                {
                    if (IsMove && IsNoneClick && IsSizeChage)
                    {
                        int x = (int)((float)(e.X - LastX) / (float)1);
                        int y = (int)((float)(e.Y - LastY) / (float)1);
                        this.Size = new Size(this.Width + x, this.Height + y);
                        LastX = e.X; 
                        LastY = e.Y;
                        return;
                    }
                    else if (IsMove && IsNoneClick)
                    {
                        this.Location = new Point(this.Location.X + e.X - ClickX, this.Location.Y + e.Y - ClickY);
                    }
                };

                noneClickPanel.MouseLeave += (o, e) =>
                {
                    IsMove = false;
                };

                noneClickPanel.MouseUp += (o, e) =>
                {
                    IsMove = false;
                };
                this.Controls.Add(noneClickPanel);
            }
            if (IsNoneClick)
            {
                this.Controls.SetChildIndex(noneClickPanel, 0);
            }
            else
            {
                this.Controls.SetChildIndex(noneClickPanel, this.Controls.Count);
            }
        }

        private void FormWebView_Load(object sender, EventArgs e)
        {

        }
    }
}
