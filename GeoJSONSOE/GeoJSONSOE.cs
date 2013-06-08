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
            soe_name = "Open Data Formats"; // this.GetType().Name;
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

            RestOperation sampleOper = new RestOperation("GeoJSON",
                                                      new string[] { "query","layer" },
                                                      new string[] { "json" },
                                                      ExportGeoJsonHandler);

            rootRes.operations.Add(sampleOper);

            return rootRes;
        }

        private byte[] RootResHandler(NameValueCollection boundVariables, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = null;

            JsonObject result = new JsonObject();
 
            return Encoding.UTF8.GetBytes("GeoJSON Export");
        }

        private byte[] ExportGeoJsonHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;
            bool applyQuery = true;
            string retval = "{\"test\": \"result\"}";
            string parm1Value;
            bool found = operationInput.TryGetString("query", out parm1Value);
            if (!found || string.IsNullOrEmpty(parm1Value))
            {
                //throw new ArgumentNullException("parm1");
                applyQuery = false;
            }

            string parm2Value;
            long? parm2num;
            found = operationInput.TryGetAsLong("layer", out parm2num); //.TryGetString("layer", out parm2Value);
            if (!found)
            {
                throw new ArgumentNullException("layer");
            }
            //else if (!long.TryParse(parm2Value, out parm2num))
            //{
            //    throw new Exception("layer must be an integer");
            //}

            JsonObject result = new JsonObject();
            string s = "";
            //result.AddString("parm1", parm1Value);
            //result.AddString("parm2", parm2Value);
            ESRI.ArcGIS.Carto.IMapServer mapServer = (ESRI.ArcGIS.Carto.IMapServer)serverObjectHelper.ServerObject;
            ESRI.ArcGIS.Carto.IMapServerDataAccess mapServerObjects = (ESRI.ArcGIS.Carto.IMapServerDataAccess)mapServer;
            //ESRI.ArcGIS.Carto.IMap map = mapServerObjects.get_Map(mapServer.DefaultMapName);
            var lyr = mapServerObjects.GetDataSource(mapServer.DefaultMapName, Convert.ToInt32(parm2num));
            //ILayer lyr = map.Layer[parm2num];
            if (lyr is IFeatureClass)
            {
                IFeatureClass fclass = (IFeatureClass)lyr;
                retval = "{\"shape\": \"" + fclass.ShapeFieldName + "\"}";
                IQueryFilter filter = new QueryFilterClass();
                filter.set_OutputSpatialReference(fclass.ShapeFieldName, getWGS84());
                if (applyQuery)
                {
                    filter.WhereClause = parm1Value;
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
                //IFeature feat = curs.NextFeature();
                //var s = feat.Value[2].ToString();
                retval = s;
            }
            else
            {
                throw new Exception("Layer " + parm2num.ToString() + " is not a feature layer.");
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
