﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PluginShared
{
    public class Utils
    {
        public struct ResultInfo
        {
            public string EventName;
            public string MSG;
            public string AIJSON;
            public ResultInfo(string eventName, string msg = "", string aijson = "")
            {
                EventName = eventName;
                MSG = msg;
                AIJSON = aijson;
            }
        }

        public static Exception LastException { get; set; }

        public static bool TaskRunning(Task t)
        {
            if (t == null)
                return false;
            try
            {
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                    case TaskStatus.Faulted:
                    case TaskStatus.Canceled:
                        return false;

                }
                return true;
            }
            catch
            {
                return true;
            }
        }

        public static dynamic PopulateResponse(string resp, object o)
        {
            dynamic d = JsonConvert.DeserializeObject(resp);
            foreach (var sec in d.sections)
            {
                if (sec.items != null)
                {
                    foreach (var item in sec.items)
                    {
                        var bt = item["bindto"];
                        if (bt != null && o != null)
                        {
                            string[] prop = bt.ToString().Split(',');
                            if (prop.Length == 1)
                            {
                                try
                                {
                                    if (item["type"] == "MultiSelect")
                                        item["value"] = JToken.Parse(GetPropValue(o, bt.ToString()).ToString());
                                    else
                                    {
                                        item["value"] = GetPropValue(o, bt.ToString());
                                    }
                                    var nv = item["nvident"];
                                    if (nv != null)
                                    {
                                        item["value"] = NV(item["value"].ToString(), nv.ToString());
                                        if (item["value"] != "")
                                        {
                                            if (item["type"] == "Boolean")
                                                item["value"] = Convert.ToBoolean(item["value"]);
                                            if (item["type"] == "Int32")
                                                item["value"] = Convert.ToInt32(item["value"]);
                                            if (item["type"] == "Decimal" || item["type"] == "Single")
                                                item["value"] = Convert.ToDecimal(item["value"]);
                                            if (item["type"] == "Select")
                                                item["value"] = Convert.ToString(item["value"]);
                                        }
                                    }
                                    var conv = item["converter"];
                                    if (conv != null)
                                    {
                                        switch ((string)conv)
                                        {
                                            case "daysofweek":
                                                string[] days = item["value"].ToString().Trim(',').Split(',');
                                                int i = 0;
                                                foreach (var opt in item.options)
                                                {
                                                    if (days.Contains(i.ToString(CultureInfo.InvariantCulture)))
                                                    {
                                                        opt["value"] = true;
                                                    }
                                                    i++;
                                                }
                                                break;
                                            case "datetimetoint":
                                                var dt = (DateTime)item["value"];
                                                item["value"] = dt.TimeOfDay.TotalMinutes;
                                                break;
                                            case "rgbtohex":
                                                var rgb = (string)item["value"].ToString();
                                                var rgbarr = rgb.Split(',');
                                                if (rgbarr.Length == 3)
                                                {
                                                    item["value"] = "#" + (Convert.ToInt16(rgbarr[0])).ToString("X2") +
                                                                    (Convert.ToInt16(rgbarr[1])).ToString("X2") +
                                                                    (Convert.ToInt16(rgbarr[2])).ToString("X2");
                                                }

                                                break;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LastException = ex;
                                    item["value"] = "";
                                }
                            }
                            else
                            {
                                string json = prop.Aggregate("[", (current, s) => current + (GetPropValue(o, s) + ","));
                                json = json.Trim(',');
                                json += "]";
                                item["value"] = JToken.Parse(json);
                            }
                        }
                    }
                }
            }
            return d;
        }

        private static string NV(string source, string name)
        {
            if (string.IsNullOrEmpty(source))
                return "";
            name = name.ToLower().Trim();
            string[] settings = source.Split(',');
            foreach (string[] nv in settings.Select(s => s.Split('=')).Where(nv => nv[0].ToLower().Trim() == name))
            {
                return nv[1];
            }
            return "";
        }

        private static object GetPropValue(object src, string propName)
        {
            object currentObject = src;
            string[] fieldNames = propName.Split('.');

            foreach (string fieldName in fieldNames)
            {
                // Get type of current record 
                Type curentRecordType = currentObject.GetType();
                PropertyInfo property = curentRecordType.GetProperty(fieldName);

                if (property != null)
                {
                    currentObject = property.GetValue(currentObject, null);
                }
                else
                {
                    return null;
                }
            }
            return currentObject;
        }

        public static void PopulateObject(dynamic d, object o)
        {
            foreach (var sec in d.sections)
            {
                foreach (var item in sec.items)
                {
                    var bt = item["bindto"];
                    if (bt != null)
                    {
                        var val = item["value"];
                        if (val != null)
                        {
                            Populate(item, o);
                        }
                    }
                }
            }
        }

        static void Populate(dynamic item, object o)
        {
            var bt = item["bindto"];
            var val = item["value"];
            var conv = item["converter"];
            var nvident = item["nvident"];

            if (conv != null)
            {
                switch ((string)conv)
                {
                    case "daysofweek":
                        string dow = "";
                        int i = 0;
                        foreach (var opt in item.options)
                        {
                            if (opt.value == true)
                            {
                                dow += i.ToString(CultureInfo.InvariantCulture) + ",";
                            }
                            i++;
                        }
                        dow = dow.Trim(',');
                        val = dow;
                        break;
                    case "datetimetoint":
                        TimeSpan ts = TimeSpan.FromMinutes(Convert.ToInt64(val));
                        val = DateTime.MinValue.Add(ts);
                        break;
                    case "rgbtohex":
                        try
                        {
                            //convert back to rgb

                            var hex = (string)item["value"].ToString();
                            if (hex.StartsWith("#"))
                                hex = hex.Substring(1);

                            if (hex.Length != 6) throw new Exception("Color not valid");

                            val = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber)+","+
                                int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber) + "," +
                                int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

                        }
                        catch (Exception ex)
                        {
                            LastException = ex;
                        }
                        break;
                }
            }

            var props = bt.ToString().Split(',');
            if (props.Length > 1)
            {
                int i = 0;
                if (val.Type.ToString() == "String")
                {
                    val = val.ToString().Split(',');
                }
                foreach (string s in props)
                {
                    try
                    {
                        SetPropValue(o, s, val[i]);
                    }
                    catch (Exception ex)
                    {
                        LastException = ex;
                    }
                    i++;
                }
            }
            else
            {
                if (nvident != null)
                {
                    var nv = nvident.ToString();
                    var nvstring = GetPropValue(o, props[0]).ToString();
                    val = NVSet(nvstring, nv, val.ToString());
                }
                try
                {
                    SetPropValue(o, props[0], val);
                }
                catch (Exception ex)
                {
                    LastException = ex;
                }
            }
        }

        public static Rectangle[] ImageZones(string zoneMap, Size imageSize)
        {
            if (zoneMap.Length > 0)
            {
                double wmulti = Convert.ToDouble(imageSize.Width) / Convert.ToDouble(100);
                double hmulti = Convert.ToDouble(imageSize.Height) / Convert.ToDouble(100);

                var l = new List<Rectangle>();
                int x = 0, y = 0;
                var p = 5d;
                int ylim = 48;

                double pcx = (p / 320d) * 100d;
                double pcy = (p / 240d) * 100d;
                int rx = Convert.ToInt32(pcx * wmulti);
                int ry = Convert.ToInt32(pcy * hmulti);
                foreach (var c in zoneMap)
                {
                    if (c != '0')
                    {
                        l.Add(new Rectangle(Convert.ToInt32(x * pcx * wmulti), Convert.ToInt32(y * pcy * hmulti), rx, ry));
                    }
                    y++;
                    if (y == ylim)
                    {
                        x++;
                        y = 0;
                    }
                }
                return l.ToArray();
            }

            return new[] { new Rectangle(0, 0, imageSize.Width, imageSize.Height) };
        }

        //given a zone map, image size and point return zone at the location
        public static char GetZone(Point p, Size imageSize, string zoneMap)
        {
            if (imageSize.Width > 0 && imageSize.Height > 0)
            {
                var x = Convert.ToInt32(Math.Floor((Convert.ToDouble(p.X) / imageSize.Width) * 64d));
                var y = Convert.ToInt32(Math.Floor((Convert.ToDouble(p.Y) / imageSize.Height) * 48d));
                int ind = Convert.ToInt32(x * 48d + y);
                //convert p to index in zonemap
                if (ind <= zoneMap.Length)
                {
                    return zoneMap[ind];
                }

            }
            return '0';

        }

        static void SetPropValue(object src, string propName, object propValue)
        {
            object currentObject = src;
            string[] fieldNames = propName.Split('.');

            for (int i = 0; i < fieldNames.Length - 1; i++)
            {
                string fieldName = fieldNames[i];
                currentObject = currentObject.GetType().GetProperty(fieldName).GetValue(currentObject, null);
            }
            var val = currentObject.GetType().GetProperty(fieldNames[fieldNames.Length - 1]);
            if (val == null) return; //support example json with no bindings

            var t = val.PropertyType.Name;
            switch (t)
            {
                case "String":
                    val.SetValue(currentObject, propValue.ToString(), null);
                    break;
                case "Int32":
                    val.SetValue(currentObject, Convert.ToInt32(propValue, CultureInfo.InvariantCulture), null);
                    break;
                case "Decimal":
                    val.SetValue(currentObject, Convert.ToDecimal(propValue, CultureInfo.InvariantCulture), null);
                    break;
                case "Single":
                    val.SetValue(currentObject, Convert.ToSingle(propValue, CultureInfo.InvariantCulture), null);
                    break;
                case "Double":
                    val.SetValue(currentObject, Convert.ToDouble(propValue, CultureInfo.InvariantCulture), null);
                    break;
                case "Boolean":
                    val.SetValue(currentObject, Convert.ToBoolean(propValue, CultureInfo.InvariantCulture), null);
                    break;
                case "DateTime":
                    val.SetValue(currentObject, Convert.ToDateTime(propValue, CultureInfo.InvariantCulture), null);
                    break;
                default:
                    throw new Exception("missing conversion (" + t + ")");
            }

        }

        static string NVSet(string source, string name, string value)
        {
            if (source == null) source = "";

            name = name.ToLower().Trim();

            string[] settings = source.Split(',');
            bool isset = false;
            for (int i = 0; i < settings.Length; i++)
            {
                if (settings[i].ToLower().StartsWith(name + "="))
                {
                    settings[i] = name + "=" + value;
                    isset = true;
                    break;
                }
            }
            if (!isset)
            {
                var l = settings.ToList();
                l.Add(name + "=" + value);
                settings = l.ToArray();
            }
            return string.Join(",", settings);
        }


    }
}
