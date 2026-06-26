using System;
using System.Drawing;
using System.Windows.Forms;

namespace CyberSecurityBotGuI
{
    public partial class Form1 : Form
    {
        // Declare the chatbot logic object
        private ChatBot bot;

        // UI controls
        private TextBox txtInput;
        private Button btnSend;
        private RichTextBox txtChat;
        private Button btnVoice;
        private Button btnClear;
        private Button btnQuiz;
        private Button btnTasks;

        public Form1()
        {
            InitializeComponent();
            SetupUI();

            // Create the chatbot logic instance
            bot = new ChatBot();
            bot.MessageReceived += Bot_MessageReceived;
            bot.OpenQuizRequested += Bot_OpenQuizRequested;
            bot.OpenTaskManagerRequested += Bot_OpenTaskManagerRequested;

            // Display ASCII art and welcome messages
            DisplayAsciiArt();
            bot.SendBotMessage("Welcome to Cybersecurity Awareness Bot!");
            bot.SendBotMessage("I can answer questions about password safety, phishing, safe browsing, and more.");
            bot.SendBotMessage("Type 'exit' to end the conversation.");
            bot.SendBotMessage("📋 Try: 'add task', 'start quiz', or 'show activity log'");

            // Play voice greeting if the audio file exists (optional)
            try
            {
                bot.PlayVoiceGreeting();
            }
            catch
            {
                // If file missing, silently ignore – not critical
            }
        }

        private void SetupUI()
        {
            // Set the window title and appearance
            this.Text = "Cybersecurity Awareness Bot";
            this.Size = new Size(700, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 25, 47);

            // Chat display area (RichTextBox)
            txtChat = new RichTextBox();
            txtChat.Location = new Point(12, 12);
            txtChat.Size = new Size(660, 400);
            txtChat.ReadOnly = true;
            txtChat.BackColor = Color.FromArgb(17, 34, 64);
            txtChat.ForeColor = Color.FromArgb(204, 214, 246);
            txtChat.Font = new Font("Consolas", 10);
            txtChat.BorderStyle = BorderStyle.FixedSingle;

            // Input text box
            txtInput = new TextBox();
            txtInput.Location = new Point(12, 425);
            txtInput.Size = new Size(540, 30);
            txtInput.BackColor = Color.White;
            txtInput.ForeColor = Color.Black;
            txtInput.Font = new Font("Segoe UI", 11);
            txtInput.Enabled = true;
            txtInput.ReadOnly = false;
            txtInput.TabIndex = 0;
            txtInput.Text = "";

            // Send button
            btnSend = new Button();
            btnSend.Text = "Send";
            btnSend.Location = new Point(562, 423);
            btnSend.Size = new Size(110, 35);
            btnSend.BackColor = Color.FromArgb(100, 255, 218);
            btnSend.ForeColor = Color.FromArgb(10, 25, 47);
            btnSend.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnSend.FlatStyle = FlatStyle.Flat;

            // Voice button
            btnVoice = new Button();
            btnVoice.Text = "🔊 Play Greeting";
            btnVoice.Location = new Point(12, 470);
            btnVoice.Size = new Size(140, 35);
            btnVoice.BackColor = Color.FromArgb(30, 42, 71);
            btnVoice.ForeColor = Color.FromArgb(100, 255, 218);
            btnVoice.FlatStyle = FlatStyle.Flat;
            btnVoice.Click += (s, e) => { try { bot.PlayVoiceGreeting(); } catch { } };

            // Clear button
            btnClear = new Button();
            btnClear.Text = "🧹 Clear Chat";
            btnClear.Location = new Point(160, 470);
            btnClear.Size = new Size(140, 35);
            btnClear.BackColor = Color.FromArgb(30, 42, 71);
            btnClear.ForeColor = Color.FromArgb(255, 107, 107);
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.Click += (s, e) => { txtChat.Clear(); DisplayAsciiArt(); };

            // ============ NEW: Quiz Button (Task 2) ============
            btnQuiz = new Button();
            btnQuiz.Text = "🎯 Play Quiz";
            btnQuiz.Location = new Point(310, 470);
            btnQuiz.Size = new Size(140, 35);
            btnQuiz.BackColor = Color.FromArgb(30, 42, 71);
            btnQuiz.ForeColor = Color.FromArgb(255, 215, 0);
            btnQuiz.FlatStyle = FlatStyle.Flat;
            btnQuiz.Click += (s, e) => OpenQuiz();

            // ============ NEW: Tasks Button (Task 1) ============
            btnTasks = new Button();
            btnTasks.Text = "📋 Manage Tasks";
            btnTasks.Location = new Point(460, 470);
            btnTasks.Size = new Size(140, 35);
            btnTasks.BackColor = Color.FromArgb(30, 42, 71);
            btnTasks.ForeColor = Color.FromArgb(100, 255, 218);
            btnTasks.FlatStyle = FlatStyle.Flat;
            btnTasks.Click += (s, e) => OpenTaskManager();

            // Add all controls to the form
            this.Controls.Add(txtChat);
            this.Controls.Add(txtInput);
            this.Controls.Add(btnSend);
            this.Controls.Add(btnVoice);
            this.Controls.Add(btnClear);
            this.Controls.Add(btnQuiz);
            this.Controls.Add(btnTasks);

            // Wire up events
            btnSend.Click += (s, e) => SendMessage();
            txtInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SendMessage();
                    e.SuppressKeyPress = true;
                }
            };

            // Ensure the text box gets focus when the form is shown
            this.Shown += (s, e) => txtInput.Focus();
        }

        // ============ NEW: Open Quiz Method ============
        private void OpenQuiz()
        {
            try
            {
                QuizForm quiz = new QuizForm();
                quiz.ShowDialog();
                bot.LogAction("User completed the quiz");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening quiz: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============ NEW: Open Task Manager Method ============
        private void OpenTaskManager()
        {
            try
            {
                TaskManagerForm taskForm = new TaskManagerForm();
                taskForm.ShowDialog();
                bot.LogAction("User opened task manager");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening task manager: {ex.Message}\n\nMake sure MySQL is running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============ NEW: Event Handlers for Quiz/Task Requests from Bot ============
        private void Bot_OpenQuizRequested(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Bot_OpenQuizRequested(message)));
                return;
            }
            OpenQuiz();
        }

        private void Bot_OpenTaskManagerRequested(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Bot_OpenTaskManagerRequested(message)));
                return;
            }
            OpenTaskManager();
        }

        private void SendMessage()
        {
            string msg = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            AppendMessage("You", msg, Color.FromArgb(246, 241, 213));
            txtInput.Clear();
            txtInput.Focus();

            bot.StartChat(msg);
        }

        private void Bot_MessageReceived(string sender, string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Bot_MessageReceived(sender, message)));
                return;
            }

            Color color = (sender == "Bot") ? Color.FromArgb(100, 255, 218) : Color.FromArgb(100, 255, 218);
            AppendMessage(sender, message, color);
        }

        private void AppendMessage(string sender, string message, Color color)
        {
            txtChat.SelectionStart = txtChat.TextLength;
            txtChat.SelectionColor = color;
            txtChat.AppendText($"{sender}: {message}\r\n\r\n");
            txtChat.ScrollToCaret();
        }

        private void DisplayAsciiArt()
        {
            string art = bot.GetAsciiArt();
            AppendMessage("System", art, Color.FromArgb(100, 255, 218));
        }
    }
}