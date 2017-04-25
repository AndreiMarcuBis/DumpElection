using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

namespace DumpElection
{
    class HttpGetter
    {
        string base_url;
        List<HttpClient> clients;
        List<bool> client_status;
        Semaphore s;
        object t_lock;

        public HttpGetter(string base_url, int clients_count)
        {
            this.base_url = base_url;
            clients = new List<HttpClient>();
            client_status = new List<bool>();
            t_lock = new object();
            s = new Semaphore(clients_count, clients_count);

            for (int i = 0; i < clients_count; ++i)
            {
                HttpClient c = new HttpClient();
                c.BaseAddress = new Uri(base_url);
                client_status.Add(false);
                clients.Add(c);
            }
        }

        public string get_page_content(string url)
        {
            s.WaitOne();

            int current_client = -1;
            HttpClient c;

            lock (t_lock)
            {
                for (int i = 0; i < client_status.Count; ++i)
                {
                    if (!client_status[i])
                    {
                        client_status[i] = true;
                        current_client = i;
                        break;
                    }
                }

                c = clients[current_client];
            }

            byte[] b = c.GetAsync(url).Result.Content.ReadAsByteArrayAsync().Result;
            string content = Encoding.GetEncoding("ISO-8859-15").GetString(b);

            lock (t_lock)
            {
                client_status[current_client] = false;
            }

            s.Release();

            return content;
        }
    }
}
