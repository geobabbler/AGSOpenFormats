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

using System.Collections.Specialized;

using System.Runtime.InteropServices;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Server;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SOESupport;
//using ESRI.ArcGIS.ADF.Connection.Local;
using Zekiah.GeoJSON;
using Zekiah.CSV;


//TODO: sign the project (project properties > signing tab > sign the assembly)
//      this is strongly suggested if the dll will be registered using regasm.exe <your>.dll /codebase


namespace GeoJSONSOE
{
    [ComVisible(true)]
    [Guid("afb59411-9172-4250-b024-673fd3308d46")]
    [ClassInterface(ClassInterfaceType.None)]
    [ServerObjectExtension("MapServer",
        AllCapabilities = "",
        DefaultCapabilities = "",
        Description = "Open data output support",
        DisplayName = "Open Data Formats",
        Properties = "",
        SupportsREST = true,
        SupportsSOAP = false)]
    public class GeoJSONServer : IServerObjectExtension, IObjectConstruct, IRESTRequestHandler
    {
        private string soe_name;

        private IPropertySet configProps;
        private IServerObjectHelper serverObjectHelper;
        private ServerLogger logger;
        private IRESTRequestHandler reqHandler;

        public GeoJSONServer()
        {
            soe_name = this.GetType().Name;
            logger = new ServerLogger();
            reqHandler = new SoeRestImpl(soe_name, CreateRestSchema()) as IRESTRequestHandler;
        }

        #region IServerObjectExtension Members

        public void Init(IServerObjectHelper pSOH)
        {
            serverObjectHelper = pSOH;
        }

        public void Shutdown()
        {
        }

        #endregion

        #region IObjectConstruct Members

        public void Construct(IPropertySet props)
        {
            configProps = props;
        }

        #endregion

        #region IRESTRequestHandler Members

        public string GetSchema()
        {
            return reqHandler.GetSchema();
        }

        public byte[] HandleRESTRequest(string Capabilities, string resourceName, string operationName, string operationInput, string outputFormat, string requestProperties, out string responseProperties)
        {
            return reqHandler.HandleRESTRequest(Capabilities, resourceName, operationName, operationInput, outputFormat, requestProperties, out responseProperties);
        }

        #endregion

        private RestResource CreateRestSchema()
        {
            RestResource rootRes = new RestResource(soe_name, false, RootResHandler);

            RestOperation geoJsonOper = new RestOperation("GeoJSON",
                                                      new string[] { "query", "layer" },
                                                      new string[] { "geojson" },
                                                      ExportGeoJsonHandler);

            RestOperation csvOper = new RestOperation("CSV",
                                                      new string[] { "query", "layer", "headers"},
                                                      new string[] { "csv" },
                                                      ExportCsvHandler);

            rootRes.operations.Add(geoJsonOper);
            rootRes.operations.Add(csvOper);

            return rootRes;
        }

        private byte[] RootResHandler(NameValueCollection boundVariables, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = null;

            JsonObject result = new JsonObject();

            return Encoding.UTF8.GetBytes("GeoJSON Export");
        }

        private byte[] ExportCsvHandler(NameValueCollection boundVariables,
                                          JsonObject operationInput,
                                              string outputFormat,
                                              string requestProperties,
                                          out string responseProperties)
        {
            string retval = "";
            StringBuilder sb = new StringBuilder();
            string s = "";
            bool applyQuery = true;
            bool? applyHeader = true;
            //bool? applyGeoms = true;
            bool addHeader = false;
            //bool addGeoms = false;
            responseProperties = "{\"Content-Type\" : \"text/csv\"}";

            string whereClause = "";
            bool found = operationInput.TryGetString("query", out whereClause);
            if (!found || string.IsNullOrEmpty(whereClause))
            {
                //then no definition query
                applyQuery = false;
            }

            long? layerOrdinal;
            found = operationInput.TryGetAsLong("layer", out layerOrdinal); //.TryGetString("layer", out parm2Value);
            if (!found)
            {
                throw new ArgumentNullException("layer");
            }

            bool useHeader = operationInput.TryGetAsBoolean("headers", out applyHeader);
            if (useHeader)
            {
                if ((bool)applyHeader)
                {
                    addHeader = true;
                }
            }

            //bool useGeoms = operationInput.TryGetAsBoolean("addgeoms", out applyGeoms);
            //if (useGeoms)
            //{
            //    if ((bool)applyGeoms)
            //    {
            //        addGeoms = true;
            //    }
            //}

            ESRI.ArcGIS.Carto.IMapServer mapServer = (ESRI.ArcGIS.Carto.IMapServer)serverObjectHelper.ServerObject;
            ESRI.ArcGIS.Carto.IMapServerDataAccess mapServerObjects = (ESRI.ArcGIS.Carto.IMapServerDataAccess)mapServer;
            var lyr = mapServerObjects.GetDataSource(mapServer.DefaultMapName, Convert.ToInt32(layerOrdinal));
            if (lyr is IFeatureClass)
            {
                IFeatureClass fclass = (IFeatureClass)lyr;
                IQueryFilter filter = new QueryFilterClass();
                filter.set_OutputSpatialReference(fclass.ShapeFieldName, getWGS84());
                if (applyQuery)
                {
                    filter.WhereClause = whereClause;
                }
                IFeatureCursor curs = fclass.Search(filter, false);
                try
                {
                    //();
                    s = curs.ToCSV(addHeader);
                    Marshal.ReleaseComObject(curs);

                }
                catch (Exception ex)
                {
                    s = ex.GetBaseException().ToString(); //.StackTrace;
                }
                retval = s;
                sb.Append(retval);
            }
            else
            {
                throw new Exception("Layer " + layerOrdinal.ToString() + " is not a feature layer.");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private byte[] ExportGeoJsonHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = "{\"Content-Type\" : \"application/json\"}"; ;
            bool applyQuery = true;
            string retval = "";
            string whereClause;
            bool found = operationInput.TryGetString("query", out whereClause);
            if (!found || string.IsNullOrEmpty(whereClause))
            {
                //then no definition query
                applyQuery = false;
            }

            long? layerOrdinal;
            found = operationInput.TryGetAsLong("layer", out layerOrdinal); //.TryGetString("layer", out parm2Value);
            if (!found)
            {
                throw new ArgumentNullException("layer");
            }

            string s = "";
            ESRI.ArcGIS.Carto.IMapServer mapServer = (ESRI.ArcGIS.Carto.IMapServer)serverObjectHelper.ServerObject;
            ESRI.ArcGIS.Carto.IMapServerDataAccess mapServerObjects = (ESRI.ArcGIS.Carto.IMapServerDataAccess)mapServer;
            var lyr = mapServerObjects.GetDataSource(mapServer.DefaultMapName, Convert.ToInt32(layerOrdinal));

            if (lyr is IFeatureClass)
            {
                IFeatureClass fclass = (IFeatureClass)lyr;
                retval = "{\"shape\": \"" + fclass.ShapeFieldName + "\"}";
                IQueryFilter filter = new QueryFilterClass();
                filter.set_OutputSpatialReference(fclass.ShapeFieldName, getWGS84());
                if (applyQuery)
                {
                    filter.WhereClause = whereClause;
                }
                IFeatureCursor curs = fclass.Search(filter, false);
                //apply extension methods here
                try
                {
                    s = curs.ToGeoJson();
                    Marshal.ReleaseComObject(curs);

                }
                catch (Exception ex)
                {
                    s = ex.GetBaseException().ToString(); //.StackTrace;
                }
                retval = s;
            }
            else
            {
                throw new Exception("Layer " + layerOrdinal.ToString() + " is not a feature layer.");
            }
            return Encoding.UTF8.GetBytes(retval);
        }

        private ESRI.ArcGIS.Geometry.ISpatialReference getWGS84()
        {
            ISpatialReferenceFactory oSpatialReferenceFactory;
            try
            {
                oSpatialReferenceFactory = new SpatialReferenceEnvironment();
                return oSpatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
            }
            catch //(Exception ex) 
            {
                return GetSpatialReference(esriSRGeoCSType.esriSRGeoCS_WGS1984);
            }
        }

        private ISpatialReference GetSpatialReference(ESRI.ArcGIS.Geometry.esriSRGeoCSType FactoryCode)
        {
            ISpatialReferenceFactory oSpatialReferenceFactory;
            IGeographicCoordinateSystem oGCS;
            try
            {
                oSpatialReferenceFactory = new SpatialReferenceEnvironment();
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
