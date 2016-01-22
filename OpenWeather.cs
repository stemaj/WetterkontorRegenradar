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
            const string urlDresden = "http://api.openweathermap.org/data/2.5/weather?id=2935022&appid=2de143494c0b295cca9337e1e96b00e0";

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
