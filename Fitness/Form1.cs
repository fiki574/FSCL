/*
    C# application for administration of gym/fitness memberships etc.
    Copyright (C) 2016 Bruno Fištrek

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
using System.Data.SQLite;
using System.IO;
using Fitness.Database;
using System.Collections.Generic;

namespace Fitness
{
    public partial class Form1 : Form
    {
        public static SQLiteConnection m_dbConnection;
        public static bool loaded;
        public static bool usluga;
        public static bool produlji;

        private ContextMenuStrip listboxContextMenu;

        public Form1()
        {
            InitializeComponent();

            System.Timers.Timer aTimer = new System.Timers.Timer(5000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

            textBox1.KeyDown += new KeyEventHandler(textBox1_KeyDown);
            textBox2.KeyDown += new KeyEventHandler(textBox23_KeyDown);
            textBox3.KeyDown += new KeyEventHandler(textBox23_KeyDown);

            listboxContextMenu = new ContextMenuStrip();
            listboxContextMenu.Items.Add("Odlazak");
            listboxContextMenu.Click += new EventHandler(OnClick);
            listBox1.ContextMenuStrip = listboxContextMenu;

            //TODO: još opcija
            comboBox1.Items.Add("nema aktivne usluge");
            comboBox1.Items.Add("TERETANA NEO");
            comboBox1.Items.Add("TERETANA NEO DO 16H");
            comboBox1.Items.Add("TERETANA SA POP NEO");
            comboBox1.Items.Add("TERETANA SA POP DO 16H");
            comboBox1.Items.Add("GRUPNI TRENINZI 2X TJEDNO");
            comboBox1.Items.Add("GRUPNI TRENINZI NEO");
            comboBox1.Items.Add("KOREKTIVNA");
            comboBox1.Enabled = false;

            loaded = false;
            usluga = false;
            produlji = false;

            try
            {
                if (!File.Exists("fitness.sqlite"))
                {
                    SQLiteConnection.CreateFile("fitness.sqlite");
                    FitnessDB.Load();
                }
                else
                    FitnessDB.Load();

                string danas = DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year;
                Dolasci d = FitnessDB.Dolasci.SingleOrDefault(dol => dol.Datum == danas);
                if (d.Index < 1)
                {
                    Dolasci dol = new Dolasci();
                    dol.Index = FitnessDB.Dolasci.GenerateIndex();
                    dol.Datum = danas;
                    dol.BrojDolazaka = 0;
                    FitnessDB.Dolasci.Add(dol);
                    File.Copy("fitness.sqlite", "fitness_backup.sqlite");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "POGREŠKA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        private void OnClick(object sender, EventArgs e)
        {
            if(listBox1.SelectedItem != null)
                listBox1.Items.Remove(listBox1.SelectedItem);
        }

        private static void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            FitnessDB.Korisnici.Load();
            FitnessDB.Dolasci.Load();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!String.IsNullOrEmpty(textBox1.Text) && IsDigitsOnly(textBox1.Text))
                {
                    int bi = Convert.ToInt32(textBox1.Text);
                    Korisnik k = FitnessDB.Korisnici.SingleOrDefault(ko => ko.BrojIskaznice == bi);
                    if (k.Index > 0)
                    {
                        if(k.AktivnaUsluga != "nema aktivne usluge")
                        {
                            string[] date = k.AktivnaDo.Split(new char[] { '.' });
                            DateTime dt = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                            TimeSpan diff = dt - DateTime.Today;
                            if (diff.Days < 0)
                            {
                                k.AktivnaUsluga = "nema aktivne usluge";
                                k.AktivnaOd = "";
                                k.AktivnaDo = "";
                                FitnessDB.Korisnici.Update(k);
                            }
                        }

                        textBox2.Text = k.Ime;
                        textBox3.Text = k.Prezime;
                        textBox4.Text = k.DatumUclanjenja;
                        textBox12.Text = k.DatumRodenja;
                        richTextBox1.Text = k.Napomena;
                        textBox5.Text = k.ZadnjiDolazak;
                        comboBox1.SelectedIndex = UslugaToIndex(k.AktivnaUsluga);
                        textBox7.Text = k.AktivnaOd;
                        textBox8.Text = k.AktivnaDo;
                        loaded = true;
                    }
                    else
                    {
                        loaded = false;
                        EmptyAll();
                        MessageBox.Show("Korisnik ne postoji!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    EmptyAll();
                    loaded = false;
                }
            }
        }

        public static int UslugaToIndex(string usluga)
        {
            //TODO: još opcija
            if (usluga == "nema aktivne usluge")
                return 0;
            else if (usluga == "TERETANA NEO")
                return 1;
            else if (usluga == "TERETANA NEO DO 16H")
                return 2;
            else if (usluga == "TERETANA SA POP NEO")
                return 3;
            else if (usluga == "TERETANA SA POP DO 16H")
                return 4;
            else if (usluga == "GRUPNI TRENINZI 2X TJEDNO")
                return 5;
            else if (usluga == "GRUPNI TRENINZI NEO")
                return 6;
            else if (usluga == "KOREKTIVNA")
                return 7;
            else
                return 0;
        }

        public static int GetDaysInMonth(int month, int year)
        {
            if (month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10 || month == 12) return 31;
            else if (month == 4 || month == 6 || month == 9 || month == 11) return 30;
            else if (month == 2) return IsLeapYear(year) == true ? 29 : 28;
            else return 0;
        }

        public static bool IsLeapYear(int year)
        {
            if (year % 4 == 0)
            {
                if (year % 100 == 0)
                {
                    if (year % 400 == 0) return true;
                    else return false;
                }
                else return true;
            }
            else return false;
        }

        private void textBox23_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter) button2_Click(sender, e);
        }

        public static bool IsDigitsOnly(string str, bool allow_dot = false)
        {
            if (!allow_dot)
                foreach (char c in str)
                {
                    if (c < '0' || c > '9')
                        return false;
                }
            else
                foreach (char c in str)
                    if (c < '0' || c > '9')
                        if (c != '.')
                            return false;
            return true;
        }

        private void EmptyAll()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox12.Text = "";
            richTextBox1.Text = "";
            textBox5.Text = "";
            comboBox1.SelectedIndex = -1;
            textBox7.Text = "";
            textBox8.Text = "";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox10.Text) && !String.IsNullOrEmpty(textBox9.Text) && !String.IsNullOrEmpty(textBox11.Text) && !String.IsNullOrEmpty(textBox9.Text))
            {
                if (IsDigitsOnly(textBox9.Text))
                {
                    int bi = Convert.ToInt32(textBox9.Text);
                    if (FitnessDB.Korisnici.Count(k => k.BrojIskaznice == bi) == 0)
                    {
                        string ime = textBox10.Text;
                        string prezime = textBox11.Text;
                        string rodenje = textBox13.Text;
                        if (IsDigitsOnly(rodenje, true))
                        {
                            if (FitnessDB.Korisnici.Count(k => k.Ime == ime && k.Prezime == prezime && k.DatumRodenja == rodenje) == 0)
                            {
                                Korisnik novi = new Korisnik();
                                novi.Index = FitnessDB.Korisnici.GenerateIndex();
                                novi.BrojIskaznice = bi;
                                novi.Ime = ime;
                                novi.Prezime = prezime;
                                novi.DatumUclanjenja = DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year;
                                novi.DatumRodenja = rodenje;
                                novi.ZadnjiDolazak = "nepoznato";
                                novi.Napomena = "";
                                novi.AktivnaUsluga = "nema aktivne usluge";
                                novi.AktivnaOd = "";
                                novi.AktivnaDo = "";
                                FitnessDB.Korisnici.Add(novi);
                                MessageBox.Show("Korisnik uspješno dodan!", "USPJEH", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                FitnessDB.Korisnici.Load();
                                textBox9.Text = "";
                                textBox10.Text = "";
                                textBox11.Text = "";
                                textBox13.Text = "";
                            }
                            else MessageBox.Show("Član sa danim detaljima već postoji!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else MessageBox.Show("Datum rođenja striktno mora biti numerički i u sljedećem obliku (npr.):\n\n2.2.2016\n27.9.2016", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else MessageBox.Show("Član sa odabranim brojem iskaznice već postoji!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else MessageBox.Show("Broj iskaznice striktno mora biti numerički!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else MessageBox.Show("Neka polja su prazna!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox1.Text) && IsDigitsOnly(textBox1.Text))
            {
                Korisnik k = FitnessDB.Korisnici.SingleOrDefault(ko => ko.BrojIskaznice == Convert.ToInt32(textBox1.Text));
                k.Napomena = richTextBox1.Text;
                FitnessDB.Korisnici.Update(k);
                MessageBox.Show("Napomena uspješno spremljena!", "USPJEH", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox1.Text) && IsDigitsOnly(textBox1.Text))
            {
                textBox1_KeyDown(sender, new KeyEventArgs(Keys.Enter));
                if (loaded == true)
                {
                    System.Threading.Thread.Sleep(100);
                    string vrijeme = DateTime.Now.Hour + "h" + DateTime.Now.Minute + "m";
                    string item = textBox1.Text + "\t" + textBox2.Text + " " + textBox3.Text + "\t" + vrijeme + "\t\t" + comboBox1.SelectedItem.ToString();
                    bool contains = false;
                    foreach (var i in listBox1.Items)
                    {
                        if (i.ToString().Contains(textBox2.Text) && i.ToString().Contains(textBox3.Text))
                            contains = true;
                        else
                            continue;
                    }

                    if (!contains)
                    {
                        if (comboBox1.SelectedItem.ToString() != "nema aktivne usluge")
                        {
                            //TODO: smanjiti broj preostalih dolazaka u slučaju usluge sa ograničenim brojem dolazaka
                            listBox1.Items.Add(item);
                            string danas = DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year;
                            Dolasci d = FitnessDB.Dolasci.SingleOrDefault(dol => dol.Datum == danas);
                            if (d.Index > 0)
                            {
                                d.BrojDolazaka += 1;
                                FitnessDB.Dolasci.Update(d);
                            }

                            int bi = Convert.ToInt32(textBox1.Text);
                            Korisnik k = FitnessDB.Korisnici.SingleOrDefault(ko => ko.BrojIskaznice == bi);
                            k.ZadnjiDolazak = danas + " u " + DateTime.Now.Hour + "h" + DateTime.Now.Minute + "m";
                            FitnessDB.Korisnici.Update(k);
                        }
                        else MessageBox.Show("Korisnik nema aktivnu uslugu!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    System.Threading.Thread.Sleep(100);
                    loaded = false;
                    EmptyAll();
                }
                else
                {
                    loaded = false;
                    EmptyAll();
                }
            }
            else EmptyAll();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string date = dateTimePicker1.Value.Date.Day + "." + dateTimePicker1.Value.Date.Month + "." + dateTimePicker1.Value.Date.Year;
            Dolasci d = FitnessDB.Dolasci.SingleOrDefault(dol => dol.Datum == date);
            if (d.Index > 0)
                MessageBox.Show("Ukupno dolazaka za datum '" + date + "':\t" + d.BrojDolazaka.ToString(), "DOLASCI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Datum ne postoji u bazi podataka!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if(loaded)
            {
                if (!usluga)
                {
                    comboBox1.Enabled = true;
                    button5.Text = "Spremi";
                    usluga = true;
                }
                else if (usluga)
                {
                    if(!String.IsNullOrEmpty(textBox1.Text) && IsDigitsOnly(textBox1.Text))
                    {
                        string u = comboBox1.SelectedItem.ToString();
                        int bi = Convert.ToInt32(textBox1.Text);
                        Korisnik k = FitnessDB.Korisnici.SingleOrDefault(ko => ko.BrojIskaznice == bi);
                        if(k.Index > 0)
                        {
                            k.AktivnaUsluga = u;
                            int dan = DateTime.Now.Day;
                            int mjesec = DateTime.Now.Month;
                            int godina = DateTime.Now.Year;
                            int ovaj_mjesec = GetDaysInMonth(mjesec, godina);
                            k.AktivnaOd = dan + "." + mjesec + "." + godina;
                            if(mjesec + 1 > 12)
                            {
                                mjesec = 0;
                                godina += 1;
                            }
                            int slj_mjesec = GetDaysInMonth(mjesec+1, godina);
                            int razlika = ovaj_mjesec - slj_mjesec;
                            if (razlika > 0)
                            {
                                dan = razlika;
                                mjesec += 1;
                            }
                            k.AktivnaDo = dan + "." + (mjesec+1).ToString() + "." + godina;
                            FitnessDB.Korisnici.Update(k);
                            comboBox1.Enabled = false;
                            button5.Text = "Promijeni";
                            usluga = false;
                            MessageBox.Show("Usluga uspješno promijenjena!", "USPJEH", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            EmptyAll();
                            textBox1.Text = bi.ToString();
                            textBox1_KeyDown(sender, new KeyEventArgs(Keys.Enter));
                        }
                    }
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (loaded)
            {
                if (comboBox1.SelectedItem.ToString() != "nema aktivne usluge")
                {
                    if (!produlji)
                    {
                        textBox8.ReadOnly = false;
                        button6.Text = "Spremi";
                        produlji = true;
                    }
                    else if (produlji)
                    {
                        string datum = textBox8.Text;
                        if (IsDigitsOnly(datum, true))
                        {
                            if (!String.IsNullOrEmpty(textBox1.Text) && IsDigitsOnly(textBox1.Text))
                            {
                                int bi = Convert.ToInt32(textBox1.Text);
                                Korisnik k = FitnessDB.Korisnici.SingleOrDefault(ko => ko.BrojIskaznice == bi);
                                k.AktivnaDo = datum;
                                FitnessDB.Korisnici.Update(k);
                                textBox8.ReadOnly = true;
                                button6.Text = "Produlji";
                                produlji = false;
                                MessageBox.Show("Usluga uspješno produljena!", "USPJEH", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else MessageBox.Show("Broj iskaznice ne smije biti 0 ili prazan!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else MessageBox.Show("Datum rođenja striktno mora biti numerički i u sljedećem obliku (npr.):\n\n2.2.2016\n27.9.2016", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!loaded)
            {
                string ime = textBox2.Text;
                string prezime = textBox3.Text;
                if (String.IsNullOrEmpty(ime) && !String.IsNullOrEmpty(prezime))
                {
                    List<Korisnik> ks = FitnessDB.Korisnici.Select(ko => ko.Prezime == prezime);
                    if (ks.Count > 0)
                    {
                        string korisnici = null;
                        foreach (Korisnik k in ks)
                            korisnici += k.BrojIskaznice + "\t" + k.Ime + " " + k.Prezime + "\n";
                        MessageBox.Show("Korisnici sa prezimenom '" + prezime + "':\n\n" + korisnici, "INFORMACIJA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else MessageBox.Show("Ne postoji korisnik sa tim prezimenom!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (!String.IsNullOrEmpty(ime) && String.IsNullOrEmpty(prezime))
                {
                    List<Korisnik> ks = FitnessDB.Korisnici.Select(ko => ko.Ime == ime);
                    if (ks.Count > 0)
                    {
                        string korisnici = null;
                        foreach (Korisnik k in ks)
                            korisnici += k.BrojIskaznice + "\t" + k.Ime + " " + k.Prezime + "\n";
                        MessageBox.Show("Korisnici sa imenom '" + ime + "':\n\n" + korisnici, "INFORMACIJA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else MessageBox.Show("Ne postoji korisnik sa tim imenom!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    List<Korisnik> ks = FitnessDB.Korisnici.Select(ko => ko.Ime == ime && ko.Prezime == prezime);
                    if (ks.Count > 0)
                    {
                        string korisnici = null;
                        foreach (Korisnik k in ks)
                            korisnici += k.BrojIskaznice + "\t" + k.Ime + " " + k.Prezime + "\n";
                        MessageBox.Show(korisnici, "INFORMACIJA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else MessageBox.Show("Ne postoji korisnik sa tim imenom i prezimenom!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                EmptyAll();
            }
        }
    }
}
