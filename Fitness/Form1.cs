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
using System.Data.SQLite;
using System.IO;
using Fitness.Database;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Fitness
{
    public partial class Form1 : Form
    {
        public static SQLiteConnection m_dbConnection = null;
        public static Form1 Instance = null;
        public static bool loaded, usluga, produlji, promijeni;
        private ContextMenuStrip listboxContextMenu;
        public static HttpServer m_http = null;
        public static string ApiKey = null;

        public Form1()
        {
            InitializeComponent();
            Instance = this;

            System.Timers.Timer aTimer = new System.Timers.Timer(5000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            pictureBox1.Image = System.Drawing.Image.FromFile("Files/sc.ico");

            textBox1.KeyDown += new KeyEventHandler(textBox1_KeyDown);
            textBox2.KeyDown += new KeyEventHandler(textBox23_KeyDown);
            textBox3.KeyDown += new KeyEventHandler(textBox23_KeyDown);

            listboxContextMenu = new ContextMenuStrip();
            listboxContextMenu.Items.Add("Odlazak");
            listboxContextMenu.Items.Add("Prikaži korisničke podatke");
            listboxContextMenu.Items[0].Click += new EventHandler(OnClick);
            listboxContextMenu.Items[1].Click += new EventHandler(OnClick2);
            listBox1.ContextMenuStrip = listboxContextMenu;

            comboBox1.Items.Add("nema aktivne usluge");
            comboBox1.Items.Add("TERETANA NEO");
            comboBox1.Items.Add("TERETANA NEO DO 16H");
            comboBox1.Items.Add("TERETANA SA POP NEO");
            comboBox1.Items.Add("TERETANA SA POP DO 16H");
            comboBox1.Items.Add("TERETANA 12 DOLAZAKA");
            comboBox1.Items.Add("GRUPNI TRENINZI 2X TJEDNO");
            comboBox1.Items.Add("GRUPNI TRENINZI NEO");
            comboBox1.Items.Add("GRUPNI TRENINZI SA POP NEO");
            comboBox1.Items.Add("KOREKTIVNA");
            comboBox1.Items.Add("POJEDINAČNI TRENING");
            comboBox1.Enabled = false;

            loaded = false;
            usluga = false;
            produlji = false;
            promijeni = false;
            m_http = null;
            ApiKey = Utilities.GenerateApiKey();

            TryCatch(new Action(() =>
            {
                if (!File.Exists("Files/fitness.sqlite"))
                    SQLiteConnection.CreateFile("Files/fitness.sqlite");

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
                    Utilities.CreateBackup();
                }
            }));

            try
            {
                HttpServer.MapHandlers();
                m_http = new HttpServer();
                m_http.Start();

                Process.Start($"http://{Utilities.GetLocalIP()}:8181/pregled&api=" + ApiKey);
            }
            catch
            {
                m_http = null;
            }
        }

        private void OnClick(object sender, EventArgs e)
        {
            TryCatch(new Action(() =>
            {
                if (listBox1.SelectedItem != null)
                    listBox1.Items.Remove(listBox1.SelectedItem);
            }));
        }

        private void OnClick2(object sender, EventArgs e)
        {
            TryCatch(new Action(() =>
            {
                if (listBox1.SelectedItem != null)
                {
                    textBox1.Text = listBox1.SelectedItem.ToString().Split('\t')[0];
                    textBox1_KeyDown(sender, new KeyEventArgs(Keys.Enter));
                }
            }));
        }

        private static void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            TryCatch(new Action(() =>
            {
                FitnessDB.Load();
            }));
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            TryCatch(new Action(() =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (!string.IsNullOrEmpty(textBox1.Text) && Utilities.IsDigitsOnly(textBox1.Text))
                    {
                        int bi = Convert.ToInt32(textBox1.Text);
                        Korisnik k = FitnessDB.Korisnici.SingleOrDefault(ko => ko.BrojIskaznice == bi);
                        if (k.Index > 0)
                        {
                            if (k.AktivnaUsluga != "nema aktivne usluge")
                            {
                                string[] date = k.AktivnaDo.Split(new char[] { '.' });
                                DateTime dt = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                                TimeSpan diff = dt - DateTime.Today;
                                if (diff.Days < 0)
                                {
                                    k.Napomena += $"\nZadnja usluga ({Convert.ToInt32(date[1])}/{Convert.ToInt32(date[2])}): {k.AktivnaUsluga}";
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
                            comboBox1.SelectedIndex = Utilities.UslugaToIndex(k.AktivnaUsluga);
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
            }));
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

        private void textBox23_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                button2_Click(sender, e);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            TryCatch(new Action(() =>
            {
                if (!string.IsNullOrEmpty(textBox10.Text) && !string.IsNullOrEmpty(textBox9.Text) && !string.IsNullOrEmpty(textBox11.Text) && !string.IsNullOrEmpty(textBox9.Text))
                {
                    if (Utilities.IsDigitsOnly(textBox9.Text))
                    {
                        int bi = Convert.ToInt32(textBox9.Text);
                        if (FitnessDB.Korisnici.Count(k => k.BrojIskaznice == bi) == 0)
                        {
                            string ime = textBox10.Text, prezime = textBox11.Text, rodenje = textBox13.Text;
                            if (Utilities.IsDigitsOnly(rodenje, true))
                            {
                                if (FitnessDB.Korisnici.Count(k => k.Ime.ToLowerInvariant() == ime.ToLowerInvariant() && k.Prezime.ToLowerInvariant() == prezime.ToLowerInvariant() && k.DatumRodenja == rodenje) == 0)
                                {
                                    Korisnik novi = new Korisnik();
                                    novi.Index = FitnessDB.Korisnici.GenerateIndex();
                                    novi.BrojIskaznice = bi;
                                    novi.Ime = ime.ToUpperInvariant();
                                    novi.Prezime = prezime.ToUpperInvariant();
                                    novi.DatumUclanjenja = DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year;
                                    novi.DatumRodenja = rodenje;
                                    novi.ZadnjiDolazak = "nepoznato";
                                    novi.Napomena = "";
                                    novi.AktivnaUsluga = "nema aktivne usluge";
                                    novi.AktivnaOd = "";
                                    novi.AktivnaDo = "";
                                    novi.Dolazaka = 0;
                                    FitnessDB.Korisnici.Add(novi);
                                    MessageBox.Show("Korisnik uspješno dodan!", "USPJEH", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    FitnessDB.Korisnici.Load();
                                    textBox9.Text = "";
                                    textBox10.Text = "";
                                    textBox11.Text = "";
                                    textBox13.Text = "";
                                }
                                else
                                    MessageBox.Show("Član sa danim detaljima već postoji!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                                MessageBox.Show("Datum rođenja striktno mora biti numerički i u sljedećem obliku (npr.):\n\n2.2.2016\n27.9.2016", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                            MessageBox.Show("Član sa odabranim brojem iskaznice već postoji!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                        MessageBox.Show("Broj iskaznice striktno mora biti numerički!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                    MessageBox.Show("Neka polja su prazna!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            TryCatch(new Action(() =>
            {
                if (!string.IsNullOrEmpty(textBox1.Text) && Utilities.IsDigitsOnly(textBox1.Text))
                {
                    Korisnik k = FitnessDB.Korisnici.SingleOrDefault(ko => ko.BrojIskaznice == Convert.ToInt32(textBox1.Text));
                    k.Napomena = richTextBox1.Text;
                    FitnessDB.Korisnici.Update(k);
                    MessageBox.Show("Napomena uspješno spremljena!", "USPJEH", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text) && Utilities.IsDigitsOnly(textBox1.Text))
            {
                textBox1_KeyDown(sender, new KeyEventArgs(Keys.Enter));
                if (loaded == true)
                {
                    TryCatch(new Action(() =>
                    {
                        Thread.Sleep(100);
                        string vrijeme = DateTime.Now.Hour + "h" + DateTime.Now.Minute + "m";
                        string item = textBox1.Text + "\t\t" + vrijeme + "\t\t" + comboBox1.SelectedItem.ToString();
                        bool contains = false;
                        foreach (var i in listBox1.Items)
                        {
                            string[] it = i.ToString().Split(new char[] { '\t' });
                            if (it[0] == textBox1.Text)
                            {
                                contains = true;
                                break;
                            }
                        }

                        if (!contains)
                        {
                            if (comboBox1.SelectedItem.ToString() != "nema aktivne usluge")
                            {
                                int bi = Convert.ToInt32(textBox1.Text);
                                Korisnik k = FitnessDB.Korisnici.SingleOrDefault(ko => ko.BrojIskaznice == bi);
                                if (k.AktivnaUsluga == "TERETANA 12 DOLAZAKA" || k.AktivnaUsluga == "GRUPNI TRENINZI 2X TJEDNO" || k.AktivnaUsluga == "POJEDINAČNI TRENING")
                                {
                                    if (k.Dolazaka > 0)
                                        k.Dolazaka -= 1;
                                    else
                                    {
                                        MessageBox.Show("Korisnik je iskoristio sve dolaske!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        goto label;
                                    }
                                }

                                listBox1.Items.Add(item);
                                string danas = DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year;
                                Dolasci d = FitnessDB.Dolasci.SingleOrDefault(dol => dol.Datum == danas);
                                if (d.Index > 0)
                                {
                                    d.BrojDolazaka += 1;
                                    FitnessDB.Dolasci.Update(d);
                                }

                                k.ZadnjiDolazak = danas + " u " + DateTime.Now.Hour + "h" + DateTime.Now.Minute + "m";
                                FitnessDB.Korisnici.Update(k);
                                goto label;
                            }
                            else
                                MessageBox.Show("Korisnik nema aktivnu uslugu!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                            MessageBox.Show("Korisnik je već prisutan!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        label:
                        Thread.Sleep(100);
                        loaded = false;
                        EmptyAll();
                    }));
                }
                else
                {
                    loaded = false;
                    EmptyAll();
                }
            }
            else
                EmptyAll();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            TryCatch(new Action(() =>
            {
                int Day = dateTimePicker1.Value.Day, Month = dateTimePicker1.Value.Month, Year = dateTimePicker1.Value.Year;
                string danass = Day + "." + Month + "." + Year;
                DateTime dt = new DateTime(Year, Month, Day), now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                if (dt != now)
                {
                    int[] TotalUsersPerDay = new int[7] { 0, 0, 0, 0, 0, 0, 0 }, NumberOfDays = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
                    string output = null;
                    Dolasci danas = FitnessDB.Dolasci.SingleOrDefault(dol => dol.Datum == danass);
                    if (danas.Index > 0)
                        output = "Ukupno dolazaka za datum '" + dt.ToString("dd.MM.yyyy") + "':\t" + danas.BrojDolazaka.ToString();

                    foreach (DateTime o in EachDay(dt, now))
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

                    int[] Dani = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
                    for(int i = 0; i < 7; i++)
                        if (NumberOfDays[i] != 0)
                            Dani[i] = TotalUsersPerDay[i] / NumberOfDays[i];

                    output += $"\n\n--\n\nProsječni posjetioci po danima od '{danass}':\n\nPonedjeljak: {Dani[0]}\nUtorak: {Dani[1]}\nSrijeda: {Dani[2]}\nČetvrtak: {Dani[3]}\nPetak: {Dani[4]}\nSubota: {Dani[5]}\nNedjelja: {Dani[6]}";
                    MessageBox.Show(output, "DOLASCI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string date = Day + "." + Month + "." + Year;
                    Dolasci d = FitnessDB.Dolasci.SingleOrDefault(dol => dol.Datum == date);
                    if (d.Index > 0)
                        MessageBox.Show("Ukupno dolazaka za datum '" + date + "':\t" + d.BrojDolazaka.ToString(), "DOLASCI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }));
        }

        private IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            TryCatch(new Action(() =>
            {
                if (loaded)
                {
                    if (!usluga)
                    {
                        comboBox1.Enabled = true;
                        button5.Text = "Spremi";
                        usluga = true;
                    }
                    else if (usluga)
                    {
                        if (!string.IsNullOrEmpty(textBox1.Text) && Utilities.IsDigitsOnly(textBox1.Text))
                        {
                            string u = comboBox1.SelectedItem.ToString();
                            int bi = Convert.ToInt32(textBox1.Text);
                            Korisnik k = FitnessDB.Korisnici.SingleOrDefault(ko => ko.BrojIskaznice == bi);
                            if (k.Index > 0)
                            {
                                k.AktivnaUsluga = u;
                                if (u == "TERETANA 12 DOLAZAKA")
                                    k.Dolazaka = 12;
                                if (u == "GRUPNI TRENINZI 2X TJEDNO")
                                    k.Dolazaka = 8;
                                if (u == "POJEDINAČNI TRENING")
                                    k.Dolazaka = 1;

                                DateTime dt1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                                k.AktivnaOd = dt1.Day + "." + dt1.Month + "." + dt1.Year;
                                DateTime dt2 = dt1.AddMonths(1);
                                k.AktivnaDo = dt2.Day + "." + dt2.Month + "." + dt2.Year;
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
            }));
        }

        private void button6_Click(object sender, EventArgs e)
        {
            TryCatch(new Action(() =>
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
                            if (Utilities.IsDigitsOnly(datum, true))
                            {
                                if (!string.IsNullOrEmpty(textBox1.Text) && Utilities.IsDigitsOnly(textBox1.Text))
                                {
                                    int bi = Convert.ToInt32(textBox1.Text);
                                    Korisnik k = FitnessDB.Korisnici.SingleOrDefault(ko => ko.BrojIskaznice == bi);
                                    k.AktivnaDo = datum;
                                    FitnessDB.Korisnici.Update(k);
                                    textBox8.ReadOnly = true;
                                    button6.Text = "Promijeni";
                                    produlji = false;
                                    MessageBox.Show("Usluga uspješno produljena!", "USPJEH", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                    MessageBox.Show("Broj iskaznice ne smije biti 0 ili prazan!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                                MessageBox.Show("Datum striktno mora biti numerički i u sljedećem obliku (npr.):\n\n2.2.2016\n27.9.2016", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TryCatch(new Action(() =>
            {
                if (!loaded)
                {
                    string ime = textBox2.Text.ToLowerInvariant(), prezime = textBox3.Text.ToLowerInvariant();
                    if (string.IsNullOrEmpty(ime) && !string.IsNullOrEmpty(prezime))
                    {
                        List<Korisnik> ks = FitnessDB.Korisnici.Select(ko => ko.Prezime.ToLowerInvariant() == prezime);
                        if (ks.Count > 0)
                        {
                            string korisnici = null;
                            foreach (Korisnik k in ks)
                                korisnici += k.BrojIskaznice + "\t" + k.Ime + " " + k.Prezime + "\n";
                            MessageBox.Show("Korisnici sa prezimenom '" + prezime + "':\n\n" + korisnici, "INFORMACIJA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                            MessageBox.Show("Ne postoji korisnik sa tim prezimenom!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (!string.IsNullOrEmpty(ime) && string.IsNullOrEmpty(prezime))
                    {
                        List<Korisnik> ks = FitnessDB.Korisnici.Select(ko => ko.Ime.ToLowerInvariant() == ime);
                        if (ks.Count > 0)
                        {
                            string korisnici = null;
                            foreach (Korisnik k in ks)
                                korisnici += k.BrojIskaznice + "\t" + k.Ime + " " + k.Prezime + "\n";
                            MessageBox.Show("Korisnici sa imenom '" + ime + "':\n\n" + korisnici, "INFORMACIJA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                            MessageBox.Show("Ne postoji korisnik sa tim imenom!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        List<Korisnik> ks = FitnessDB.Korisnici.Select(ko => ko.Ime.ToLowerInvariant() == ime && ko.Prezime.ToLowerInvariant() == prezime);
                        if (ks.Count > 0)
                        {
                            string korisnici = null;
                            foreach (Korisnik k in ks)
                                korisnici += k.BrojIskaznice + "\t" + k.Ime + " " + k.Prezime + "\n";
                            MessageBox.Show(korisnici, "INFORMACIJA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                            MessageBox.Show("Ne postoji korisnik sa tim imenom i prezimenom!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    EmptyAll();
                }
            }));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            TryCatch(new Action(() =>
            {
                if (loaded)
                {
                    if (comboBox1.SelectedItem.ToString() != "nema aktivne usluge")
                    {
                        if (!promijeni)
                        {
                            textBox7.ReadOnly = false;
                            button3.Text = "Spremi";
                            promijeni = true;
                        }
                        else if (promijeni)
                        {
                            string datum = textBox7.Text;
                            if (Utilities.IsDigitsOnly(datum, true))
                            {
                                if (!string.IsNullOrEmpty(textBox1.Text) && Utilities.IsDigitsOnly(textBox1.Text))
                                {
                                    int bi = Convert.ToInt32(textBox1.Text);
                                    Korisnik k = FitnessDB.Korisnici.SingleOrDefault(ko => ko.BrojIskaznice == bi);
                                    k.AktivnaOd = datum;
                                    FitnessDB.Korisnici.Update(k);
                                    textBox7.ReadOnly = true;
                                    button3.Text = "Promijeni";
                                    promijeni = false;
                                    MessageBox.Show("Promijenjeno!", "USPJEH", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                    MessageBox.Show("Broj iskaznice ne smije biti 0 ili prazan!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                                MessageBox.Show("Datum striktno mora biti numerički i u sljedećem obliku (npr.):\n\n2.2.2016\n27.9.2016", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }));
        }

        private void label7_Click(object sender, EventArgs e)
        {
            TryCatch(new Action(() =>
            {
                if (loaded)
                {
                    string usluga = comboBox1.Text.ToString();
                    if (usluga == "TERETANA 12 DOLAZAKA" || usluga == "GRUPNI TRENINZI 2X TJEDNO" || usluga == "POJEDINAČNI TRENING")
                    {
                        if (!string.IsNullOrEmpty(textBox1.Text) && Utilities.IsDigitsOnly(textBox1.Text))
                        {
                            int bi = Convert.ToInt32(textBox1.Text);
                            Korisnik k = FitnessDB.Korisnici.SingleOrDefault(ko => ko.BrojIskaznice == bi);
                            if (k.Index > 0)
                                MessageBox.Show("Preostalo dolazaka za uslugu '" + usluga + "':\t" + k.Dolazaka.ToString(), "INFORMACIJA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                            MessageBox.Show("Broj iskaznice ne smije biti 0 ili prazan!", "UPOZORENJE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }));
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (m_http != null)
                Process.Start($"http://{Utilities.GetLocalIP()}:8181/pregled&api=" + ApiKey);
        }

        public static void TryCatch(Action a)
        {
            try
            {
                a.Invoke();
            }
            catch (Exception ex)
            {
                m_http.Stop();
                m_http = null;
                MessageBox.Show("Postupak prijave pogreške:\n1. Slikajte ovu poruku pomoću tipke \"Print Screen\"\n2. Otiđite na \"www.pasteboard.co\" sa Google Chrome-om\n3. Prisnite tipku \"Ctrl\" i u isto vrijeme tipku \"V\" (dakle CTRL+V)\n4. Na otvorenoj web stranici odaberite zelenu tipku na kojoj piše \"UPLOAD\"5. Pošaljite mi link koji će se prikazati nakon pritiska na spomenutu tipku na sljedeći mail -> fiki.xperia@gmail.com\n\n" + ex.ToString(), "POGREŠKA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        #region Web stvari

        public int GetTotalMembers()
        {
            return FitnessDB.Korisnici.Count(k => true);
        }

        public int GetActiveMembers()
        {
            return FitnessDB.Korisnici.Count(k => k.AktivnaUsluga != "nema aktivne usluge");
        }

        public int GetCurrentMembersCount()
        {
            return listBox1.Items.Count;
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
            foreach (DateTime o in EachDay(now.AddMonths(-for_last_month), now))
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

        #endregion
    }
}