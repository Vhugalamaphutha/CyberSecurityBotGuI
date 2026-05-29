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

        public void SendBotMessage(string message)
        {
            MessageReceived?.Invoke("Bot", message);
        }

        public void SendUserMessage(string message)
        {
            MessageReceived?.Invoke(userName, message);
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
                    SendBotMessage($"Nice to meet you, {userName}! I'll remember your name.");
                    SendBotMessage("Now, ask me about cybersecurity: password safety, phishing, safe browsing, updates, or email.");
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

            // Exit command
            if (userInput.ToLower() == "exit")
            {
                SendBotMessage($"Goodbye, {userName}! Stay safe online.");
                Application.Exit();
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
                    return responses[new Random().Next(responses.Count)];
                }
            }

            // Special non-keyword questions
            if (lowerInput.Contains("how are you"))
                return "I'm functioning perfectly, thank you for asking! How can I help you with cybersecurity today?";
            if (lowerInput.Contains("purpose") || lowerInput.Contains("what can you do"))
                return "My purpose is to educate you about staying safe online. You can ask me about password safety, phishing, safe browsing, and more.";

            return defaultResponses[new Random().Next(defaultResponses.Count)];
        }

        // Voice greeting (plays WAV file)
        public void PlayVoiceGreeting()
        {
            string wavPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Awarness chatbot audio.wav");
            if (File.Exists(wavPath))
            {
                try
                {
                    using (SoundPlayer player = new SoundPlayer(wavPath))
                    {
                        player.PlaySync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Voice greeting error: {ex.Message}");
                }
            }
            // If file missing, do nothing – no error message to avoid annoyance
        }

        // ASCII art
        public string GetAsciiArt()
        {
            return @"
    ╔══════════════════════════════════════════════════════════════╗
    ║                     🛡️ CYBERSECURITY BOT 🛡️                    ║
    ║                                                              ║
    ║           ███████╗██╗   ██╗██████╗ ███████╗██████╗           ║
    ║           ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗          ║
    ║           █████╗   ╚████╔╝ ██████╔╝█████╗  ██████╔╝          ║
    ║           ██╔══╝    ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗          ║
    ║           ███████╗   ██║   ██████╔╝███████╗██║  ██║          ║
    ║           ╚══════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝          ║
    ║                                                              ║
    ║              Your Privacy. Our Mission. 💪                    ║
    ╚══════════════════════════════════════════════════════════════╝
            ";
        }
    }
}