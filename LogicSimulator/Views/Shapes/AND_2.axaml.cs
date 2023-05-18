using Avalonia;
using Avalonia.Controls;
using System.ComponentModel;

namespace LogicSimulator.Views.Shapes {
    public partial class AND_2: GateBase, IGate, INotifyPropertyChanged {
        public override int TypeId => 0;

        public override int CountIns => 2;
        public override int CountOuts => 1;
        public override UserControl GetSelf() => this;
        protected override IGate GetSelfI => this;

        protected override void Init() {
            height = 30 * 3;
            InitializeComponent();
            DataContext = this;
        }

        /*
         * Обработка размеров внутренностей
         */

        public override Point[][] PinPoints { get {
            double X = EllipseSize - EllipseStrokeSize / 2;
            double X2 = base_size + width - EllipseStrokeSize / 2;
            double R = BodyRadius.TopLeft;
            double Y_s = R, Y_m = height / 2, Y_e = height - Y_s;
            double min = EllipseSize + BaseFraction * 2;
            // .1..2.
            double Y = Y_s + (Y_e - Y_s) / 4;
            double Y2 = Y_s + (Y_e - Y_s) / 4 * 3;
            if (Y2 - Y < min) { Y = Y_m - min / 2; Y2 = Y_m + min / 2; }
            double PinWidth = base_size - EllipseSize + PinStrokeSize;
            return new Point[][] {
                new Point[] { new(X, Y), new(X + PinWidth, Y) }, // Первый вход
                new Point[] { new(X, Y2), new(X + PinWidth, Y2) }, // Второй вход
                new Point[] { new(X2, Y_m), new(X2 + PinWidth, Y_m) }, // Единственный выход
            };
        } }

        /*
         * Мозги
         */

        public void Brain(ref bool[] ins, ref bool[] outs) => outs[0] = ins[0] && ins[1];
    }
}
