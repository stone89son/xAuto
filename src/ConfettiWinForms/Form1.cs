using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfettiWinForms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    namespace ConfettiWinForms
    {
        public partial class Form1 : Form
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
            }

            private readonly List<Particle> particles = new List<Particle>();
            private readonly Random rnd = new Random();
            private readonly Color[] palette = new[]
            {
            Color.FromArgb(255, 239, 71),
            Color.FromArgb(255, 121, 198),
            Color.FromArgb(79, 195, 247),
            Color.FromArgb(129, 199, 132),
            Color.FromArgb(255, 183, 77),
            Color.FromArgb(205, 220, 57),
            Color.Magenta,
            Color.Coral
        };

            private readonly Stopwatch stopwatch = new Stopwatch();
            private double lastTime = 0;

            public Form1()
            {
                InitializeComponent();

                // Tối ưu vẽ (double buffering)
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
                this.UpdateStyles();

                this.BackColor = Color.White;
                this.ClientSize = new Size(800, 600);
                this.Text = "Confetti Effect - Smooth Version";

                Application.Idle += GameLoop;
                stopwatch.Start();

                // Click chuột để tạo confetti
                this.MouseClick += (s, e) => StartConfetti(e.Location, 150);

                // Tự động tạo 1 lần khi khởi động (nếu muốn)
                // StartConfetti(new Point(ClientSize.Width / 2, 100), 200);
            }

            // Game loop
            private void GameLoop(object sender, EventArgs e)
            {
                while (AppStillIdle)
                {
                    double now = stopwatch.Elapsed.TotalSeconds;
                    float dt = (float)(now - lastTime);
                    lastTime = now;

                    UpdateParticles(dt);
                    Invalidate();
                }
            }

            private void UpdateParticles(float dt)
            {
                const float gravity = 400f;
                const float drag = 1.0f;

                for (int i = particles.Count - 1; i >= 0; i--)
                {
                    var p = particles[i];
                    // Áp dụng trọng lực
                    p.Velocity = new PointF(p.Velocity.X * (1 - drag * dt * 0.5f), p.Velocity.Y + gravity * dt);

                    // Cập nhật vị trí (có damping nhẹ)
                    p.Position = new PointF(
                        p.Position.X + p.Velocity.X * dt * 0.95f,
                        p.Position.Y + p.Velocity.Y * dt * 0.95f
                    );

                    // Quay mảnh giấy
                    p.Rotation += p.RotationSpeed * dt;

                    // Giảm thời gian sống
                    p.Life -= dt;
                    if (p.Life <= 0 || p.Position.Y > this.ClientSize.Height + 50 || p.Position.X < -50 || p.Position.X > this.ClientSize.Width + 50)
                    {
                        particles.RemoveAt(i);
                    }
                }
            }

            public void StartConfetti(Point origin, int count = 100)
            {
                for (int i = 0; i < count; i++)
                {
                    var angle = (float)((rnd.NextDouble() * Math.PI) - Math.PI / 2.0);
                    var speed = (float)(rnd.NextDouble() * 300 + 100);
                    float vx = (float)(Math.Cos(angle) * speed + (rnd.NextDouble() - 0.5) * 100);
                    float vy = (float)(Math.Sin(angle) * speed * 0.6 + (rnd.NextDouble() * -150));

                    var p = new Particle
                    {
                        Position = new PointF(origin.X + (float)(rnd.NextDouble() * 40 - 20), origin.Y + (float)(rnd.NextDouble() * 20 - 10)),
                        Velocity = new PointF(vx, vy),
                        Size = (float)(rnd.NextDouble() * 6 + 6),
                        Color = palette[rnd.Next(palette.Length)],
                        Rotation = (float)(rnd.NextDouble() * 360),
                        RotationSpeed = (float)(rnd.NextDouble() * 400 - 200),
                        Life = (float)(rnd.NextDouble() * 2.5 + 1.0)
                    };
                    particles.Add(p);
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;

                foreach (var p in particles)
                {
                    var state = e.Graphics.Save();

                    e.Graphics.TranslateTransform(p.Position.X, p.Position.Y);
                    e.Graphics.RotateTransform(p.Rotation);

                    var rect = new RectangleF(-p.Size / 2, -p.Size / 2, p.Size, p.Size * 1.6f);
                    using (var brush = new SolidBrush(p.Color))
                        e.Graphics.FillRectangle(brush, rect);

                    e.Graphics.Restore(state);
                }

                using (var f = new Font("Segoe UI", 9))
                using (var b = new SolidBrush(Color.FromArgb(120, Color.Black)))
                    e.Graphics.DrawString($"Particles: {particles.Count}", f, b, 6, 6);
            }

            // Kiểm tra Idle state
            private static bool AppStillIdle
            {
                get
                {
                    PeekMessage(out _, IntPtr.Zero, 0, 0, 0);
                    return true;
                }
            }

            [DllImport("user32.dll")]
            private static extern bool PeekMessage(out NativeMessage msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);

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

            private void InitializeComponent()
            {
                this.SuspendLayout();
                this.Name = "Form1";
                this.Text = "Confetti Effect - Smooth";
                this.ResumeLayout(false);
            }
        }
    }

}
