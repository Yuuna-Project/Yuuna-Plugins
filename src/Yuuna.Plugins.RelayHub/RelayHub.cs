
namespace Yuuna.Plugins.RelayHub
{
    using System;
    using System.Linq;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Yuuna.Contracts.Interaction;
    using Yuuna.Contracts.Modules; 
    using Yuuna.Contracts.Patterns;
    using Yuuna.Contracts.Semantics; 
    using Yuuna.Contracts.Evaluation;
    using Yuuna.Common.Linq;

    [ModuleMetadata("RelayHub",
        Author = "Yuuna-Project@Orlys",
        Description = "Provides a universal control relay interface for Yuuna call and control software and hardware",
        Url = "https://github.com/Yuuna-Project/Yuuna-Plugins")]
    public class RelayHub : ModuleBase
    {

        protected override void BuildPatterns(IGroupManager g, IPatternBuilder p, dynamic config, dynamic session)
        {
            var open = g.Define("open");
            open.AppendOrCreate(new[] { "打開", "開啟" });

            var facebook = g.Define("facebook");
            facebook.AppendOrCreate(new[] { "臉書", "facebook", "fb" });

            p.Build(open, facebook).OnInvoke(this.OnInvoke);
        }

        private Response OnInvoke(Match m)
        {
            UriLauncher.Launch(new Uri("http://fb.me"));
            return (Moods.Happy, "已經打開" + m.Matches.Last().RandomTakeOne());
        }
    }

    public static class UriLauncher
    {
        static readonly Func<Uri, ProcessStartInfo> s_startInfoBuilder;
        static UriLauncher()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                s_startInfoBuilder = url => new ProcessStartInfo("cmd", $"/c start {url}");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                s_startInfoBuilder = url => new ProcessStartInfo("open", url.ToString());
            else
                s_startInfoBuilder = url => new ProcessStartInfo("xdg-open", url.ToString());
        }

        public static void Launch(Uri uri)
        {
            using (var p = new Process
            {
                StartInfo = s_startInfoBuilder(uri)
            })
            {

                if (p.Start())
                {
                    p.WaitForExit();
                }
            }
        }
    }
}
