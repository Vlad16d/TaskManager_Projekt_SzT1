using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace TodoApp
{
    public static class TaskManager
    {
        private static readonly string filePath = "tasks.json";

        public static List<TaskItem> LoadTasks()
        {
            if (!File.Exists(filePath))
                return new List<TaskItem>();

            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<TaskItem>>(json) ?? new List<TaskItem>();
        }

        public static void SaveTasks(List<TaskItem> tasks)
        {
            var json = JsonConvert.SerializeObject(tasks, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}
