
namespace Yuuna.Plugins.Spotify
{
    using SpotifyAPI.Web;
    using SpotifyAPI.Web.Auth;
    using SpotifyAPI.Web.Enums;
    using SpotifyAPI.Web.Models;

    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    using Yuuna.Contracts.Interaction;
    using Yuuna.Contracts.Modules;
    using Yuuna.Contracts.Patterns;
    using Yuuna.Contracts.Semantics;
    using Yuuna.Contracts.Utils;


    [ModuleMetadata("Spotify",
        Author = "Yuuna-Project@Orlys", 
        Description = "Provides control support for Spotify",
        Url = "https://github.com/Yuuna-Project/Yuuna-Plugins")]
    public sealed class Spotify : ModuleBase
    {
        private volatile SpotifyWebAPI _api ;

        private readonly AuthorizationCodeAuth _auth; 
        private Token _token;
        public Spotify()
        {
           this. _api = new SpotifyWebAPI();
            var url = "http://localhost:4002";
            var scopes = Scope.PlaylistReadPrivate |
                        Scope.PlaylistReadCollaborative |
                        Scope.UserReadPlaybackState |
                        Scope.AppRemoteControl |
                        Scope.UserModifyPlaybackState;
            this._auth = new AuthorizationCodeAuth(
                Encoding.Unicode.GetString(Convert.FromBase64String("MQAwADcAMgA4AGMAZgA0ADEAZgBhADEANAA3ADMAZgA4AGYANAA3AGMAOAAxADcAZABlAGUAZAAzAGMANQBhAA==")),
                Encoding.Unicode.GetString(Convert.FromBase64String("OQA4ADIANAAxAGUANABiADYAMwAxAGIANAAyAGEANABhADQAMgA2ADMAYwAwADQAMAAzAGQANwA4ADgAMABiAA==")),
                url, url, scopes);

            this._auth.AuthReceived += async (sender, payload) =>
            {
                this._auth.Stop();
                if (this._token.IsExpired())
                    this._token = await this._auth.RefreshToken(this._token.RefreshToken);
                this._token = await this._auth.ExchangeCode(payload.Code);
                this._api = new SpotifyWebAPI()
                {
                    TokenType = this._token.TokenType,
                    AccessToken = this._token.AccessToken
                };
            };

            this._auth.Start();
            this._auth.OpenBrowser(); 
        }

         
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task CheckToken()
        { 
            if (this._token.IsExpired())
                this._token = await this._auth.RefreshToken(this._token.RefreshToken);
        }

        protected override void BuildPatterns(IGroupManager g, IPatternBuilder p, dynamic config, dynamic session)
        {
            g.Define("pause").AppendOrCreate(new[] { "暫停" });
            g.Define("resume").AppendOrCreate(new[] { "繼續" });
            g.Define("play").AppendOrCreate(new[] { "播放" });

            g.Define("next").AppendOrCreate(new[] { "下一首" });
            g.Define("previous").AppendOrCreate(new[] { "上一首" });
            g.Define("song").AppendOrCreate(new[] { "歌", "歌曲", "曲目", "樂曲", "音樂" });

            Invoke pause = m =>
            {
                Task.WaitAll(this.CheckToken());

                var er = this._api.PausePlayback();
                if (er.HasError())
                {
                    return (Moods.Sad, er.Error.Message);
                }
                return (Moods.Happy, "已暫停播放");
            };

            p.Build(g["pause"], g["play"]).OnInvoke(pause);
            p.Build(g["pause"], g["song"]).OnInvoke(pause);

            Invoke resume = m =>
            {
                Task.WaitAll(this.CheckToken());

                var er = this._api.ResumePlayback(offset: default(int?));
                if (er.HasError())
                {
                    return (Moods.Sad, er.Error.Message);
                }
                return (Moods.Happy, "已繼續播放");
            };
            p.Build(g["play"], g["song"]).OnInvoke(resume);
            p.Build(g["resume"], g["play"]).OnInvoke(resume);
            p.Build(g["resume"], g["play"], g["song"]).OnInvoke(resume);


            Invoke next = m =>
            {
                Task.WaitAll(this.CheckToken());

                var er = this._api.SkipPlaybackToNext();
                if (er.HasError())
                {
                    return (Moods.Sad, er.Error.Message);
                }
                return (Moods.Happy, "已經換至下一首囉");
            };
            p.Build(g["play"], g["next"]).OnInvoke(next);
            p.Build(g["play"], g["next"], g["song"]).OnInvoke(next); 
            p.Build(g["next"], g["song"]).OnInvoke(next);


            new Invoke(m => {
                Task.WaitAll(this.CheckToken());
                var er = this._api.SkipPlaybackToPrevious();
                return er.HasError() ?
                    (Moods.Sad, er.Error.Message) : 
                    (Moods.Happy, "已經回到上一首囉"); 
            }).InvokeWith(
                p.Build(g["play"], g["previous"]),
                p.Build(g["play"], g["previous"], g["song"]),
                p.Build(g["previous"], g["song"])); 
        }
    } 
}
