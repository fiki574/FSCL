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

    Credits: https://github.com/usertoroot
*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fitness.Database
{
    public class Table<T> where T : struct
    {
        private Database m_database;
        private DatabaseTable m_databaseTable;
        private List<T> m_entries = null;
        private int m_maxIndex = 0;
        private FieldInfo m_indexFieldInfo;

        public int Rows
        {
            get
            {
                return m_entries.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                lock (m_entries)
                    return m_entries[index];
            }
            set
            {
                lock (m_entries)
                    m_entries[index] = value;
            }
        }

        public Table(Database database)
        {
            m_database = database;

            object[] attributes = typeof(T).GetCustomAttributes(typeof(DatabaseTable), false);
            if (attributes.Length < 1)
                return;

            m_databaseTable = (DatabaseTable)attributes[0];

            m_indexFieldInfo = typeof(T).GetField("Index");
            if (m_indexFieldInfo == null)
                return;
        }

        public void Load()
        {
            if (m_database.ContainsTable(m_databaseTable))
            {
                try
                {
                    m_database.UpdateStructure(typeof(T));
                }
                catch (Exception)
                {
                    m_database.CreateStructure(typeof(T));
                }
            }
            else
            {
                m_database.CreateStructure(typeof(T));
            }

            if (m_entries != null)
            {
                lock (m_entries)
                {
                    lock (m_database)
                    {
                        m_entries = m_database.GetObjects<T>();
                    }
                }
            }
            else
            {
                lock (m_database)
                {
                    m_entries = m_database.GetObjects<T>();
                }
            }

            lock (m_entries)
            {
                for (int i = 0; i < m_entries.Count; i++)
                {
                    int index = (int)m_indexFieldInfo.GetValue(m_entries[i]);
                    if (index > m_maxIndex)
                        m_maxIndex = index;
                }
            }
        }

        public void Add(T entry)
        {
            if (m_entries == null)
                return;

            lock (m_entries)
            {
                m_entries.Add(entry);
            }

            lock (m_database)
            {
                m_database.Insert(entry);
            }
        }

        public void Remove(int index)
        {
            if (m_entries == null)
                return;

            bool found = false;
            T entry = default(T);

            lock (m_entries)
            {
                for (int i = 0; i < m_entries.Count; i++)
                {
                    if (index == (int)m_indexFieldInfo.GetValue(m_entries[i]))
                    {
                        entry = m_entries[i];
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
                return;
            else
                Remove(entry);
        }

        public void Remove(T entry)
        {
            if (m_entries == null)
                return;

            lock (m_entries)
            {
                m_entries.Remove(entry);
            }

            lock (m_database)
            {
                m_database.Remove(entry);
            }
        }

        public void Update(T entry)
        {
            if (m_entries == null)
                return;

            bool found = false;

            lock (m_entries)
            {
                for (int i = 0; i < m_entries.Count; i++)
                {
                    if ((int)m_indexFieldInfo.GetValue(entry) == (int)m_indexFieldInfo.GetValue(m_entries[i]))
                    {
                        m_entries[i] = entry;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
                return;
            else
                lock (m_database)
                    m_database.Update(entry);
        }

        public T Single(int index)
        {
            if (m_entries == null)
                return default(T);

            lock (m_entries)
            {
                for (int i = 0; i < m_entries.Count; i++)
                    if (index == (int)m_indexFieldInfo.GetValue(m_entries[i]))
                        return m_entries[i];
            }

            return default(T);
        }

        public T Single(Func<T, bool> comparator)
        {
            if (m_entries == null)
                return default(T);

            lock (m_entries)
            {
                for (int i = 0; i < m_entries.Count; i++)
                    if (comparator(m_entries[i]))
                        return m_entries[i];
            }

            return default(T);
        }

        public T SingleOrDefault(Func<T, bool> comparator)
        {
            if (m_entries == null)
                return default(T);

            lock (m_entries)
            {
                for (int i = 0; i < m_entries.Count; i++)
                    if (comparator(m_entries[i]))
                        return m_entries[i];
            }

            return default(T);
        }

        public List<T> Select(Func<T, bool> comparator)
        {
            if (m_entries == null)
                return null;

            List<T> results = new List<T>();
            lock (m_entries)
            {
                for (int i = 0; i < m_entries.Count; i++)
                    if (comparator(m_entries[i]))
                        results.Add(m_entries[i]);
            }

            return results;
        }

        public bool Exists(Func<T, bool> comparator)
        {
            lock (m_entries)
            {
                for (int i = 0; i < m_entries.Count; i++)
                    if (comparator(m_entries[i]))
                        return true;
            }
            return false;
        }

        public int Count(Func<T, bool> comparator)
        {
            int count = 0;
            lock (m_entries)
            {
                for (int i = 0; i < m_entries.Count; i++)
                    if (comparator(m_entries[i]))
                        count++;
            }
            return count;
        }

        public int GenerateIndex()
        {
            return ++m_maxIndex;
        }
    }
}
