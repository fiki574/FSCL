using System.Collections.Generic;
using System.Net;
using System.IO;
using System;

namespace Fitness
{
    public partial class HttpServer
    {
        [HttpHandler("/pregled")]
        private static string HandlePregled(HttpServer server, HttpListenerRequest request, Dictionary<string, string> parameters)
        {
            string text = File.ReadAllText("pregled.html");
            text = text.Replace("@1", DateTime.Now.ToString());
            text = text.Replace("@2", Form1.Instance.GetCurrentMembersCount().ToString());
            text = text.Replace("@3", Form1.Instance.GetLast());
            text = text.Replace("@4", Form1.Instance.GetTodaysPayments().ToString());
            text = text.Replace("@5", Form1.Instance.GetNumberOfVisits().ToString());
            return text;
        }

        [HttpHandler("/korisnik")]
        private static string HandleKorisnik(HttpServer server, HttpListenerRequest request, Dictionary<string, string> parameters)
        {
            return "todo";
        }
    }
}