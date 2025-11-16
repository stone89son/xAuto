using ApSetting.Forms;
using System;
using System.Threading;
using System.Windows.Forms;

namespace ApSetting
{
    internal static class NotificationManager
    {
        private static Thread _uiThread;
        private static fCustomNotification _form;
        private static readonly object _lock = new object();
        private static ManualResetEvent _initEvent = new ManualResetEvent(false);

        public static void Start()
        {
            lock (_lock)
            {
                if (_form != null && !_form.IsDisposed) return;

                _initEvent.Reset();
                _uiThread = new Thread(() =>
                {
                    // Create form on this STA thread and run message loop
                    _form = new fCustomNotification();
                    _form.FormClosed += (s, e) =>
                    {
                        // Ensure thread message loop exits when form closed
                        Application.ExitThread();
                    };
                    _form.Shown += (s, e) =>
                    {
                        _initEvent.Set();
                    };
                    Application.Run(_form);
                });
                _uiThread.IsBackground = true;
                _uiThread.SetApartmentState(ApartmentState.STA);
                _uiThread.Start();

                // Wait briefly for the form to be created
                _initEvent.WaitOne(5000);
            }
        }

        public static void ShowMessage(string message)
        {
            Start();
            var f = _form;
            if (f == null) return;

            if (f.IsHandleCreated && !f.IsDisposed)
            {
                try
                {
                    f.BeginInvoke((Action)(() =>
                    {
                        if (f.IsDisposed) return;
                        f.UpdateMessage(message);
                        f.TopMost = true;
                        // bring to front without stealing focus
                        f.BringToFront();
                    }));
                }
                catch { /* ignore invoke errors during shutdown */ }
            }
        }

        public static void Close()
        {
            var f = _form;
            if (f == null) return;

            if (f.IsHandleCreated && !f.IsDisposed)
            {
                try
                {
                    f.BeginInvoke((Action)(() =>
                    {
                        if (!f.IsDisposed) f.Close();
                    }));
                }
                catch { /* ignore */ }
            }
            _form = null;
        }
    }
}