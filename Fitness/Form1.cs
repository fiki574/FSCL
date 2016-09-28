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

namespace Fitness
{
    public partial class Form1 : Form
    {
        public static SQLiteConnection m_dbConnection;
        public static bool loaded;
        public static bool usluga;
        public static bool produlji;

        public Form1()
        {
            InitializeComponent();
            System.Timers.Timer aTimer = new System.Timers.Timer(10000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            textBox1.KeyDown += new KeyEventHandler(textBox1_KeyDown);
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
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "POGREŠKA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
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
                        textBox2.Text = k.Ime;
                        textBox3.Text = k.Prezime;
                        textBox4.Text = k.DatumUclanjenja;
                        textBox12.Text = k.DatumRodenja;
                        richTextBox1.Text = k.Napomena;
                        textBox5.Text = k.ZadnjiDolazak;
                        textBox6.Text = k.AktivnaUsluga;
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

        public static bool IsDigitsOnly(string str, bool allow_dot = false)
        {
            if(!allow_dot)
                foreach (char c in str)
                {
                    if (c < '0' || c > '9')
                        return false;
                }  
            else
                foreach (char c in str)
                    if (c < '0' || c > '9')
                        if(c != '.')
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
            textBox6.Text = "";
            textBox7.Text = "";
            textBox8.Text = "";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox10.Text) && !String.IsNullOrEmpty(textBox9.Text) && !String.IsNullOrEmpty(textBox11.Text) && !String.IsNullOrEmpty(textBox9.Text))
            {
                if(IsDigitsOnly(textBox9.Text))
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
            if(!String.IsNullOrEmpty(textBox1.Text) && IsDigitsOnly(textBox1.Text))
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
                if(loaded == true)
                {
                    System.Threading.Thread.Sleep(100);
                    string vrijeme = DateTime.Now.Hour + "h" + DateTime.Now.Minute + "m";
                    string item = textBox1.Text + "\t" + textBox2.Text + " " + textBox3.Text + "\t" + vrijeme + "\t\t" + textBox6.Text;
                    bool contains = false;
                    foreach (var i in listBox1.Items)
                    {
                        if (i.ToString().Contains(textBox2.Text) && i.ToString().Contains(textBox3.Text))
                            contains = true;
                        else
                            continue;
                    }

                    if(!contains)
                    {
                        if (textBox6.Text != "nema aktivne usluge")
                        {
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
                            k.ZadnjiDolazak = danas + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute;
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
            //promijeni uslugu
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (loaded)
            {
                if (textBox6.Text != "nema aktivne usluge")
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
                                MessageBox.Show("Usluga uspješno produljena!", "USPJEH", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else MessageBox.Show("Broj iskaznice ne smije biti 0 ili prazan!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else MessageBox.Show("Datum rođenja striktno mora biti numerički i u sljedećem obliku (npr.):\n\n2.2.2016\n27.9.2016", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            else MessageBox.Show("Nije odabran niti jedan član!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //nađi po imenu i prezimenu
        }
    }
}
