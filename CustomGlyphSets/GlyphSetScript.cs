using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;

using VisuMap.Plugin;
using VisuMap.Script;

namespace CustomGlyphSets {
    public class GlyphSetScript : IPluginObject {
        int[] colorList = new int[] { -65536,  -186, -16776961, -33024, -16746240, -9238363, -85248, -16732498, -3407697, -16711936, -9238875, -175352};
        IVisuMap script = CustomGlyphSet.App.ScriptApp;
        string directoryPath;
        string errMsg;

        public string Name {
            get { return "CustomGlyphSets"; }
            set { ; }
        }

        bool ValidateSetName(string glyphsetName) {
            if (string.IsNullOrEmpty(glyphsetName)) {
                errMsg = "Invalid glyphsetName";
                return false;
            }

            directoryPath = script.HomeDirectory + "config\\" + glyphsetName + "\\";

            if (Directory.Exists(directoryPath)) {
                errMsg = "Glyph set directory already exisits";
                return false;
            } else {
                try {
                    Directory.CreateDirectory(directoryPath);
                } catch (Exception ex) {
                    errMsg = ex.ToString() + ": " + ex.Message;
                }
            }
            return true;
        }

        public bool ClearGlyphSet(string glyphsetName) {
            directoryPath = script.HomeDirectory + "config\\" + glyphsetName + "\\";

            if (Directory.Exists(directoryPath)) {
                try {
                    Directory.Delete(directoryPath, true);
                } catch (Exception) {
                    return false;
                }
                return true;
            } else {
                return false;
            }
        }

        public string CreatePhaseGlyphs(string glyphsetName, int phases, int size, Color color) {
            if (!ValidateSetName(glyphsetName)) return errMsg;
            if ((phases < 2) || (size < 4) || (color == null)) {
                return "Invalid parameters";
            }

            double angle = Math.PI / phases;
            float cx = size / 2.0f;
            float cy = size / 2.0f;

            using (Pen pen = new Pen(color, 1.0f)) {
                for (int i = 0; i < phases; i++) {
                    string fName = directoryPath + "Phase" + i.ToString("00") + ".png";
                    Bitmap bm = new Bitmap(size, size);
                    using (Graphics g = Graphics.FromImage(bm)) {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.Clear(Color.Transparent);
                        float x = (float)(Math.Cos(i * angle) * size / 2.0f);
                        float y = (float)(Math.Sin(i * angle) * size / 2.0f); 
                        g.DrawLine(pen, cx-x, cx-y, cx+x, cx+y);
                    }
                    bm.Save(fName);
                }
            }

            script.InstallGlyphSet(glyphsetName, directoryPath, 0, 1.0f);
            return null;
        }

        public string CreateOrderedGlyphs(string glyphsetName, float opacity, int minSize, int level, int classes) {
            if (!ValidateSetName(glyphsetName)) return errMsg;

            if ((opacity < 0) || (level < 1) || (classes < 1) || (minSize<1) ) {
                return "Invalid parameters.";
            }

            int alpha = (int)(opacity * 255);
            alpha = Math.Max(0, Math.Min(255, alpha));
            for (int i = 0; i < level; i++) {
                for (int c = 0; c < classes; c++) {
                    Color clr = Color.FromArgb(colorList[c % colorList.Length]);
                    using (Brush brush = new SolidBrush(Color.FromArgb(alpha, clr))) {
                        string fName = directoryPath + "G" + i.ToString("00") + "_" + c.ToString("00") + ".png";
                        int sz = minSize + i * 2;
                        Bitmap bm = new Bitmap(sz+1, sz+1);
                        using (Graphics g = Graphics.FromImage(bm)) {
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.Clear(Color.Transparent);
                            if (sz > 1) {
                                g.FillEllipse(brush, new Rectangle(0, 0, sz, sz));
                            } else {
                                g.FillRectangle(brush, 0, 0, sz, sz);
                            }
                        }
                        bm.Save(fName);
                    }
                }
            }

            script.InstallGlyphSet(glyphsetName, directoryPath, 0, 1.0f);
            return null;
        }

        public string CreateColorWheelGlyphs(string glyphsetName, int colors, int size, bool rectangle) {
            // (new ColorWheel()).Show();

            if (!ValidateSetName(glyphsetName)) return errMsg;

            if ( (colors < 2) || (size<1) ) {
                return "Invalid parameters.";
            }

            int bmSize = rectangle ? size : size + 1;
            double dTheta = 2 * Math.PI / colors;
            for(int i=0; i<colors; i++) {
                Color clr = ColorWheel.ColorAtAngle(-Math.PI + i * dTheta);
                using (Brush brush = new SolidBrush(clr)) {
                    string fName = directoryPath + "G" + i.ToString("000") + ".png";
                    Bitmap bm = new Bitmap(bmSize, bmSize);
                    using (Graphics g = Graphics.FromImage(bm)) {
                        if (rectangle) {
                            g.Clear(clr);
                        } else {
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.Clear(Color.Transparent);
                            g.FillEllipse(brush, new Rectangle(0, 0, size, size));
                        }
                    }
                    bm.Save(fName);
                }

            }

            script.InstallGlyphSet(glyphsetName, directoryPath, 0, 1.0f);
            return null;
        }


        public string CreateFieldGlyphs(string glyphsetName) {
            if (!ValidateSetName(glyphsetName)) return errMsg;

            Color clr = Color.Yellow;
            int levels = 4;
            int directions = 32;
            Color clr0 = Color.LightGreen;
            Color clr1 = Color.Red;

            int[] glyphSizes = new int[] { 2, 10, 16, 24 };
            for (int i = 0; i < levels; i++) {
                int sz = glyphSizes[i];
                int dirs = (i == 0) ? 1 : directions;
                for (int c = 0; c < dirs; c++) {
                    string fName = directoryPath + "G" + i.ToString("00") + "_" + c.ToString("00") + ".png";
                    Bitmap bm = new Bitmap(sz, sz);
                    using (Graphics g = Graphics.FromImage(bm)) {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.Clear(Color.Transparent);

                        if (i == 0) {
                            g.Clear(clr0);
                        } else {
                            float angle = c * 360.0f / directions ;
                            g.TranslateTransform(sz / 2.0f, sz / 2.0f);
                            g.RotateTransform(angle);
                            using(Pen pen0 = new Pen(clr0))
                            using(Pen pen1 = new Pen(clr1)) {
                                float r = sz/2;
                                g.DrawLine(pen0, new PointF(-r, 0), new PointF(0, 0));
                                g.DrawLine(pen1, new PointF(0, 0), new PointF(r, 0));
                            }
                        }
                    }
                    bm.Save(fName);
                }                   
            }

            script.InstallGlyphSet(glyphsetName, directoryPath, 0, 1.0f);
            return null;
        }
    }
}
