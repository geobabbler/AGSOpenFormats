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
