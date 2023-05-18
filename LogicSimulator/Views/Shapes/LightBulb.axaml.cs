using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.ComponentModel;

namespace LogicSimulator.Views.Shapes {
    public partial class LightBulb: GateBase, IGate, INotifyPropertyChanged {
        public override int TypeId => 7;

        public override int CountIns => 1;
        public override int CountOuts => 0;
        public override UserControl GetSelf() => this;
        protected override IGate GetSelfI => this;

        protected override void Init() {
            width = 30 * 2.5;
            height = 30 * 2.5;
            InitializeComponent();
            DataContext = this;
        }

        readonly Border border;
        public LightBulb(): base() {
            if (LogicalChildren[0].LogicalChildren[1] is not Border b) throw new Exception("Такого не бывает");
            border = b;
        }

        /*
         * Обработка размеров внутренностей
         */

        public override Point[][] PinPoints { get {
            double X = EllipseSize - EllipseStrokeSize / 2;
            double Y = height / 2;
            double PinWidth = base_size - EllipseSize + PinStrokeSize;
            return new Point[][] {
                new Point[] { new(X, Y), new(X + PinWidth, Y) }, // Единственный вход
            };
        } }

        /*
         * Мозги
         */

        readonly SolidColorBrush ColorA = new(Color.Parse("#00ff00")); // On
        readonly SolidColorBrush ColorB = new(Color.Parse("#1c1c1c")); // Off
        public void Brain(ref bool[] ins, ref bool[] outs) {
            var value = state = ins[0];
            Dispatcher.UIThread.InvokeAsync(() => {
                border.Background = value ? ColorA : ColorB;
            });
        }

        private static readonly Random rand = new(213821);
        bool state;

        public bool GetState() => rand.Next(2) == 1;
    }
}
