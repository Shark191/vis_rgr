using LogicSimulator.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace LogicSimulator.Models {
    public class FileHandler {
        readonly static string dir = "../../../../storage/";
        readonly List<Project> projects = new();

        public FileHandler() {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            foreach (var fullname in Directory.EnumerateFiles(dir)) {
                var name = fullname.Split("/")[^1];
                if (name.StartsWith("proj_")) LoadProject(name);
            }
        }



        public static string GetProjectFileName() {
            int n = 0;
            while (true) {
                string name = "proj_" + ++n + ".json";
                if (!File.Exists(dir + name)) return name;
            }
        }
        public static string GetSchemeFileName() {
            int n = 0;
            while (true) {
                string name = "scheme_" + ++n + ".yaml";
                if (!File.Exists(dir + name)) return name;
            }
        }



        public Project CreateProject() {
            var proj = new Project();
            projects.Add(proj);
            return proj;
        }
        private Project? LoadProject(string fileName) {
            try {
                var obj = Utils.Json2obj(File.ReadAllText(dir + fileName)) ?? throw new DataException("Не верная структура JSON-файла проекта!");
                var proj = new Project(fileName, obj);
                projects.Add(proj);
                return proj;
            } catch (Exception e) { Log.Write("Неудачная попытка загрузить проект:\n" + e); }
            return null;
        }
        public static Scheme? LoadScheme(Project parent, string fileName) {
            try {
                var obj = Utils.Yaml2obj(File.ReadAllText(dir + fileName)) ?? throw new DataException("Не верная структура YAML-файла схемы!");
                var scheme = new Scheme(parent, fileName, obj);
                return scheme;
            } catch (Exception e) { Log.Write("Неудачная попытка загрузить схему:\n" + e); }
            return null;
        }



        public static void SaveProject(Project proj) {
            var data = Utils.Obj2json(proj.Export());
            if (!test_mode) File.WriteAllText(dir + proj.FileName, data);
        }
        public static void SaveScheme(Scheme scheme) {
            var data = Utils.Obj2yaml(scheme.Export());
            if (!test_mode) File.WriteAllText(dir + scheme.FileName, data);
        }

        public Project[] GetSortedProjects() {
            projects.Sort();
            return projects.ToArray();
        }

        public static bool test_mode = false;
    }
}
