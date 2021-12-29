/***************************************************************************
 *  Copyright (C) 2009 by Peter L Jones                                    *
 *  pljones@users.sf.net                                                   *
 *                                                                         *
 *  This file is part of the Sims 3 Package Interface (s3pi)               *
 *                                                                         *
 *  s3pi is free software: you can redistribute it and/or modify           *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  s3pi is distributed in the hope that it will be useful,                *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with s3pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Regul.S3PI.Interfaces;

namespace Regul.S3PI.Resources
{
    public class ScriptResource : AResource
    {
        #region Attributes

        byte _version = 1;
        string _gameVersion;
        uint _unknown2 = 0x2BC4F79F;
        byte[] _md5Sum = new byte[64];
        byte[] _md5Table = new byte[0];
        byte[] _md5data = new byte[0];
        byte[] _cleardata = new byte[0];

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance of the resource
        /// </summary>
        /// <param name="s">Data stream to use, or null to create from scratch</param>
        public ScriptResource(Stream s) : base(s)
        {
            if (stream == null)
            {
                stream = UnParse();
                OnResourceChanged(this, EventArgs.Empty);
            }

            stream.Position = 0;
            Parse(stream);
        }

        #endregion

        #region Data I/O

        void Parse(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            _version = br.ReadByte();
            if (_version > 1)
                _gameVersion = System.Text.Encoding.Unicode.GetString(br.ReadBytes(br.ReadInt32() * 2));
            else
                _gameVersion = "";
            _unknown2 = br.ReadUInt32();
            _md5Sum = br.ReadBytes(64);
            ushort count = br.ReadUInt16();
            _md5Table = br.ReadBytes(count * 8);
            _md5data = br.ReadBytes(count * 512);
            _cleardata = Decrypt();
        }

        byte[] Decrypt()
        {
            ulong seed = 0;
            for (int i = 0; i < _md5Table.Length; i += 8) seed += BitConverter.ToUInt64(_md5Table, i);
            seed = (ulong) (_md5Table.Length - 1) & seed;

            MemoryStream w = new MemoryStream();
            MemoryStream r = new MemoryStream(_md5data);

            for (int i = 0; i < _md5Table.Length; i += 8)
            {
                byte[] buffer = new byte[512];

                if ((_md5Table[i] & 1) == 0)
                {
                    r.Read(buffer, 0, buffer.Length);

                    for (int j = 0; j < 512; j++)
                    {
                        byte value = buffer[j];
                        buffer[j] ^= _md5Table[seed];
                        seed = (seed + value) % (ulong) _md5Table.Length;
                    }
                }

                w.Write(buffer, 0, buffer.Length);
            }

            return w.ToArray();
        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(_version);
            if (_version > 1)
            {
                bw.Write(_gameVersion.Length);
                bw.Write(System.Text.Encoding.Unicode.GetBytes(_gameVersion));
            }

            bw.Write(_unknown2);
            _md5Table = new byte[(((_cleardata.Length & 0x1ff) == 0 ? 0 : 1) + (_cleardata.Length >> 9)) << 3];
            _md5data = Encrypt();
            bw.Write(_md5Sum);
            bw.Write((ushort) (_md5Table.Length >> 3));
            bw.Write(_md5Table);
            bw.Write(_md5data);
            return ms;
        }

        byte[] Encrypt()
        {
            ulong seed = 0;
            for (int i = 0; i < _md5Table.Length; i += 8) seed += BitConverter.ToUInt64(_md5Table, i);
            seed = (ulong) (_md5Table.Length - 1) & seed;

            MemoryStream w = new MemoryStream();
            MemoryStream r = new MemoryStream(_cleardata);

            for (int i = 0; i < _md5Table.Length; i += 8)
            {
                byte[] buffer = new byte[512];
                r.Read(buffer, 0, buffer.Length);

                for (int j = 0; j < 512; j++)
                {
                    buffer[j] ^= _md5Table[seed];
                    seed = (seed + buffer[j]) % (ulong) _md5Table.Length;
                }

                w.Write(buffer, 0, buffer.Length);
            }

            return w.ToArray();
        }

        #endregion

        #region AApiVersionedFields

        /// <summary>
        /// The list of available field names on this API object
        /// </summary>
        public override List<string> ContentFields => GetContentFields(GetType());

        #endregion

        #region Content Fields

        [ElementPriority(1)]
        public byte Version
        {
            get => _version;
            set
            {
                if (_version != value)
                {
                    _version = value;
                    OnResourceChanged(this, EventArgs.Empty);
                }
            }
        }

        [ElementPriority(2)]
        public string GameVersion
        {
            get => _gameVersion;
            set
            {
                if (_gameVersion != value)
                {
                    _gameVersion = value;
                    OnResourceChanged(this, EventArgs.Empty);
                }
            }
        }

        [ElementPriority(3)]
        public uint Unknown2
        {
            get => _unknown2;
            set
            {
                if (_unknown2 != value)
                {
                    _unknown2 = value;
                    OnResourceChanged(this, EventArgs.Empty);
                }
            }
        }

        [ElementPriority(99)]
        public BinaryReader Assembly
        {
            get => new BinaryReader(new MemoryStream(_cleardata));
            set
            {
                if (value.BaseStream.CanSeek)
                {
                    value.BaseStream.Position = 0;
                    _cleardata = value.ReadBytes((int) value.BaseStream.Length);
                }
                else
                {
                    MemoryStream ms = new MemoryStream();
                    byte[] buffer = new byte[1024 * 1024];
                    for (int read = value.BaseStream.Read(buffer, 0, buffer.Length);
                        read > 0;
                        read = value.BaseStream.Read(buffer, 0, buffer.Length))
                        ms.Write(buffer, 0, read);
                    _cleardata = ms.ToArray();
                }

                OnResourceChanged(this, EventArgs.Empty);
            }
        }

        public string Value
        {
            get
            {
                List<string> fields = ValueBuilderFields;
                string s = "";

                foreach (string f in fields)
                {
                    if (f.Equals("Assembly"))
                    {
                        if (_cleardata.Length > 0)
                        {
                            try
                            {
                                SafeLoader loader = (SafeLoader) Activator.CreateInstance(typeof(SafeLoader));
                                //SafeLoader loader = (SafeLoader) ap.CreateInstanceAndUnwrap(this.GetType().Assembly.FullName, typeof(SafeLoader).FullName);
                                s += loader.Value(_cleardata);
                            }
                            catch (Exception ex)
                            {
                                s += $"{f}: Error: {ex.Message}\n";
                            }
                        }
                        else
                        {
                            s += "Assembly: no data\n";
                        }
                    }
                    else
                        s += $"{f}: {"" + this[f]}\n";
                }

                return s;
            }
        }
        class SafeLoader : MarshalByRefObject
        {
            public string Value(byte[] rawAssembly)
            {
                string s = "";
                try
                {
                    Assembly assy = System.Reflection.Assembly.Load(rawAssembly);
                    string h = $"\n---------\n---------\n{assy.GetType().Name}: Assembly\n---------\n";
                    string t = "---------\n";
                    s += h;
                    s += assy + "\n";

                    foreach (PropertyInfo p in typeof(Assembly).GetProperties())
                    {
                        try
                        {
                            s += $"  {p.Name}: {"" + p.GetValue(assy, null)}\n";
                        }
                        catch (Exception ex)
                        {
                            s += $"  {p.Name}: Error reading Value: {ex.Message}\n";
                        }
                    }

                    s += "\nReferences:\n";
                    foreach (AssemblyName p in assy.GetReferencedAssemblies())
                        s += $"  Ref: {p}\n";

                    s += "\nExported Types:\n";
                    try
                    {
                        Type[] exportedTypes = assy.GetExportedTypes();
                        foreach (Type p in exportedTypes)
                            s += $"  Type: {p}\n";
                    }
                    catch (Exception ex)
                    {
                        s += "  Cannot get Exported Types: " + ex.Message + "\n";
                    }

                    s += t;
                }
                catch (Exception ex)
                {
                    s = "\n---------\n---------\n Assembly\n---------\n";
                    for (Exception inex = ex; inex != null; inex = inex.InnerException)
                    {
                        s += "\n" + inex.Message;
                        s += "\n" + inex.StackTrace;
                        s += "\n-----";
                    }

                    s += "---------\n";
                }

                return s;
            }
        }

        #endregion
    }

    /// <summary>
    /// ResourceHandler for NameMapResource wrapper
    /// </summary>
    public class ScriptResourceHandler : AResourceHandler
    {
        /// <summary>
        /// Create the content of the Dictionary
        /// </summary>
        public ScriptResourceHandler() => Add(typeof(ScriptResource), new List<string>(new[] {"0x073FAA07"}));
    }
}