# AGSOpenFormats

A Server Object Extension to add open data format output to ArcGIS Server 10.1 and 10.2

## Example URLs

Full dataset: http://localhost/arcgis/rest/services/SampleWorldCities/MapServer/exts/GeoJSONServer/GeoJSON?layer=0&f=geojson

Attribute filter: http://localhost/arcgis/rest/services/SampleWorldCities/MapServer/exts/GeoJSONServer/GeoJSON?query=CITY_NAME%3D%27Baltimore%27&layer=0&f=geojson

The above URL will query layer 0 of the SampleWorldCities map service for the city name "Baltimore" and return a GeoJSON feature collection containing this record.

Bounding box filter: http://localhost/arcgis/rest/services/SampleWorldCities/MapServer/exts/GeoJSONServer/GeoJSON?layer=0&bbox=-79,35,-75,40&f=geojson

The above URL will query layer 0 of the SampleWorldCities map service for features intersecting the defined bounding box and return a GeoJSON feature collection.

## Updates

### 5 September 2013

Bounding box filters - You can supply a bounding box parameter to filter results (GeoJSON only). This parameter generally follows the convention established by the ArcGIS Server REST API [Export Map](http://resources.arcgis.com/en/help/rest/apiref/export.html) function, with a key difference. Like that function, you will supply a parameter called "bbox" with a set of comma-separated values xmin,ymin,xmax,ymax. The key difference is that these values must currently be decimal degrees and the extension will assume WGS84 for the spatial reference.

## License
The MIT License

Copyright (c) 2013 Zekiah Technologies, Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
