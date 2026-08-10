using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Task
{
    public class Management
    {
        public List<Data> TaskList { get; private set; }

        public Management()
        {
            TaskList = new List<Data>();
        }

        public void AddTask(Data newTask)
        {
            TaskList.Add(newTask);
        }

        public void ClearDoneTasks()
        {
            TaskList.RemoveAll(t => t.Status == "Done");
        }

        public void UpdateTaskStatus(Data task, string newStatus)
        {
            if (task != null)
            {
                task.Status = newStatus;
            }
        }

        /// <summary>
        /// Salveaza toata lista curenta de task-uri cu ajutorul functiei ToSaveString -> formateaza datele ca sa le avem separate prin |
        /// </summary>
        public void SaveToFile(string filePath)
        {
            List<string> linesToSave = new List<string>();

            foreach (var task in TaskList)
            {
                linesToSave.Add(task.ToSaveString());
            }

            File.WriteAllLines(filePath, linesToSave);
        }

        /// <summary>
        /// Citeste fisierul text de pe disc, reconstruieste obiectele de tip Data si le incarca in memoria aplicatiei la Start-up
        /// </summary>
        public void LoadFromFile(string filePath)
        {
            TaskList.Clear(); // Golesc lista ca sa nu duplic datele;

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    string[] fragments = line.Split('|');

                    if (fragments.Length == 4)
                    {
                        string status = fragments[0];
                        string name = fragments[1];
                        string description = fragments[2];
                        string dataText = fragments[3];

                        if (DateTime.TryParseExact(dataText, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime deadline))
                            {
                            Data loadedTask = new Data(name, description, deadline);
                            loadedTask.Status = status;
                            TaskList.Add(loadedTask);
                        }
                    }
                }
            }
        }
    }
}
