using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitness
{
    public class Utilities
    {
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
    }
}
