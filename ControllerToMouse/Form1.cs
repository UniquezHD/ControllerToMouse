using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ControllerToMouse
{
    public partial class Form1 : Form
    {
        #region variables

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, UInt32 dwFlags, UInt32 dwExtraInfo);

        [DllImport("user32.dll")]
        static extern Byte MapVirtualKey(UInt32 uCode, UInt32 uMapType);

        private const UInt32 KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const UInt32 KEYEVENTF_KEYUP = 0x0002;

        private const byte VK_MEDIA_PLAY_PAUSE = 0xB3;
        private const byte VK_ESCAPE = 0x1B;
        private const byte VK_LWIN = 0x5B;

        private const byte VK_PRIOR = 0x21;
        private const byte VK_NEXT = 0x22;

        private const byte VK_BROWSER_FORWARD = 0xA7;
        private const byte VK_BROWSER_BACK = 0xA6;

        private const byte VK_BROWSER_REFRESH = 0xA8;

        private const int LEFT_MOUSE_DOWN = 0x02;
        private const int LEFT_MOUSE_UP = 0x04;

        private const int RIGHT_MOUSE_DOWN = 0x08;
        private const int RIGHT_MOUSE_UP = 0x10;

        private const int MIDDLE_MOUSE_DOWN = 0x20;
        private const int MIDDLE_MOUSE_UP = 0x40;

        private Joystick joystick;
        private bool[] joystickButtons;

        private int sensitivity = 20;
        private int sensitivityDivider = 2;
        private int sensitivityMultiply = 5;

        private bool isDragging = false;

        #endregion

        private bool isDebug = true;

        private enum ControllerButtons 
        {
            Square,
            X,
            Circle,
            Triangle,
            L1,
            R1,
            L2,
            R2,
            Share,
            Options,
            L3,
            R3,
            PSN,
            TouchPad
        }

        private string[] ControllerButtonNames =  
        {
            "Square",
            "X",
            "Cirle",
            "Triangle",
            "L1",
            "R1",
            "L2",
            "R2",
            "Share",
            "Options",
            "L3",
            "R3",
            "PSN",
            "TouchPad"
        };

        public Form1()
        {
            InitializeComponent();

            if (isDebug)
            {
                this.Size = new System.Drawing.Size(606, 246);
                richTextBox1.Visible = true;
            }
            else
            {
                this.Size = new System.Drawing.Size(227, 246);
                richTextBox1.Visible = false;
            }

            sensitivity = Properties.Settings.Default.Sinsitivity;
            sensitivityMultiply = Properties.Settings.Default.SinsitivityMultiply;
            sensitivityDivider = Properties.Settings.Default.SinsitivityDivider;

            textBox1.Text = sensitivity.ToString();
            textBox2.Text = sensitivityMultiply.ToString();
            textBox3.Text = sensitivityDivider.ToString();


            joystick = new Joystick(this.Handle);
            ConnectToJoystick(joystick);

            RePaint();

            TopMost = true;
        }

        private void ConnectToJoystick(Joystick joystick)
        {
            while (true)
            {
                string sticks = joystick.FindJoysticks();
                if (sticks != null)
                {
                    if (joystick.AcquireJoystick(sticks))
                    {
                        EnableTimer();
                        break;
                    }
                }
            }
        }

        private void EnableTimer()
        {
            if (this.InvokeRequired)
            {
                BeginInvoke(new ThreadStart(delegate ()
                {
                    timer1.Enabled = true;
                }));
            }
            else
                timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                joystick.UpdateStatus();
                joystickButtons = joystick.buttons;

                label1.Text = $"X: {System.Windows.Forms.Cursor.Position.X}";

                label2.Text = $"Y: {System.Windows.Forms.Cursor.Position.Y}";

                label8.Text = $"X: {joystick.Xaxis}";

                label7.Text = $"Y: {joystick.Yaxis}";

                if (joystick.Xaxis == 0)
                {
                    richTextBox1.AppendText("Left\n");

                    Screen[] screens = Screen.AllScreens;

                    int newX = System.Windows.Forms.Cursor.Position.X;

                    if (joystickButtons[(int)ControllerButtons.L2])
                    {
                        newX -= sensitivity * sensitivityMultiply;
                    }
                    else if (joystickButtons[(int)ControllerButtons.R2])
                    {
                        newX -= sensitivity / sensitivityDivider;
                    }
                    else
                    {
                        newX -= sensitivity;
                    }

                    foreach (Screen screen in screens)
                    {
                        if (newX >= screen.Bounds.Left && newX <= screen.Bounds.Right)
                        {
                            Mouse.MoveTo(newX, System.Windows.Forms.Cursor.Position.Y);
                            break; 
                        }
                    }
                }

                if (joystick.Xaxis == 65535)
                {
                    richTextBox1.AppendText("Right\n");

                    Screen[] screens = Screen.AllScreens;

                    int newX = System.Windows.Forms.Cursor.Position.X;

                    if (joystickButtons[(int)ControllerButtons.L2])
                    {
                        newX += sensitivity * sensitivityMultiply;
                    }
                    else if (joystickButtons[(int)ControllerButtons.R2])
                    {
                        newX += sensitivity / sensitivityDivider;
                    }
                    else
                    {
                        newX += sensitivity;
                    }

                    foreach (Screen screen in screens)
                    {
                        if (newX >= screen.Bounds.Left && newX <= screen.Bounds.Right)
                        {
                            Mouse.MoveTo(newX, System.Windows.Forms.Cursor.Position.Y);
                            break; 
                        }
                    }
                }

                if (joystick.Yaxis == 0)
                {
                    richTextBox1.AppendText("Up\n");

                    Screen[] screens = Screen.AllScreens;

                    int newY = System.Windows.Forms.Cursor.Position.Y;
                    int newX = System.Windows.Forms.Cursor.Position.X;

                    if (joystickButtons[(int)ControllerButtons.L2])
                    {
                        newY -= sensitivity * sensitivityMultiply;
                    }
                    else if (joystickButtons[(int)ControllerButtons.R2])
                    {
                        newY -= sensitivity / sensitivityDivider;
                    }
                    else
                    {
                        newY -= sensitivity;
                    }

                    foreach (Screen screen in screens)
                    {
                        if (newX >= screen.Bounds.Left && newX <= screen.Bounds.Right &&
                        newY >= screen.Bounds.Top && newY <= screen.Bounds.Bottom)
                        {
                            
                            Mouse.MoveTo(newX, newY);
                            break;
                        }
                    }
                }

                if (joystick.Xaxis >= 100 && joystick.Xaxis <= 10000 && joystick.Yaxis >= 50000 && joystick.Yaxis <= 63000)
                {
                    richTextBox1.AppendText("Diagonal Bottom Left\n");

                    Screen[] screens = Screen.AllScreens;

                    int newY = System.Windows.Forms.Cursor.Position.Y;
                    int newX = System.Windows.Forms.Cursor.Position.X;

                    if (joystickButtons[(int)ControllerButtons.L2])
                    {
                        newY += sensitivity * sensitivityMultiply;
                        newX -= sensitivity * sensitivityMultiply;
                    }
                    else if (joystickButtons[(int)ControllerButtons.R2])
                    {
                        newY += sensitivity / sensitivityDivider;
                        newX -= sensitivity / sensitivityDivider;
                    }
                    else
                    {
                        newY += sensitivity;
                        newX -= sensitivity;
                    }

                    foreach (Screen screen in screens)
                    {
                        if (newX >= screen.Bounds.Left && newX <= screen.Bounds.Right &&
                            newY >= screen.Bounds.Top && newY <= screen.Bounds.Bottom)
                        {
                            Mouse.MoveTo(newX, newY);
                            break; 
                        }
                    }
                }

                if (joystick.Xaxis >= 100 && joystick.Xaxis <= 10000 && joystick.Yaxis >= 100 && joystick.Yaxis <= 20000)
                {
                    richTextBox1.AppendText("Diagonal Top Left\n");

                    Screen[] screens = Screen.AllScreens;

                    int newY = System.Windows.Forms.Cursor.Position.Y;
                    int newX = System.Windows.Forms.Cursor.Position.X;

                    if (joystickButtons[(int)ControllerButtons.L2])
                    {
                        newY -= sensitivity * sensitivityMultiply;
                        newX -= sensitivity * sensitivityMultiply;
                    }
                    else if (joystickButtons[(int)ControllerButtons.R2])
                    {
                        newY -= sensitivity / sensitivityDivider;
                        newX -= sensitivity / sensitivityDivider;
                    }
                    else
                    {
                        newY -= sensitivity;
                        newX -= sensitivity;
                    }

                    foreach (Screen screen in screens)
                    {
                        if (newX >= screen.Bounds.Left && newX <= screen.Bounds.Right &&
                            newY >= screen.Bounds.Top && newY <= screen.Bounds.Bottom)
                        {
                            Mouse.MoveTo(newX, newY);
                            break; 
                        }
                    }
                }

                if (joystick.Xaxis >= 55000 && joystick.Xaxis <= 64535 && joystick.Yaxis >= 100 && joystick.Yaxis <= 13000)
                {
                    richTextBox1.AppendText("Diagonal Top Right\n");

                    Screen[] screens = Screen.AllScreens;

                    int newY = System.Windows.Forms.Cursor.Position.Y;
                    int newX = System.Windows.Forms.Cursor.Position.X;

                    if (joystickButtons[(int)ControllerButtons.L2])
                    {
                        newY -= sensitivity * sensitivityMultiply;
                        newX += sensitivity * sensitivityMultiply;
                    }
                    else if (joystickButtons[(int)ControllerButtons.R2])
                    {
                        newY -= sensitivity / sensitivityDivider;
                        newX += sensitivity / sensitivityDivider;
                    }
                    else
                    {
                        newY -= sensitivity;
                        newX += sensitivity;
                    }

                    foreach (Screen screen in screens)
                    {
                        if (newX >= screen.Bounds.Left && newX <= screen.Bounds.Right &&
                            newY >= screen.Bounds.Top && newY <= screen.Bounds.Bottom)
                        {
                            Mouse.MoveTo(newX, newY);
                            break; 
                        }
                    }
                }

                if (joystick.Xaxis >= 47300 && joystick.Xaxis <= 64535 && joystick.Yaxis >= 50000 && joystick.Yaxis <= 63000 )
                {
                    richTextBox1.AppendText("Diagonal Bottom Right\n");

                    Screen[] screens = Screen.AllScreens;

                    int newY = System.Windows.Forms.Cursor.Position.Y;
                    int newX = System.Windows.Forms.Cursor.Position.X;

                    if (joystickButtons[(int)ControllerButtons.L2])
                    {
                        newY += sensitivity * sensitivityMultiply;
                        newX += sensitivity * sensitivityMultiply;
                    }
                    else if (joystickButtons[(int)ControllerButtons.R2])
                    {
                        newY += sensitivity / sensitivityDivider;
                        newX += sensitivity / sensitivityDivider;
                    }
                    else
                    {
                        newY += sensitivity;
                        newX += sensitivity;
                    }

                    foreach (Screen screen in screens)
                    {
                        if (newX >= screen.Bounds.Left && newX <= screen.Bounds.Right &&
                            newY >= screen.Bounds.Top && newY <= screen.Bounds.Bottom)
                        {
                            Mouse.MoveTo(newX, newY);
                            break; 
                        }
                    }
                }

                if (joystick.Yaxis == 65535)
                {
                    richTextBox1.AppendText("Down\n");

                    Screen[] screens = Screen.AllScreens;

                    int newY = System.Windows.Forms.Cursor.Position.Y;
                    int newX = System.Windows.Forms.Cursor.Position.X;

                    if (joystickButtons[(int)ControllerButtons.L2])
                    {
                        newY += sensitivity * sensitivityMultiply;
                    }
                    else if (joystickButtons[(int)ControllerButtons.R2])
                    {
                        newY += sensitivity / sensitivityDivider;
                    }
                    else
                    {
                        newY += sensitivity;
                    }

                    foreach (Screen screen in screens)
                    {
                        if (newX >= screen.Bounds.Left && newX <= screen.Bounds.Right &&
                        newY >= screen.Bounds.Top && newY <= screen.Bounds.Bottom)
                        {
                            Mouse.MoveTo(newX, newY);
                            break;
                        }
                    }
                }

                if (joystickButtons[(int)ControllerButtons.X])
                {
                    mouse_event(LEFT_MOUSE_DOWN, 0, 0, 0, 0);
                    
                    isDragging = true;
                }

                if (isDragging == true && !joystickButtons[(int)ControllerButtons.X])
                {
                    mouse_event(LEFT_MOUSE_UP, 0, 0, 0, 0);
                    isDragging = false;
                }

                if (joystickButtons[(int)ControllerButtons.Triangle])
                {
                    mouse_event(RIGHT_MOUSE_DOWN, 0, 0, 0, 0);
                    mouse_event(RIGHT_MOUSE_UP, 0, 0, 0, 0);
                }

                if (joystickButtons[(int)ControllerButtons.TouchPad])
                {
                    keybd_event(VK_MEDIA_PLAY_PAUSE, MapVirtualKey(VK_MEDIA_PLAY_PAUSE, 0), KEYEVENTF_EXTENDEDKEY, 0);
                    keybd_event(VK_MEDIA_PLAY_PAUSE, MapVirtualKey(VK_MEDIA_PLAY_PAUSE, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                }

                if (joystickButtons[(int)ControllerButtons.Circle])
                {
                    keybd_event(VK_BROWSER_BACK, MapVirtualKey(VK_BROWSER_BACK, 0), KEYEVENTF_EXTENDEDKEY, 0);
                    keybd_event(VK_BROWSER_BACK, MapVirtualKey(VK_BROWSER_BACK, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                }

                if (joystickButtons[(int)ControllerButtons.Square])
                {
                    keybd_event(VK_BROWSER_FORWARD, MapVirtualKey(VK_BROWSER_FORWARD, 0), KEYEVENTF_EXTENDEDKEY, 0);
                    keybd_event(VK_BROWSER_FORWARD, MapVirtualKey(VK_BROWSER_FORWARD, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                }

                if (joystickButtons[(int)ControllerButtons.R3])
                {
                    keybd_event(VK_BROWSER_REFRESH, MapVirtualKey(VK_BROWSER_REFRESH, 0), KEYEVENTF_EXTENDEDKEY, 0);
                    keybd_event(VK_BROWSER_REFRESH, MapVirtualKey(VK_BROWSER_REFRESH, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                }

                if (joystickButtons[(int)ControllerButtons.Options] && joystickButtons[(int)ControllerButtons.PSN])
                {
                    Environment.Exit(0);
                }

                if (joystickButtons[(int)ControllerButtons.Share])
                {
                    string processName = "On Screen Keyboard";

                    if (IsProcessRunning(processName))
                    {
                        Process[] processes = Process.GetProcessesByName(processName);

                        if (processes.Length > 0)
                        {
                            foreach (Process process in processes)
                            {
                                process.Kill();
                                richTextBox1.AppendText("On Screen Keyboard Killed\n");
                            }
                        }

                    }
                    else
                    {
                        Process.Start("C:\\Users\\cecilie\\source\\repos\\On Screen Keyboard\\On Screen Keyboard\\bin\\Debug\\On Screen Keyboard.exe");
                        richTextBox1.AppendText("On Screen Keyboard Spawned\n");
                    }
                }

                if (joystickButtons[(int)ControllerButtons.L3])
                {
                    mouse_event(MIDDLE_MOUSE_DOWN, 0, 0, 0, 0);
                    mouse_event(MIDDLE_MOUSE_UP, 0, 0, 0, 0);

                }

                if (joystickButtons[(int)ControllerButtons.PSN])
                {
                    keybd_event(VK_LWIN, MapVirtualKey(VK_LWIN, 0), KEYEVENTF_EXTENDEDKEY, 0);
                    keybd_event(VK_LWIN, MapVirtualKey(VK_LWIN, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                }

                if (joystickButtons[(int)ControllerButtons.Options])
                {
                    keybd_event(VK_ESCAPE, MapVirtualKey(VK_ESCAPE, 0), KEYEVENTF_EXTENDEDKEY, 0);
                    keybd_event(VK_ESCAPE, MapVirtualKey(VK_ESCAPE, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                }

                if (joystickButtons[(int)ControllerButtons.L1])
                {
                    keybd_event(VK_PRIOR, MapVirtualKey(VK_PRIOR, 0), KEYEVENTF_EXTENDEDKEY, 0);
                    keybd_event(VK_PRIOR, MapVirtualKey(VK_PRIOR, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                }

                if (joystickButtons[(int)ControllerButtons.R1])
                {
                    keybd_event(VK_NEXT, MapVirtualKey(VK_NEXT, 0), KEYEVENTF_EXTENDEDKEY, 0);
                    keybd_event(VK_NEXT, MapVirtualKey(VK_NEXT, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                }

                for (int i = 0; i < joystickButtons.Length; i++)
                {
                    if (joystickButtons[i] == true)
                    {
                        richTextBox1.AppendText($"{ControllerButtonNames[i]}\n");
                    }
                }
            }
            catch
            {
                timer1.Enabled = false;
                //ConnectToJoystick(joystick);
            }
        }

        static bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                if (process.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = $"X: {System.Windows.Forms.Cursor.Position.X}";

            label2.Text = $"Y: {System.Windows.Forms.Cursor.Position.Y}";
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x84:
                    base.WndProc(ref m);
                    if ((int)m.Result == 0x1)
                        m.Result = (IntPtr)0x2;
                    return;
            }
            base.WndProc(ref m);
        }
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        protected void RePaint()
        {
            GraphicsPath graphicpath = new GraphicsPath();
            graphicpath.StartFigure();
            graphicpath.AddArc(0, 0, 25, 25, 180, 90);
            graphicpath.AddLine(25, 0, this.Width - 25, 0);
            graphicpath.AddArc(this.Width - 25, 0, 25, 25, 270, 90);
            graphicpath.AddLine(this.Width, 25, this.Width, this.Height - 25);
            graphicpath.AddArc(this.Width - 25, this.Height - 25, 25, 25, 0, 90);
            graphicpath.AddLine(this.Width - 25, this.Height, 25, this.Height);
            graphicpath.AddArc(0, this.Height - 25, 25, 25, 90, 90);
            graphicpath.CloseFigure();
            this.Region = new Region(graphicpath);
        }

        private void label3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                Properties.Settings.Default.Sinsitivity = int.Parse(textBox1.Text);
                Properties.Settings.Default.SinsitivityMultiply = int.Parse(textBox2.Text);
                Properties.Settings.Default.SinsitivityDivider = int.Parse(textBox3.Text);
                Properties.Settings.Default.Save();
            }
            catch
            {
                MessageBox.Show("Only Numbers");
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.TopMost = !this.TopMost;
            
            if(toolStripMenuItem1.CheckState == CheckState.Unchecked)
            {
                toolStripMenuItem1.CheckState = CheckState.Checked;
            }
            else if(toolStripMenuItem1.CheckState == CheckState.Checked)
            {
                toolStripMenuItem1.CheckState = CheckState.Unchecked;
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            isDebug = !isDebug;

            if (isDebug == true)
            {
                this.Size = new System.Drawing.Size(606, 246);
                richTextBox1.Visible = true;
            }
            else if(isDebug == false)
            {
                this.Size = new System.Drawing.Size(227, 246);
                richTextBox1.Visible = false;
            }

            if (toolStripMenuItem2.CheckState == CheckState.Unchecked)
            {
                toolStripMenuItem2.CheckState = CheckState.Checked;
            }
            else if (toolStripMenuItem2.CheckState == CheckState.Checked)
            {
                toolStripMenuItem2.CheckState = CheckState.Unchecked;
            }
        }
    }
}
