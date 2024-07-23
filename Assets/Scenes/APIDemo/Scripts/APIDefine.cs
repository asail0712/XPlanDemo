using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

using UnityEngine;

using XPlan.Net;

namespace XPlan.Demo.APIDemo
{
    public class APIDefine
    {
        public const string WeatherLicense      = "CWA-776A4925-8FAC-4A0B-9D3D-33B4E961BC75";
        public const string WeatherUrl          = "https://opendata.cwa.gov.tw/api";
        public const string TemperatureAPI      = "/v1/rest/datastore/F-D0047-065";
        public const string UVraysAPI           = "/v1/rest/datastore/O-A0005-001";
        public const string KaohsiungSection    = "前金區";
    }

    public class TemperatureInfo
    {
        public string value;
        public string measures;
    }

    public class TimeInfo
    {
        public string dataTime;
        public TemperatureInfo[] elementValue;
    }

    public class TemperatureElement
    {
        public object elementName;
        public object description;
        public TimeInfo[] time;
    }

    public class LocationTemperature
    {
        public object locationName;
        public object geocode;
        public object lat;
        public object lon;
        public TemperatureElement[] weatherElement;
    }

    public class LocationInfo
    {
        public object datasetDescription;
        public object locationsName;
        public object dataid;
        public LocationTemperature[] location;
    }

    public class TemperatureRecords
    {
        public LocationInfo[] locations;
    }

    public class TemperatureResponse
    {
        public object success;
        public object result;
        public TemperatureRecords records;
    }

}