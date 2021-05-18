using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using VisuMap.Script;

namespace VisuMap.DataModeling {
    public partial class InputPanel : Form {
        List<Point> trace = new List<Point>();
        const int MaxPoints = 500;
        Graphics gPanel;
        bool activated;
        Color dotColor = Color.Yellow;
        Color hiColor = Color.Red;
        float margin = 8.0f;
        float cx, cy, w, h;
        public delegate void EvalCall(double x, double y);

        public EvalCall CallBack { get; set; }

        public InputPanel() {
            InitializeComponent();
            RefreshConstants();
        }

        void RefreshConstants() {
            w = panel.Width;
            h = panel.Height;
            cx = w * 0.5f;
            cy = h * 0.5f;

            gPanel = panel.CreateGraphics();
            gPanel.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        }

        private void panel_Paint(object sender, PaintEventArgs e) {
            RedrawPanel( e.Graphics );
        }

        void RedrawPanel(Graphics g) {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(panel.BackColor);

            using (Pen pen = new Pen(Color.FromArgb(32, Color.Yellow), 0.5f)) {
                g.DrawLine(pen, margin, h / 2, w-margin, h / 2);
                g.DrawLine(pen, w / 2, margin, w / 2, h-margin);
                g.DrawEllipse(pen, margin, margin, w-2*margin, h-2*margin);
            }
            if ( trace.Count > 0) {
                using(Brush b = new SolidBrush(dotColor))
                    foreach(var p in trace)
                        DrawDot(g, b, p);
            }
        }

        void DrawDot(Graphics g, Brush b, Point p) {
                g.FillEllipse(b, p.X - 1, p.Y - 1, 2, 2);
        }

        void EvalPoint(Point p) {
            float x = (p.X - cx) / (cx - margin);
            float y = (cy - p.Y) / (cy - margin);
            CallBack?.Invoke(x, -y);
        }

        private void panel_MouseMove(object sender, MouseEventArgs e) {
            if( (CallBack == null) || !activated) return;

            EvalPoint(e.Location);

            using (Brush b = new SolidBrush(dotColor))
                DrawDot(gPanel, b, e.Location);
            trace.Add(e.Location);
            if (trace.Count > MaxPoints)
                trace.RemoveAt(0);
        }

        private void panel_Click(object sender, EventArgs e) {
            activated = !activated;
            if (!activated) {
                panel.Refresh();
            }
        }

        private void miRepeat_Click(object sender, EventArgs e) {
            using (Brush b = new SolidBrush(Color.Red)) {
                foreach(var p in trace) {
                    gPanel.FillEllipse(b, p.X - 1.5f, p.Y - 1.5f, 3, 3);
                    EvalPoint(p);
                }
            }
            panel.Refresh();
        }

        private void miClearHistory_Click(object sender, EventArgs e) {
            trace.Clear();
            panel.Refresh();
        }
    }
}
