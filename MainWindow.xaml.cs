using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mastermind
{
    public partial class MainWindow : Window
    {
        private readonly List<string> colors = new List<string> { "Rood", "Geel", "Oranje", "Wit", "Groen", "Blauw" };
        private List<string> secretCode;
        private List<string> playerNames = new List<string>();
        private string playerName = "";
        private int attempts = 0;
        private int maxAttempts = 10;
        private int score = 100;
        private string[] highscores = new string[15];
        private Timer countdownTimer;
        private int timeLeft = 10;
        private int currentPlayerIndex = 0;


        public MainWindow()
        {
            InitializeComponent();
            StartGame();
        }

        private void StartGame()
        {
            // Controleer of er spelers zijn
            if (playerNames.Count == 0)
            {
                MessageBox.Show("Er zijn geen spelers toegevoegd. Het spel wordt afgesloten.");
                Application.Current.Shutdown();
                return;
            }

            // Stel de naam in van de huidige speler
            playerName = playerNames[currentPlayerIndex];

            // Toon de naam van de speler
            MessageBox.Show($"Het spel begint voor: {playerName}", "Nieuwe speler");

            // Reset spelwaarden
            foreach (var comboBox in new[] { ColorBox1, ColorBox2, ColorBox3, ColorBox4 })
            {
                comboBox.ItemsSource = colors;
                comboBox.SelectedIndex = 0;
            }

            secretCode = GenerateRandomCode();
            DebugTextBox.Text = string.Join(", ", secretCode);
            HistoryPanel.Children.Clear();
            attempts = 0;
            score = 100;
            UpdateAttemptsLabel();
            UpdateScoreLabel();
            StartCountdown();
        }


        private string AskPlayerName()
        {
            string name = "";
            bool addMore = true;

            do
            {
                // Vraag de speler om een naam in te voeren
                name = Microsoft.VisualBasic.Interaction.InputBox(
                    "Voer je naam in:", "Spelernaam", "");

                // Controleer of de naam geldig is
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("De naam mag niet leeg zijn. Probeer opnieuw.");
                    continue;
                }

                // Voeg de naam toe als deze nog niet bestaat
                if (!playerNames.Contains(name))
                {
                    playerNames.Add(name);

                }
                else
                {
                    MessageBox.Show($"Naam '{name}' bestaat al. Probeer opnieuw.");
                }

                // Vraag of de speler nog een naam wil toevoegen
                var result = MessageBox.Show("Wil je nog een speler toevoegen?", "Speler toevoegen", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    addMore = false;
                }
            } while (addMore);

            // Retourneer de eerste ingevoerde naam als de actieve speler
            return playerNames.First();
        }



        private void MenuNewGame_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void MenuHighscores_Click(object sender, RoutedEventArgs e)
        {
            string highscoreList = string.Join(Environment.NewLine, highscores.Where(h => !string.IsNullOrEmpty(h)));
            MessageBox.Show(highscoreList, "Highscores");
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuSetAttempts_Click(object sender, RoutedEventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Voer het maximaal aantal pogingen in (tussen 3 en 20):",
                "Aantal pogingen", maxAttempts.ToString());

            if (int.TryParse(input, out int newMax) && newMax >= 3 && newMax <= 20)
            {
                maxAttempts = newMax;
                UpdateAttemptsLabel();
            }
            else
            {
                MessageBox.Show("Voer een geldig getal in tussen 3 en 20.");
            }
        }

        private List<string> GenerateRandomCode()
        {
            Random random = new Random();
            return Enumerable.Range(0, 4).Select(_ => colors[random.Next(colors.Count)]).ToList();
        }

        private void CheckCode_Click(object sender, RoutedEventArgs e)
        {
            StopCountdown();

            var guessedCode = new List<string>
            {
                ColorBox1.Text,
                ColorBox2.Text,
                ColorBox3.Text,
                ColorBox4.Text
            };

            if (guessedCode.SequenceEqual(secretCode))
            {
                EndGame("Gefeliciteerd! Je hebt de code gekraakt!");
                return;
            }

            ProvideFeedback(guessedCode);
            UpdateScore(guessedCode);

            attempts++;
            UpdateAttemptsLabel();

            if (attempts >= maxAttempts)
            {
                EndGame($"Helaas, je hebt geen pogingen meer. De code was: {string.Join(", ", secretCode)}");
            }
            else
            {
                StartCountdown();
            }
        }

        private void ProvideFeedback(List<string> guessedCode)
        {
            var feedbackRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
            for (int i = 0; i < guessedCode.Count; i++)
            {
                var border = new Border
                {
                    Child = new TextBlock
                    {
                        Text = guessedCode[i],
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    BorderThickness = new Thickness(2),
                    Margin = new Thickness(5)
                };

                if (guessedCode[i] == secretCode[i])
                {
                    border.BorderBrush = Brushes.DarkRed;
                }
                else if (secretCode.Contains(guessedCode[i]))
                {
                    border.BorderBrush = Brushes.Wheat;
                }
                else
                {
                    border.BorderBrush = Brushes.Gray;
                }

                feedbackRow.Children.Add(border);
            }
            HistoryPanel.Children.Add(feedbackRow);
        }

        private void UpdateScore(List<string> guessedCode)
        {
            for (int i = 0; i < guessedCode.Count; i++)
            {
                if (guessedCode[i] == secretCode[i])
                {
                    // Geen strafpunten
                }
                else if (secretCode.Contains(guessedCode[i]))
                {
                    score -= 1; // 1 strafpunt
                }
                else
                {
                    score -= 2; // 2 strafpunten
                }
            }
            UpdateScoreLabel();
        }

        private void UpdateScoreLabel()
        {
            ScoreLabel.Content = $"Score: {score}";
        }

        private void UpdateAttemptsLabel()
        {
            AttemptsLabel.Content = $"Pogingen: {attempts} / {maxAttempts}";
        }

        private void StartCountdown()
        {
            timeLeft = 10;
            TimerLabel.Content = $"Tijd: {timeLeft}s";

            if (countdownTimer == null)
            {
                countdownTimer = new Timer(1000);
                countdownTimer.Elapsed += CountdownTimer_Elapsed;
            }

            countdownTimer.Start();
        }

        private void CountdownTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timeLeft--;
            Dispatcher.Invoke(() =>
            {
                TimerLabel.Content = $"Tijd: {timeLeft}s";
                if (timeLeft <= 0)
                {
                    countdownTimer.Stop();
                    attempts++;
                    UpdateAttemptsLabel();

                    if (attempts >= maxAttempts)
                    {
                        EndGame($"Helaas, je hebt geen pogingen meer. De code was: {string.Join(", ", secretCode)}");
                    }
                    else
                    {
                        StartCountdown();
                    }
                }
            });
        }

        private void StopCountdown()
        {
            countdownTimer?.Stop();
        }

        private void EndGame(string message)
        {
            // Voeg de score van de speler toe aan de highscores
            highscores = highscores.OrderByDescending(h => h).Take(15).ToArray();
            highscores[attempts] = $"{playerName} - {attempts} pogingen - {score}/100";

            // Toon het eindebericht
            MessageBox.Show(message);

            // Ga door naar de volgende speler
            currentPlayerIndex++;

            if (currentPlayerIndex < playerNames.Count)
            {
                // Start een nieuw spel voor de volgende speler
                StartGame();
            }
            else
            {
                // Geen spelers meer over, spel beëindigen
                MessageBox.Show("Het spel is voorbij! Bedankt voor het spelen!", "Einde van het spel");
                Application.Current.Shutdown();
            }
        }
        
        

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Weet je zeker dat je wilt afsluiten?", "Afsluiten", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}
