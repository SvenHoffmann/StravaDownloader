using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StravaDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string AccessToken { get; set; }

        public String ClientId { get; set; }

        public String ClientSecret { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            webBrowser.Navigating += WebBrowser_Navigating;

            List<Activity> sample = new List<Activity>() {  new Activity() {  Distance="1", Name="Name 1", StartDate=DateTime.UtcNow.ToLongDateString()},
            new Activity() {  Distance="2", Name="Name 2", StartDate=DateTime.UtcNow.ToLongDateString()},
            new Activity() {  Distance="3", Name="Name 3", StartDate=DateTime.UtcNow.ToLongDateString()}};
            dgAct.ItemsSource = sample;

            AccessToken = ConfigurationManager.AppSettings[ "at" ];
            ClientId = ConfigurationManager.AppSettings[ "clientId" ];
            ClientSecret = ConfigurationManager.AppSettings[ "clientSecret" ];
        }

        private void WebBrowser_Navigating( object sender, NavigatingCancelEventArgs e )
        {
            if ( String.Compare( e.Uri.Host, "localhost" ) == 0 )
            {
                var nameValues = RestSharp.Extensions.MonoHttp.HttpUtility.ParseQueryString( e.Uri.Query );
                String code = nameValues[ "code" ];
                obtainAT( code );

                //webBrowser.Navigate("http://www.google.de");
                webBrowser.Visibility = Visibility.Hidden;
            }
        }

        private void obtainAT( string code )
        {
            RestClient c = new RestClient( "https://www.strava.com" );
            RestRequest req = new RestRequest( "/oauth/token" );
            req.Parameters.Add( new Parameter() { Name = "client_id", Value = "17471", Type = ParameterType.GetOrPost } );
            req.Parameters.Add( new Parameter() { Name = "client_secret", Value = "7b746d1812333d5597817f9c0693350afd29931a", Type = ParameterType.GetOrPost } );
            req.Parameters.Add( new Parameter() { Name = "code", Value = code, Type = ParameterType.GetOrPost } );
            req.Method = Method.POST;

            var res = c.Execute( req );
            dynamic rslt = JObject.Parse( res.Content );
            AccessToken = rslt.access_token;
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            //api/v3/athlete/activities
            RestClient c = new RestClient( "https://www.strava.com" );
            RestRequest req = new RestRequest( "/api/v3/athlete/activities" );
            req.AddHeader( "Authorization", "Bearer " + AccessToken );
            req.Parameters.Add( new Parameter() { Type = ParameterType.QueryString, Name = "per_page", Value = 100 } );
            req.Method = Method.GET;

            var res = c.Execute( req );
            dynamic rslt = JArray.Parse( res.Content );

            dgAct.ItemsSource = rslt;

        }

        private void Button_Click_tcx( object sender, RoutedEventArgs e )
        {
            dynamic data = dgAct.ItemsSource;
            dynamic myride = null;
            foreach ( dynamic d in data )
            {
                if ( d.name.ToString() == "WdS-60er" )
                {
                    myride = d;
                    break;
                }
            }


            //api/v3/athlete/activities
            //RestClient c = new RestClient("https://www.strava.com");
            //RestRequest req = new RestRequest("/api/v3/athlete/activities/" + myride.id + "/export_original");
            //req = new RestRequest("/api/v3/athlete/activities/" + myride.id + "/export_tcx");
            //req.AddHeader("Authorization", "Bearer " + _accessToken);
            //req.Method = Method.GET;
            HttpClient c = new HttpClient();
            Task<HttpResponseMessage> r = c.GetAsync( "https://www.strava.com/api/v3/athlete/activities/" + myride.id + "/export_original" );
            r.Wait();
            Task<String> t = r.Result.Content.ReadAsStringAsync();
            t.Wait();


            //var res = c.Execute(req);
        }

    }
}
