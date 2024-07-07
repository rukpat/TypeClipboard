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
using TextCopy;
using System.Threading;
using TypeClipboard;


namespace TypeClipboard
{
    public class TypeClipboardApp : Form
    {
        private NotifyIcon notifyIcon;
        private IContainer components;
        private NotifyIcon trayIcon;

        public TypeClipboardApp()
        {
            InitializeComponent();
            //this.ShowInTaskbar = false; // Hide the main form from the taskbar

            trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Visible = true;

            ContextMenuStrip trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Type Clipboard", null, TypeFromClipboard); // Add an option to trigger typing
            trayMenu.Items.Add("Exit", null, OnExit);
            trayIcon.ContextMenuStrip = trayMenu;

            trayIcon.DoubleClick += TypeFromClipboard;

        }

        private void OnExit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }

        [DllImport("user32.dll")]
        static extern void SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public void TypeFromClipboard(object sender, EventArgs e)
        {
            string clipboardText = ClipboardService.GetText() ?? string.Empty; //Handle null clipboard

            if (!string.IsNullOrEmpty(clipboardText))
            {
                INPUT[] inputs = new INPUT[clipboardText.Length];
                for (int i = 0; i < clipboardText.Length; i++)
                {
                    inputs[i] = new INPUT
                    {
                        type = 1, // Keyboard input
                        wVk = 0,
                        wScan = (ushort)clipboardText[i],
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    };
                }

                SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            }
            // (No need for "Clipboard is empty" message in the tray icon version)
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TypeClipboardApp()); // Run the application with the tray icon context
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TypeClipboardApp));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon.BalloonTipText = "Rukesh\'s message";
            this.notifyIcon.BalloonTipTitle = "Rukesh \'s title";
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Type clipboard text (to active window)";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // TypeClipboardApp
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "TypeClipboardApp";
            this.ResumeLayout(false);

        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TypeFromClipboard(sender, e);
        }
    }
}
