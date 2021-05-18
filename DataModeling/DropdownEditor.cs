using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Security.Permissions;
using System.Collections.Generic;
using System.IO;

namespace VisuMap.DataModeling {

    /// <summary>
    /// A base class to help creating drow-down list UTType editor classes.
    /// </summary>        
    public abstract class DropdownEditor : UITypeEditor {
        
        protected IWindowsFormsEditorService fEdSvc;

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted=true)]
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
            if (context != null && context.Instance != null) {
                return UITypeEditorEditStyle.DropDown;
            }
            return base.GetEditStyle(context);
        }
               

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted=true)]
        public override object EditValue(
                ITypeDescriptorContext context, 
                IServiceProvider provider, 
                object value) 
        {            
            if ( (context != null) && (context.Instance != null) && (provider != null) ) {
                try {
                    // get the editor service
                    fEdSvc = (IWindowsFormsEditorService)
                        provider.GetService(typeof(IWindowsFormsEditorService));
                   
                    
                    // create the control(s) we want for the UI
                    ListBox vListBox = new ListBox();
                    vListBox.Click += new EventHandler(ListClick);
            
                    // modify the list's properties including the Item list
                    foreach (string item in GetValueList()) {
                        vListBox.Items.Add(item);
                    }

                    // initialize the selection on the list
                    vListBox.SelectedItem = value;
         
                    // let the editor service place the list on screen and manage its events
                    fEdSvc.DropDownControl(vListBox);
   
                    // return the updated value;
                    return vListBox.SelectedItem;
                } 
                finally {
                    fEdSvc = null;
                }
            }
            else {
                return value;
            }
        }

        protected void ListClick(object sender, EventArgs args) {
            fEdSvc.CloseDropDown();
        }

        public abstract IList<string> GetValueList();
    }
}
