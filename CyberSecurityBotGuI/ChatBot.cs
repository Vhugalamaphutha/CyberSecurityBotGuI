using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.IO;
using System.Windows.Forms;

namespace CyberSecurityBotGuI
{
    public class ChatBot
    {
        private string userName;           // Stores user's name
        private string favoriteTopic;      // Stores user's favorite topic (for memory)
        private string lastTopic;          // Stores last discussed topic (for follow-up)
        private List<string> conversationHistory = new List<string>();

        // ============ NEW: Activity Log (Task 4) ============
        private List<string> activityLog = new List<string>();

        // ============ NEW: Task Manager (Task 1) ============
        private TaskManager taskManager = new TaskManager();

        // Keyword responses (Task 2 & 3)
        private Dictionary<string, List<string>> keywordResponses = new Dictionary<string, List<string>>()
        {
            ["password"] = new List<string> {
                "🔐 Use strong, unique passwords for each account. Enable two-factor authentication (2FA).",
                "🛡️ Avoid using personal info like birthdays. Use a mix of letters, numbers, and symbols.",
                "🧠 Consider using a password manager to generate and store complex passwords."
            },
            ["phish"] = new List<string> {
                "🎣 Phishing: attackers pretend to be legitimate companies to steal your info. Never click suspicious links.",
                "📧 Check the sender's email address carefully – scammers use slight misspellings.",
                "🚫 Don't enter personal info on a site you reached from an email link. Type the URL manually."
            },
            ["safe browsing"] = new List<string> {
                "🌐 Keep your browser updated, avoid clicking pop-ups, and use HTTPS websites.",
                "🔒 Use ad-blockers and privacy extensions to reduce tracking.",
                "📁 Don't save passwords in your browser unless it's encrypted with a master password."
            },
            ["update"] = new List<string> {
                "🔄 Always keep your operating system and apps updated. Updates fix security holes.",
                "⚙️ Enable automatic updates where possible so you don't miss critical patches."
            },
            ["email"] = new List<string> {
                "📨 Be cautious with emails. Check sender addresses and don't click suspicious links.",
                "⚠️ Look for poor grammar, urgent requests, and unexpected attachments – these are red flags."
            }
        };

        // Sentiment keywords (Task 6)
        private Dictionary<string, List<string>> sentimentKeywords = new Dictionary<string, List<string>>()
        {
            ["worried"] = new List<string> { "worried", "scared", "anxious", "nervous", "afraid" },
            ["frustrated"] = new List<string> { "frustrated", "annoyed", "angry", "tired" },
            ["curious"] = new List<string> { "curious", "interested", "tell me more", "explain" }
        };

        // Default error responses (Task 7)
        private List<string> defaultResponses = new List<string>
        {
            "🤖 I'm not sure I understand. Can you rephrase? Try asking about passwords, phishing, safe browsing, or updates.",
            "🔍 Hmm, I didn't catch that. Ask me about cybersecurity topics like 'password safety' or 'email safety'.",
            "📚 I'm still learning! Please ask about online safety, phishing, or how to keep software updated."
        };

        // Event to send messages to the GUI
        public event Action<string, string> MessageReceived;
        public event Action<string> OpenQuizRequested;
        public event Action<string> OpenTaskManagerRequested;

        public void SendBotMessage(string message)
        {
            MessageReceived?.Invoke("Bot", message);
        }

        public void SendUserMessage(string message)
        {
            MessageReceived?.Invoke(userName, message);
        }

        // ============ NEW: Activity Log Methods (Task 4) ============
        public void LogAction(string action)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            activityLog.Add($"[{timestamp}] {action}");

            // Keep only last 20 to prevent memory issues
            if (activityLog.Count > 20)
                activityLog.RemoveAt(0);
        }

        public string GetActivityLog()
        {
            if (activityLog.Count == 0)
                return "No actions have been logged yet.";

            string log = "Here's a summary of recent actions:\n\n";
            int count = Math.Min(10, activityLog.Count);
            for (int i = activityLog.Count - count; i < activityLog.Count; i++)
            {
                log += $"{i + 1}. {activityLog[i]}\n";
            }
            return log;
        }

        // Main entry point for processing user input
        public void StartChat(string userInput)
        {
            // If user name is not yet known, treat this input as the name
            if (string.IsNullOrEmpty(userName))
            {
                userName = userInput.Trim();
                if (!string.IsNullOrEmpty(userName))
                {
                    LogAction($"User registered: {userName}");
                    SendBotMessage($"Nice to meet you, {userName}! I'll remember your name.");
                    SendBotMessage("Now, ask me about cybersecurity: password safety, phishing, safe browsing, updates, or email.");
                    SendBotMessage("You can also say 'add task', 'start quiz', or 'show activity log'.");
                }
                else
                {
                    SendBotMessage("Please tell me your name.");
                }
                return;
            }

            // If input is empty, ignore
            if (string.IsNullOrEmpty(userInput)) return;

            // Store in history
            conversationHistory.Add($"User: {userInput}");
            LogAction($"User asked: {userInput}");

            // Exit command
            if (userInput.ToLower() == "exit")
            {
                LogAction($"User {userName} exited");
                SendBotMessage($"Goodbye, {userName}! Stay safe online.");
                Application.Exit();
                return;
            }

            // ============ NEW: NLP for Tasks, Quiz, and Activity Log (Task 3 & 4) ============
            string nlpResponse = HandleNLP(userInput);
            if (nlpResponse != null)
            {
                SendBotMessage(nlpResponse);
                return;
            }

            // Follow-up handling (Task 4)
            string followUpResponse = HandleFollowUp(userInput);
            if (followUpResponse != null)
            {
                SendBotMessage(followUpResponse);
                return;
            }

            // Memory: remember favorite topic (Task 5)
            string memoryResponse = RememberUserInfo(userInput);
            if (memoryResponse != null)
            {
                SendBotMessage(memoryResponse);
                return;
            }

            // Sentiment detection (Task 6)
            string sentiment = DetectSentiment(userInput);

            // Get response based on keyword
            string response = GetResponse(userInput);

            // Adjust response based on sentiment
            if (sentiment != "neutral")
            {
                response = AdjustResponseForSentiment(sentiment, response);
            }

            // Personalize response with user's name
            response = $"{userName}, {response}";

            SendBotMessage(response);

            // Optional reminder about favorite topic (Task 5)
            if (!string.IsNullOrEmpty(favoriteTopic) && new Random().Next(5) == 0)
            {
                SendBotMessage($"As someone interested in {favoriteTopic}, remember to stay vigilant!");
            }
        }

        // ============ NEW: NLP Handler (Task 3) ============
        private string HandleNLP(string input)
        {
            string lower = input.ToLower();

            // Check for activity log request (Task 4)
            if (lower.Contains("activity log") || lower.Contains("what have you done") ||
                lower.Contains("show log") || lower.Contains("show me what") ||
                lower.Contains("recent actions"))
            {
                LogAction("User viewed activity log");
                return GetActivityLog();
            }

            // Check for quiz request (Task 2)
            if (lower.Contains("quiz") || lower.Contains("start quiz") ||
                lower.Contains("play quiz") || lower.Contains("take quiz"))
            {
                LogAction("User requested to start the quiz");
                OpenQuizRequested?.Invoke("Open quiz");
                return "🔐 Opening the cybersecurity quiz for you! Good luck! 🎯";
            }

            // Check for task manager request
            if (lower.Contains("manage tasks") || lower.Contains("view tasks") ||
                lower.Contains("task manager") || lower.Contains("open tasks"))
            {
                LogAction("User opened task manager");
                OpenTaskManagerRequested?.Invoke("Open tasks");
                return "📋 Opening your task manager...";
            }

            // Check for task addition (various phrasings)
            if (lower.Contains("add task") || lower.Contains("new task") ||
                lower.Contains("create task") || lower.Contains("remind me to"))
            {
                // Extract task from input
                string taskTitle = ExtractTaskFromInput(input);
                string description = ExtractDescriptionFromInput(input);
                DateTime? reminder = ExtractReminderFromInput(input);

                bool success = taskManager.AddTask(taskTitle, description, reminder);
                if (success)
                {
                    LogAction($"Task added: '{taskTitle}' (Reminder: {reminder?.ToString("yyyy-MM-dd HH:mm") ?? "None"})");
                    string response = $"✅ Task added: '{taskTitle}'.";
                    if (reminder.HasValue)
                        response += $" I'll remind you on {reminder.Value.ToString("yyyy-MM-dd HH:mm")}.";
                    else
                        response += " Would you like to set a reminder? (say 'remind me in X days')";
                    return response;
                }
                else
                {
                    LogAction("Failed to add task - database connection issue");
                    return "⚠️ I couldn't add the task. Make sure MySQL is running and the connection string is correct.";
                }
            }

            // Check for task viewing
            if (lower.Contains("show tasks") || lower.Contains("list tasks") ||
                lower.Contains("my tasks") || lower.Contains("what are my tasks"))
            {
                LogAction("User viewed task list");
                var tasks = taskManager.GetTasks();
                if (tasks.Count == 0)
                    return "📋 You have no tasks yet. Say 'Add task' to create one!";

                string response = "📋 Here are your tasks:\n";
                foreach (var task in tasks)
                {
                    string status = task.IsCompleted ? "✅" : "⏳";
                    string reminder = task.ReminderDate.HasValue ? $" (Reminder: {task.ReminderDate.Value:yyyy-MM-dd HH:mm})" : "";
                    response += $"{status} {task.Title} - {task.Description}{reminder}\n";
                }
                return response;
            }

            // Check for task completion
            if (lower.Contains("complete task") || lower.Contains("mark done") ||
                lower.Contains("finish task"))
            {
                return "📋 Please open the Task Manager window to complete or delete tasks.";
            }

            return null;
        }

        // Helper method to extract task title from user input
        private string ExtractTaskFromInput(string input)
        {
            string lower = input.ToLower();
            string result = input;

            if (lower.Contains("remind me to"))
            {
                int idx = lower.IndexOf("remind me to") + 11;
                if (idx < input.Length)
                    result = input.Substring(idx).Trim();
            }
            else if (lower.Contains("add task"))
            {
                int idx = lower.IndexOf("add task") + 8;
                if (idx < input.Length)
                    result = input.Substring(idx).Trim();
            }
            else if (lower.Contains("new task"))
            {
                int idx = lower.IndexOf("new task") + 8;
                if (idx < input.Length)
                    result = input.Substring(idx).Trim();
            }
            else if (lower.Contains("create task"))
            {
                int idx = lower.IndexOf("create task") + 11;
                if (idx < input.Length)
                    result = input.Substring(idx).Trim();
            }

            // Remove "to" if it's at the start
            if (result.StartsWith("to ", StringComparison.OrdinalIgnoreCase))
                result = result.Substring(3).Trim();

            // Remove reminder phrases
            if (result.Contains(" tomorrow"))
                result = result.Replace(" tomorrow", "").Trim();
            if (result.Contains(" in "))
            {
                int idx = result.IndexOf(" in ");
                result = result.Substring(0, idx).Trim();
            }

            // If result is empty or the same as input, return default
            if (string.IsNullOrEmpty(result) || result == input)
                return "Cybersecurity task";

            return result;
        }

        private string ExtractDescriptionFromInput(string input)
        {
            string lower = input.ToLower();
            if (lower.Contains(" for "))
            {
                int idx = lower.IndexOf(" for ") + 4;
                if (idx < input.Length)
                    return input.Substring(idx).Trim();
            }
            if (lower.Contains(" about "))
            {
                int idx = lower.IndexOf(" about ") + 6;
                if (idx < input.Length)
                    return input.Substring(idx).Trim();
            }
            return "Cybersecurity task";
        }

        private DateTime? ExtractReminderFromInput(string input)
        {
            string lower = input.ToLower();

            if (lower.Contains("tomorrow"))
                return DateTime.Now.AddDays(1);

            if (lower.Contains("in "))
            {
                try
                {
                    string[] parts = lower.Split(' ');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (parts[i] == "in" && i + 1 < parts.Length)
                        {
                            if (int.TryParse(parts[i + 1], out int days))
                                return DateTime.Now.AddDays(days);
                        }
                    }
                }
                catch { }
            }

            if (lower.Contains("next week"))
                return DateTime.Now.AddDays(7);

            return null;
        }

        // Follow-up: "tell me more", "another tip", etc.
        private string HandleFollowUp(string input)
        {
            string lower = input.ToLower();
            if (lower.Contains("tell me more") || lower.Contains("explain more") ||
                lower.Contains("another tip") || lower.Contains("more tips"))
            {
                if (!string.IsNullOrEmpty(lastTopic) && keywordResponses.ContainsKey(lastTopic))
                {
                    var responses = keywordResponses[lastTopic];
                    string newTip = responses[new Random().Next(responses.Count)];
                    LogAction($"Follow-up requested on topic: {lastTopic}");
                    return $"Sure! Here's another tip about {lastTopic}: {newTip}";
                }
                return "What topic would you like more information on? Try 'password', 'phishing', or 'safe browsing'.";
            }
            return null;
        }

        // Remember user's favorite topic when they say "I like X" or "interested in X"
        private string RememberUserInfo(string input)
        {
            string lower = input.ToLower();
            if ((lower.Contains("like") || lower.Contains("interested in")) && string.IsNullOrEmpty(favoriteTopic))
            {
                foreach (var topic in keywordResponses.Keys)
                {
                    if (lower.Contains(topic))
                    {
                        favoriteTopic = topic;
                        LogAction($"User showed interest in: {topic}");
                        return $"Got it! I'll remember that you're interested in {topic}. Great choice for staying safe online!";
                    }
                }
            }
            return null;
        }

        // Simple sentiment detection
        private string DetectSentiment(string input)
        {
            string lower = input.ToLower();
            foreach (var sentiment in sentimentKeywords)
            {
                foreach (var word in sentiment.Value)
                {
                    if (lower.Contains(word))
                        return sentiment.Key;
                }
            }
            return "neutral";
        }

        private string AdjustResponseForSentiment(string sentiment, string response)
        {
            if (sentiment == "worried")
                return $"It's completely understandable to feel worried. {response}\n\nYou're taking the right steps to protect yourself!";
            if (sentiment == "frustrated")
                return $"I hear your frustration. Cybersecurity can be confusing. {response}\n\nTake a deep breath – you've got this!";
            if (sentiment == "curious")
                return $"Great curiosity! {response}\n\nWould you like me to explain more?";
            return response;
        }

        // Keyword-based response with random selection (Task 3)
        private string GetResponse(string input)
        {
            string lowerInput = input.ToLower();

            foreach (var kvp in keywordResponses)
            {
                if (lowerInput.Contains(kvp.Key))
                {
                    lastTopic = kvp.Key;
                    var responses = kvp.Value;
                    LogAction($"Provided info on topic: {kvp.Key}");
                    return responses[new Random().Next(responses.Count)];
                }
            }

            // Special non-keyword questions
            if (lowerInput.Contains("how are you"))
                return "I'm functioning perfectly, thank you for asking! How can I help you with cybersecurity today?";
            if (lowerInput.Contains("purpose") || lowerInput.Contains("what can you do"))
                return "My purpose is to educate you about staying safe online. You can ask me about password safety, phishing, safe browsing, and more.";

            LogAction($"Unknown input: {input}");
            return defaultResponses[new Random().Next(defaultResponses.Count)];
        }

        // Voice greeting (plays WAV file)

        public void PlayVoiceGreeting()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Try both possible file names
            string[] possibleNames = {
        "Awareness chatbot audio.wav",
        "Awareness_chatbot_audio.wav"
    };

            string foundPath = null;

            foreach (var name in possibleNames)
            {
                string testPath = Path.Combine(baseDir, "Resources", name);
                if (File.Exists(testPath))
                {
                    foundPath = testPath;
                    break;
                }
            }

            if (foundPath != null)
            {
                try
                {
                    using (SoundPlayer player = new SoundPlayer(foundPath))
                    {
                        player.PlaySync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error playing audio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Show both paths checked
                string message = "Audio file not found. Checked:\n\n";
                foreach (var name in possibleNames)
                {
                    message += Path.Combine(baseDir, "Resources", name) + "\n";
                }
                MessageBox.Show(message, "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        // ASCII art
        public string GetAsciiArt()
        {
            return @"
+----------------------------------------------------------+
|                                                          |
|              🔒  CYBERSECURITY BOT  🔒                   |
|                                                          |
|       _   _   _   _   _   _   _   _   _   _             |
|      / \ / \ / \ / \ / \ / \ / \ / \ / \ / \            |
|     ( C | Y | B | E | R |   | B | O | T )               |
|      \_/ \_/ \_/ \_/ \_/ \_/ \_/ \_/ \_/ \_/            |
|                                                          |
|          Your Privacy. Our Mission. 💪                    |
|                                                          |
|    📋 Tasks | 🎯 Quiz | 📊 Activity Log | 🤖 NLP        |
|                                                          |
+----------------------------------------------------------+
";
        }
    }
}