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

namespace Fitness.Database
{
    class FitnessDB
    {
        private static Database Server = new Database(Constants.DbLocation);
        public static Table<Korisnik> Korisnici = new Table<Korisnik>(Server);
        public static Table<Dolasci> Dolasci = new Table<Dolasci>(Server);
        public static Table<Statistika> Statistika = new Table<Statistika>(Server);

        public static void Load()
        {
            Korisnici.Load();
            Dolasci.Load();
            Statistika.Load();
        }
    }
}