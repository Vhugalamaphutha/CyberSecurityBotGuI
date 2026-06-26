using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace CyberSecurityBotGuI
{
    public partial class TaskManagerForm : Form
    {
        private TaskManager taskManager;
        private ListBox lstTasks;
        private TextBox txtTitle;
        private TextBox txtDescription;
        private DateTimePicker dtpReminder;
        private Button btnAdd;
        private Button btnComplete;
        private Button btnDelete;
        private Button btnRefresh;

        public TaskManagerForm()
        {
            InitializeComponent();   // from the designer file
            SetupUI();               // build controls programmatically
            taskManager = new TaskManager();
            RefreshTasks();
        }

        private void SetupUI()
        {
            this.Text = "📋 Task Manager - Cybersecurity";
            this.Size = new Size(750, 580);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 25, 47);

            // Title label & textbox
            Label lblTitle = new Label();
            lblTitle.Text = "Task Title:";
            lblTitle.Location = new Point(20, 20);
            lblTitle.Size = new Size(100, 25);
            lblTitle.ForeColor = Color.White;

            txtTitle = new TextBox();
            txtTitle.Location = new Point(130, 20);
            txtTitle.Size = new Size(300, 30);
            txtTitle.BackColor = Color.FromArgb(30, 42, 71);
            txtTitle.ForeColor = Color.White;

            // Description label & textbox
            Label lblDesc = new Label();
            lblDesc.Text = "Description:";
            lblDesc.Location = new Point(20, 60);
            lblDesc.Size = new Size(100, 25);
            lblDesc.ForeColor = Color.White;

            txtDescription = new TextBox();
            txtDescription.Location = new Point(130, 60);
            txtDescription.Size = new Size(450, 30);
            txtDescription.BackColor = Color.FromArgb(30, 42, 71);
            txtDescription.ForeColor = Color.White;

            // Reminder date picker
            Label lblReminder = new Label();
            lblReminder.Text = "Reminder:";
            lblReminder.Location = new Point(20, 100);
            lblReminder.Size = new Size(100, 25);
            lblReminder.ForeColor = Color.White;

            dtpReminder = new DateTimePicker();
            dtpReminder.Location = new Point(130, 100);
            dtpReminder.Size = new Size(250, 30);
            dtpReminder.Format = DateTimePickerFormat.Custom;
            dtpReminder.CustomFormat = "yyyy-MM-dd HH:mm";

            // Add Task button
            btnAdd = new Button();
            btnAdd.Text = "➕ Add Task";
            btnAdd.Location = new Point(20, 150);
            btnAdd.Size = new Size(120, 40);
            btnAdd.BackColor = Color.FromArgb(100, 255, 218);
            btnAdd.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnAdd.Click += BtnAdd_Click;

            // ListBox to display tasks
            lstTasks = new ListBox();
            lstTasks.Location = new Point(20, 200);
            lstTasks.Size = new Size(700, 280);
            lstTasks.BackColor = Color.FromArgb(17, 34, 64);
            lstTasks.ForeColor = Color.White;
            lstTasks.Font = new Font("Consolas", 10);

            // Complete button
            btnComplete = new Button();
            btnComplete.Text = "✅ Complete";
            btnComplete.Location = new Point(20, 495);
            btnComplete.Size = new Size(120, 40);
            btnComplete.BackColor = Color.FromArgb(30, 42, 71);
            btnComplete.ForeColor = Color.LightGreen;
            btnComplete.Click += BtnComplete_Click;

            // Delete button
            btnDelete = new Button();
            btnDelete.Text = "🗑️ Delete";
            btnDelete.Location = new Point(150, 495);
            btnDelete.Size = new Size(120, 40);
            btnDelete.BackColor = Color.FromArgb(30, 42, 71);
            btnDelete.ForeColor = Color.LightCoral;
            btnDelete.Click += BtnDelete_Click;

            // Refresh button
            btnRefresh = new Button();
            btnRefresh.Text = "🔄 Refresh";
            btnRefresh.Location = new Point(280, 495);
            btnRefresh.Size = new Size(120, 40);
            btnRefresh.BackColor = Color.FromArgb(30, 42, 71);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.Click += (s, e) => RefreshTasks();

            // Add all controls to the form
            this.Controls.Add(lblTitle);
            this.Controls.Add(txtTitle);
            this.Controls.Add(lblDesc);
            this.Controls.Add(txtDescription);
            this.Controls.Add(lblReminder);
            this.Controls.Add(dtpReminder);
            this.Controls.Add(btnAdd);
            this.Controls.Add(lstTasks);
            this.Controls.Add(btnComplete);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnRefresh);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Please enter a task title.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string desc = txtDescription.Text.Trim();
            DateTime? reminder = dtpReminder.Checked ? dtpReminder.Value : (DateTime?)null;

            if (taskManager.AddTask(title, desc, reminder))
            {
                MessageBox.Show("✅ Task added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtTitle.Clear();
                txtDescription.Clear();
                RefreshTasks();
            }
            else
            {
                MessageBox.Show("⚠️ Error adding task. Make sure MySQL is running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            if (lstTasks.SelectedItem == null)
            {
                MessageBox.Show("Please select a task to complete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int id = ExtractId(lstTasks.SelectedItem.ToString());
            if (id > 0)
            {
                taskManager.CompleteTask(id);
                RefreshTasks();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (lstTasks.SelectedItem == null)
            {
                MessageBox.Show("Please select a task to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int id = ExtractId(lstTasks.SelectedItem.ToString());
            if (id > 0 && MessageBox.Show("Delete this task?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                taskManager.DeleteTask(id);
                RefreshTasks();
            }
        }

        private int ExtractId(string taskText)
        {
            try
            {
                string idPart = taskText.Substring(0, taskText.IndexOf('.'));
                return int.Parse(idPart);
            }
            catch { return -1; }
        }

        private void RefreshTasks()
        {
            lstTasks.Items.Clear();
            var tasks = taskManager.GetTasks();
            foreach (var task in tasks)
            {
                string status = task.IsCompleted ? "✅" : "⏳";
                string reminder = task.ReminderDate.HasValue ? $" ⏰ {task.ReminderDate.Value:yyyy-MM-dd HH:mm}" : "";
                lstTasks.Items.Add($"{task.Id}. {status} {task.Title} - {task.Description}{reminder}");
            }
            if (tasks.Count == 0)
                lstTasks.Items.Add("📋 No tasks yet. Add one above!");
        }
    }
}