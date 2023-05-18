using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using LogicSimulator.Models;
using System.ComponentModel;

namespace LogicSimulator.Views.Shapes {
    public partial class Button: GateBase, IGate, INotifyPropertyChanged {
        public override int TypeId => 6;

        public override int CountIns => 0;
        public override int CountOuts => 1;
        public override UserControl GetSelf() => this;
        protected override IGate GetSelfI => this;

        protected override void Init() {
            width = 30 * 2.5;
            height = 30 * 2.5;
            InitializeComponent();
            DataContext = this;
        }

        /*
         * Обработка размеров внутренностей
         */

        public double ButtonSize => width.Min(height) - BodyStrokeSize.Left * 5.5;

        public override Point[][] PinPoints { get {
            double X = base_size + width - EllipseStrokeSize / 2;
            double Y = height / 2;
            double PinWidth = base_size - EllipseSize + PinStrokeSize;
            return new Point[][] {
                new Point[] { new(X, Y), new(X + PinWidth, Y) }, // Единственный выход
            };
        } }

        /*
         * Мозги
         */

        bool my_state = false;

        private void Press(object? sender, PointerPressedEventArgs e) {
            if (e.Source is not Ellipse button) return;
            my_state = true;
            button.Fill = new SolidColorBrush(Color.Parse("#7d1414"));
        }
        private void Release(object? sender, PointerReleasedEventArgs e) {
            if (e.Source is not Ellipse button) return;
            my_state = false;
            button.Fill = new SolidColorBrush(Color.Parse("#d32f2e"));
        }

        public void Brain(ref bool[] ins, ref bool[] outs) => outs[0] = my_state;
    }
}
