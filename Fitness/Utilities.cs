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
using System.IO;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Net;
using System.Linq;

namespace Fitness
{
    public class Utilities
    {
        private static string PublicIP = null;

        public static int UslugaToIndex(string usluga)
        {
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
            else if (usluga == "TERETANA 12 DOLAZAKA")
                return 5;
            else if (usluga == "GRUPNI TRENINZI 2X TJEDNO")
                return 6;
            else if (usluga == "GRUPNI TRENINZI NEO")
                return 7;
            else if (usluga == "GRUPNI TRENINZI SA POP NEO")
                return 8;
            else if (usluga == "KOREKTIVNA")
                return 9;
            else if (usluga == "POJEDINAČNI TRENING")
                return 10;
            else
                return 0;
        }

        public static int GetDaysInMonth(int month, int year)
        {
            if (month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10 || month == 12)
                return 31;
            else if (month == 4 || month == 6 || month == 9 || month == 11)
                return 30;
            else if (month == 2)
                return IsLeapYear(year) == true ? 29 : 28;
            else return 0;
        }

        public static bool IsLeapYear(int year)
        {
            if (year % 4 == 0)
            {
                if (year % 100 == 0)
                {
                    if (year % 400 == 0)
                        return true;
                    else
                        return false;
                }
                else
                    return true;
            }
            else
                return false;
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

        public static void CreateBackup()
        {
            if (File.Exists("Files/fitness_backup.sqlite"))
                File.Delete("Files/fitness_backup.sqlite");

            File.Copy("Files/fitness.sqlite", "Files/fitness_backup.sqlite");
        }

        public static string GetLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();

            return "127.0.0.1";
        }

        public static string GetPublicIP()
        {
            try
            {
                if(PublicIP == null)
                    PublicIP = (new WebClient()).DownloadString("http://bot.whatismyipaddress.com/");

                return PublicIP;
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        private static Random random = new Random();
        public static string GenerateApiKey()
        {
            return new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 32).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}