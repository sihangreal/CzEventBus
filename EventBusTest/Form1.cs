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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Form2 form2 = new Form2();
            form2.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //EventBus.PublishEvent("EventTest");

           // EventBus.PublishEvent("EventTest",  "hello C#" );

            //EventBus.PublishEvent("EventTest",this, "hello C#");

            EventBus.PublishEvent("EventTest",this,new object[] {"hello C++"});
        }
    }
}
