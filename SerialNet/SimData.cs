using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SerialNet
{
    public enum SimDataType
    {
        NORMAL,
        ONCE
    }
    public struct SimData
    {
        public SimData(string d1, long d2, bool d3) { this.data = d1; this.delay = d2; this.once = d3; }
        public ListViewItem AsListViewItem() { return new ListViewItem(new String[] { (once?"?":"")+this.delay.ToString(), this.data }); }
        public string data;
        public long delay;
        public bool once;
    }
}
