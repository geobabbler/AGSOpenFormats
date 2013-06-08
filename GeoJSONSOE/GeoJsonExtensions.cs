//The MIT License

//Copyright (c) 2012 Zekiah Technologies, Inc.

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
namespace Zekiah.GeoJSON
{
    public static class GeoJsonExtensions
    {
        public static string ToGeoJson(this IFeatureCursor rows)
        {
            try
            {
                string retval = "{ \"type\": \"FeatureCollection\", ";
                retval += "\"features\": [";
                List<string> feats = new List<string>();
                IFeature row = rows.NextFeature();
                while (row != null)
                {
                    feats.Add(row.ToGeoJson());
                    row = rows.NextFeature();
                }
                var featarray = feats.ToArray();
                var featjoin = string.Join(",", featarray);
                retval += featjoin;
                retval += "]}";
                return retval;
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing row collection", ex);
            }
        }

        public static string ToGeoJson(this IFeature row)
        {
            try
            {
                StringBuilder sb = new StringBuilder("{ \"type\": \"Feature\", ");
                var geom = row.Shape;
                sb.Append("\"geometry\": ");
                sb.Append(geom.ToGeoJson());
                sb.Append(", ");
                sb.Append(processFields(row));
                sb.Append("}");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing row", ex);
            }
        }

        private static string processFields(IFeature row)
        {
            try
            {
                StringBuilder sb = new StringBuilder("\"properties\": {");
                List<string> props = new List<string>();
                string proptemplate = "\"{0}\": \"{1}\"";
                for (int fldnum = 0; fldnum < row.Fields.FieldCount; fldnum++)
                {
                    string fldname = row.Fields.Field[fldnum].Name;
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
                                fldval = "geometry";
                                break;
                            case esriFieldType.esriFieldTypeBlob:
                                fldval = "blob";
                                break;
                            case esriFieldType.esriFieldTypeSmallInteger:
                                fldval = row.Value[fldnum].ToString();
                                break;
                            case esriFieldType.esriFieldTypeInteger:
                                fldval = row.Value[fldnum].ToString();
                                break;
                            case esriFieldType.esriFieldTypeSingle:
                                fldval = row.Value[fldnum].ToString();
                                break;
                            case esriFieldType.esriFieldTypeDouble:
                                fldval = row.Value[fldnum].ToString();
                                break;
                            case esriFieldType.esriFieldTypeString:
                                fldval = row.Value[fldnum].ToString();
                                break;
                            case esriFieldType.esriFieldTypeDate:
                                fldval = Convert.ToDateTime(row.Value[fldnum]).ToLongTimeString();
                                break;
                            case esriFieldType.esriFieldTypeOID:
                                fldval = row.Value[fldnum].ToString();
                                break;
                            case esriFieldType.esriFieldTypeGUID:
                                fldval = row.Value[fldnum].ToString();
                                break;
                            case esriFieldType.esriFieldTypeGlobalID:
                                fldval = row.Value[fldnum].ToString();
                                break;
                            default:
                                break;
                        }
                    }
                    string propval = string.Format(proptemplate, fldname, fldval);
                    props.Add(propval);
                }
                var proparray = props.ToArray();
                string propsjoin = string.Join(",", proparray);
                sb.Append(propsjoin);
                sb.Append("}");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing attributes", ex);
            }
        }

        public static string ToGeoJson(this IGeometry geometry)
        {
            try
            {
                string retval = "";

                var shapeType = geometry.GeometryType;
                //note: "M" values are not really supported. Relevant geometries are handled to extract Z values, if present.
                switch (shapeType)
                {
                    case esriGeometryType.esriGeometryMultipoint:
                        IMultipoint mptbuff = (IMultipoint)geometry;
                        retval = processMultiPointBuffer(mptbuff);
                        break;
                    case esriGeometryType.esriGeometryPoint:
                        IPoint pt = (IPoint)geometry;
                        retval = processPointBuffer(pt);
                        break;
                    case esriGeometryType.esriGeometryPolyline:
                        IPolyline lbuff = (IPolyline)geometry;
                        retval = processMultiPartBuffer((IGeometryCollection)lbuff, "MultiLineString");
                        break;
                    case esriGeometryType.esriGeometryPolygon:
                        IPolygon pbuff = (IPolygon)geometry;
                        retval = processMultiPartBuffer((IGeometryCollection)pbuff, "MultiPolygon");
                        break;
                }
                return retval;
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing geometry", ex);
            }
        }

        private static string processPointBuffer(IPoint buffer)
        {
            try
            {
                StringBuilder retval = new StringBuilder("{\"type\":\"Point\", \"coordinates\": ");
                bool hasZ = false;
                string coord = hasZ ? getCoordinate(buffer.X, buffer.Y, buffer.Z) : getCoordinate(buffer.X, buffer.Y);
                retval.Append(coord);
                retval.Append("}");
                return retval.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing point buffer", ex);
            }
        }

        private static string processMultiPointBuffer(IMultipoint buffer)
        {
            try
            {
                StringBuilder retval = new StringBuilder("{\"type\":\"MultiPoint\", \"coordinates\": [");
                bool hasZ = false;
                IPointCollection points = (IPointCollection)buffer;

                List<string> coords = new List<string>();
                for (int i = 0; i < points.PointCount; i++)
                {
                    string coord = hasZ ? getCoordinate(points.Point[i].X, points.Point[i].Y, points.Point[i].Z) : getCoordinate(points.Point[i].X, points.Point[i].Y);
                    coords.Add(coord);
                }
                string[] coordArray = coords.ToArray();
                string coordList = string.Join(",", coordArray);
                retval.Append(coordList);
                retval.Append("]}");
                return retval.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing multipoint buffer", ex);
            }
        }

        private static string processMultiPartBuffer(IGeometryCollection buffer, string geoJsonType)
        {
            try
            {
                List<string> delims = getMultipartDelimiter(geoJsonType);
                bool hasZ = false;
                StringBuilder retval = new StringBuilder("{\"type\":\"" + geoJsonType + "\", \"coordinates\": [");

                int numPts = 0;
                int numParts = buffer.GeometryCount;
                List<string> coords = new List<string>();
                List<string> polys = new List<string>();
                for (int i = 0; i < numParts; i++)
                {
                    if (coords.Count > 0)
                    {
                        string[] coordArray = coords.ToArray();
                        string coordList = string.Join(",", coordArray);
                        polys.Add(delims[0] + coordList + delims[1]);
                    }
                    coords = new List<string>();
                    IGeometry part = buffer.Geometry[i];
                    IPointCollection pcoll = (IPointCollection)part;
                    numPts = pcoll.PointCount;
                    for (int j = 0; j < numPts; j++)
                    {
                        IPoint pt = pcoll.Point[j];
                        string coord = hasZ ? getCoordinate(pt.X, pt.Y, pt.Z) : getCoordinate(pt.X, pt.Y);
                        coords.Add(coord);
                    }
                    //if (geoJsonType == "MultiPolygon")
                    //{
                    //    IPoint pt = pcoll.Point[0];
                    //    string coord = hasZ ? getCoordinate(pt.X, pt.Y, pt.Z) : getCoordinate(pt.X, pt.Y);
                    //    coords.Add(coord);
                    //}
                }
                if (coords.Count > 0)
                {
                    string[] coordArray = coords.ToArray();
                    string coordList = string.Join(",", coordArray);
                    polys.Add(delims[0] + coordList + delims[1]);
                }
                string[] polyArray = polys.ToArray();
                string polyList = string.Join(",", polyArray);
                retval.Append(polyList);
                retval.Append("]}");
                return retval.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing multipart buffer", ex);
            }
        }

        private static List<string> getMultipartDelimiter(string geoJsonType)
        {
            try
            {
                List<string> retval = new List<string>();

                switch (geoJsonType.ToLower())
                {
                    case "multipoint":
                        retval.Add("");
                        retval.Add("");
                        break;
                    case "multilinestring":
                        retval.Add("[");
                        retval.Add("]");
                        break;
                    case "multipolygon":
                        retval.Add("[[");
                        retval.Add("]]");
                        break;
                }

                return retval;
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating delimiter", ex);
            }
        }

        private static string getCoordinate(double x, double y)
        {
            try
            {
                string retval = string.Format(CultureInfo.InvariantCulture, "[{0}, {1}]", x, y);
                return retval;
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating coordinate", ex);
            }
        }

        private static string getCoordinate(double x, double y, double z)
        {
            try
            {
                string retval = string.Format(CultureInfo.InvariantCulture, "[{0}, {1}, {2}]", x, y, z);
                return retval;
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating coordinate", ex);
            }
        }
    }
}
