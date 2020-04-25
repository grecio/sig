using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Impressao
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            

            Thread mythread;
            mythread = new Thread(new ThreadStart(ThreadLoop));
            mythread.Start();
            Application.Run(new MdiForm(mythread));
        }

        public static void ThreadLoop()
        {
            Application.Run(new FrmSplashScreen());
        }
    }
}
