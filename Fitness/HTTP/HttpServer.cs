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
    
    Credits: https://github.com/usertoroot
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;

namespace Fitness
{
    public partial class HttpServer
    {
        private delegate string HttpHandlerDelegate(HttpServer server, HttpListenerRequest request, Dictionary<string, string> parameters);
        private static Dictionary<string, KeyValuePair<HttpHandler, HttpHandlerDelegate>> _handlers = new Dictionary<string, KeyValuePair<HttpHandler, HttpHandlerDelegate>>();

        public static void MapHandlers()
        {
            foreach (MethodInfo methodInfo in typeof(HttpServer).GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
            {
                var attributes = methodInfo.GetCustomAttributes(typeof(HttpHandler), false);
                if (attributes.Length < 1)
                    continue;

                HttpHandler attribute = (HttpHandler)attributes[0];
                if (_handlers.ContainsKey(attribute.Url))
                    continue;

                _handlers.Add(attribute.Url, new KeyValuePair<HttpHandler, HttpHandlerDelegate>(attribute, (HttpHandlerDelegate)Delegate.CreateDelegate(typeof(HttpHandlerDelegate), methodInfo)));
            }
        }

        private HttpListener m_listener;

        public HttpServer()
        {
            m_listener = new HttpListener();
            m_listener.Prefixes.Add("http://*:8181/");
        }

        public void Start()
        {
            m_listener.Start();
            m_listener.BeginGetContext(OnGetContext, null);
        }

        public void Stop()
        {
            m_listener.Close();
            m_listener = null;
        }

        private void OnGetContext(IAsyncResult result)
        {
            try
            {
                var context = m_listener.EndGetContext(result);
                ThreadPool.QueueUserWorkItem(HandleRequest, context);
            }
            finally
            {
                m_listener.BeginGetContext(OnGetContext, null);
            }
        }

        private void HandleRequest(object oContext)
        {
            HttpListenerContext context = (HttpListenerContext)oContext;
            try
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                string[] tokens = context.Request.RawUrl.Split('&');
                foreach (var token in tokens)
                {
                    string[] keyValuePair = token.Split('=');
                    if (keyValuePair.Length != 2)
                        continue;

                    var key = WebUtility.UrlDecode(keyValuePair[0]);
                    var value = WebUtility.UrlDecode(keyValuePair[1]);
                    parameters.Add(key, value);
                }

                KeyValuePair<HttpHandler, HttpHandlerDelegate> pair;
                string[] raw = context.Request.RawUrl.Split('&');
                if (raw[0] == "/favicon.ico")
                    return;
                if (!_handlers.TryGetValue(raw[0], out pair))
                    return;

                context.Response.ContentType = "text/html";
                string result = pair.Value(this, context.Request, parameters);
                if (result == null)
                    return;

                context.Response.ContentEncoding = System.Text.Encoding.UTF8;
                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream, context.Response.ContentEncoding))
                    writer.Write(result);
            }
            finally
            {
                context.Response.Close();
            }
        }
    }
}
