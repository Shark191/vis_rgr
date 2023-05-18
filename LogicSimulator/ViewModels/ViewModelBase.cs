using LogicSimulator.Models;
using ReactiveUI;

namespace LogicSimulator.ViewModels {
    public class ViewModelBase: ReactiveObject {
        public readonly static Mapper map = new();
        protected static Project? current_proj;

        public static Project? TopSecretGetProj() => current_proj;
    }
}