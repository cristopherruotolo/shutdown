using System;
using System.Threading;
using System.Windows.Forms;
using ShutdownTimerApp;

namespace shutdown
{
    internal static class Program
    {
        [STAThread]
        public static void Main()
        {
            bool isOnlyInstance = false;
            using (Mutex mutex = new Mutex(true, "ShutdownTimerAppMutex", out isOnlyInstance))
            {
                if (isOnlyInstance)
                {
                    try
                    {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new MainForm());
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
                else
                {
                    MessageBox.Show("Uma instância do aplicativo já está em execução.",
                        "Aplicativo em Execução", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
