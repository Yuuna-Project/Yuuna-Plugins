
namespace Yuuna.Plugins.Spotify
{
    using SpotifyAPI.Web;
    using SpotifyAPI.Web.Auth;
    using SpotifyAPI.Web.Enums;
    using SpotifyAPI.Web.Models;

    using System;
    using System.Text;
    using System.Threading.Tasks;

    public class TestProgram
    {
        static async Task Main(string[] args)
        {
            var token = default(Token);
            var api = new SpotifyWebAPI();
            var url = "http://localhost:4002";
            var scopes = Scope.PlaylistReadPrivate |
                        Scope.PlaylistReadCollaborative |
                        Scope.UserReadPlaybackState |
                        Scope.AppRemoteControl |
                        Scope.UserModifyPlaybackState;
            var auth = new AuthorizationCodeAuth("10728cf41fa1473f8f47c817deed3c5a",
                "98241e4b631b42a4a4263c0403d7880b",
                url, url, scopes);


            auth.AuthReceived += async (sender, payload) =>
            {
                auth.Stop();
                token = await auth.ExchangeCode(payload.Code);
                api = new SpotifyWebAPI()
                {
                    TokenType = token.TokenType,
                    AccessToken = token.AccessToken
                };  
            };

            auth.Start();
            auth.OpenBrowser();


            Console.ReadKey();
            var err = await api.SkipPlaybackToNextAsync();
            if (err.HasError())
            {
                Console.WriteLine(err.Error.Message);
                Console.WriteLine(err.Error.Status);
            }
        }
    }

     
}
