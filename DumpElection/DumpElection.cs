using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

namespace DumpElection
{
    class DumpElection
    {
        object t_lock;
        List<Dictionary<string, string>> entries;
        HttpGetter http;

        private List<string> get_hrefs(string content)
        {
            List<string> hrefs = new List<string>();

            MatchCollection mc = Regex.Matches(content, "href=\"([^\"]+)\"");
            foreach (Match m in mc)
            {
                string href = m.Groups[1].Value;
                hrefs.Add(href);
            }

            return hrefs;
        }

        List<string> get_region_codes(List<string> hrefs)
        {
            List<string> region_codes = new List<string>();

            foreach (string href in hrefs)
            {
                Match sm = Regex.Match(href, @"\./([0-9][0-9][0-9])/([0-9][0-9][0-9]).html");

                if (sm.Success && sm.Groups[1].Value == sm.Groups[2].Value)
                {
                    region_codes.Add(sm.Groups[1].Value);
                }
            }

            return region_codes;
        }

        private List<string> get_departement_codes(List<string> hrefs)
        {
            List<string> departement_codes = new List<string>();

            foreach (string href in hrefs)
            {
                Match sm = Regex.Match(href, @"\.\./[0-9][0-9][0-9]/([0-9][0-9][0-9])/([0-9][0-9][0-9]).html");

                if (sm.Success && sm.Groups[1].Value == sm.Groups[2].Value)
                {
                    departement_codes.Add(sm.Groups[1].Value);
                }
            }

            return departement_codes;
        }

        private List<string> get_letter_codes(List<string> hrefs)
        {
            List<string> letter_codes = new List<string>();

            foreach (string href in hrefs)
            {
                Match sm = Regex.Match(href, @"\.\./\.\./[0-9][0-9][0-9]/[0-9][0-9][0-9]/([0-9][0-9][0-9][A-Z]).html");

                if (sm.Success)
                {
                    letter_codes.Add(sm.Groups[1].Value);
                }
            }

            return letter_codes;
        }

        private List<string> get_city_codes(List<string> hrefs)
        {
            List<string> city_codes = new List<string>();

            foreach (string href in hrefs)
            {
                Match sm = Regex.Match(href, @"\.\./\.\./[0-9][0-9][0-9]/[0-9][0-9][0-9]/([0-9][0-9][0-9][0-9][0-9][0-9]).html");

                if (sm.Success)
                {
                    city_codes.Add(sm.Groups[1].Value);
                }
            }

            return city_codes;
        }

        private List<string> get_between_tokens(string content, string start_token, string end_token)
        {
            List<string> e = new List<string>();

            int offset = 0;

            while (offset < content.Length)
            {
                int i = content.IndexOf(start_token, offset);
                if (i == -1)
                    break;
                i += start_token.Length;
                offset = i;


                int j = content.IndexOf(end_token, offset);
                if (j == -1)
                    break;
                offset = j + end_token.Length;

                string row = content.Substring(i, j - i);

                e.Add(row);
            }

            return e;
        }

        private List<string> get_rows(string content)
        {
            return get_between_tokens(content, "<tr>", "</tr>");
        }

        private List<string> get_columns(string content)
        {
            return get_between_tokens(content, ">", "</td>");
        }

        class ThreadParemeters
        {
            List<Tuple<string, string, string, string>> lci;
            List<Dictionary<string, string>> values;

            public ThreadParemeters(List<Tuple<string, string, string, string>> lci, List<Dictionary<string, string>> values)
            {
                this.lci = lci;
                this.values = values;
            }

            public List<Tuple<string, string, string, string>> get_lci()
            {
                return lci;
            }

            public List<Dictionary<string, string>> get_values()
            {
                return values;
            }
        }

        private List<string> get_region_codes()
        {
            string content = http.get_page_content("index.html");

            List<string> hrefs = get_hrefs(content);

            List<string> region_codes = get_region_codes(hrefs);

            return region_codes;
        }

        private List<string> get_departement_codes(string region_code)
        {
            string content = http.get_page_content(region_code + "/" + region_code + ".html");

            List<string> hrefs = get_hrefs(content);

            List<string> departement_codes = get_departement_codes(hrefs);

            return departement_codes;
        }

        private List<string> get_letter_codes(string region_code, string departement_code)
        {
            string content = http.get_page_content(region_code + "/" + departement_code + "/index.html");

            List<string> hrefs = get_hrefs(content);

            List<string> letter_codes = get_letter_codes(hrefs);

            return letter_codes;
        }

        private List<string> get_city_codes(string region_code, string departement_code, string letter_code)
        {
            string content = http.get_page_content(region_code + "/" + departement_code + "/" + letter_code + ".html");

            List<string> hrefs = get_hrefs(content);

            List<string> city_codes = get_city_codes(hrefs);

            return city_codes;
        }

        private void get_city_results(string region_code, string departement_code, string city_code)
        {
            string content = http.get_page_content(region_code + "/" + departement_code + "/" + city_code + ".html");

            Dictionary<string, string> v = new Dictionary<string, string>();
            lock (t_lock)
            {
                entries.Add(v);
            }

            string city_name = Regex.Match(content, "Commune de ([^<]+)").Groups[1].Value;

            v["Ville"] = city_name;
            v["Région"] = region_code;
            v["Département"] = departement_code;

            List<string> rows = get_rows(content);
            for (int i = 2; i < 13; ++i)
            {
                string row = rows[i];
                List<string> columns = get_columns(row);

                v[columns[0]] = columns[1];
            }

            for (int i = 14; i < 20; ++i)
            {
                string row = rows[i];
                List<string> columns = get_columns(row);

                v[columns[0]] = columns[1];
            }
        }

        private void get_city_codes_main(object o)
        {
            Tuple<string, string, string> p = (Tuple<string, string, string>)o;
            string rc = p.Item1;
            string dc = p.Item2;
            string lc = p.Item3;

            List<string> city_codes = get_city_codes(rc, dc, lc);

            foreach (string cc in city_codes)
            {
                get_city_results(rc, dc, cc);
            }
        }

        private void get_letter_codes_main(object o)
        {
            Tuple<string, string> p = (Tuple<string, string>)o;
            string rc = p.Item1;
            string dc = p.Item2;

            List<string> letter_codes = get_letter_codes(rc, dc);

            List<Thread> thread_pool = new List<Thread>(); ;

            foreach (string lc in letter_codes)
            {
                Thread t = new Thread(get_city_codes_main);
                thread_pool.Add(t);
                Tuple<string, string, string> np = new Tuple<string, string, string>(rc, dc, lc);
                t.Start(np);
            }

            foreach (Thread t in thread_pool)
                t.Join();
        }

        private void get_departement_codes_main(object o)
        {
            string rc = (string)o;
            List<string> departement_codes = get_departement_codes(rc);

            List<Tuple<Thread, string>> thread_pool = new List<Tuple<Thread, string>>(); ;

            foreach (string dc in departement_codes)
            {
                Thread t = new Thread(get_letter_codes_main);
                thread_pool.Add(new Tuple<Thread, string>(t, dc));
                Tuple<string, string> p = new Tuple<string, string>(rc, dc);
                t.Start(p);
            }

            foreach (Tuple<Thread, string> t in thread_pool)
            {
                t.Item1.Join();
                Console.WriteLine("departement "+ t.Item2 + " done");
            }
        }

        public void run()
        {
            List<string> region_codes = get_region_codes();

            List<Thread> thread_pool = new List<Thread>(); ;

            foreach (string rc in region_codes)
            {
                Thread t = new Thread(get_departement_codes_main);
                thread_pool.Add(t);
                t.Start(rc);
            }

            foreach (Thread t in thread_pool)
                t.Join();


            List<string> column_names = new List<string>();

            column_names.Add("Ville");
            column_names.Add("Région");
            column_names.Add("Département");
            column_names.Add("M. Jean-Luc MÉLENCHON");
            column_names.Add("Mme Marine LE PEN");
            column_names.Add("M. Emmanuel MACRON");
            column_names.Add("M. François FILLON");
            column_names.Add("M. Nicolas DUPONT-AIGNAN");
            column_names.Add("M. Benoît HAMON");
            column_names.Add("M. Philippe POUTOU");
            column_names.Add("Mme Nathalie ARTHAUD");
            column_names.Add("M. Jean LASSALLE");
            column_names.Add("M. Jacques CHEMINADE");
            column_names.Add("M. François ASSELINEAU");
            column_names.Add("Inscrits");
            column_names.Add("Abstentions");
            column_names.Add("Votants");
            column_names.Add("Blancs");
            column_names.Add("Nuls");
            column_names.Add("Exprimés");

            StringBuilder sb = new StringBuilder();

            foreach (string column_name in column_names)
            {
                sb.Append(column_name + ",");
            }
            sb.Append("\n");

            foreach (Dictionary<string, string> e in entries)
            {
                foreach (string column_name in column_names)
                {
                    sb.Append(e[column_name] + ",");
                }
                sb.Append("\n");
            }

            File.WriteAllText("elections.csv", sb.ToString());
        }

        public DumpElection()
        {
            t_lock = new object();
            entries = new List<Dictionary<string, string>>();
            http = new HttpGetter("http://elections.interieur.gouv.fr/presidentielle-2017/", 130);
        }
    }
}
