using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicSimulator.Models {
    public class Project: IComparable {
        public string Name { get; private set; }
        public long Created;
        public long Modified;

        public List<Scheme> schemes = new();
        public List<string> scheme_files = new();
        public string FileName { get; }

        public Project() { // Новый проект
            Name = "Новый проект";
            Created = Modified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            FileName = FileHandler.GetProjectFileName();
            CreateScheme();
        }

        public Project(string fileName, object data) { // Импорт
            FileName = fileName;

            if (data is not Dictionary<string, object> dict) throw new Exception("Ожидался словарь в корне проекта");
            
            if (!dict.TryGetValue("name", out var value)) throw new Exception("В проекте нет имени");
            if (value is not string name) throw new Exception("Тип имени проекта - не строка");
            Name = name;

            if (!dict.TryGetValue("created", out var value2)) throw new Exception("В проекте нет времени создания");
            if (value2 is not int create_t) throw new Exception("Время создания проекта - не строка");
            Created = create_t;

            if (!dict.TryGetValue("modified", out var value3)) throw new Exception("В проекте нет времени изменения");
            if (value3 is not int mod_t) throw new Exception("Время изменения проекта - не строка");
            Modified = mod_t;

            if (!dict.TryGetValue("schemes", out var value4)) throw new Exception("В проекте нет списка схем");
            if (value4 is not List<object> arr) throw new Exception("Списко схем проекта - не массив строк");
            foreach (var file in arr) {
                if (file is not string str) throw new Exception("Одно из файловых имёт списка схем проекта - не строка");
                scheme_files.Add(str);
            }
        }



        public Scheme CreateScheme() {
            var scheme = new Scheme(this);
            schemes.Add(scheme);
            scheme.Save();
            scheme_files.Add(scheme.FileName);
            Save();
            return scheme;
        }

        bool loaded = false;
        private void LoadSchemes() {
            if (loaded) return;
            foreach (var fileName in scheme_files) {
                var scheme = FileHandler.LoadScheme(this, fileName);
                if (scheme != null) schemes.Add(scheme);
            }
            loaded = true;
        }
        public Scheme GetFirstCheme() {
            LoadSchemes();
            return schemes[0];
        }



        public object Export() {
            return new Dictionary<string, object> {
                ["name"] = Name,
                ["created"] = Created,
                ["modified"] = Modified,
                ["schemes"] = schemes.Select(x => x.FileName).ToArray(),
            };
        }

        public void Save() => FileHandler.SaveProject(this);

        public int CompareTo(object? obj) {
            if (obj is not Project proj) throw new ArgumentNullException(nameof(obj));
            return (int)(proj.Modified - Modified); // Не поддерживает long :///
        }

        public override string ToString() {
            return Name + "\nИзменён: " + Modified.UnixTimeStampToString() + "\nСоздан: " + Created.UnixTimeStampToString();
        }

        internal void ChangeName(string name) {
            Name = name;
            Modified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Save();
        }
    }
}
