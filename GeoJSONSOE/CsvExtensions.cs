//The MIT License

//Copyright (c) 2013 Zekiah Technologies, Inc.

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Server;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SOESupport;


//TODO: throw in some exception handling throughout
namespace Zekiah.CSV
{
    public static class CsvExtensions
    {
        public static string ToCSV(this IFeatureCursor rows, bool includeHeader)
        {
            string retval = "";
            StringBuilder sb = new StringBuilder();
            if (includeHeader)
            {
                sb.AppendLine(GetHeaderRow(rows.Fields));
            }
            IFeature row = rows.NextFeature();
            while (row != null)
            {
                sb.AppendLine(row.ToCSV(false));
                row = rows.NextFeature();
            }

            retval = sb.ToString();
            return retval;
        }

        public static string ToCSV(this IFeature row, bool includegeoms)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(processFields(row, includegeoms));
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing row", ex);
            }
        }

        private static string processFields(IFeature row, bool includegeoms)
        {
            try
            {
                List<string> props = new List<string>();
                for (int fldnum = 0; fldnum < row.Fields.FieldCount; fldnum++)
                {
                    IField fld = row.Fields.Field[fldnum];
                    string fldval = "";
                    if (row.Value[fldnum].Equals(null))
                    {
                        fldval = "null";
                    }
                    else
                    {
                        switch (fld.Type)
                        {
                            case esriFieldType.esriFieldTypeGeometry:
                                if (!includegeoms)
                                {
                                    fldval = "\"geometry\"";
                                }
                                else
                                {
                                    fldval = GetHexWkb(row.Shape);
                                }
                                break;
                            case esriFieldType.esriFieldTypeBlob:
                                fldval = "\"blob\"";
                                break;
                            case esriFieldType.esriFieldTypeSmallInteger:
                            case esriFieldType.esriFieldTypeInteger:
                            case esriFieldType.esriFieldTypeSingle:
                            case esriFieldType.esriFieldTypeDouble:
                            case esriFieldType.esriFieldTypeOID:
                            case esriFieldType.esriFieldTypeGUID:
                            case esriFieldType.esriFieldTypeGlobalID:
                                fldval = row.Value[fldnum].ToString();
                                break;
                            case esriFieldType.esriFieldTypeDate:
                                fldval = Convert.ToDateTime(row.Value[fldnum]).ToLongTimeString();
                                break;
                            case esriFieldType.esriFieldTypeString:
                                fldval = "\"" + row.Value[fldnum].ToString() + "\"";
                                break;
                            default:
                                break;
                        }
                    }
                    props.Add(fldval);
                }
                var proparray = props.ToArray();
                string propsjoin = string.Join(",", proparray);


                return propsjoin;
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing attributes", ex);
            }
        }

        private static string GetHeaderRow(IFields flds)
        {
            string retval = "";
            try
            {
                List<string> fldnames = new List<string>();
                for (int fldnum = 0; fldnum < flds.FieldCount; fldnum++)
                {
                    fldnames.Add(flds.Field[fldnum].Name);
                }
                retval = string.Join(",", fldnames.ToArray());
            }
            catch { }
            return retval;
        }

        private static string GetHexWkb(IGeometry geom)
        {
            string retval = "";
            try
            {
                IWkb wkb = geom as IWkb;
                int byteCount = wkb.WkbSize;
                byte[] wkbBytes = new byte[byteCount];
                wkb.ExportToWkb(ref byteCount, out wkbBytes[0]);
                retval = ByteArrayToString(wkbBytes, byteCount);
            }
            catch { }
            return retval;
        }

        private static string ByteArrayToString(byte[] ba, int len)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
            //StringBuilder hex = new StringBuilder(len * 2);
            //for (int i = 0; i < len; i++)
            //{
            //    byte b = ba[i];
            //    hex.AppendFormat("{0:x2}", b);
            //}
            ////foreach (byte b in ba)
            ////    hex.AppendFormat("{0:x2}", b);
            //return hex.ToString();
        }
    }
}
