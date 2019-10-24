/*
    C# application for administration of gym/fitness memberships etc.
    Copyright (C)2018/2019 Bruno Fištrek

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Windows.Forms;
using Fitness.Database;
using System.Collections.Generic;
using System.Linq;

namespace Fitness
{
    public partial class Form2 : Form
    {
        public static Form2 Instance = null;

        public Form2()
        {
            InitializeComponent();
            Instance = this;
            FillData();
        }

        public void FillData()
        {
            TryCatch(() =>
            {
                label1.Text = "Ukupan broj članova: " + GetTotalMembers();
                label2.Text = "Ukupan broj aktivnih članova: " + GetActiveMembers();
                label3.Text = "Produženja usluga ovaj dan: " + GetTodaysPayments();
                label4.Text = "Produženja usluga ovaj mjesec: " + GetMonthsPayments();
                label5.Text = "Broj dolazaka ovaj dan: " + GetNumberOfVisitsToday();
                label6.Text = "Broj dolazaka ovaj mjesec: " + GetNumberOfVisitsThisMonth();
                label7.Text = "Novi korisnici ovaj dan: " + GetNewUsersToday();
                label8.Text = "Novi korisnici ovaj mjesec: " + GetNewUsersThisMonth();
                radioButton1.Checked = true;
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FillData();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
                return;

            listBox1.Items.Clear();
            textBox1.Text = "";
            TryCatch(() =>
            {
                int count = 1;
                if (comboBox1.SelectedItem.ToString() == "Aktivni članovi")
                {
                    List<Korisnik> aktivni = FitnessDB.Korisnici.Select(k => k.AktivnaUsluga != "nema aktivne usluge");
                    foreach (var korisnik in aktivni)
                        listBox1.Items.Add($"{count++}\t{korisnik.BrojIskaznice}\t{korisnik.Ime} {korisnik.Prezime}");
                }
                else if (comboBox1.SelectedItem.ToString() == "Neaktivni članovi")
                {
                    List<Korisnik> aktivni = FitnessDB.Korisnici.Select(k => k.AktivnaUsluga == "nema aktivne usluge");
                    foreach (var korisnik in aktivni)
                        listBox1.Items.Add($"{count++}\t{korisnik.BrojIskaznice}\t{korisnik.Ime} {korisnik.Prezime}");
                }
                else if (comboBox1.SelectedItem.ToString() == "Članovi i broj uplata")
                {
                    List<Statistika> uplate = FitnessDB.Statistika.Select(sta => true).OrderBy(sta => sta.UkupnoPlacanja).ToList();
                    foreach (var stat in uplate)
                        listBox1.Items.Add($"{count++}\t{stat.UkupnoPlacanja}\t{stat.BrojKartice}");
                }
                else if (comboBox1.SelectedItem.ToString() == "Članovi i broj dolazaka")
                {
                    List<Statistika> uplate = FitnessDB.Statistika.Select(sta => true).OrderBy(sta => sta.UkupnoDolazaka).ToList();
                    foreach (var stat in uplate)
                        listBox1.Items.Add($"{count++}\t{stat.UkupnoDolazaka}\t{stat.BrojKartice}");
                }
            });
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            TryCatch(() =>
            {
                Dictionary<string, int> visits = GetVisitsPerDay(1);
                label10.Text = "Ponedjeljak: " + visits["PON"].ToString();
                label11.Text = "Utorak: " + visits["UTO"].ToString();
                label12.Text = "Srijeda:" + visits["SRI"].ToString();
                label13.Text = "Četvrtak: " + visits["CET"].ToString();
                label14.Text = "Petak: " + visits["PET"].ToString();
                label15.Text = "Subota: " + visits["SUB"].ToString();
                label16.Text = "Nedjelja: " + visits["NED"].ToString();
            });
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            TryCatch(() =>
            {
                Dictionary<string, int> visits = GetVisitsPerDay(3);
                label10.Text = "Ponedjeljak: " + visits["PON"].ToString();
                label11.Text = "Utorak: " + visits["UTO"].ToString();
                label12.Text = "Srijeda:" + visits["SRI"].ToString();
                label13.Text = "Četvrtak: " + visits["CET"].ToString();
                label14.Text = "Petak: " + visits["PET"].ToString();
                label15.Text = "Subota: " + visits["SUB"].ToString();
                label16.Text = "Nedjelja: " + visits["NED"].ToString();
            });
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            TryCatch(() =>
            {
                Dictionary<string, int> visits = GetVisitsPerDay(6);
                label10.Text = "Ponedjeljak: " + visits["PON"].ToString();
                label11.Text = "Utorak: " + visits["UTO"].ToString();
                label12.Text = "Srijeda:" + visits["SRI"].ToString();
                label13.Text = "Četvrtak: " + visits["CET"].ToString();
                label14.Text = "Petak: " + visits["PET"].ToString();
                label15.Text = "Subota: " + visits["SUB"].ToString();
                label16.Text = "Nedjelja: " + visits["NED"].ToString();
            });
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            TryCatch(() =>
            {
                Dictionary<string, int> visits = GetVisitsPerDay(12);
                label10.Text = "Ponedjeljak: " + visits["PON"].ToString();
                label11.Text = "Utorak: " + visits["UTO"].ToString();
                label12.Text = "Srijeda:" + visits["SRI"].ToString();
                label13.Text = "Četvrtak: " + visits["CET"].ToString();
                label14.Text = "Petak: " + visits["PET"].ToString();
                label15.Text = "Subota: " + visits["SUB"].ToString();
                label16.Text = "Nedjelja: " + visits["NED"].ToString();
            });
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            TryCatch(() =>
            {
                Dictionary<string, int> visits = GetVisitsPerDay(24);
                label10.Text = "Ponedjeljak: " + visits["PON"].ToString();
                label11.Text = "Utorak: " + visits["UTO"].ToString();
                label12.Text = "Srijeda:" + visits["SRI"].ToString();
                label13.Text = "Četvrtak: " + visits["CET"].ToString();
                label14.Text = "Petak: " + visits["PET"].ToString();
                label15.Text = "Subota: " + visits["SUB"].ToString();
                label16.Text = "Nedjelja: " + visits["NED"].ToString();
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
                return;

            TryCatch(() => 
            {
                List<string> remove = new List<string>();
                foreach (var entry in listBox1.Items)
                {
                    if (!entry.ToString().ToLowerInvariant().Contains("\t" + textBox1.Text.ToLowerInvariant()))
                        remove.Add(entry.ToString());
                }

                if (remove.Count != listBox1.Items.Count)
                {
                    foreach (var entry in remove)
                        listBox1.Items.Remove(entry);

                    remove.Clear();
                    textBox1.Text = "";
                }
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
                return;

            TryCatch(() =>
            {
                List<string> remove = new List<string>();
                foreach (var entry in listBox1.Items)
                {
                    if (!entry.ToString().ToLowerInvariant().Contains(" " + textBox1.Text.ToLowerInvariant()))
                        remove.Add(entry.ToString());
                }

                if (remove.Count != listBox1.Items.Count)
                {
                    foreach (var entry in remove)
                        listBox1.Items.Remove(entry);

                    remove.Clear();
                    textBox1.Text = "";
                }
            });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
                return;

            TryCatch(() =>
            {
                List<string> remove = new List<string>();
                foreach (var entry in listBox1.Items)
                {
                    if (!entry.ToString().ToLowerInvariant().Contains("\t" + textBox1.Text))
                        remove.Add(entry.ToString());
                }

                if (remove.Count != listBox1.Items.Count)
                {
                    foreach (var entry in remove)
                        listBox1.Items.Remove(entry);

                    remove.Clear();
                    textBox1.Text = "";
                }
            });
        }

        public int GetTotalMembers()
        {
            return FitnessDB.Korisnici.Count(k => true);
        }

        public int GetActiveMembers()
        {
            return FitnessDB.Korisnici.Count(k => k.AktivnaUsluga != "nema aktivne usluge");
        }

        public int GetNumberOfVisitsToday()
        {
            return FitnessDB.Dolasci.SingleOrDefault(d => d.Datum == (DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year)).BrojDolazaka;
        }

        public int GetNumberOfVisitsThisMonth()
        {
            return FitnessDB.Dolasci.SingleOrDefault(d => d.Datum.Contains($"{DateTime.Now.Month}.{DateTime.Now.Year}")).BrojDolazaka;
        }

        public int GetTodaysPayments()
        {
            return FitnessDB.Korisnici.Count(k => k.AktivnaOd == (DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year));
        }

        public int GetMonthsPayments()
        {
            return FitnessDB.Korisnici.Count(k => k.AktivnaOd.Contains($"{DateTime.Now.Month}.{DateTime.Now.Year}"));
        }

        public Dictionary<string, int> GetVisitsPerDay(int for_last_month)
        {
            Dictionary<string, int> results = new Dictionary<string, int>();
            int[] TotalUsersPerDay = new int[7] { 0, 0, 0, 0, 0, 0, 0 }, NumberOfDays = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
            DateTime now = DateTime.Now;
            foreach (DateTime o in Form1.EachDay(now.AddMonths(-for_last_month), now))
            {
                string date = o.Day + "." + o.Month + "." + o.Year;
                Dolasci d = FitnessDB.Dolasci.SingleOrDefault(dol => dol.Datum == date);
                if (d.Index > 0)
                {
                    int pos = (int)o.DayOfWeek - 1;
                    if (pos < 0)
                        pos = 6;

                    TotalUsersPerDay[pos] += d.BrojDolazaka;
                    NumberOfDays[pos] += 1;
                }
            }

            results.Add("PON", NumberOfDays[0] != 0 ? TotalUsersPerDay[0] / NumberOfDays[0] : 0);
            results.Add("UTO", NumberOfDays[1] != 0 ? TotalUsersPerDay[1] / NumberOfDays[1] : 0);
            results.Add("SRI", NumberOfDays[2] != 0 ? TotalUsersPerDay[2] / NumberOfDays[2] : 0);
            results.Add("CET", NumberOfDays[3] != 0 ? TotalUsersPerDay[3] / NumberOfDays[3] : 0);
            results.Add("PET", NumberOfDays[4] != 0 ? TotalUsersPerDay[4] / NumberOfDays[4] : 0);
            results.Add("SUB", NumberOfDays[5] != 0 ? TotalUsersPerDay[5] / NumberOfDays[5] : 0);
            results.Add("NED", NumberOfDays[6] != 0 ? TotalUsersPerDay[6] / NumberOfDays[6] : 0);
            return results;
        }

        public int GetNewUsersToday()
        {
            return FitnessDB.Korisnici.Count(k => k.DatumUclanjenja == DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year);
        }

        public int GetNewUsersThisMonth()
        {
            return FitnessDB.Korisnici.Count(k => k.DatumUclanjenja.Contains($"{DateTime.Now.Month}.{DateTime.Now.Year}"));
        }

        private static void TryCatch(Action a, bool exit = true)
        {
            try
            {
                a.Invoke();
            }
            catch (Exception ex)
            {
                if (exit)
                {
                    MessageBox.Show("Postupak prijave pogreške:\n1. Slikajte ovu poruku pomoću tipke \"Print Screen\"\n2. Otiđite na \"www.pasteboard.co\" sa Google Chrome-om\n3. Prisnite tipku \"Ctrl\" i u isto vrijeme tipku \"V\" (dakle CTRL+V)\n4. Na otvorenoj web stranici odaberite zelenu tipku na kojoj piše \"UPLOAD\"5. Pošaljite mi link koji će se prikazati nakon pritiska na spomenutu tipku na sljedeći mail -> fiki.xperia@gmail.com\n\n" + ex.ToString(), "POGREŠKA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }
        }
    }
}