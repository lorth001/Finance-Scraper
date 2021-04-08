/*
 * Author:  Luke Orth
 * Date:    01.05.2021
 * Version: 0.0.1
 * Updates: Initial version
 * 
 * About:   This "ParseAmex" class reads a provided .csv file (typically from the Selenium process)
 *          using the CsvHelper library and inserts the relevant fields into the database.
*/

using System;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using CsvHelper;
using CsvHelper.Configuration;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;

namespace FinancialScraper
{
    public class ParseAmex
    {
        /* Parse the downloaded .csv file */
        private void ReadCsvFile()
        {
            string filename = System.AppDomain.CurrentDomain.BaseDirectory + @"\activity.csv";      // .csv file path
            TextReader textReader = new StreamReader(filename);                                     // instantiate file reader
            var csv = new CsvReader(textReader, System.Globalization.CultureInfo.CurrentCulture);   // instantiate CsvReader to parse .csv file
            csv.Configuration.RegisterClassMap<CsvHeadersMap>();                                    // use custom .csv header mapping
            IEnumerable<CsvHeaders> headers = csv.GetRecords<CsvHeaders>();

            InsertRecords(headers); // insert records into the database
            textReader.Close();     // close the file reader
        }

        /* Insert relevant data from the .csv file into the database */
        private void InsertRecords(IEnumerable<CsvHeaders> headers)
        {
            DbConnection conn = new DbConnection(); // instantiate new database connection
            conn.OpenConnection();                  // open connection

            foreach (var header in headers)         // iterate through .csv and execute stored procedure
            {

                if (header.Address != "" && GetMap(header.Address).Count >= 3)
                {
                    List<object> address = GetMap(header.Address + " " + header.CityState + " " + header.ZipCode);
                    conn.ExecuteQueries($"EXEC insert_transactions 'AMEX', '{header.Date}', '{(header.Description).Replace("'", "\''").Replace("\"", "")}', '{header.CardMember}', {header.Amount}, {address[1]}, {address[2]}, '{address[0]}', '{header.Category}', 'lorth001@gmail.com';");
                }
                else
                {
                    conn.ExecuteQueries($"EXEC insert_transactions 'AMEX', '{header.Date}', '{(header.Description).Replace("'", "\''").Replace("\"", "")}', '{header.CardMember}', {header.Amount}, NULL, NULL, NULL, '{header.Category}', 'lorth001@gmail.com';");
                }
            }

            conn.CloseConnection(); // close database connection
        }

        public List<object> GetMap(string address)
        {
            string mapboxToken = "MY_MAPBOX_TOKEN";
            string uriAddress = Uri.EscapeDataString(address);
            string url = String.Format("https://api.mapbox.com/geocoding/v5/mapbox.places/{0}.json?access_token={1}", uriAddress, mapboxToken);
            
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(url);

            WebReq.Method = "GET";

            List<object> locationInfo = new List<object>();

            try
            {
                HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();

                string jsonString;
                using (Stream stream = WebResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                    jsonString = reader.ReadToEnd();
                }

                Map map = JsonConvert.DeserializeObject<Map>(jsonString);

                if(map.Features.Count >= 3)
                {
                    locationInfo.Add(map.Features[0].PlaceName);
                    locationInfo.Add(map.Features[0].Geometry.Coordinates[0]);
                    locationInfo.Add(map.Features[0].Geometry.Coordinates[1]);
                }
                else
                {
                    locationInfo.Clear();
                }

                return locationInfo;
            }
            catch (WebException exception)
            {
                locationInfo.Add("");
                locationInfo.Add(null);
                locationInfo.Add(null);

                return locationInfo;
            }
        }

        /* Controller function */
        public void Go()
        {
            ReadCsvFile();
        }
    }

    /* CSV headers */
    public class CsvHeaders
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string CardMember { get; set; }
        public decimal Amount { get; set; }
        public string Address { get; set; }
        public string CityState { get; set; }
        public string ZipCode { get; set; }
        public string Category { get; set; }
    }

    /* Map .csv headers */
    public class CsvHeadersMap : ClassMap<CsvHeaders>
    {
        public CsvHeadersMap()
        {
            Map(m => m.Date).Name("Date");
            Map(m => m.Description).Name("Description");
            Map(m => m.CardMember).Name("Card Member");
            Map(m => m.Amount).Name("Amount");
            Map(m => m.Address).Name("Address");
            Map(m => m.CityState).Name("City/State");
            Map(m => m.ZipCode).Name("Zip Code");
            Map(m => m.Category).Name("Category");
        }
    }

    /* Class for Mapbox API deserialization */
    [DataContract]
    public class Property
    {
        [DataMember(Name = "wikidata")]
        public string Wikidata { get; set; }
    }

    [DataContract]
    public class Geometry
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }
        [DataMember(Name = "coordinates")]
        public IList<double> Coordinates { get; set; }
    }

    [DataContract]
    public class Context
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "wikidata")]
        public string Wikidata { get; set; }
        [DataMember(Name = "short_code")]
        public string ShortCode { get; set; }
        [DataMember(Name = "text")]
        public string Text { get; set; }
    }

    [DataContract]
    public class Feature
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "type")]
        public string Type { get; set; }
        [DataMember(Name = "place_type")]
        public IList<string> PlaceType { get; set; }
        [DataMember(Name = "relevance")]
        public string Relevance { get; set; }
        [DataMember(Name = "properties")]
        public Property Properties { get; set; }
        [DataMember(Name = "text")]
        public string Text { get; set; }
        [DataMember(Name = "place_name")]
        public string PlaceName { get; set; }
        [DataMember(Name = "bbox")]
        public IList<double> Bbox { get; set; }
        [DataMember(Name = "center")]
        public IList<double> Center { get; set; }
        [DataMember(Name = "geometry")]
        public Geometry Geometry { get; set; }
        [DataMember(Name = "context")]
        public IList<Context> Context { get; set; }
    }

    [DataContract]
    public class Map
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }
        [DataMember(Name = "query")]
        public IList<string> Query { get; set; }
        [DataMember(Name = "features")]
        public IList<Feature> Features { get; set; }
        [DataMember(Name = "attribution")]
        public string Attribution { get; set; }
    }
}
