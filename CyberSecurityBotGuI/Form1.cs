using System;
using System.Drawing;
using System.Windows.Forms;

// No need for "using CyberSecurityBotGUI;" because ChatBot is in the same namespace
// (your namespace is CyberSecurityBotGuI, which already contains ChatBot)

namespace CyberSecurityBotGuI
{
    public partial class Form1 : Form
    {
        // Declare the chatbot logic object
        private ChatBot bot;

        // UI controls
        private TextBox txtInput;      // where user types messages
        private Button btnSend;        // button to send a message
        private RichTextBox txtChat;   // area that displays conversation history

        public Form1()
        {
            // Call the designer-generated InitializeComponent (if any)
            // If you have no designer controls, you can comment it out.
            InitializeComponent();

            // Set the window title and appearance
            this.Text = "Cybersecurity Awareness Bot";
            this.Size = new Size(650, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 25, 47); // dark blue theme

            // Create the chat display area (RichTextBox)
            txtChat = new RichTextBox();
            txtChat.Location = new Point(12, 12);
            txtChat.Size = new Size(610, 400);
            txtChat.ReadOnly = true;                     // user cannot edit chat history
            txtChat.BackColor = Color.FromArgb(17, 34, 64);
            txtChat.ForeColor = Color.FromArgb(204, 214, 246);
            txtChat.Font = new Font("Consolas", 10);
            txtChat.BorderStyle = BorderStyle.FixedSingle;

            // Create the input text box – where user types
            txtInput = new TextBox();
            txtInput.Location = new Point(12, 425);
            txtInput.Size = new Size(500, 30);
            txtInput.BackColor = Color.White;            // white background for visibility
            txtInput.ForeColor = Color.Black;            // black text
            txtInput.Font = new Font("Segoe UI", 11);
            txtInput.Enabled = true;                     // must be true to allow typing
            txtInput.ReadOnly = false;                   // must be false to allow typing
            txtInput.TabIndex = 0;                       // make it the first focusable control
            txtInput.Text = "";                          // start empty

            // Create Send button
            btnSend = new Button();
            btnSend.Text = "Send";
            btnSend.Location = new Point(522, 423);
            btnSend.Size = new Size(100, 35);
            btnSend.BackColor = Color.FromArgb(100, 255, 218);
            btnSend.ForeColor = Color.FromArgb(10, 25, 47);
            btnSend.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnSend.FlatStyle = FlatStyle.Flat;

            // Add all controls to the form
            this.Controls.Add(txtChat);
            this.Controls.Add(txtInput);
            this.Controls.Add(btnSend);

            // Wire up events
            btnSend.Click += (s, e) => SendMessage();
            txtInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SendMessage();
                    e.SuppressKeyPress = true; // prevents system beep
                }
            };

            // Ensure the text box gets focus when the form is shown
            this.Shown += (s, e) => txtInput.Focus();

            // Create the chatbot logic instance
            bot = new ChatBot();
            bot.MessageReceived += Bot_MessageReceived;   // connect event

            // Display ASCII art and welcome messages
            DisplayAsciiArt();
            bot.SendBotMessage("Welcome to Cybersecurity Awareness Bot!");
            bot.SendBotMessage("I can answer questions about password safety, phishing, safe browsing, and more.");
            bot.SendBotMessage("Type 'exit' to end the conversation.");

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

        // This method is called when the user sends a message
        private void SendMessage()
        {
            string msg = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            // Show user's message in the chat
            AppendMessage("You", msg, Color.FromArgb(246, 241, 213));

            // Clear input box and return focus to it
            txtInput.Clear();
            txtInput.Focus();

            // Let the chatbot process the user's input
            bot.StartChat(msg);
        }

        // Event handler: receives messages from the chatbot logic
        private void Bot_MessageReceived(string sender, string message)
        {
            // If called from a background thread, switch to UI thread
            if (InvokeRequired)
            {
                Invoke(new Action(() => Bot_MessageReceived(sender, message)));
                return;
            }

            // Choose color based on sender (Bot or System)
            Color color = (sender == "Bot") ? Color.FromArgb(100, 255, 218) : Color.FromArgb(100, 255, 218);
            AppendMessage(sender, message, color);
        }

        // Helper method to add a line to the chat display with a specific color
        private void AppendMessage(string sender, string message, Color color)
        {
            txtChat.SelectionStart = txtChat.TextLength;
            txtChat.SelectionColor = color;
            txtChat.AppendText($"{sender}: {message}\r\n\r\n");
            txtChat.ScrollToCaret();   // auto-scroll to the newest message
        }

        // Display the ASCII art from the ChatBot class
        private void DisplayAsciiArt()
        {
            string art = bot.GetAsciiArt();
            AppendMessage("System", art, Color.FromArgb(100, 255, 218));
        }
    }
}