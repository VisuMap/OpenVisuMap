using System;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;

class ProgressBar : IDisposable {
    Panel panel;
    int currentValue;
    int maximum; // maximun reachable value. minimum is zero be default.
    Brush brush;
    int markerIndex = -1;

    public ProgressBar(Panel panel){
        this.panel = panel;
        currentValue = maximum = 0;
        panel.Paint += new PaintEventHandler(PanelPaint);        
        brush = new TextureBrush(global::ClipRecorder.Properties.Resources.ProgressBar);

        typeof(Panel).InvokeMember("DoubleBuffered",
            BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
            null, panel, new object[] { true });
    }

    void PanelPaint(object sender, PaintEventArgs e) {
        if (maximum <= 0) return;
        float barLen = (float)panel.Width * currentValue / maximum;
        e.Graphics.FillRectangle(brush, 0, 0, barLen, panel.Height);
        if ( (markerIndex >= 0) && (markerIndex != currentValue) ) {
            using (Brush br = new SolidBrush(Color.FromArgb(64, Color.Green))) {
                float mkLoc = (float)panel.Width * markerIndex / maximum;
                if (mkLoc < barLen)
                    e.Graphics.FillRectangle(br, mkLoc, 0, barLen - mkLoc, panel.Height);
                else
                    e.Graphics.FillRectangle(br, barLen, 0, mkLoc - barLen, panel.Height);
            }
        }
    }

    public int Value {
        get { return currentValue; }
        set { currentValue = value; panel.Refresh(); }
    }

    public int Maximum {
        get { return maximum; }
        set { maximum = value; }
    }

    public int Width {
        get { return panel.Width; }
    }

    public int MarkerIndex
    {
        get => markerIndex;
        set => markerIndex = value;
    }

    public void Dispose() {
        brush.Dispose();
    }
}

