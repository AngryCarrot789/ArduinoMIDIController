using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;
using MIDIController.Serial;

namespace ArduinoMIDIController {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        AnalogDataDispatcher d;

        DispatcherTimer timer;

        public MainWindow() {
            InitializeComponent();
            d = new AnalogDataDispatcher("COM4", 9600, (e) => {
                Debug.WriteLine($"{e.A0}, {e.A1}, {e.A2}, {e.A3}");
            });

            d.Open();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += this.Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e) {
            d.PopQueue();
        }
    }
}
