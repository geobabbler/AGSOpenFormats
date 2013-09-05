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
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Server;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;

namespace GeoJSONSOE
{
    public class Helpers
    {
        public ESRI.ArcGIS.Geometry.ISpatialReference getWGS84()
        {
            ISpatialReferenceFactory oSpatialReferenceFactory;
            try
            {
                Type t = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
                oSpatialReferenceFactory = (ISpatialReferenceFactory)Activator.CreateInstance(t);
                return oSpatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
            }
            catch //(Exception ex) 
            {
                return GetSpatialReference(esriSRGeoCSType.esriSRGeoCS_WGS1984);
            }
        } 

        public IGeometry TransformShapeCS(IGeometry pGeometry, ISpatialReference pInputSR, ISpatialReference pOutputSR)
        {
            IGeometry oGeometry;
            try
            {
                oGeometry = pGeometry;
                oGeometry.SpatialReference = pInputSR;
                oGeometry.Project(pOutputSR);
                return oGeometry;
            }
            catch
            {
                return pGeometry;
            }
        } 

        public ISpatialReference GetSpatialReference(ESRI.ArcGIS.Geometry.esriSRGeoCSType FactoryCode)
        {
            ISpatialReferenceFactory oSpatialReferenceFactory;
            IGeographicCoordinateSystem oGCS;
            try
            {
                Type t = Type.GetTypeFromProgID("esriGeometry.SpatialReferenceEnvironment");
                oSpatialReferenceFactory = (ISpatialReferenceFactory)Activator.CreateInstance(t);
                oGCS = oSpatialReferenceFactory.CreateGeographicCoordinateSystem((int)FactoryCode);
                return oGCS;
            }
            catch
            {
                return null;
            }
        } 
    }
}
