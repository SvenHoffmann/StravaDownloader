using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Net;

namespace StravaDownloader.Test
{
    [TestClass]
    public class Test_AccessActivity
    {
        private static readonly String _stravaUri = "https://www.strava.com";
        private static readonly String _accessToken = "0d4193a8da90b46adf3e033801054685d0d1110f";

        [ TestMethod]
        public void GetActivities()
        {
            RestClient c = new RestClient( _stravaUri );
            RestRequest req = new RestRequest( "/api/v3/athlete/activities" );
            req.AddHeader( "Authorization", "Bearer " + _accessToken );
            req.Parameters.Add( new Parameter() { Type = ParameterType.QueryString, Name = "per_page", Value = 100 } );
            req.Method = Method.GET;

            var res = c.Execute( req );
            dynamic rslt = JArray.Parse( res.Content );
            Assert.IsTrue( rslt.Count > 0 );
        }

        [TestMethod]
        public void GetActivity()
        {
            String actId = "956405328";

            RestClient c = new RestClient( _stravaUri );
            RestRequest req = new RestRequest( "/api/v3/activities/" + actId );
            req.AddHeader( "Authorization", "Bearer " + _accessToken );
            req.Method = Method.GET;

            var res = c.Execute( req );
            dynamic rslt = JObject.Parse( res.Content );
            Assert.AreEqual( actId, rslt.id.ToString() );
        }

        [TestMethod]
        public void GetActivityTCX()
        {
            String actId = "956405328";

            HttpClientHandler handler = new HttpClientHandler
            {
                Proxy = new WebProxy( "http://localhost:8888", false, new string[] { } ),
                UseProxy = true
            };

            HttpClient c = new HttpClient( handler );
            HttpRequestMessage req = new HttpRequestMessage( HttpMethod.Get, _stravaUri + "/activities/" + actId + "/export_tcx" );
            req.Headers.Authorization = System.Net.Http.Headers.AuthenticationHeaderValue.Parse( "Bearer " + _accessToken );
            var task = c.SendAsync( req );
            task.Wait( 60 * 100 );

            Assert.IsTrue( task.Result.IsSuccessStatusCode );

            using ( FileStream fs = File.Create( "test.tcx" ) )
            {
                Task<Stream> readStream = task.Result.Content.ReadAsStreamAsync();
                readStream.Wait();

                readStream.Result.CopyTo( fs );
            }
        }
    }
}
