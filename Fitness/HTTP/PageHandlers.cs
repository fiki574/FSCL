using System.Collections.Generic;
using System.Net;
using System.IO;
using System;
using Fitness.Database;

namespace Fitness
{
    public partial class HttpServer
    {
        [HttpHandler("/pregled")]
        private static string HandlePregled(HttpServer server, HttpListenerRequest request, Dictionary<string, string> parameters)
        {
            string text = File.ReadAllText("../../pregled.html");
            text = text.Replace("@1", DateTime.Now.ToString());
            text = text.Replace("@Z", Form1.Instance.GetTotalMembers().ToString());
            text = text.Replace("@X", Form1.Instance.GetActiveMembers().ToString());
            text = text.Replace("@2", Form1.Instance.GetCurrentMembersCount().ToString());
            text = text.Replace("@3", Form1.Instance.GetTodaysPayments().ToString());
            text = text.Replace("@4", Form1.Instance.GetMonthsPayments().ToString());
            text = text.Replace("@5", Form1.Instance.GetNumberOfVisitsToday().ToString());
            text = text.Replace("@6", Form1.Instance.GetNumberOfVisitsThisMonth().ToString());
            text = text.Replace("@T", Form1.Instance.GetNewUsersToday().ToString());
            text = text.Replace("@U", Form1.Instance.GetNewUsersThisMonth().ToString());
            text = request.IsLocal ? text.Replace("@V", Utilities.GetLocalIP()) : text.Replace("@V", Utilities.GetPublicIP());
            
            #region Prosječni dolasci

            Dictionary<string, int> 
                MonthVisitsLastOneMonth = Form1.Instance.GetVisitsPerDay(1),
                MonthVisitsLastThreeMonths = Form1.Instance.GetVisitsPerDay(3),
                MonthVisitsLastSixMonths = Form1.Instance.GetVisitsPerDay(6);
            text = text.Replace("@7", MonthVisitsLastOneMonth["PON"].ToString());
            text = text.Replace("@8", MonthVisitsLastOneMonth["UTO"].ToString());
            text = text.Replace("@9", MonthVisitsLastOneMonth["SRI"].ToString());
            text = text.Replace("@A", MonthVisitsLastOneMonth["CET"].ToString());
            text = text.Replace("@B", MonthVisitsLastOneMonth["PET"].ToString());
            text = text.Replace("@C", MonthVisitsLastOneMonth["SUB"].ToString());
            text = text.Replace("@D", MonthVisitsLastOneMonth["NED"].ToString());
            text = text.Replace("@E", MonthVisitsLastThreeMonths["PON"].ToString());
            text = text.Replace("@F", MonthVisitsLastThreeMonths["UTO"].ToString());
            text = text.Replace("@G", MonthVisitsLastThreeMonths["SRI"].ToString());
            text = text.Replace("@H", MonthVisitsLastThreeMonths["CET"].ToString());
            text = text.Replace("@I", MonthVisitsLastThreeMonths["PET"].ToString());
            text = text.Replace("@J", MonthVisitsLastThreeMonths["SUB"].ToString());
            text = text.Replace("@K", MonthVisitsLastThreeMonths["NED"].ToString());
            text = text.Replace("@L", MonthVisitsLastSixMonths["PON"].ToString());
            text = text.Replace("@M", MonthVisitsLastSixMonths["UTO"].ToString());
            text = text.Replace("@N", MonthVisitsLastSixMonths["SRI"].ToString());
            text = text.Replace("@O", MonthVisitsLastSixMonths["CET"].ToString());
            text = text.Replace("@P", MonthVisitsLastSixMonths["PET"].ToString());
            text = text.Replace("@R", MonthVisitsLastSixMonths["SUB"].ToString());
            text = text.Replace("@S", MonthVisitsLastSixMonths["NED"].ToString());

            #endregion

            return text;
        }

        [HttpHandler("/korisnik")]
        private static string HandleKorisnik(HttpServer server, HttpListenerRequest request, Dictionary<string, string> parameters)
        {
            byte type = 0;
            string result = "error";

            if (!parameters.ContainsKey("id") && !parameters.ContainsKey("ime") && !parameters.ContainsKey("prez"))
                return "Nedostaje parametar!";
            else if (parameters.ContainsKey("id") && !parameters.ContainsKey("ime") && !parameters.ContainsKey("prez"))
                type = 1;
            else if (!parameters.ContainsKey("id") && parameters.ContainsKey("ime") && !parameters.ContainsKey("prez"))
                type = 2;
            else if (!parameters.ContainsKey("id") && !parameters.ContainsKey("ime") && parameters.ContainsKey("prez"))
                type = 3;
            else
                return result;

            try
            {
                if (type > 0)
                {
                    if (type == 1 && !string.IsNullOrWhiteSpace(parameters["id"]) && !string.IsNullOrEmpty(parameters["id"]))
                    {
                        Korisnik kor = FitnessDB.Korisnici.SingleOrDefault(k => k.BrojIskaznice == Convert.ToInt32(parameters["id"]));
                        result = $"Indeks u bazi: {kor.Index}<br>Broj kartice: {kor.BrojIskaznice}<br>Ime: {kor.Ime}<br>Prezime: {kor.Prezime}<br>Usluga: {kor.AktivnaUsluga}";
                    }
                    else
                    {
                        List<Korisnik> kors = new List<Korisnik>();
                        if (type == 2 && !string.IsNullOrWhiteSpace(parameters["ime"]) && !string.IsNullOrEmpty(parameters["ime"]))
                            kors = FitnessDB.Korisnici.Select(k => k.Ime.ToLower().Contains(parameters["ime"].ToLower()));
                        else if (type == 3 && !string.IsNullOrWhiteSpace(parameters["prez"]) && !string.IsNullOrEmpty(parameters["prez"]))
                            kors = FitnessDB.Korisnici.Select(k => k.Prezime.ToLower().Contains(parameters["prez"].ToLower()));

                        if (kors.Count > 0)
                        {
                            result = "";
                            int count = 1;
                            foreach(Korisnik k in kors)
                                result += $"<b>{count++}. pronađeni korisnik</b>:<br>Indeks u bazi: {k.Index}<br>Broj kartice: {k.BrojIskaznice}<br>Ime: {k.Ime}<br>Prezime: {k.Prezime}<br>Usluga: {k.AktivnaUsluga}<br><br>";
                        }
                        else
                            result = "Korisnik ne postoji!";
                    }
                }
            }
            catch
            {
            }
            return result;
        }
    }
}