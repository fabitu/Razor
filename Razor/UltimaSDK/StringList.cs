#region license
// Razor: An Ultima Online Assistant
// Copyright (c) 2022 Razor Development Community on GitHub <https://github.com/markdwags/Razor>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Assistant;

namespace Ultima
{
    public sealed class StringList
    {
        private int m_Header1;
        private short m_Header2;

        public List<StringEntry> Entries { get; set; }
        public string Language { get; private set; }

        private Dictionary<int, string> m_StringTable;
        private Dictionary<int, StringEntry> m_EntryTable;

        private static byte[] m_Buffer = new byte[1024];

        /// <summary>
        /// Initialize <see cref="StringList"/> of Language
        /// </summary>
        /// <param name="language"></param>
        public StringList(string language)
        {
            Language = language;
            LoadEntry(Files.GetFilePath($"cliloc.{language}"));
        }

        /// <summary>
        /// Initialize <see cref="StringList"/> of Language from path
        /// </summary>
        /// <param name="language"></param>
        /// <param name="path"></param>
        public StringList(string language, string path)
        {
            Language = language;
            
            LoadEntry(path);
        }

        private void LoadEntry(string path)
        {
            if (Engine.ClientVersion.Major >= 7 && Engine.ClientVersion.Build >= 105)
            {
                LoadNewEntryFormat(path);
            }
            else
            {
                LoadOldEntryFormat(path);
            }
        }

        private void LoadOldEntryFormat(string path)
        {
            if (path == null)
            {
                Entries = new List<StringEntry>(0);
                return;
            }

            Entries = new List<StringEntry>();
            m_StringTable = new Dictionary<int, string>();
            m_EntryTable = new Dictionary<int, StringEntry>();

            using (BinaryReader bin =
                new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                m_Header1 = bin.ReadInt32();
                m_Header2 = bin.ReadInt16();

                while (bin.BaseStream.Length != bin.BaseStream.Position)
                {
                    int number = bin.ReadInt32();
                    byte flag = bin.ReadByte();
                    int length = bin.ReadInt16();

                    if (length > m_Buffer.Length)
                        m_Buffer = new byte[(length + 1023) & ~1023];

                    bin.Read(m_Buffer, 0, length);
                    string text = Encoding.UTF8.GetString(m_Buffer, 0, length);

                    StringEntry se = new StringEntry(number, text, flag);
                    Entries.Add(se);

                    m_StringTable[number] = text;
                    m_EntryTable[number] = se;
                }
            }
        }
        
        // Based on the implementation from Karasho @ ClassicUO
        private void LoadNewEntryFormat(string path)
        {
            if (path == null)
            {
                Entries = new List<StringEntry>(0);
                return;
            }

            Entries = new List<StringEntry>();
            m_StringTable = new Dictionary<int, string>();
            m_EntryTable = new Dictionary<int, StringEntry>();

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                int bytesRead;
                var totalRead = 0;
                var buf = new byte[fileStream.Length];
                while ((bytesRead = fileStream.Read(buf, totalRead, Math.Min(4096, buf.Length - totalRead))) > 0)
                    totalRead += bytesRead;

                var output = buf[3] == 0x8E ? BwtDecompress.Decompress(buf) : buf;

                var reader = new StackDataReader(output);
                m_Header1 = reader.ReadInt32LE();
                m_Header2 = reader.ReadInt16LE();

                while (reader.Remaining > 0)
                {
                    var number = reader.ReadInt32LE();
                    var flag = reader.ReadUInt8();
                    var length = reader.ReadInt16LE();
                    var text = string.Intern(reader.ReadUTF8(length));

                    m_StringTable[number] = text;
                    StringEntry se = new StringEntry(number, text, flag);
                    Entries.Add(se);
                    m_EntryTable[number] = se;
                }
            }
        }

        /// <summary>
        /// Saves <see cref="SaveStringList"/> to FileName
        /// </summary>
        /// <param name="FileName"></param>
        public void SaveStringList(string FileName)
        {
            using (FileStream fs = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter bin = new BinaryWriter(fs))
                {
                    bin.Write(m_Header1);
                    bin.Write(m_Header2);
                    Entries.Sort(new StringList.NumberComparer(false));
                    foreach (StringEntry entry in Entries)
                    {
                        bin.Write(entry.Number);
                        bin.Write((byte) entry.Flag);
                        byte[] utf8String = Encoding.UTF8.GetBytes(entry.Text);
                        ushort length = (ushort) utf8String.Length;
                        bin.Write(length);
                        bin.Write(utf8String);
                    }
                }
            }
        }

        public string GetString(int number)
        {
            if (m_StringTable == null || !m_StringTable.ContainsKey(number))
                return null;

            return m_StringTable[number];
        }

        public StringEntry GetEntry(int number)
        {
            if (m_EntryTable == null || !m_EntryTable.ContainsKey(number))
                return null;

            return m_EntryTable[number];
        }

        #region SortComparer

        public class NumberComparer : IComparer<StringEntry>
        {
            private bool m_desc;

            public NumberComparer(bool desc)
            {
                m_desc = desc;
            }

            public int Compare(StringEntry objA, StringEntry objB)
            {
                if (objA.Number == objB.Number)
                    return 0;
                else if (m_desc)
                    return (objA.Number < objB.Number) ? 1 : -1;
                else
                    return (objA.Number < objB.Number) ? -1 : 1;
            }
        }

        public class FlagComparer : IComparer<StringEntry>
        {
            private bool m_desc;

            public FlagComparer(bool desc)
            {
                m_desc = desc;
            }

            public int Compare(StringEntry objA, StringEntry objB)
            {
                if ((byte) objA.Flag == (byte) objB.Flag)
                {
                    if (objA.Number == objB.Number)
                        return 0;
                    else if (m_desc)
                        return (objA.Number < objB.Number) ? 1 : -1;
                    else
                        return (objA.Number < objB.Number) ? -1 : 1;
                }
                else if (m_desc)
                    return ((byte) objA.Flag < (byte) objB.Flag) ? 1 : -1;
                else
                    return ((byte) objA.Flag < (byte) objB.Flag) ? -1 : 1;
            }
        }

        public class TextComparer : IComparer<StringEntry>
        {
            private bool m_desc;

            public TextComparer(bool desc)
            {
                m_desc = desc;
            }

            public int Compare(StringEntry objA, StringEntry objB)
            {
                if (m_desc)
                    return String.Compare(objB.Text, objA.Text);
                else
                    return String.Compare(objA.Text, objB.Text);
            }
        }

        #endregion
    }
}