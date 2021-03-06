﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Meziantou.Framework.Win32
{
    /// <summary>
    /// Defines a file's perceived type based on its extension.
    /// </summary>
    public sealed class Perceived
    {
        private static readonly Dictionary<string, Perceived> _perceivedTypes = new Dictionary<string, Perceived>(StringComparer.OrdinalIgnoreCase);

        private static object SyncObject { get; } = new object();

        private Perceived()
        {
        }

        /// <summary>
        /// Adds a perceived instance to the list.
        /// </summary>
        /// <param name="extension">The file extension. May not be null.</param>
        /// <param name="type">The perceived type.</param>
        public static Perceived AddPerceived(string extension, PerceivedType type)
        {
            if (extension == null) throw new ArgumentNullException(nameof(extension));

            var perceived = new Perceived();
            perceived.Extension = extension;
            perceived.PerceivedType = type;
            perceived.PerceivedTypeSource = PerceivedTypeSource.HardCoded;
            lock (SyncObject)
            {
                _perceivedTypes[perceived.Extension] = perceived;
            }
            return perceived;
        }

        [DllImport("shlwapi.dll")]
        private static extern int AssocGetPerceivedType(
            [MarshalAs(UnmanagedType.LPWStr)] string pszExt,
            ref PerceivedType ptype,
            ref PerceivedTypeSource pflag,
            ref IntPtr ppszType);

        /// <summary>
        /// Gets the file's xtension.
        /// </summary>
        /// <value>The file's extension.</value>
        public string Extension { get; set; }

        /// <summary>
        /// Indicates the normalized perceived type.
        /// </summary>
        /// <value>The normalized perceived type.</value>
        public PerceivedType PerceivedType { get; private set; }

        /// <summary>
        /// Indicates the source of the perceived type information.
        /// </summary>
        /// <value>the source of the perceived type information.</value>
        public PerceivedTypeSource PerceivedTypeSource { get; set; }

        /// <summary>
        /// Gets a file's perceived type based on its extension.
        /// </summary>
        /// <param name="fileName">The file name. May not be null..</param>
        /// <returns>An instance of the PerceivedType type.</returns>
        public static Perceived GetPerceivedType(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            var extension = Path.GetExtension(fileName);
            if (extension == null)
                throw new ArgumentException(null, nameof(fileName));

            extension = extension.ToLowerInvariant();

            if (_perceivedTypes.TryGetValue(extension, out Perceived ptype))
                return ptype;

            lock (SyncObject)
            {
                if (!_perceivedTypes.TryGetValue(extension, out ptype))
                {
                    ptype = new Perceived();
                    ptype.Extension = extension;

                    using (var key = Registry.ClassesRoot.OpenSubKey(extension, writable: false))
                    {
                        if (key != null)
                        {
                            var ct = key.GetStringValue("PerceivedType");
                            if (ct != null)
                            {
                                ptype.PerceivedType = Extensions.GetEnumValue(ct, PerceivedType.Custom);
                                ptype.PerceivedTypeSource = PerceivedTypeSource.SoftCoded;
                            }
                            else
                            {
                                ct = key.GetStringValue("Content Type");
                                if (ct != null)
                                {
                                    var pos = ct.IndexOf('/');
                                    if (pos > 0)
                                    {
                                        ptype.PerceivedType = Extensions.GetEnumValue(ct.Substring(0, pos), PerceivedType.Custom);
                                        ptype.PerceivedTypeSource = PerceivedTypeSource.Mime;
                                    }
                                }
                            }
                        }
                    }

                    if (ptype.PerceivedType == PerceivedType.Unknown)
                    {
                        var text = IntPtr.Zero;
                        var type = PerceivedType.Unknown;
                        var source = PerceivedTypeSource.Undefined;
                        var hr = AssocGetPerceivedType(extension, ref type, ref source, ref text);
                        if (hr != 0)
                        {
                            ptype.PerceivedType = PerceivedType.Unspecified;
                            ptype.PerceivedTypeSource = PerceivedTypeSource.Undefined;
                        }
                        else
                        {
                            ptype.PerceivedType = type;
                            ptype.PerceivedTypeSource = source;
                        }
                    }

                    _perceivedTypes.Add(ptype.Extension, ptype);
                }

                return ptype;
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return Extension + ":" + PerceivedType + " (" + PerceivedTypeSource + ")";
        }
    }
}
