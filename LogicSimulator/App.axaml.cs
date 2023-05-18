using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LogicSimulator.Views;
using System.IO;

namespace LogicSimulator {
    public partial class App: Application {
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = new LauncherWindow();

            base.OnFrameworkInitializationCompleted();
            IncrementBuildNum();
        }

        private static void IncrementBuildNum() {
            string path = "../../../../build.num";
            int num;
            try { num = int.Parse(File.ReadAllText(path)); }
            catch (FileNotFoundException) { num = 0; }
            num++;
            File.WriteAllText(path, num.ToString());
        }
    }
}