using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using CSGSI;
using CSGSI.Nodes;
using Microsoft.Win32;

namespace csgo_AutoDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool enabled = true;

        private GameStateListener gsl;

        private static readonly string csgopath =
                (string)
                    Registry.GetValue(
                        @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 730",
                        "InstallLocation", "");

        private void Record()
        {
            var demosubdir = $"{DateTime.Now.ToString("yyy")}_{DateTime.Now.ToString("M_MMM")}";
            Directory.CreateDirectory(csgopath + $@"\csgo\autodemo\{demosubdir}");

            var demoname = $"autodemo/{demosubdir}/{DateTime.Now.ToString("d_dddd__H_m_s")}";

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.FileName = "SourceCmd";
            psi.Arguments = $"csgo.exe \"record \\\"{demoname}\\\"";
            Process.Start(psi);
            Log(psi.Arguments);
            Log($"Recording to {demoname}...");
        }

        private bool firststate = true;
        private void OnNewGameState(GameState gs)
        {
            if (firststate == true)
            {
                if (gs.Player.Activity == PlayerActivity.Playing)
                    Record();

                firststate = false;
                return;
            }

            if (gs.Player.Activity == PlayerActivity.Playing && gs.Previously.Player.Activity == PlayerActivity.Menu)
            {
                Log("Started playing");

                Record();

            }
            else if (gs.Player.Activity == PlayerActivity.Menu && gs.Previously.Player.Activity == PlayerActivity.Playing)
            {
                Log("Stopped playing");
            }
        }

        private void PlaceGSLConfig()
        {
            try
            {
                File.Copy("gamestate_integration_autodemo.cfg", $@"{csgopath}\csgo\cfg\gamestate_integration_autodemo.cfg", false);
                Log("Placed gsl config if it didn't already exist");
            }
            catch (Exception) {}
        }

        public MainWindow()
        {
            InitializeComponent();

            if (csgopath == "")
            {
                Log("Couldn't find csgo, is it installed?");
            }
            else
            {
                PlaceGSLConfig();
                Directory.CreateDirectory(csgopath + @"\csgo\autodemo");
                Log("Created csgo/autodemo directory if didn't already exist");
            }

            gsl = new GameStateListener(13337);

            gsl.NewGameState += OnNewGameState;

            if (!gsl.Start())
            {
                Log("Couldn't start even listener ");
                Log($"Maybe port {gsl.Port} can't be bound?");
            }
            else
            {
                Log("Game State Listener started");
            }

            this.Closed += (_, __) =>
            {
                Environment.Exit(0);
            };

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon("csgo-AutoDemo.ico");
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void Log(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                DebugOutput.Text = DebugOutput.Text + "\n" + msg;
                DebugOutput.ScrollToEnd();
            });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (enabled)
            {
                gsl.NewGameState -= OnNewGameState;
                Enabler.Content = "Enable";

                Log("Stopped listening");
            }
            else
            {
                gsl.NewGameState += OnNewGameState;
                Enabler.Content = "Disable";
                Log("Started listening");
            }
            enabled = !enabled;
        }
    }
}
