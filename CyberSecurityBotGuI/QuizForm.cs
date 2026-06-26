using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CyberSecurityBotGuI
{
    public partial class QuizForm : Form
    {
        // List to store all quiz questions
        private List<QuizQuestion> questions;
        private int currentIndex = 0;   // Tracks which question we are on
        private int score = 0;          // Tracks correct answers

        // UI Controls
        private Label lblQuestion;
        private RadioButton[] options;
        private Button btnNext;
        private Label lblScore;
        private RichTextBox txtFeedback;

        public QuizForm()
        {
            InitializeComponent();   // Required for the designer file
            SetupUI();               // Builds the form controls programmatically
            LoadQuestions();         // Loads the 11 cybersecurity questions
            ShowQuestion();          // Displays the first question
        }

        // This method creates all the buttons, labels, and text boxes for the quiz
        private void SetupUI()
        {
            this.Text = "🛡️ Cybersecurity Quiz";
            this.Size = new Size(650, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 25, 47);

            // Question Label (displays the question text)
            lblQuestion = new Label();
            lblQuestion.Location = new Point(20, 20);
            lblQuestion.Size = new Size(590, 60);
            lblQuestion.ForeColor = Color.White;
            lblQuestion.Font = new Font("Segoe UI", 12, FontStyle.Bold);

            // Radio Buttons for the 4 answer options (A, B, C, D)
            options = new RadioButton[4];
            for (int i = 0; i < 4; i++)
            {
                options[i] = new RadioButton();
                options[i].Location = new Point(30, 90 + i * 40);
                options[i].Size = new Size(560, 30);
                options[i].ForeColor = Color.White;
                options[i].Font = new Font("Segoe UI", 10);
            }

            // Feedback Text Box (shows if answer was correct/incorrect)
            txtFeedback = new RichTextBox();
            txtFeedback.Location = new Point(20, 280);
            txtFeedback.Size = new Size(590, 80);
            txtFeedback.ReadOnly = true;
            txtFeedback.BackColor = Color.FromArgb(17, 34, 64);
            txtFeedback.ForeColor = Color.LightGreen;
            txtFeedback.Font = new Font("Segoe UI", 10);

            // Next Button (goes to next question or finishes quiz)
            btnNext = new Button();
            btnNext.Text = "Next Question ➡️";
            btnNext.Location = new Point(230, 380);
            btnNext.Size = new Size(180, 45);
            btnNext.BackColor = Color.FromArgb(100, 255, 218);
            btnNext.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnNext.Click += BtnNext_Click;   // Event handler for clicking the button

            // Score Label (shows current score)
            lblScore = new Label();
            lblScore.Location = new Point(20, 385);
            lblScore.Size = new Size(200, 35);
            lblScore.ForeColor = Color.Yellow;
            lblScore.Font = new Font("Segoe UI", 12, FontStyle.Bold);

            // Add all controls to the form
            this.Controls.Add(lblQuestion);
            foreach (var rb in options) this.Controls.Add(rb);
            this.Controls.Add(txtFeedback);
            this.Controls.Add(btnNext);
            this.Controls.Add(lblScore);
        }

        // Loads 11 cybersecurity questions (mix of multiple-choice and true/false)
        private void LoadQuestions()
        {
            questions = new List<QuizQuestion>
            {
                // ---- MULTIPLE CHOICE QUESTIONS ----
                new QuizQuestion(
                    "1. What should you do if you receive an email asking for your password?",
                    new string[] { "Reply with your password", "Delete the email", "Report it as phishing", "Ignore it" },
                    2,  // Correct answer is index 2 (Report it as phishing)
                    "✅ Correct! Reporting phishing emails helps prevent scams. Never share passwords via email."
                ),
                new QuizQuestion(
                    "2. What is the best way to create a strong password?",
                    new string[] { "Use your birthday", "Use a common word", "Use a mix of letters, numbers, and symbols", "Use only numbers" },
                    2,  // Correct answer is index 2
                    "✅ Correct! A mix of characters makes passwords much harder to crack."
                ),
                new QuizQuestion(
                    "3. What does '2FA' stand for?",
                    new string[] { "Two-Form Authentication", "Two-Factor Authentication", "Triple-Factor Authentication", "Two-Factor Authorization" },
                    1,  // Correct answer is index 1
                    "✅ Correct! Two-Factor Authentication adds an extra layer of security."
                ),
                new QuizQuestion(
                    "4. Which of these is a sign of a phishing email?",
                    new string[] { "Professional language", "Urgent request for personal info", "Friendly tone", "No attachments" },
                    1,  // Correct answer is index 1
                    "✅ Correct! Phishing emails often create urgency to trick you into acting quickly."
                ),
                new QuizQuestion(
                    "5. What should you do if a website doesn't have HTTPS?",
                    new string[] { "Enter your info anyway", "Avoid entering sensitive information", "Bookmark it", "Share the link" },
                    1,  // Correct answer is index 1
                    "✅ Correct! Always avoid entering personal info on non-HTTPS sites."
                ),
                new QuizQuestion(
                    "6. What is social engineering in cybersecurity?",
                    new string[] { "Building social networks", "Manipulating people into revealing information", "Engineering software", "Creating fake profiles" },
                    1,  // Correct answer is index 1
                    "✅ Correct! Social engineering exploits human psychology to gain access to information."
                ),
                new QuizQuestion(
                    "7. How often should you update your passwords?",
                    new string[] { "Every year", "Every 3-6 months", "Never", "Only when you're hacked" },
                    1,  // Correct answer is index 1
                    "✅ Correct! Regular password updates reduce the risk of unauthorized access."
                ),

                // ---- TRUE / FALSE QUESTIONS ----
                new QuizQuestion(
                    "8. True or False: Using the same password for multiple accounts is safe.",
                    new string[] { "True", "False", "", "" },
                    1,  // Correct answer is index 1 (False)
                    "✅ Correct! Using the same password for multiple accounts is dangerous. If one gets hacked, all are at risk."
                ),
                new QuizQuestion(
                    "9. True or False: Public Wi-Fi is always safe for banking.",
                    new string[] { "True", "False", "", "" },
                    1,  // Correct answer is index 1 (False)
                    "✅ Correct! Public Wi-Fi is often unsecured. Avoid banking on public networks."
                ),
                new QuizQuestion(
                    "10. True or False: Antivirus software alone can protect against all cyber threats.",
                    new string[] { "True", "False", "", "" },
                    1,  // Correct answer is index 1 (False)
                    "✅ Correct! Antivirus is important but not enough. Combine with safe browsing habits."
                ),
                new QuizQuestion(
                    "11. True or False: You should click on links in suspicious emails to check if they're real.",
                    new string[] { "True", "False", "", "" },
                    1,  // Correct answer is index 1 (False)
                    "✅ Correct! Never click suspicious links – hover over them first or report the email."
                )
            };
        }

        // Displays the current question on the screen
        private void ShowQuestion()
        {
            if (currentIndex >= questions.Count)
            {
                EndQuiz();
                return;
            }

            var q = questions[currentIndex];
            lblQuestion.Text = q.QuestionText;

            // Clear all radio buttons
            foreach (var rb in options) rb.Checked = false;

            // Show the options (hide empty ones for true/false)
            for (int i = 0; i < 4; i++)
            {
                if (i < q.Options.Length && !string.IsNullOrEmpty(q.Options[i]))
                {
                    options[i].Text = q.Options[i];
                    options[i].Visible = true;
                }
                else
                {
                    options[i].Visible = false;
                }
            }

            // Clear feedback from previous question
            txtFeedback.Clear();

            // Change button text for the last question
            btnNext.Text = (currentIndex == questions.Count - 1) ? "🏁 Finish" : "Next Question ➡️";

            // Update the score display
            lblScore.Text = $"Score: {score}/{currentIndex}";
        }

        // Event handler when the user clicks the Next/Finish button
        private void BtnNext_Click(object sender, EventArgs e)
        {
            // Check which radio button (if any) is selected
            int selected = -1;
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i].Checked)
                {
                    selected = i;
                    break;
                }
            }

            if (selected == -1)
            {
                MessageBox.Show("Please select an answer.", "Quiz", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if the selected answer is correct
            var q = questions[currentIndex];
            if (selected == q.CorrectIndex)
            {
                score++;
                txtFeedback.ForeColor = Color.LightGreen;
            }
            else
            {
                txtFeedback.ForeColor = Color.LightCoral;
            }

            // Show the feedback text
            txtFeedback.Text = q.Feedback;

            // Move to the next question (or finish)
            currentIndex++;
            if (currentIndex >= questions.Count)
            {
                EndQuiz();
            }
            else
            {
                ShowQuestion();
            }
        }

        // Displays the final score when the quiz is complete
        private void EndQuiz()
        {
            string message = $"🏆 Quiz Complete!\n\nYou scored {score} out of {questions.Count}!\n\n";

            if (score >= questions.Count * 0.8)
                message += "🌟 Excellent! You're a cybersecurity pro!";
            else if (score >= questions.Count * 0.5)
                message += "📚 Good effort! Keep learning to stay safe online!";
            else
                message += "🔒 Don't worry! Keep learning – cybersecurity is important for everyone!";

            MessageBox.Show(message, "Quiz Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();   // Close the quiz window
        }

        // Helper class to store question details
        public class QuizQuestion
        {
            public string QuestionText { get; set; }
            public string[] Options { get; set; }
            public int CorrectIndex { get; set; }
            public string Feedback { get; set; }

            public QuizQuestion(string question, string[] options, int correctIndex, string feedback)
            {
                QuestionText = question;
                Options = options;
                CorrectIndex = correctIndex;
                Feedback = feedback;
            }
        }
    }
}