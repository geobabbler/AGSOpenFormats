AGSOpenFormats
==============

A Server Object Extension to add open data format output to ArcGIS Server 10.1

Example URL: 
http://localhost/arcgis/rest/services/SampleWorldCities/MapServer/exts/GeoJSONServer/GeoJSON?query=CITY_NAME%3D%27Baltimore%27&layer=0&f=json

This URL will query layer 0 of the SampleWorldCities map service for the city name "Baltimore" and return a GeoJSON feature collection containing this record.

Updates
==============
5 September 2013

Bounding box filters - You can supply a bounding box parameter to filter results (GeoJSON only). This parameter generally follows the convention established by the ArcGIS Server REST API [Export Map](http://resources.arcgis.com/en/help/rest/apiref/export.html) function, with a key difference. Like that function, you will supply a parameter called "bbox" with a set of comma-separated values <xmin>,<ymin>,<xmax>,<ymax>. The key difference is that these values must currently be decimal degrees and the extension will assume WGS84 for the spatial reference.
