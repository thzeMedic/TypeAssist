using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using TypeAssist.Services;

namespace TypeAssist
{
    public partial class App : Application
    {
        // Wir machen den Client "public static", damit wir von überall (z.B. MainWindow) darauf zugreifen können.
        // In Profi-Apps nutzt man "Dependency Injection", aber für jetzt reicht das völlig.
        public static LlmClient LlmService { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Service erstellen
            LlmService = new LlmClient();

            // 2. Warmup im Hintergrund starten (Fire and Forget)
            // Wir warten nicht darauf (kein await), damit das Fenster sofort aufgeht.
            _ = LlmService.WarmupAsync();
            // Das MainWindow öffnet sich automatisch durch die App.xaml Konfiguration
        }
    }

}
