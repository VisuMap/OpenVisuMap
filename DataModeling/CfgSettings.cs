using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VisuMap.DataModeling {
    public partial class CfgSettings : Form {
        public CfgSettings(object propObj) {
            InitializeComponent();
            this.propertyGrid1.SelectedObject = propObj;
            this.propertyGrid1.BrowsableAttributes = new AttributeCollection(new Attribute[] { new VisuMap.Lib.ConfigurableAttribute() });
        }
    }
}
