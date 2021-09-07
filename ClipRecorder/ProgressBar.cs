using System;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;

class ProgressBar : IDisposable {
    Panel panel;
    int currentValue;
    int maximum; // maximun reachable value. minimum is zero be default.
    Brush brush;

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
        e.Graphics.Clear(panel.BackColor);
        if (maximum > 0) {
            e.Graphics.FillRectangle(brush, 0, 0, panel.Width * currentValue / maximum, panel.Height);            
        }
    }

    public int Value {
        get { return currentValue; }
        set { currentValue = value; }
    }

    public int Maximum {
        get { return maximum; }
        set { maximum = value; }
    }

    public int Width {
        get { return panel.Width; }
    }

    public void Dispose() {
        brush.Dispose();
    }
}

