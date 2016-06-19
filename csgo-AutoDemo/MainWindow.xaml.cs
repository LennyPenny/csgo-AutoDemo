using System;
using System.Collections.Generic;
using System.Linq;
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
using CSGSI;

namespace csgo_AutoDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool enabled = true;

        private GameStateListener gsl;

        private static void OnNewGameState(GameState gs)
        {

        }

        public MainWindow()
        {
            InitializeComponent();

            gsl = new GameStateListener(13337);

            gsl.NewGameState += OnNewGameState;

            if (!gsl.Start()) Environment.Exit(0);

            Log("Game State Listener started");

            this.Closed += (_, __) =>
            {
                Environment.Exit(0);
            };

        }

        private void Log(string msg)
        {
            DebugOutput.Text = DebugOutput.Text + "\n" + msg;
            DebugOutput.ScrollToEnd();
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
