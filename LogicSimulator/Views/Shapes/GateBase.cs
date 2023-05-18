using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using LogicSimulator.Models;
using LogicSimulator.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LogicSimulator.Views.Shapes {
    public abstract class GateBase: UserControl {
        public abstract int CountIns { get; }
        public abstract int CountOuts { get; }
        public abstract UserControl GetSelf();
        protected abstract IGate GetSelfI { get; }
        protected abstract void Init();

        protected Ellipse[] pins;

        public GateBase() {
            Init();
            int count = CountIns + CountOuts;

            List<Ellipse> list = new();
            foreach (var logic in LogicalChildren[0].LogicalChildren)
                if (logic is Ellipse @ellipse) list.Add(@ellipse);
            if (list.Count != count) throw new Exception("Чё?!"); // У этой фигуры всегда count пинов
            pins = list.ToArray();

            joins = new JoinedItems?[count];
        }

        public void Move(Point pos) {
            Margin = new(pos.X, pos.Y, 0, 0);
            UpdateJoins(false);
        }

        public void Resize(Size size, bool global) {
            double limit = (9 + 32) * 2;
            width = size.Width.Max(limit / 3 * (CountIns == 0 || CountOuts == 0 ? 2.25 : 3));
            height = size.Height.Max(limit / 3 * (1.5 + 0.75 * CountIns.Max(CountOuts)));
            RecalcSizes();
            UpdateJoins(global);
        }

        public Point GetPos() => new(Margin.Left, Margin.Top);
        public Size GetSize() => new(Width, Height);
        public Size GetBodySize() => new(width, height);

        /*
         * Обработка размеров внутренностей
         */

        protected readonly double base_size = 25;
        protected double width = 30 * 3; // Размеры тела, а не всего UserControl
        protected double height = 30 * 3;

        public double BaseSize => base_size;
        public double BaseFraction => base_size / 40;
        public double EllipseSize => BaseFraction * 30;

        public Thickness BodyStrokeSize => new(BaseFraction * 3);
        public double EllipseStrokeSize => BaseFraction * 5;
        public double PinStrokeSize => BaseFraction * 6;

        public Thickness BodyMargin => new(base_size, 0, 0, 0);
        public double BodyWidth => width;
        public double BodyHeight => height;
        public CornerRadius BodyRadius => new(width.Min(height) / 3 + BodyStrokeSize.Top);

        public double UC_Width => base_size * 2 + width;
        public double UC_Height => height;

        public double FontSizze => BodyRadius.TopLeft / 1.3;

        public Thickness[] ImageMargins {
            get {
                double R = BodyRadius.BottomLeft;
                double num = R - R / Math.Sqrt(2);
                return new Thickness[] {
                new(0, 0, num, num), // Картинка с удалителем
                new(num, 0, 0, num), // Картинка с переместителем
            };
        } }



        public abstract Point[][] PinPoints { get; }
        public Thickness[] EllipseMargins { get {
            Point[][] pins = PinPoints;
            double R2 = EllipseSize / 2;
            double X = UC_Width - EllipseSize;
            int n = 0;
            List<Thickness> list = new();
            foreach (var pin_line in pins)
                list.Add(new(n++ < CountIns ? 0 : X, pin_line[0].Y - R2, 0, 0));
            return list.ToArray();
        } }



#pragma warning disable CS0108
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0108

        protected void RecalcSizes() {
            // Log.Write("Size: " + width + " " + height);
            PropertyChanged?.Invoke(this, new(nameof(EllipseSize)));
            PropertyChanged?.Invoke(this, new(nameof(BodyStrokeSize)));
            PropertyChanged?.Invoke(this, new(nameof(EllipseStrokeSize)));
            PropertyChanged?.Invoke(this, new(nameof(PinStrokeSize)));
            PropertyChanged?.Invoke(this, new(nameof(BodyMargin)));
            PropertyChanged?.Invoke(this, new(nameof(BodyWidth)));
            PropertyChanged?.Invoke(this, new(nameof(BodyHeight)));
            PropertyChanged?.Invoke(this, new(nameof(BodyRadius)));
            PropertyChanged?.Invoke(this, new(nameof(EllipseMargins)));
            PropertyChanged?.Invoke(this, new(nameof(PinPoints)));
            PropertyChanged?.Invoke(this, new(nameof(UC_Width)));
            PropertyChanged?.Invoke(this, new(nameof(UC_Height)));
            PropertyChanged?.Invoke(this, new(nameof(FontSizze)));
            PropertyChanged?.Invoke(this, new(nameof(ImageMargins)));

            PropertyChanged?.Invoke(this, new("ButtonSize"));
        }

        /*
         * Обработка соединений
         */

        protected JoinedItems?[] joins;

        public void AddJoin(JoinedItems join) {
            if (join.A.parent == this) {
                int n = join.A.num;
                joins[n]?.Delete();
                joins[n] = join;
            }
            if (join.B.parent == this) {
                int n = join.B.num;
                joins[n]?.Delete();
                joins[n] = join;
            }
            skip_upd = false;
        }

        public void RemoveJoin(JoinedItems join) {
            try {
                if (join.A.parent == this) joins[join.A.num] = null;
                if (join.B.parent == this) joins[join.B.num] = null;
            } catch { }
            skip_upd = false;
        }

        public void UpdateJoins(bool global) {
            foreach (var join in joins)
                if (join != null && (!global || join.A.parent == this)) join.Update();
        }

        public void ClearJoins() {
            foreach (var join in joins) join?.Delete();
        }

        public void SetJoinColor(int o_num, bool value) {
            var join = joins[o_num + CountIns];
            if (join != null)
                Dispatcher.UIThread.InvokeAsync(() => { // Ох, знакомая головная боль с андроида, где даже Toast за пределами главного потока не вызовешь :/ XD :D
                    join.line.Stroke = value ? Brushes.Lime : Brushes.DarkGray;
                });
        }

        /*
         * Обработка пинов
         */

        public Distantor GetPin(Ellipse finded, Visual? ref_point) {
            int n = 0;
            foreach (var pin in pins) {
                if (pin == finded) return new(GetSelfI, n, ref_point, (string?) finded.Tag ?? "");
                n++;
            }
            throw new Exception("Так не бывает");
        }

        public Point GetPinPos(int n, Visual? ref_point) {
            if (n >= pins.Length) n = 0;
            var pin = pins[n];
            return pin.Center(ref_point); // Смотрите Utils ;'-} Там круто сделан метод
        }

        /*
         * Мозги
         */

        bool skip_upd = true;
        public void LogicUpdate(Dictionary<IGate, Meta> ids, Meta me) {
            if (skip_upd) return;
            skip_upd = true;

            int ins = CountIns;
            for (int i = 0; i < ins; i++) {
                var join = joins[i];
                if (join == null) { me.ins[i] = 0; continue; }

                if (join.A.parent == this) {
                    var item = join.B;
                    if (item.tag == "Out" || item.tag == "IO") {
                        var p = item.parent;
                        Meta meta = ids[p];
                        int n = item.num - p.CountIns;
                        me.ins[i] = n >= meta.outs.Length ? 0 : meta.outs[n];
                    }
                }
                if (join.B.parent == this) {
                    var item = join.A;
                    if (item.tag == "Out" || item.tag == "IO") {
                        var p = item.parent;
                        Meta meta = ids[p];
                        int n = item.num - p.CountIns;
                        me.ins[i] = n >= meta.outs.Length ? 0 : meta.outs[n];
                    }
                }
            }
        }

        /*
         * Экспорт, но может быть прокачан в дочернем классе, если есть что добавить
         */

        public abstract int TypeId { get; }

        public virtual object Export() {
            return new Dictionary<string, object> {
                ["id"] = TypeId,
                ["pos"] = GetPos(),
                ["size"] = GetBodySize()
            };
        }

        public List<object[]> ExportJoins(Dictionary<IGate, int> to_num) {
            List<object[]> res = new();
            int n = 0, ins = CountIns;
            foreach (var join in joins) {
                if (++n > ins) break;
                if (join == null) continue;
                Distantor a = join.A, b = join.B;
                res.Add(new object[] {
                    to_num[a.parent], a.num, a.tag,
                    to_num[b.parent], b.num, b.tag,
                });
            }
            return res;
        }

        public virtual void Import(Dictionary<string, object> dict) {
            if (!@dict.TryGetValue("pos", out var @value)) { Log.Write("pos-запись элемента не обнаружен"); return; }
            if (@value is not Point @pos) { Log.Write("Неверный тип pos-записи элемента: " + @value); return; }
            Move(@pos);

            if (!@dict.TryGetValue("size", out var @value2)) { Log.Write("size-запись элемента не обнаружен"); return; }
            if (@value2 is not Size @size) { Log.Write("Неверный тип size-записи элемента: " + @value2); return; }
            Resize(@size, false);
        }

        public Ellipse SecretGetPin(int n) => pins[n];
    }
}
