using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TypingSpeedTest
{
    public partial class Form1 : Form
    {
        // Sample texts: each difficulty has a set of prompts the player may be asked to type.
        private readonly string[] easySamples =
        {
            "elephant", "strawberry", "chocolate", "pineapple",
            "watermelon", "butterfly", "sunflower", "rainbow",
            "mountain", "football"
        };

        private readonly string[] normalSamples =
        {
            "Typing practice makes learning fun",
            "The blue sky is full of clouds",
            "Reading books improves knowledge daily",
            "Coding challenges help students grow",
            "Fast typing helps finish homework quickly",
            "Simple sentences are easy to understand"
        };

        private readonly string[] mediumSamples =
        {
            "Consistency in practice builds strong typing skills",
            "Programming requires patience, focus, and determination",
            "Typing faster improves productivity in school and work",
            "Accuracy is more important than speed when learning",
            "Windows Forms applications are useful for beginners"
        };

        private readonly string[] proSamples =
        {
            "A truly skilled typist balances speed, rhythm, and precision.",
            "Errors are inevitable, but persistence leads to mastery.",
            "Complex sentences with commas, quotes, and punctuation test focus.",
            "Typing under pressure requires concentration and accuracy.",
            "Advanced typists often reach speeds above one hundred words per minute!"
        };

        // Game state: stores the chosen prompts, current prompt, timing, attempts, and whether a round is active.
        private string[] samples;
        private string currentText = string.Empty;

        private readonly Random rand = new Random();

        private int elapsedSeconds = 0;
        private int attemptCount = 0;
        private int countdownValue = 3;
        private int totalTimeLimit = 60;

        private bool gameStarted = false;

        // Initializes the Windows Form and sets the window's basic appearance and title.
        public Form1()
        {
            InitializeComponent(); // Creates the controls designed in Form1.Designer.cs.

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Word Dash";
        }

        // FORM LOAD
        // Runs when the form opens: configures labels, timer, control visibility, button styles, logo, and keyboard handling.
        private void Form1_Load(object sender, EventArgs e)
        {
            button2.Visible = false;
            label11.Text = "Word Dash";
            label11.Font = new Font("Arial", 33, FontStyle.Bold);
            label11.ForeColor = Color.DarkBlue;

            label1.Font = new Font("Arial", 24, FontStyle.Bold);
            label3.Font = new Font("Arial", 18, FontStyle.Bold);
            label4.Font = new Font("Arial", 18, FontStyle.Bold);
            label4.Text = "Leaderboard";

            timer1.Interval = 1000; // Makes the timer tick once every 1,000 milliseconds (one second).
            timer1.Stop();

            richTextBox1.Enabled = false;
            listBox1.Enabled = false;
            listBox2.Enabled = false;
            listBox3.Enabled = false;
            listBox4.Enabled = false;
            listBox5.Enabled = false;

            // Hide controls at startup
            // Collects controls that should not appear on the home screen at startup.
            Control[] hiddenControls =
            {
                richTextBox1, textBox1, label1, label3,
                button2, listBox1, listBox2, listBox3,
                listBox4, listBox5, button3, button4,
                label2, label4, label7, label8, label9,
                label10, label12, button13, button15
            };

            foreach (Control control in hiddenControls)
                control.Visible = false;

            // Difficulty buttons
            // Groups the four difficulty buttons so the same styling can be applied to each.
            Button[] difficultyButtons =
            {
                button5, button6, button7, button8
            };

            Color[] difficultyColors =
            {
                Color.Green,
                Color.RoyalBlue,
                Color.MediumPurple,
                Color.Red
            };

            for (int i = 0; i < difficultyButtons.Length; i++)
                MakeButtonRound(difficultyButtons[i], difficultyColors[i]);

            // Other buttons
            // Groups the remaining buttons for consistent styling.
            Button[] otherButtons =
            {
                button1, button2, button3, button4,
                button9, button10, button11, button12,
                button13, button14, button15
            };

            Color[] otherColors =
            {
                Color.DodgerBlue,
                Color.SteelBlue,
                Color.Orange,
                Color.Gray,
                Color.Teal,
                Color.Teal,
                Color.Teal,
                Color.Teal,
                Color.DarkOrange,
                Color.LightGray,
                Color.DarkOrange
            };

            for (int i = 0; i < otherButtons.Length; i++)
                MakeButtonRound(otherButtons[i], otherColors[i]);

            ShowDifficultyButtons(false);
            ShowTimeButtons(false);

            // Load the logo if the file exists
            // Builds the expected path to logo.png beside the application executable.
            string logoPath = Path.Combine(
                Application.StartupPath, "logo.png");


            // Prevent duplicate Enter key handlers
            // Removes then re-adds the handler so pressing Enter is not handled twice.
            textBox1.KeyDown -= textBox1_KeyDown;
            textBox1.KeyDown += textBox1_KeyDown;
        }

        // BUTTON STYLING
        // Gives a button rounded corners, removes its border, sets its colors, and connects hover effects.
        private void MakeButtonRound(
            Button btn, Color baseColor, int radius = 20)
        {
            // Creates a custom outline used to make the button's corners rounded.
            GraphicsPath path = new GraphicsPath();

            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(
                btn.Width - radius, 0,
                radius, radius, 270, 90);

            path.AddArc(
                btn.Width - radius,
                btn.Height - radius,
                radius, radius, 0, 90);

            path.AddArc(
                0, btn.Height - radius,
                radius, radius, 90, 90);

            path.CloseFigure();

            // Applies the rounded outline to the button, then releases the temporary path.
            btn.Region = new Region(path);
            path.Dispose();

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = baseColor;
            btn.ForeColor = Color.White;
            btn.Tag = baseColor;

            btn.MouseEnter -= Button_MouseEnter;
            btn.MouseLeave -= Button_MouseLeave;

            btn.MouseEnter += Button_MouseEnter;
            btn.MouseLeave += Button_MouseLeave;
        }

        // Lightens a button's background while the mouse pointer is over it.
        private void Button_MouseEnter(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            btn.BackColor = ControlPaint.Light((Color)btn.Tag, 0.2f);
        }

        // Restores a button's original background color when the mouse leaves it.
        private void Button_MouseLeave(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            btn.BackColor = (Color)btn.Tag;
        }

        // VISIBILITY HELPERS
        // Shows or hides the difficulty choices and their heading together.
        private void ShowDifficultyButtons(bool show)
        {
            button5.Visible = show;
            button6.Visible = show;
            button7.Visible = show;
            button8.Visible = show;
            label5.Visible = show;

            // Update the heading only when these choices are being shown.
            if (show)
                label5.Text = "Choose Category";
        }

        // Shows or hides the time-limit choices and their heading together.
        private void ShowTimeButtons(bool show)
        {
            button9.Visible = show;
            button10.Visible = show;
            button11.Visible = show;
            button12.Visible = show;
            label6.Visible = show;

            if (show)
                label6.Text = "Time";
        }

        // Hides all four difficulty leaderboards and their related labels.
        private void HideLeaderboards()
        {
            listBox2.Visible = false;
            listBox3.Visible = false;
            listBox4.Visible = false;
            listBox5.Visible = false;
            label4.Visible = false;
            label7.Visible = false;
            label8.Visible = false;
            label9.Visible = false;
            label10.Visible = false;
        }

        // START BUTTON
        // Handles Start: hides the home screen and opens the difficulty-selection screen.
        private void button1_Click_1(object sender, EventArgs e)
        {
            button1.Visible = false;
            button2.Visible = false;
            button13.Visible = false;
            button14.Visible = false;

            ShowDifficultyButtons(true);
            ShowTimeButtons(false);

            listBox1.Visible = false;
            listBox2.Visible = false;
            label2.Visible = false;
            label4.Visible = false;
            label11.Visible = false;
            label12.Visible = false;
        }

        // DIFFICULTY BUTTONS
        // Selects Easy sample text and then displays the time-limit choices.
        private void button5_Click(object sender, EventArgs e)
        {
            samples = easySamples;
            ShowDifficultyButtons(false);
            ShowTimeButtons(true);
        }

        // Selects Normal sample text and then displays the time-limit choices.
        private void button6_Click(object sender, EventArgs e)
        {
            samples = normalSamples;
            ShowDifficultyButtons(false);
            ShowTimeButtons(true);
        }

        // Selects Medium sample text and then displays the time-limit choices.
        private void button7_Click(object sender, EventArgs e)
        {
            samples = mediumSamples;
            ShowDifficultyButtons(false);
            ShowTimeButtons(true);
        }

        // Selects Pro sample text and then displays the time-limit choices.
        private void button8_Click(object sender, EventArgs e)
        {
            samples = proSamples;
            ShowDifficultyButtons(false);
            ShowTimeButtons(true);
        }

        // TIME BUTTONS
        // Starts a typing test with a 60-second time limit.
        private void button9_Click(object sender, EventArgs e)
        {
            StartGameWithTime(60);
        }

        // Starts a typing test with a 90-second time limit.
        private void button10_Click(object sender, EventArgs e)
        {
            StartGameWithTime(90);
        }

        // Starts a typing test with a 120-second time limit.
        private void button11_Click(object sender, EventArgs e)
        {
            StartGameWithTime(120);
        }

        // Starts a typing test with a 150-second time limit.
        private void button12_Click(object sender, EventArgs e)
        {
            StartGameWithTime(150);
        }

        // START GAME
        // Resets game values, hides unrelated controls, shows the countdown, and starts the timer.
        private void StartGameWithTime(int seconds)
        {
            // Do not start a round if no difficulty/sample set has been selected.
            if (samples == null || samples.Length == 0)
                return;

            timer1.Stop();

            countdownValue = 3; // Reset the pre-game countdown to three seconds.
            elapsedSeconds = 0;
            totalTimeLimit = seconds;
            gameStarted = false;

            ShowTimeButtons(false);
            HideLeaderboards();

            richTextBox1.Visible = false;
            textBox1.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button13.Visible = false;
            button15.Visible = false;
            label2.Visible = false;
            label12.Visible = false;

            label1.Visible = true;
            label1.Text = "Get Ready...";

            timer1.Start();
        }

        // COUNTDOWN AND TIMER
        // Updates the countdown and elapsed time; starts the typing round and ends it when time runs out.
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!gameStarted)
            {
                if (countdownValue > 0)
                {
                    label1.Text = countdownValue.ToString();
                    countdownValue--;
                    return;
                }

                if (countdownValue == 0)
                {
                    label1.Text = "Go!";
                    countdownValue--;

                    currentText = samples[rand.Next(samples.Length)];

                    richTextBox1.Text = currentText;
                    richTextBox1.Visible = true;
                    richTextBox1.Enabled = false;

                    textBox1.Clear();
                    textBox1.Visible = true;
                    textBox1.Enabled = true;


                    elapsedSeconds = 0;
                    label3.Text = "Time: 0s";
                    label3.Visible = true;

                    richTextBox1.SelectAll();
                    richTextBox1.SelectionColor = Color.Black;
                    richTextBox1.Select(0, 0);

                    gameStarted = true;
                    textBox1.Focus();

                    return;
                }
            }

            elapsedSeconds++;

            label3.Text = "Time: " + elapsedSeconds + "s";

            if (elapsedSeconds >= totalTimeLimit)
            {
                timer1.Stop();

                MessageBox.Show(
                    "Time is up!",
                    "Word Dash",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                button2_Click_1(sender, e);
            }
        }

        // FINISH TEST AND CALCULATE RESULTS
        // Finishes the test, calculates WPM and accuracy, saves the attempt, updates the correct leaderboard, and shows results.
        private void button2_Click_1(object sender, EventArgs e)
        {
            if (!gameStarted)
                return;

            timer1.Stop();
            gameStarted = false;

            textBox1.Enabled = false;
            attemptCount++;

            string typedText = textBox1.Text;
            int safeElapsedSeconds = Math.Max(1, elapsedSeconds);

            int wordCount = typedText.Split(
                new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries).Length;

            double wpm = wordCount * 60.0 / safeElapsedSeconds;

            int correctChars = 0;

            for (int i = 0;
                 i < Math.Min(typedText.Length, currentText.Length);
                 i++)
            {
                if (typedText[i] == currentText[i])
                    correctChars++;
            }

            double accuracy = currentText.Length == 0
                ? 0
                : correctChars * 100.0 / currentText.Length;

            // Attempt history
            listBox1.Items.Add(
                $"Attempt {attemptCount}: WPM {wpm:F1}, Accuracy {accuracy:F1}%");

            // Typing speed category
            if (wpm < 30)
            {
                label2.Text =
                    "Below 30 WPM:\nKeep practicing\nto improve your speed.";
            }
            else if (wpm < 40)
            {
                label2.Text =
                    "30–39 WPM:\nYou are building\nyour typing skills.";
            }
            else if (wpm < 60)
            {
                label2.Text =
                    "40–59 WPM:\nA solid speed for everyday typing.";
            }
            else if (wpm < 80)
            {
                label2.Text =
                    "60–79 WPM:\nYou have good typing speed!";
            }
            else
            {
                label2.Text =
                    "80+ WPM:\nOutstanding typing speed!";
            }

            label2.Visible = true;

            // Select the leaderboard for the current difficulty
            ListBox targetListBox = GetLeaderboard();

            if (targetListBox != null)
                UpdateLeaderboard(targetListBox, wpm, accuracy);

            // Hide typing controls
            richTextBox1.Visible = false;
            textBox1.Visible = false;
            label1.Visible = false;
            label3.Visible = false;
            button2.Visible = false;

            // Show results
            listBox1.Visible = true;
            HideLeaderboards();

            label2.Visible = true;
            button3.Visible = true;
            button4.Visible = true;
            button13.Visible = true;
            button15.Visible = false;
            label12.Visible = true;
        }

        // GET CORRECT LEADERBOARD
        // Returns the leaderboard ListBox that matches the currently selected difficulty.
        private ListBox GetLeaderboard()
        {
            if (samples == easySamples)
                return listBox2;

            if (samples == normalSamples)
                return listBox3;

            if (samples == mediumSamples)
                return listBox4;

            if (samples == proSamples)
                return listBox5;

            return null;
        }

        // UPDATE TOP 10 LEADERBOARD
        // Stores a score, sorts scores by WPM, keeps the top 10, and displays them in the chosen leaderboard.
        private void UpdateLeaderboard(
            ListBox targetListBox, double wpm, double accuracy)
        {
            if (targetListBox.Tag == null)
            {
                targetListBox.Tag =
                    new List<Tuple<double, double>>();
            }

            var scores =
                (List<Tuple<double, double>>)targetListBox.Tag;

            scores.Add(Tuple.Create(wpm, accuracy));

            var sortedScores = scores
                .OrderByDescending(score => score.Item1)
                .Take(10)
                .ToList();

            targetListBox.Items.Clear();

            for (int i = 0; i < sortedScores.Count; i++)
            {
                var score = sortedScores[i];

                targetListBox.Items.Add(
                    $"Top {i + 1}: WPM {score.Item1:F1}, " +
                    $"Accuracy {score.Item2:F1}%");
            }
        }

        // COLOR CORRECT AND INCORRECT CHARACTERS
        // Colors matching typed characters green, incorrect characters red, and not-yet-typed characters black.
        private void HighlightDifferences(string typed, string sample)
        {
            if (string.IsNullOrEmpty(sample))
                return;

            richTextBox1.SelectAll();
            richTextBox1.SelectionColor = Color.Black;

            int length = Math.Min(typed.Length, sample.Length);

            for (int i = 0; i < sample.Length; i++)
            {
                richTextBox1.Select(i, 1);

                if (i < length)
                {
                    richTextBox1.SelectionColor =
                        typed[i] == sample[i]
                        ? Color.Green
                        : Color.Red;
                }
                else
                {
                    richTextBox1.SelectionColor = Color.Black;
                }
            }

            richTextBox1.Select(0, 0);
        }

        // TEXTBOX EVENTS
        // Refreshes the character highlighting whenever the user types during a game.
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (gameStarted)
                HighlightDifferences(textBox1.Text, currentText);
        }

        // Uses Enter to finish the test and suppresses the Enter key's normal beep/new-line behavior.
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                if (gameStarted)
                    button2_Click_1(sender, e);
            }
        }

        // RETRY / RETURN TO DIFFICULTY SELECTION
        // Stops the current round and returns to difficulty selection so the user can play again.
        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Stop();

            gameStarted = false;
            countdownValue = 3;
            elapsedSeconds = 0;

            richTextBox1.Visible = false;
            textBox1.Visible = false;
            label1.Visible = false;
            label3.Visible = false;
            label2.Visible = false;
            label12.Visible = false;

            listBox1.Visible = false;
            HideLeaderboards();

            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button13.Visible = false;
            button15.Visible = false;

            ShowDifficultyButtons(true);
            ShowTimeButtons(false);
        }

        // EXIT BUTTON
        // Closes the application from the results screen.
        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // VIEW SAMPLES / LEADERBOARD
        // Opens the leaderboard screen, showing the four difficulty leaderboards and their labels.
        private void button13_Click(object sender, EventArgs e)
        {
            button13.Visible = false;

            listBox1.Visible = false;
            HideLeaderboards();

            label2.Visible = false;
            label11.Visible = false;
            label12.Visible = false;

            button3.Visible = false;
            button4.Visible = false;
            button1.Visible = false;
            button14.Visible = false;
            button15.Visible = true;

            // Show all leaderboards
            listBox2.Visible = true;
            listBox3.Visible = true;
            listBox4.Visible = true;
            listBox5.Visible = true;

            // Show difficulty labels
            label4.Visible = true;
            label7.Visible = true;
            label8.Visible = true;
            label9.Visible = true;
            label10.Visible = true;

            ListBox selectedLeaderboard = GetLeaderboard();

            if (selectedLeaderboard != null)
            {
                selectedLeaderboard.Visible = true;

                if (selectedLeaderboard == listBox2)
                    label7.Text = "Easy";
                else if (selectedLeaderboard == listBox3)
                    label7.Text = "Normal";
                else if (selectedLeaderboard == listBox4)
                    label7.Text = "Medium";
                else
                    label7.Text = "Pro";

                label7.Visible = true;
            }

            label8.Visible = true;
            label9.Visible = true;
            label10.Visible = true;
        }

        // RETURN TO HOME SCREEN
        // Leaves the leaderboard screen and returns to the home screen.
        private void button15_Click(object sender, EventArgs e)
        {
            timer1.Stop();

            gameStarted = false;
            countdownValue = 3;
            elapsedSeconds = 0;

            richTextBox1.Visible = false;
            textBox1.Visible = false;
            label1.Visible = false;
            label3.Visible = false;

            listBox1.Visible = false;
            HideLeaderboards();

            label2.Visible = false;
            label4.Visible = false;
            label12.Visible = false;

            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button13.Visible = true;
            button15.Visible = false;

            label11.Visible = true;
            button1.Visible = true;
            button14.Visible = true;
        }

        // EXIT BUTTON ON HOME SCREEN
        // Closes the application from the home screen.
        private void button14_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // OPTIONAL DESIGNER EVENT HANDLERS
        // Keep these if your Designer connects to them.

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e) { }
        private void listBox3_SelectedIndexChanged(object sender, EventArgs e) { }
        private void listBox4_SelectedIndexChanged(object sender, EventArgs e) { }
        private void listBox5_SelectedIndexChanged(object sender, EventArgs e) { }

        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void label7_Click(object sender, EventArgs e) { }
        private void label8_Click(object sender, EventArgs e) { }
        private void label9_Click(object sender, EventArgs e) { }
        private void label10_Click(object sender, EventArgs e) { }
        private void label11_Click(object sender, EventArgs e) { }
        private void label12_Click(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
    }
}
