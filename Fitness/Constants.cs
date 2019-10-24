using System.Collections.Generic;

namespace Fitness
{
    public static class Constants
    {
        public static readonly string
            DbLocation = "Files/fitness.sqlite",
            DbBackupLocation = "Files/fitness_backup.sqlite";

        public static readonly List<string> Usluge = new List<string>()
        {
            "nema aktivne usluge",
            "TERETANA NEO",
            "TERETANA NEO DO 16H",
            "TERETANA SA POP NEO",
            "TERETANA SA POP DO 16H",
            "TERETANA 12 DOLAZAKA",
            "GRUPNI TRENINZI 2X TJEDNO",
            "GRUPNI TRENINZI NEO",
            "GRUPNI TRENINZI SA POP NEO",
            "KOREKTIVNA",
            "POJEDINAČNI TRENING"
        };

        public static readonly List<string> Menu = new List<string>()
        {
            "Odlazak",
            "Prikaži korisničke podatke"
        };
    }
}