using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeyboardHook1;

namespace Windows_DLL_Function_Testing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        KeyboardHook _listener;
        List<KeyboardHook.VKeys> KeysList;

        protected override CreateParams CreateParams //Hide in Alt-Tab (If pressing alt-tab while it's showing, it won't work until hiding again)
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }


        //--------------
        //WINDOW RELATED
        //--------------
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags); //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos
        const short HWND_BOTTOM = 1; //SET TO BOTTOM UNTIL REOPENED TO TOP
        const short HWND_NOTOPMOST = -2; //TRYS TO ALWAYS BE AT BOTTOM
        const short HWND_TOP = 0; //SETS TO TOP UNTIL DIFFERENT PROGRAM OPENED TO TOP
        const short HWND_TOPMOST = -1; // KEEP PROGRAM AT TOP ALWAYS

        //NOSIZE + NOMOVE = KEEPTOPMOST
        //Not all of the SetWindowsPos Flags:
        const short SWP_NOMOVE = 0X2;
        const short SWP_NOSIZE = 1;
        const short SWP_NOZORDER = 0X4;
        const short SWP_SHOWWINDOW = 0x0040;

        //---

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags); //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setlayeredwindowattributes?redirectedfrom=MSDN
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;

        [DllImport("user32.dll", EntryPoint = "SetWindowLongA", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong); //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowlonga

        [DllImport("user32.dll", EntryPoint = "GetWindowLongA", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hwnd, int nIndex); //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowlonga

        //---
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetFocus(IntPtr hWnd); //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setfocus

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow(); //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdesktopwindow

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window); //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowdc



        //--------------
        //CURSOR RELATED
        //--------------
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref Point lpPoint); //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcursorpos

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y); //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setcursorpos


        int cursorValue;
        [DllImport("user32.dll")]
        static extern int ShowCursor(bool bShow); //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showcursor

        //----------------
        //GRPAHICS RELATED
        //----------------
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd); //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdc

        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc); //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-releasedc

        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos); //https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-getpixel

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop); //https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-bitblt

        ColorViewForm cvf;
        CursorPointForm cpf;

        bool ColorViewActive;
        bool CVJustDeactivated;

        bool CursorPointActive;
        bool CPJustDeactivated;

        bool CursorPixColorActive;
        bool CPCJustDeactivated;

        Point pt1;
        IntPtr FormHandle;
        //SEE NOTE WIN.1
        private void Form1_Load(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            UsingColorViewForm(true);
            UsingCursorPoint(true, Color.Red, 1, 1);
            
            this.Location = new Point(0, 0);
            this.Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            this.WindowState = FormWindowState.Maximized;
            FormHandle = this.Handle;
            SetWindowPos(FormHandle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);


            //Debug.WriteLine(Screen.PrimaryScreen.Bounds.Width + "   " + Screen.PrimaryScreen.Bounds.Height);
            pt1 = new Point();
            _listener = new KeyboardHook();
            KeysList = new List<KeyboardHook.VKeys>();
            _listener.Install();
            _listener.KeyDown += _listener_KeyDown;
            _listener.KeyUp += _listener_KeyUp;

            MakeFormTransparent();

            t1.Enabled = true;
            t1.Interval = 10;
            this.FormClosing += (osender, eargs) =>
            {
                t1.Tick -= t1_Tick;
                _listener.Uninstall();
            };
        }

        bool CVFMHoverEnabled;
        public void UsingColorViewForm(bool boolean)
        {
            if (boolean)
            {
                //cvf = new ColorViewForm("MHover");
                cvf = new ColorViewForm("MHover");

                if (cvf.pposition == "MHover")
                {
                    CVFMHoverEnabled = true;
                }

                SetWindowPos(cvf.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            }
        }

        public void UsingCursorPoint(bool boolean, Color color, int width, int height)
        {
            if (boolean)
            {
                cpf = new CursorPointForm(width, height);
                cpf.BackColor = color;
                cpf.FormBorderStyle = FormBorderStyle.None;
                SetWindowPos(cpf.Handle, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            }
        }

        public void MakeFormTransparent()
        {
            SetWindowLong(this.Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) ^ WS_EX_LAYERED);
            SetLayeredWindowAttributes(Handle, 0, 1, LWA_ALPHA);
        }

        Color MousePixelColor;
        bool MousePixelPressed;
        bool MousePixelPressingStopped;
        public void GetPixelAtMousePoint()
        {
            this.Activate();
            if (!MousePixelPressingStopped)
            {
                if (!MousePixelPressed)
                {
                    SetWindowPos(FormHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE); //Not going to hide cursor cause it doesn't matter
                    MousePixelPressed = true;
                }

                GetCursorPos(ref pt1);
                MousePixelColor = GetColorAt(pt1);
                Debug.WriteLine("The Current Color at Mouse Cursor is: " + MousePixelColor);
            }
            else
            {
                SetWindowPos(FormHandle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                MousePixelPressed = false;
                MousePixelPressingStopped = false;
            }

            //if (!MousePixelPressingStopped)
            //{
            //    if (!MousePixelPressed)
            //    {
            //        SetWindowPos(FormHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE); // SWP_NOZORDER - DON'T USE, SWP_SHOWWINDOW - NOT NEEDED I GUESS
            //        ShowCursor(false);
            //        MousePixelPressed = true;
            //    }

            //    GetCursorPos(ref pt1);
            //    IntPtr hdc = GetDC(IntPtr.Zero);
            //    uint pixel = GetPixel(hdc, pt1.X, pt1.Y);
            //    ReleaseDC(IntPtr.Zero, hdc);
            //    MousePixelColor = Color.FromArgb((int)pixel);
            //    Console.WriteLine("Color at CURSOR is {0}", MousePixelColor);
            //}
            //else
            //{
            //    SetWindowPos(FormHandle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            //    ShowCursor(true);
            //    MousePixelPressed = false;
            //    MousePixelPressingStopped = false;
            //}
        }

        Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        public Color GetColorAt(Point location)
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return screenPixel.GetPixel(0, 0);
        }

        //public static Color GetColorAt1(int x, int y) //might be better or not
        //{
        //    IntPtr desk = GetDesktopWindow();
        //    IntPtr dc = GetWindowDC(desk);
        //    int a = (int)GetPixel(dc, x, y);
        //    ReleaseDC(desk, dc);
        //    return Color.FromArgb(255, (a >> 0) & 0xff, (a >> 8) & 0xff, (a >> 16) & 0xff);
        //}

        bool cvfShown;

        bool cpfShown;
        Point cpfPoint;

        int cvfcommonXConfig = 20 - 70 - 1;
        int cvfcommonYConfig = - 10 + 60 - 1;
        private void _listener_KeyDown(KeyboardHook.VKeys key)
        {
            if (!KeysList.Contains(key))
            {
                KeysList.Add(key);
            }

            if (KeysList.Contains(KeyboardHook.VKeys.KEY_C) && KeysList.Contains(KeyboardHook.VKeys.KEY_G))
            {
                if (!CursorPixColorActive)
                {
                    CursorPixColorActive = true;
                }
            }

            if (KeysList.Contains(KeyboardHook.VKeys.KEY_P) && KeysList.Contains(KeyboardHook.VKeys.KEY_C))
            {
                if (!CursorPointActive)
                {
                    CursorPointActive = true;
                }
            }

            if (KeysList.Contains(KeyboardHook.VKeys.LCONTROL) && KeysList.Contains(KeyboardHook.VKeys.LSHIFT) && KeysList.Contains(KeyboardHook.VKeys.KEY_G))
            {
                if (!ColorViewActive)
                {
                    ColorViewActive = true;
                }
            }

            if (KeysList.Contains(KeyboardHook.VKeys.LWIN))
            {
                Thread.Sleep(2000);
            }

            if (KeysList.Contains(KeyboardHook.VKeys.LCONTROL) && KeysList.Contains(KeyboardHook.VKeys.LSHIFT) && KeysList.Contains(KeyboardHook.VKeys.KEY_F))
            {
                GetCursorPos(ref pt1);
                Debug.WriteLine("Position At Press: " + pt1);
            }
        }

        private void _listener_KeyUp(KeyboardHook.VKeys key)
        {
            KeysList.Remove(key);

            if (key != KeyboardHook.VKeys.KEY_G || key != KeyboardHook.VKeys.KEY_C)
            {
                CPCJustDeactivated = true;
            }

            if (cpfShown && (key != KeyboardHook.VKeys.KEY_C || key != KeyboardHook.VKeys.KEY_P))
            {
                CPJustDeactivated = true;
            }

            if (MousePixelPressed && (key != KeyboardHook.VKeys.LCONTROL || key != KeyboardHook.VKeys.LSHIFT || key != KeyboardHook.VKeys.KEY_G))
            {
                CVJustDeactivated = true;
            }
        }

        //bool Finished;
        //public void TestFunction() //doesn't really work, bitbtl makes so slow.
        //{
        //    cvf.Show();
        //    for (int h = 0; h < 1439; h += 5)
        //    {
        //        if (Finished)
        //        {
        //            break;
        //        }

        //        for (int w = 0; w < 2559; w += 5)
        //        {
        //            if (Finished)
        //            {
        //                break;
        //            }

        //            SetCursorPos(w, h);

        //            MousePixelColor = GetColorAt1(w, h);
        //            cvf.BackColor = Color.FromArgb(MousePixelColor.R, MousePixelColor.G, MousePixelColor.B);

        //            Debug.WriteLine(MousePixelColor);

        //            if (MousePixelColor.R == 74 && MousePixelColor.G == 138 && MousePixelColor.B == 255)
        //            {
        //                Finished = true;
        //                SetCursorPos(w, h);
        //                Debug.WriteLine("DONE");
        //            }
        //        }
        //    }

        //    Debug.WriteLine("COULD NOT FIND");
        //}

        bool runOnce;
        private void t1_Tick(object sender, EventArgs e)
        {
            //if (!runOnce)
            //{
            //    TestFunction();
            //    runOnce = true;
            //}

            if (CursorPixColorActive)
            {
                GetPixelAtMousePoint();

                if (CPCJustDeactivated)
                {
                    CursorPixColorActive = false;
                    CPCJustDeactivated = false;

                    MousePixelPressingStopped = true;
                    GetPixelAtMousePoint();
                    SetWindowPos(FormHandle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                }
            }

            if (CursorPointActive)
            {
                if (cpf != null)
                {
                    if (!cpfShown)
                    {
                        this.Activate();
                        SetWindowPos(FormHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                        SetFocus(FormHandle);
                        if (cursorValue > -1)
                        {
                            cursorValue = ShowCursor(false);
                        }
                        Debug.WriteLine("Cursor Hidden");
                        cpf.Show();
                        cpfShown = true;
                    }

                    GetCursorPos(ref pt1);
                    if (cpfPoint.X != pt1.X && cpfPoint.Y != pt1.Y)
                    {
                        cpf.Location = pt1;
                    }
                }
                else
                {
                    Debug.WriteLine("THE \"CURSOR POINT\" FUNCTION IS CURRENTLY DISABLED.");
                }

                if (CPJustDeactivated)
                {
                    CursorPointActive = false;
                    CPJustDeactivated = false;

                    cpf.Hide();
                    cpfShown = false;
                    if (cursorValue < 0)
                    {
                        cursorValue = ShowCursor(true);

                    }
                    Debug.WriteLine("Cursor Shown");

                    SetWindowPos(FormHandle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                }
            }

            if (ColorViewActive)
            {
                if (!cvfShown)
                {
                    this.Activate();
                    SetWindowPos(FormHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                    //ShowCursor(false);
                    cvf.Show();
                    cvfShown = true;
                }

                GetPixelAtMousePoint();//Just calling function to get mouse pixel
                cvf.BackColor = MousePixelColor;

                if (CVFMHoverEnabled) //may need to make compatable with all screens!
                {
                    if (pt1.X < Screen.PrimaryScreen.Bounds.Width - cvf.Size.Width + cvfcommonXConfig && pt1.Y > cvf.Height + cvfcommonYConfig) //no issue
                    {
                        cvf.Location = new Point(pt1.X + 20, pt1.Y - cvf.Height - 10);
                    }
                    else if ((pt1.X < Screen.PrimaryScreen.Bounds.Width - cvf.Size.Width + cvfcommonXConfig) && !(pt1.Y > cvf.Height + cvfcommonYConfig)) //Top issue
                    {
                        cvf.Location = new Point(pt1.X + 20, cvf.Height + cvfcommonYConfig + 20);
                    }
                    else if (!(pt1.X < Screen.PrimaryScreen.Bounds.Width - cvf.Size.Width + cvfcommonXConfig) && (pt1.Y > cvf.Height + cvfcommonYConfig)) //Right issue
                    {
                        cvf.Location = new Point(Screen.PrimaryScreen.Bounds.Width - cvf.Size.Width * 2 + cvfcommonXConfig - 15, pt1.Y - cvf.Height - 10);
                    }
                    else if (!(pt1.X < Screen.PrimaryScreen.Bounds.Width - cvf.Size.Width + cvfcommonXConfig) && !(pt1.Y > cvf.Height + cvfcommonYConfig)) //Top and Right issue
                    {
                        cvf.Location = new Point(Screen.PrimaryScreen.Bounds.Width - cvf.Size.Width * 2 + cvfcommonXConfig - 10, cvf.Height + cvfcommonYConfig + 20);
                    }

                }
                
                if (CVJustDeactivated)
                {
                    ColorViewActive = false;
                    CVJustDeactivated = false;

                    Debug.WriteLine("Key Released");
                    MousePixelPressingStopped = true;
                    GetPixelAtMousePoint();
                    cvf.Hide();
                    cvfShown = false;
                    SetWindowPos(FormHandle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                    if (cursorValue < 0)
                    {
                        cursorValue = ShowCursor(true);
                    }
                    Debug.WriteLine("Cursor Shown");

                    SendKeys.Flush(); //may need to remove, just added to fix sticky keys
                }
            }
        }
    }
}

//INDIVIDUAL KEY MEANINGS:
//G: Graphics
//C: Cursor
//P: Point, Relating to Point();

//LIST OF KEYBINDS:
// G + C = Graphics at Cursor (Pixel Color)
// C + P = Point at cursor
// LCTRL + LSHIFT + G = Show Graphics at Cursor in ColorViewForm
//LCTRL + LSHIFT + F = GET CURSOR POS ON PRESS


//NOTES:
//CSR:



//GFX:



//WIN:
//1. CAN CONTROL OTHER WINDOWS:
//{IntPtr ProcessHandle; 
//EX: If you use Process p = new Process()
//p.Filename = "%CHROME DIRECTORY%\CHROME.EXE";
//ProcessHandle = p.MainWindowHandle; //This might be how it works, I think as long as you assign above it will work properly.
//SetWindowPos(arguments); //Control the window pos and location ehre.
//}


