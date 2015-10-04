using System;
using System.Windows.Forms;

namespace SimpleDataAccessLayer.Common.wizard
{
    public class WizardTabControl : TabControl
    {
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x1328 && !DesignMode) // Hide tabs by trapping the TCM_ADJUSTRECT message
                m.Result = IntPtr.Zero;
            else
                base.WndProc(ref m);
        }
    }
}