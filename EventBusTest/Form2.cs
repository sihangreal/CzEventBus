using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CzEventBus;

namespace EventBusTest
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            EventBus.RegisterEvent(this);
        }

        //[EventAttr("EventTest")]
        //public void EventTest()
        //{
        //    this.label1.Text = "hello world!";
        //}

        //[EventAttr("EventTest")]
        //public void EventTest(string msg)
        //{
        //    this.label1.Text = msg;
        //}


        [EventAttr("EventTest")]
        public void EventTest(object sender, string msg)
        {
            this.label1.Text = msg;
        }
    }
}
