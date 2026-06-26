using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace CyberSecurityBotGuI
{
    public class TaskManager
    {
       
        private string connectionString = "server=localhost;database=cyberbot;uid=root;pwd=vhugala;";

        public TaskManager()
        {
            CreateDatabase();
        }

        private void CreateDatabase()
        {
            try
            {
                // ============ FIXED: Connection String ============
                using (MySqlConnection conn = new MySqlConnection("server=localhost;uid=root;pwd=vhugala;"))
                {
                    conn.Open();
                    string createDb = "CREATE DATABASE IF NOT EXISTS cyberbot;";
                    MySqlCommand cmd = new MySqlCommand(createDb, conn);
                    cmd.ExecuteNonQuery();
                }

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string createTable = @"CREATE TABLE IF NOT EXISTS tasks (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        title VARCHAR(100) NOT NULL,
                        description TEXT,
                        reminder_date DATETIME,
                        is_completed BOOLEAN DEFAULT FALSE,
                        created_at DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";
                    MySqlCommand cmd = new MySqlCommand(createTable, conn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}\nMake sure MySQL is installed and running.", "Database Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public bool AddTask(string title, string description, DateTime? reminderDate = null)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO tasks (title, description, reminder_date) 
                                     VALUES (@title, @desc, @reminder)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@desc", description ?? "");
                    cmd.Parameters.AddWithValue("@reminder", reminderDate.HasValue ? reminderDate.Value : (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding task: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public List<Task> GetTasks()
        {
            List<Task> tasks = new List<Task>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM tasks ORDER BY created_at DESC";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        int idOrdinal = reader.GetOrdinal("id");
                        int titleOrdinal = reader.GetOrdinal("title");
                        int descOrdinal = reader.GetOrdinal("description");
                        int reminderOrdinal = reader.GetOrdinal("reminder_date");
                        int completedOrdinal = reader.GetOrdinal("is_completed");
                        int createdOrdinal = reader.GetOrdinal("created_at");

                        while (reader.Read())
                        {
                            tasks.Add(new Task
                            {
                                Id = reader.GetInt32(idOrdinal),
                                Title = reader.GetString(titleOrdinal),
                                Description = reader.IsDBNull(descOrdinal) ? "" : reader.GetString(descOrdinal),
                                ReminderDate = reader.IsDBNull(reminderOrdinal) ? (DateTime?)null : reader.GetDateTime(reminderOrdinal),
                                IsCompleted = reader.GetBoolean(completedOrdinal),
                                CreatedAt = reader.GetDateTime(createdOrdinal)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting tasks: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return tasks;
        }

        public bool CompleteTask(int id)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE tasks SET is_completed = TRUE WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteTask(int id)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM tasks WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}