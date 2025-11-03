using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

public static class ConfettiEffect
{
    public static void Run(int durationMs = 6000, int particleCount = 10)
    {
        Thread thread = new Thread(() =>
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new ConfettiForm(durationMs, particleCount);
            form.Show();
            form.Activate(); // ép hiển thị lên trên
            Application.Run(form);
        });

        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    private class ConfettiForm : Form
    {
        class Particle
        {
            public PointF Position;
            public PointF Velocity;
            public float Size;
            public Color Color;
            public float Rotation;
            public float RotationSpeed;
            public float Life;
            public float Alpha;
        }

        private readonly List<Particle> particles = new List<Particle>();
        private readonly Random rnd = new Random();
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly Color[] colors =
        {
            Color.FromArgb(255,239,71),
            Color.FromArgb(255,121,198),
            Color.FromArgb(79,195,247),
            Color.FromArgb(129,199,132),
            Color.FromArgb(255,183,77),
            Color.FromArgb(205,220,57),
            Color.Magenta,
            Color.Coral
        };

        private double lastTime;
        private readonly int durationMs;
        private readonly int spawnRate;
        private double elapsed;

        public ConfettiForm(int durationMs, int spawnRate)
        {
            this.durationMs = durationMs;
            this.spawnRate = spawnRate;

            FormBorderStyle = FormBorderStyle.None;
            Bounds = Screen.PrimaryScreen.Bounds;
            ShowInTaskbar = false;
            TopMost = true;
            BackColor = Color.Lime;
            TransparencyKey = Color.Lime;
            DoubleBuffered = true;

            // Làm form luôn nằm trên tất cả
            int initialStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, initialStyle | WS_EX_TOOLWINDOW | WS_EX_TOPMOST | WS_EX_LAYERED);

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            Load += (_, __) =>
            {
                stopwatch.Start();
                Application.Idle += GameLoop;

                var closeTimer = new System.Windows.Forms.Timer { Interval = durationMs };
                closeTimer.Tick += (s, e) =>
                {
                    closeTimer.Stop();
                    this.Invoke(new Action(Close));
                };
                closeTimer.Start();
            };
        }

        private void GameLoop(object sender, EventArgs e)
        {
            while (AppStillIdle)
            {
                double now = stopwatch.Elapsed.TotalSeconds;
                float dt = (float)(now - lastTime);
                lastTime = now;
                elapsed += dt;

                if (stopwatch.ElapsedMilliseconds < durationMs)
                {
                    for (int i = 0; i < spawnRate; i++)
                        SpawnConfetti();
                }

                UpdateParticles(dt);
                Invalidate();
                Thread.Sleep(10);
            }
        }

        private void SpawnConfetti()
        {
            int width = Screen.PrimaryScreen.Bounds.Width;
            int x = rnd.Next(width);
            int y = rnd.Next(50);

            float speed = (float)(rnd.NextDouble() * 200 + 50);
            float vx = (float)((rnd.NextDouble() - 0.5) * 100);
            float vy = (float)(rnd.NextDouble() * 150 + 50);

            var p = new Particle
            {
                Position = new PointF(x, y),
                Velocity = new PointF(vx, vy),
                Size = (float)(rnd.NextDouble() * 6 + 6),
                Color = colors[rnd.Next(colors.Length)],
                Rotation = (float)(rnd.NextDouble() * 360),
                RotationSpeed = (float)(rnd.NextDouble() * 400 - 200),
                Life = (float)(rnd.NextDouble() * 3 + 1),
                Alpha = 1f
            };
            particles.Add(p);
        }

        private void UpdateParticles(float dt)
        {
            const float gravity = 400f;
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i];
                p.Velocity = new PointF(p.Velocity.X, p.Velocity.Y + gravity * dt);
                p.Position = new PointF(p.Position.X + p.Velocity.X * dt, p.Position.Y + p.Velocity.Y * dt);
                p.Rotation += p.RotationSpeed * dt;
                p.Life -= dt;
                p.Alpha = Math.Max(0, p.Life / 3f);

                if (p.Life <= 0 || p.Position.Y > this.Height + 50)
                    particles.RemoveAt(i);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            foreach (var p in particles)
            {
                var state = e.Graphics.Save();
                e.Graphics.TranslateTransform(p.Position.X, p.Position.Y);
                e.Graphics.RotateTransform(p.Rotation);
                using (var brush = new SolidBrush(Color.FromArgb((int)(255 * p.Alpha), p.Color)))
                {
                    e.Graphics.FillRectangle(brush, -p.Size / 2, -p.Size / 2, p.Size, p.Size * 1.5f);
                    e.Graphics.Restore(state);
                }
            }
        }

        private static bool AppStillIdle
        {
            get
            {
                PeekMessage(out _, IntPtr.Zero, 0, 0, 0);
                return true;
            }
        }

        // native WinAPI để ép topmost
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_LAYERED = 0x00080000;

        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")] private static extern bool PeekMessage(out NativeMessage msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeMessage
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point p;
        }
    }
}
