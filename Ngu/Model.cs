using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Threading;
using Gma.System.MouseKeyHook;
using System.Linq;
using System.Text;
//using System.Reactive.Linq; // TODO removed - unneccesary for current version
//using System.Windows;

namespace Ngu
{
    public class Model : ObservableObject
    {
        #region Simulating clicks and DLL imports
        [DllImport("User32.dll")]
        static extern int                                       SetForegroundWindow(IntPtr point);

        //https://stackoverflow.com/questions/2736965/how-to-programatically-trigger-a-mouse-left-click-in-c
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void                               mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        // Simulating mouse actions
        private const int                                       MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int                                       MOUSEEVENTF_LEFTUP = 0x04;
        private const int                                       MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int                                       MOUSEEVENTF_RIGHTUP = 0x10;

        /// <summary>
        /// https://stackoverflow.com/questions/1483928/how-to-read-the-color-of-a-screen-pixel
        /// </summary>
        /// <param name="lpPoint"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern bool                                      GetCursorPos(ref Point lpPoint);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int                                BitBlt(
                                                                    IntPtr hDC, 
                                                                        int x, 
                                                                            int y, 
                                                                                int nWidth, 
                                                                                    int nHeight, 
                                                                                        IntPtr hSrcDC, 
                                                                                            int xSrc, 
                                                                                                int ySrc, 
                                                                                                    int dwRop);
        public static void                                      MouseClick()
        {
            //Call the imported function with the cursor's current position
            uint X = (uint)Cursor.Position.X;
            uint Y = (uint)Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
        }
        #endregion

        #region Hooks - Recognizing mouse clicks/events outside of app window
        private IKeyboardMouseEvents                            hooks;
        public void                                             SubscribeHooks()
        {
            hooks = Hook.GlobalEvents(); //Hook.AppEvents()
            hooks.MouseDoubleClick += SetupBossPixel_DoubleClick;
            hooks.MouseWheelExt += SetupBoostPoints_MouseWheelExt;
        }        
        public void                                             UnsubscribeHooks()
        {
            hooks.MouseDoubleClick -= SetupBossPixel_DoubleClick;
            hooks.Dispose();
        }
        #endregion

        #region Mouse/position/color helpers
        public Color                                            GetColorAt(Point location)
        {
            var screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb); // TODO if problems, make global
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
        private void                                            SetupBoostPoints_MouseWheelExt(object sender, MouseEventExtArgs e)
        {
            if (SetupComplete)
                return;

            //e.Handled = true;
            var point = Cursor.Position;
            if (BoostSlots.Add(point))
                AddAlertMessage($"Boost item location added: {point}");
        }
        private void                                            SetupBossPixel_DoubleClick(object sender, MouseEventArgs e)
        {
            SaveInputPositionColor(e);
        }
        #endregion

        #region Routine properties
        private object                                          dataLock = new object();
        private string                                          shortRoutineTimer;
        public string                                           ShortRoutineTimer { get => shortRoutineTimer; set { if (value != shortRoutineTimer) { shortRoutineTimer = value; OnPropertyChanged(); } } }
        private int                                             ShortRoutineTimerInt
        {
            get
            {
                if (int.TryParse(ShortRoutineTimer, out int srt))
                    return srt;
                else
                {
                    AddAlertMessage($"Could not parse Short routine timer input: {ShortRoutineTimer}");
                    return 200;
                }
            }
        }
        private string                                          longRoutineTimer;
        public string                                           LongRoutineTimer { get => longRoutineTimer; set { if (value != longRoutineTimer) { longRoutineTimer = value; OnPropertyChanged(); } } }
        private int                                             LongRoutineTimerInt
        {
            get
            {
                if (int.TryParse(LongRoutineTimer, out int srt))
                    return srt;
                else
                {
                    AddAlertMessage($"Could not parse Long routine timer input: {LongRoutineTimer}");
                    return 3500;
                }
            }
        }
        private Pixel                                           bossPixel = null;
        public Pixel                                            BossPixel { get => bossPixel; set { if (value != bossPixel) { bossPixel = value; OnPropertyChanged(); } } }
        public HashSet<Point>                                   BoostSlots { get; set; } = new HashSet<Point>();
        public ObservableCollection<string>                     AlertMessages { get; set; } = new ObservableCollection<string>();
        //private bool                                            setupComplete = false;
        public bool                                             SetupComplete { get; internal set; }
        #endregion

        public                                                  Model()
        {
            ShortRoutineTimer = "200";
            LongRoutineTimer = "3500";
            BindingOperations.EnableCollectionSynchronization(AlertMessages, dataLock);
            SubscribeHooks();
        }
        internal void                                           Shutdown()
        {
            try
            {
                UnsubscribeHooks();
            }
            finally
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => System.Windows.Application.Current.Shutdown());
            }
        }

        #region Routine methods
        // Logic
        public void                                             Snipe(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (BossPixel == null)
                {
                    var sb = new StringBuilder();
                    sb.Append($"Sniping bosses, not setup." + Environment.NewLine);
                    sb.Append("First navigate to adventure page zone you want to farm." + Environment.NewLine);
                    sb.Append("Then wait for boss and doubleclick crown when appears to start." + Environment.NewLine);
                    sb.Append("Make sure you can kill the bosses successively without dying as no checks are made" + Environment.NewLine);
                    AddAlertMessage(sb.ToString());

                    Thread.Sleep(LongRoutineTimerInt);
                    continue;
                }

                var currentColor = GetColorAt(BossPixel.Point);
                if (currentColor != BossPixel.Color)
                    SwitchEnemy();
                else
                    AddAlertMessage($"Fighting boss: {currentColor} vs {bossPixel.Color}.");
                
                // Enemy switchnig takes about 3:40seconds
                //TODO test
                Thread.Sleep(LongRoutineTimerInt);
            }
            // in case we want to start again
            BossPixel = null;
            AddAlertMessage($"Boss crown pixel deleted: {BossPixel}");
        }
        public void                                             SwitchEnemy(string processName = "NGUIdle")
        {
            AddAlertMessage("Not a boss. Switching");

            var nguProcess = Process.GetProcessesByName(processName).FirstOrDefault();
            if (nguProcess != null)
            {
                IntPtr windowHandle = nguProcess.MainWindowHandle;
                SetForegroundWindow(windowHandle);
                Thread.Sleep(ShortRoutineTimerInt);
                SendKeys.SendWait("{LEFT}"); // left first, as it is always possible to go to earlier zone first
                Thread.Sleep(ShortRoutineTimerInt);
                Thread.Sleep(ShortRoutineTimerInt);
                SendKeys.SendWait("{RIGHT}");
                Thread.Sleep(ShortRoutineTimerInt);
            }
        }
        public void                                             Boost(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!BoostSlots.Any())
                {
                    var sb = new StringBuilder();
                    sb.Append("Boosting not setup." + Environment.NewLine);
                    sb.Append("Use mouse wheel (down/up) to save positions of items to boost." + Environment.NewLine);
                    sb.Append("First select the location of 'Inventory' tab in the left menu." + Environment.NewLine);
                    sb.Append("Then set locations of all inventory items to be boosted." + Environment.NewLine);
                    sb.Append("No limit on number, but doesn't work on inventory pages other than first." + Environment.NewLine);
                    AddAlertMessage(sb.ToString());

                    //AddAlertMessage("Boosting not setup. Use mouse wheel (down/up) to save positions of items to boost. " +
                    //    "First select the location of 'Inventory' tab in the left menu. " +
                    //    "Then set locations of all inventory items to be boosted. " +
                    //    "No limit on number, but works only with equipped items and first inv page. " +
                    //    "When finished, click 'Setup complete' in order to free your mouse wheel again.");
                    Thread.Sleep(LongRoutineTimerInt);
                    continue;
                }
                AddAlertMessage("Boosting in 5 seconds");
                Thread.Sleep(1000);
                AddAlertMessage("Boosting in 4 seconds");
                Thread.Sleep(1000);
                AddAlertMessage("Boosting in 3 seconds");
                Thread.Sleep(1000);
                AddAlertMessage("Boosting in 2 seconds");
                Thread.Sleep(1000);
                AddAlertMessage("Boosting in 1 seconds");
                Thread.Sleep(1000);

                foreach (var point in BoostSlots)
                {
                    SendMouseKey(point, "A");
                    AddAlertMessage($"Boosting {point}");
                    Thread.Sleep(ShortRoutineTimerInt);
                }
                AddAlertMessage("Finished boosting run.");
                // 340.000 secs, ie about 6mins with alert
                Thread.Sleep(LongRoutineTimerInt * 100);

                AddAlertMessage("Boosting in 1 min");
                Thread.Sleep(60000);
            }
            BoostSlots.Clear();
            AddAlertMessage($"Cleared all boost items' positions: {BoostSlots}");
        }
        // Status updates
        public void                                             AddAlertMessage(string alert)
            => AlertMessages.Insert(0, alert); // inserting at the front of list.. hmm, naughty.. but w/e, never gets large enough to matter
        // Helpes
        public void                                             SetDesiredPixel()
        {
            // TODO - add logic for saving/retrieving user defined default pixel
            // textbox to copy/paste encoded pixel

            //BossPixel = new Pixel(new Point(1033, 474), Color.FromArgb(247, 239, 41));
            //AddAlertMessage($"Set pixel for Sniping complete: {BossPixel}.");
        }
        public Pixel                                            GetInputPositionColor()
        {
            var point = new Point();
            GetCursorPos(ref point);
            var color = GetColorAt(point);
            var pixel = new Pixel(point, color);

            AddAlertMessage($"Got pixel: {pixel}");
            return pixel;
        }
        public void                                             SaveInputPositionColor(MouseEventArgs mouseArgs)
        {
            //setup first time only
            if (BossPixel != null)
                return;

            BossPixel = GetInputPositionColor();
            AddAlertMessage($"Boss crown pixel saved: {BossPixel}");
            //setupComplete = true;
        }
        public void                                             SendMouseKey(Point? point, string key = "A", string processName = "NGUIdle")
        {
            if (point == null)
                return;

            var nguProcess = Process.GetProcessesByName(processName).FirstOrDefault();
            if (nguProcess != null)
            {
                IntPtr windowHandle = nguProcess.MainWindowHandle;
                SetForegroundWindow(windowHandle);
                Cursor.Position = (Point)point;
                MouseClick();
                Thread.Sleep(ShortRoutineTimerInt);
                SendKeys.SendWait(key);
                Thread.Sleep(ShortRoutineTimerInt);
            }
        }
        #endregion
    }
}
