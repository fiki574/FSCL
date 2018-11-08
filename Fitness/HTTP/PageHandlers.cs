/*
    C# Framework with a lot of useful functions and classes
    Copyright (C) 2018/2019 Bruno Fištrek

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
            string text = "";
            try
            {
                if (!parameters.ContainsKey("api") || parameters["api"] != Constants.ApiKey)
                    return "API error";

                text = File.ReadAllText(Constants.PregledLocation);
                text = text.Replace("@Y", $"{Utilities.GetPublicIP()}:8181");
                text = text.Replace("@c", $"{Utilities.GetLocalIP()}:8181");
                text = text.Replace("@b", Constants.ApiKey);
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
                text = text.Replace("@a", Constants.ApiKey);

                #region Prosječni dolasci

                Dictionary<string, int> MonthVisitsLastOneMonth = Form1.Instance.GetVisitsPerDay(1), MonthVisitsLastThreeMonths = Form1.Instance.GetVisitsPerDay(3), MonthVisitsLastSixMonths = Form1.Instance.GetVisitsPerDay(6);
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
            }
            catch
            {
            }
            return text;
        }

        [HttpHandler("/korisnik")]
        private static string HandleKorisnik(HttpServer server, HttpListenerRequest request, Dictionary<string, string> parameters)
        {
            string result = "", text = "";
            try
            {
                if (!parameters.ContainsKey("api") || parameters["api"] != Constants.ApiKey)
                    return "API error";

                byte type = 0;
                if (!parameters.ContainsKey("id") && !parameters.ContainsKey("ime") && !parameters.ContainsKey("prez"))
                    return "Param error";
                else if (parameters.ContainsKey("id") && !parameters.ContainsKey("ime") && !parameters.ContainsKey("prez"))
                    type = 1;
                else if (!parameters.ContainsKey("id") && parameters.ContainsKey("ime") && !parameters.ContainsKey("prez"))
                    type = 2;
                else if (!parameters.ContainsKey("id") && !parameters.ContainsKey("ime") && parameters.ContainsKey("prez"))
                    type = 3;
                else
                    return "Search error";

                if (type > 0)
                {
                    if (type == 1 && !string.IsNullOrWhiteSpace(parameters["id"]) && !string.IsNullOrEmpty(parameters["id"]))
                    {
                        Korisnik kor = FitnessDB.Korisnici.SingleOrDefault(k => k.BrojIskaznice == Convert.ToInt32(parameters["id"]));
                        result = $"<div class=\"col-lg-12 panel panel-default\">\n" +
                                                $"<h5><b>Pronađeni korisnik</b></h5>\n" +
                                                $"<p>Ime i prezime: <span style=\"color:green;\">{kor.Ime + " " + kor.Prezime}</span></p>\n" +
                                                $"<p>Broj iskaznice: <span style=\"color:green;\">{kor.BrojIskaznice}</span></p>\n" +
                                                $"<p>Usluga: <span style=\"color:green;\">{kor.AktivnaUsluga}</span></p>\n" +
                                                $"<p>Datum rođenja: <span style=\"color:green;\">{(!string.IsNullOrWhiteSpace(kor.DatumRodenja) ? kor.DatumRodenja : "nepoznato")}</span></p>" +
                                                $"<p>Datum učlanjenja: <span style=\"color:green;\">{(!string.IsNullOrWhiteSpace(kor.DatumUclanjenja) ? kor.DatumUclanjenja : "nepoznato")}</span></p>" +
                                                $"<p>Zadnji dolazak: <span style=\"color:green;\">{kor.ZadnjiDolazak}</span></p>" +
                                                $"<p>Napomena: <br><span style=\"color:green;\">{(!string.IsNullOrWhiteSpace(kor.Napomena) ? kor.Napomena : "/")}</span></p>" +
                                          $"</div>\n\n";

                        result += "<a href =\"http://" + (request.IsLocal ? Utilities.GetLocalIP() : Utilities.GetPublicIP()) + ":8181/pregled&api=" + Constants.ApiKey + "\"><button type=\"button\" class=\"btn\">Povratak</button></a>";

                        text = File.ReadAllText(Constants.PrikazLocation);
                        text = text.Replace("@1", result);
                    }
                    else
                    {
                        List<Korisnik> kors = new List<Korisnik>();
                        if (type == 2 && !string.IsNullOrWhiteSpace(parameters["ime"]) && !string.IsNullOrEmpty(parameters["ime"]))
                            kors = FitnessDB.Korisnici.Select(k => k.Ime.ToLower().Contains(parameters["ime"].ToLowerInvariant()));
                        else if (type == 3 && !string.IsNullOrWhiteSpace(parameters["prez"]) && !string.IsNullOrEmpty(parameters["prez"]))
                            kors = FitnessDB.Korisnici.Select(k => k.Prezime.ToLower().Contains(parameters["prez"].ToLowerInvariant()));

                        if (kors.Count > 0)
                        {
                            result = "";
                            int count = 1;
                            foreach (Korisnik k in kors)
                                result += $"<div class=\"col-lg-12 panel panel-default\">\n" +
                                                $"<h5><b>{count++}. korisnik</b></h5>\n" +
                                                $"<p>Ime i prezime: <span style=\"color:green;\">{k.Ime + " " + k.Prezime}</span></p>\n" +
                                                $"<p>Broj iskaznice: <span style=\"color:green;\">{k.BrojIskaznice}</span></p>\n" + 
                                                $"<p>Usluga: <span style=\"color:green;\">{k.AktivnaUsluga}</span></p>\n" +
                                                $"<p>Datum rođenja: <span style=\"color:green;\">{(!string.IsNullOrWhiteSpace(k.DatumRodenja) ? k.DatumRodenja : "nepoznato")}</span></p>" +
                                                $"<p>Datum učlanjenja: <span style=\"color:green;\">{(!string.IsNullOrWhiteSpace(k.DatumUclanjenja) ? k.DatumUclanjenja : "nepoznato")}</span></p>" +
                                                $"<p>Zadnji dolazak: <span style=\"color:green;\">{k.ZadnjiDolazak}</span></p>" +
                                                $"<p>Napomena: <br><span style=\"color:green;\">{(!string.IsNullOrWhiteSpace(k.Napomena) ? k.Napomena : "/")}</span></p>" +
                                          $"</div>\n\n";

                            result += "<a href =\"http://" + (request.IsLocal ? Utilities.GetLocalIP() : Utilities.GetPublicIP()) + ":8181/pregled&api=" + Constants.ApiKey + "\"><button type=\"button\" class=\"btn\">Povratak</button></a>";
                            text = File.ReadAllText(Constants.PrikazLocation);
                            text = text.Replace("@1", result);
                        }                
                        else
                            result = "Korisnik ne postoji!";
                    }
                }
            }
            catch
            {
            }
            return text;
        }
    }
}