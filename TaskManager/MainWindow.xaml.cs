
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Task;

namespace TaskManager
{
    public partial class MainWindow : Window
    {
        private Management taskManager = new Task.Management();

        /// <summary>
        /// CTOR-ul ferestrei principale
        /// Incarca datele din fisier, actualizeaza interfata si ataseaza handler-ul pentru randarea continutului
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            taskManager.LoadFromFile("tasks.txt");
            DisplayChanges();

            this.ContentRendered += IsWindowLoaded;
        }

        /// <summary>
        /// Handler apelat automat dupa ce elementele vizuale ale ferestrei au fost complet randate pe ecran;
        /// </summary>
        private void IsWindowLoaded(object sender, EventArgs e)
        {
            CheckForNotifications();
        }

        /// <summary>
        /// Evalueaza termenele limita ale task-urilor nefinalizate si afiseaza mesaje de avertizare daca sunt OVERDUE / DUE TODAY
        /// </summary>
        private void CheckForNotifications()
        {
            List<string> tasksDueToday = new List<string>();
            List<string> overDueTasks = new List<string>();

            foreach (var task in taskManager.TaskList)
            {
                if (task.Status == "Done")
                    continue;

                if (task.Deadline.Date == DateTime.Now.Date)
                    tasksDueToday.Add(task.DisplayName);
                else if (task.Deadline.Date < DateTime.Now.Date)
                    overDueTasks.Add(task.DisplayName);
            }

            string pluralization = string.Empty;

            if (tasksDueToday.Count > 0)
            {
                if (tasksDueToday.Count == 1) { pluralization = "is"; }
                else if (tasksDueToday.Count >= 2) { pluralization = "are"; }

                string taskNames = string.Join(", ", tasksDueToday);
                string message = $"The following task(s) {pluralization} due today:\n\n{taskNames}";
                MessageBox.Show(message, "Due Today");
            }

            if (overDueTasks.Count > 0)
            {
                if (overDueTasks.Count == 1) { pluralization = "is"; }
                else if (overDueTasks.Count >= 2) { pluralization = "are"; }

                string taskNames = string.Join(", ", overDueTasks);
                string message = $"The following task(s) {pluralization} OVERDUE:\n\n{taskNames}";
                MessageBox.Show(message, "Overdue Tasks");
            }
        }

        /// <summary>
        /// Afisez schimbarile, functie apelata la orice aditie / schimbare; verific de asemenea daca un task intra in categoria OVERDUE / DONE si este marcat pe fundal corespunzator;
        /// </summary>
        private void DisplayChanges()
        {
            ToDoListBox.Items.Clear();
            InProgressListBox.Items.Clear();
            DoneListBox.Items.Clear();

            foreach (var task in taskManager.TaskList)
            {
                string textDeAfisat = $"{task.DisplayName} (Until: {task.Deadline:dd/MM/yyyy})\n{task.Description}";

                ListBoxItem visualModifier = new ListBoxItem();
                visualModifier.Content = textDeAfisat;

                if (task.Deadline.Date < DateTime.Now.Date && task.Status != "Done")
                {
                    visualModifier.Background = Brushes.MistyRose;
                }

                if (task.Status == "To Do")
                {
                    ToDoListBox.Items.Add(visualModifier);
                }
                else if (task.Status == "In Progress")
                {
                    InProgressListBox.Items.Add(visualModifier);
                }
                else if (task.Status == "Done")
                {
                    visualModifier.Background = Brushes.Honeydew;
                    DoneListBox.Items.Add(visualModifier);
                }
            }
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskDatePicker.SelectedDate == null || string.IsNullOrWhiteSpace(TaskTitleTextBox.Text))
            {
                MessageBox.Show("No selected date & name");
                return;
            }

            DateTime deadline = TaskDatePicker.SelectedDate.Value;
            string shortName = TaskTitleTextBox.Text;
            string description = TaskDescTextBox.Text;

            Task.Data newTask = new Task.Data(shortName, description, deadline);
            taskManager.AddTask(newTask);

            DisplayChanges();

            TaskTitleTextBox.Clear();
            TaskDescTextBox.Clear();
            TaskDatePicker.SelectedDate = null;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            taskManager.SaveToFile("tasks.txt");
            MessageBox.Show("Save was successful");
        }

        private void ClearDoneButton_Click(object sender, RoutedEventArgs e)
        {
            taskManager.ClearDoneTasks();
            DisplayChanges();
            taskManager.SaveToFile("task.txt");
        }

        //Functia veche de drag and drop, nu o facea decat daca aveam un item selectat a.k.a alt click de mouse => slugish; acm e seamless;

        //private void ListBox_MouseDown(object sender, MouseButtonEventArgs e)

        //{

        //    ListBox sourceList = sender as ListBox;
        //    if (sourceList == null || sourceList.SelectedItem == null)
        //    {
        //        return;
        //    }
        //    string selectedItemText = sourceList.SelectedItem.ToString();
        //    DragDrop.DoDragDrop(sourceList, selectedItemText, DragDropEffects.Move);
        //}

        /// <summary>
        /// Drag & Drop -> ma uit la pixelul de la cursor si initiez "impachetarea"
        /// </summary>
        private void ListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ListBox sourceList = sender as ListBox;
            if (sourceList == null) return;

            DependencyObject chosenTask = e.OriginalSource as DependencyObject;
            ListBoxItem selectedRow = ItemsControl.ContainerFromElement(sourceList, chosenTask) as ListBoxItem;

            if (selectedRow == null)
            {
                return;
            }

            string selectedItemText = selectedRow.Content.ToString();
            DragDrop.DoDragDrop(sourceList, selectedItemText, DragDropEffects.Move);
        }

        /// <summary>
        /// Despachetez "pachetul" daca am container cu dropoff = true
        /// </summary>
        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat) == false)
            {
                return;
            }

            string selectedItemText = (string)e.Data.GetData(DataFormats.StringFormat);
            ListBox sourceList = sender as ListBox;

            string newStatus = string.Empty;
            switch (sourceList.Name) // verific in ce status trece task-ul selectat;
            {
                case "ToDoListBox":
                    newStatus = "To Do";
                    break;
                case "InProgressListBox":
                    newStatus = "In Progress";
                    break;
                case "DoneListBox":
                    newStatus = "Done";
                    break;
            }

            foreach (var task in taskManager.TaskList)
            {
                string textDeVerificat = $"{task.DisplayName} (Until: {task.Deadline:dd.MM.yyyy})\n{task.Description}";

                if (textDeVerificat == selectedItemText)
                {
                    task.Status = newStatus;
                    break;
                }
            }

            DisplayChanges();
        }
    }
}