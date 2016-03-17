using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Helpers;

namespace WetterkontorRegenradar
{
    class OpenWeather
    {
        public DynamicJsonObject GetWeather()
        {
            var str = JsonRequest();

            var stuff = Json.Decode(str);

            return stuff;
        }

        public string JsonRequest()
        {
            const string myApi = "6476e4aa720282dac00814a569b2019f";

            const string urlDresden = "http://api.openweathermap.org/data/2.5/weather?id=2935020&appid=" + myApi;

            try
            {
                var httpWebRequest = (HttpWebRequest) WebRequest.Create(urlDresden);
                var httpWebReponse = (HttpWebResponse) httpWebRequest.GetResponse();
                var stream = httpWebReponse.GetResponseStream();

                if (stream != null)
                {
                    var sr = new StreamReader(stream);
                    return sr.ReadToEnd();
                }
            }
            catch (WebException)
            {
                
            }

            return string.Empty;
        }
    }
}
