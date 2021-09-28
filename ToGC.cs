using System;
using System.Collections.Generic;
using System.Linq;

namespace ToGeometryConverter
{
    public static class ToGC
    {
        /*public static List<GCFormat> Formats { get; } = new List<GCFormat>()
        {
            new SVG(),
            new DXF(),
            new DEXCeil(),
            new STL(),
            new ILD(),
            new MetaFile(),
            new JSON()
        };*/

        /// <summary>
        /// Get converter class from filename
        /// </summary>
        /// <param name="Filename">Path fo file</param>
        /// <param name="formats">Format List</param>
        /// <returns></returns>
        public static GCFormat GetConverter(string Filename, ICollection<GCFormat> formats)
        {
            string InFileFormat = Filename.Split('.').Last();

            foreach (GCFormat format in formats)
            {
                foreach (string frm in format.ShortName)
                {
                    if (frm.ToLower() == InFileFormat.ToLower())
                    {
                        return format;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get All Format string for filter
        /// </summary>
        /// <param name="formats"></param>
        /// <returns></returns>
        public static string GetAllFormatsFilter(ICollection<GCFormat> formats)
        {
            string _allformat = string.Empty;
            //add standart format
            foreach (GCFormat format in formats)
            {
                foreach (string frm in format.ShortName)
                {
                    _allformat += $"*.{frm};";
                }
            }

            return $"All Format ({_allformat}) | {_allformat}";
        }

        /// <summary>
        /// Get filter string for all format
        /// </summary>
        /// <param name="AddFormat"></param>
        /// <param name="formats"></param>
        /// <returns></returns>
        public static string GetFilter(GCFormat[] AddFormat, ICollection<GCFormat> formats)
        {
            string _filter = GetAllFormatsFilter(formats);

            foreach (GCFormat format in formats)
            {
                string strformat = string.Empty;
                foreach (string frm in format.ShortName)
                {
                    strformat += $"*.{frm};";
                }
                _filter += $" | {format.Name}({strformat}) | {strformat}";
            }

            _filter += " | All Files (*.*)|*.*";

            return _filter;
        }
    }
}
