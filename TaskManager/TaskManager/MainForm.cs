using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TodoApp
{
    public partial class MainForm : Form
    {
        private Timer themeTransitionTimer;
        private Color targetBackColor;
        private Color startBackColor;
        private int animationStep;
        private const int maxAnimationSteps = 30; // więcej maxAnimationSteps - płynnniej animacja

        private void ApplyTheme()
        {
            // określanie koloru tła na podstawie aktualnej temy
            targetBackColor = isDarkTheme ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
            startBackColor = this.BackColor;

            animationStep = 0;

            if (themeTransitionTimer == null)
            {
                themeTransitionTimer = new Timer();
                themeTransitionTimer.Interval = 30; // szybkość odświeżania
                themeTransitionTimer.Tick += ThemeTransitionTimer_Tick;
            }

            themeTransitionTimer.Start();
        }

        private void ThemeTransitionTimer_Tick(object sender, EventArgs e)
        {
            animationStep++;

            float progress = animationStep / (float)maxAnimationSteps;

            int r = (int)(startBackColor.R + (targetBackColor.R - startBackColor.R) * progress);
            int g = (int)(startBackColor.G + (targetBackColor.G - startBackColor.G) * progress);
            int b = (int)(startBackColor.B + (targetBackColor.B - startBackColor.B) * progress);

            this.BackColor = Color.FromArgb(r, g, b);

            if (animationStep >= maxAnimationSteps)
            {
               
                themeTransitionTimer.Stop();


                this.BackColor = targetBackColor;

                if (isDarkTheme)
                    DarkTheme.Apply(this);
                else
                    LightTheme.Apply(this);
            }
        }


        // ---

        private List<TaskItem> filteredTasks;
        private TextBox searchTextBox; 

        public MainForm()
        {
            SetupUI();
            LoadTasks();
            ApplyTheme();
            RefreshTaskList();

            this.Resize += MainForm_Resize;  // zmiana rozmiaru okna aplikacji
        }

        private ToolTip toolTip;

        private void MainForm_Resize(object sender, EventArgs e)
        {
            AdjustColumnWidths();
        }

        private void AdjustColumnWidths()
        {
            if (taskListView.Columns.Count < 2) return;

            int totalWidth = taskListView.ClientSize.Width;

            // Przykład proporcji: pierwsza kolumna — 75%, druga — 25%
            int firstColWidth = (int)(totalWidth * 0.75);
            int secondColWidth = totalWidth - firstColWidth;

            // minimalna szerokość kolumn
            firstColWidth = Math.Max(firstColWidth, 100);
            secondColWidth = Math.Max(secondColWidth, 30);

            taskListView.Columns[0].Width = firstColWidth;
            taskListView.Columns[1].Width = secondColWidth;
        }

        private List<TaskItem> tasks;
        private ListView taskListView;
        private Label statsLabel;
        private bool isDarkTheme = true;
        private TableLayoutPanel buttonPanel;

        private float currentFontSize = 10f; // początkowy rozmiar czcionki (шрифта)
        private const float minFontSize = 6f;
        private const float maxFontSize = 24f;

        private void SetupUI()
        {
            Text = "Task Manager";
            Width = 600;
            Height = 550;

            // tworzenie pola wyszukiwania
            searchTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                Margin = new Padding(5),
                Font = new Font(FontFamily.GenericSansSerif, 10f),
                
            };
            searchTextBox.TextChanged += (s, e) => ApplySearchFilter();
            Controls.Add(searchTextBox);
            searchTextBox.BringToFront();

            // Lista zadań rozciąga sie na całą przestrzeń z wyjątkiem dolnego paska
            taskListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                AllowDrop = true,
                Dock = DockStyle.Fill,
            };
            taskListView.Columns.Add("Task", 450);
            taskListView.Columns.Add("Done", 80);
            AdjustColumnWidths();
            taskListView.ItemDrag += TaskListView_ItemDrag;
            taskListView.DragEnter += TaskListView_DragEnter;
            taskListView.DragDrop += TaskListView_DragDrop;
            Controls.Add(taskListView);

            buttonPanel = new TableLayoutPanel
            {
                RowCount = 2,
                ColumnCount = 4,
                Dock = DockStyle.Bottom,
                Height = 80,
                AutoSize = true
            };
            buttonPanel.ColumnStyles.Clear();
            for (int i = 0; i < 4; i++)
            {
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            }

            var addButton = new Button { Text = "Add", Width = 100, Anchor = AnchorStyles.None };
            var editButton = new Button { Text = "Edit", Width = 100, Anchor = AnchorStyles.None };
            var deleteButton = new Button { Text = "Delete", Width = 100, Anchor = AnchorStyles.None };
            var doneButton = new Button { Text = "Toggle Done", Width = 100, Anchor = AnchorStyles.None };
            var sortAZButton = new Button { Text = "Sort A-Z", Width = 100, Anchor = AnchorStyles.None };
            var sortDoneButton = new Button { Text = "Sort Done", Width = 100, Anchor = AnchorStyles.None };
            var themeButton = new Button { Text = "Toggle Theme", Width = 100, Anchor = AnchorStyles.None };
            statsLabel = new Label { Text = "Stats: 0 tasks (0 done)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };

            toolTip = new ToolTip();

            // opóżnienie w wyświetlaniu wszkazówek
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 200;
            toolTip.ShowAlways = true;

            // wszkazówki dla przycisków
            toolTip.SetToolTip(addButton, "Dodać nowe zadanie");
            toolTip.SetToolTip(editButton, "Redagować wybrane zadanie");
            toolTip.SetToolTip(deleteButton, "Usunąć wybrane zadanie");
            toolTip.SetToolTip(doneButton, "Oznaczenie zadania jako wykonane/niewykonane");
            toolTip.SetToolTip(sortAZButton, "Sortowanie zadań alfabetycznie");
            toolTip.SetToolTip(sortDoneButton, "Sortowanie zadań według statusu wykonania");
            toolTip.SetToolTip(themeButton, "Dark mode / Light mode");


            buttonPanel.Controls.Add(addButton, 0, 0);
            buttonPanel.Controls.Add(editButton, 1, 0);
            buttonPanel.Controls.Add(deleteButton, 2, 0);
            buttonPanel.Controls.Add(doneButton, 3, 0);
            buttonPanel.Controls.Add(sortAZButton, 0, 1);
            buttonPanel.Controls.Add(sortDoneButton, 1, 1);
            buttonPanel.Controls.Add(themeButton, 2, 1);
            buttonPanel.Controls.Add(statsLabel, 3, 1);

            Controls.Add(buttonPanel);

            addButton.Click += (s, e) => AddTask();
            editButton.Click += (s, e) => EditTask();
            deleteButton.Click += (s, e) => DeleteTask();
            doneButton.Click += (s, e) => ToggleTaskDone();
            sortAZButton.Click += (s, e) => SortTasksAZ();
            sortDoneButton.Click += (s, e) => SortTasksByDone();
            themeButton.Click += (s, e) => ToggleTheme();

            taskListView.MouseWheel += TaskListView_MouseWheel;
        }

        private void TaskListView_MouseWheel(object sender, MouseEventArgs e)
        {
            // Skalowanie za pomocą kółka myszy + Ctrl
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                if (e.Delta > 0)
                {
                    currentFontSize = Math.Min(maxFontSize, currentFontSize + 1);
                }
                else
                {
                    currentFontSize = Math.Max(minFontSize, currentFontSize - 1);
                }

                taskListView.Font = new Font(taskListView.Font.FontFamily, currentFontSize);
            }
        }

        private void LoadTasks()
        {
            tasks = TaskManager.LoadTasks();
            ApplySearchFilter();
        }

        private void SaveTasks()
        {
            TaskManager.SaveTasks(tasks);
        }

        private void RefreshTaskList()
        {
            taskListView.Items.Clear();

            var listToShow = filteredTasks ?? tasks;

            foreach (var task in listToShow)
            {
                var item = new ListViewItem(task.Title);
                item.SubItems.Add(task.IsDone ? "✔" : "");
                item.Tag = task;
                taskListView.Items.Add(item);
            }
            statsLabel.Text = $"Stats: {listToShow.Count} tasks ({listToShow.Count(t => t.IsDone)} done)";
            SaveTasks();
        }

        private void ApplySearchFilter()
        {
            if (string.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                filteredTasks = null;
            }
            else
            {
                var query = searchTextBox.Text.Trim();
                filteredTasks = tasks.Where(t => t.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            RefreshTaskList();
        }

        private void AddTask()
        {
            string input = Prompt.ShowDialog("Task name:", "Add Task");
            if (!string.IsNullOrWhiteSpace(input))
            {
                tasks.Add(new TaskItem { Title = input, IsDone = false });
                ApplySearchFilter();
            }
        }

        private void EditTask()
        {
            if (taskListView.SelectedItems.Count == 0) return;
            var task = (TaskItem)taskListView.SelectedItems[0].Tag;
            string input = Prompt.ShowDialog("Edit task:", "Edit", task.Title);
            if (!string.IsNullOrWhiteSpace(input))
            {
                task.Title = input;
                ApplySearchFilter();
            }
        }

        private void DeleteTask()
        {
            if (taskListView.SelectedItems.Count == 0) return;
            var task = (TaskItem)taskListView.SelectedItems[0].Tag;
            tasks.Remove(task);
            ApplySearchFilter();
        }

        private void ToggleTaskDone()
        {
            if (taskListView.SelectedItems.Count == 0) return;
            var task = (TaskItem)taskListView.SelectedItems[0].Tag;
            task.IsDone = !task.IsDone;
            ApplySearchFilter();
        }

        private void SortTasksAZ()
        {
            tasks = tasks.OrderBy(t => t.Title).ToList();
            ApplySearchFilter();
        }

        private void SortTasksByDone()
        {
            tasks = tasks.OrderByDescending(t => t.IsDone).ThenBy(t => t.Title).ToList();
            ApplySearchFilter();
        }

        private void TaskListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            taskListView.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void TaskListView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListViewItem)))
                e.Effect = DragDropEffects.Move;
        }

        private void TaskListView_DragDrop(object sender, DragEventArgs e)
        {
            Point cp = taskListView.PointToClient(new Point(e.X, e.Y));
            ListViewItem dragToItem = taskListView.GetItemAt(cp.X, cp.Y);
            if (dragToItem == null) return;

            ListViewItem dragItem = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
            int oldIndex = dragItem.Index;
            int newIndex = dragToItem.Index;
            if (oldIndex == newIndex) return;

            var movedItem = tasks[oldIndex];
            tasks.RemoveAt(oldIndex);
            tasks.Insert(newIndex, movedItem);
            RefreshTaskList();
        }

        private void ToggleTheme()
        {
            isDarkTheme = !isDarkTheme;
            ApplyTheme();
        }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption, string defaultText = "")
        {
            Form prompt = new Form
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label { Left = 20, Top = 20, Text = text, Width = 350 };
            TextBox inputBox = new TextBox { Left = 20, Top = 50, Width = 350, Text = defaultText };
            Button confirmation = new Button { Text = "OK", Left = 280, Width = 90, Top = 80 };
            confirmation.Click += (sender, e) => { prompt.DialogResult = DialogResult.OK; prompt.Close(); };
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(inputBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? inputBox.Text : "";
        }
    }

    public static class LightTheme
    {
        public static void Apply(Control control)
        {
            control.BackColor = SystemColors.Control;
            control.ForeColor = Color.Black;
            foreach (Control c in control.Controls)
            {
                Apply(c);
            }
        }
    }


}
