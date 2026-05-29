using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CyberbotGUI.Helpers;
using CyberbotGUI.Models;
using CyberbotGUI.Services;

namespace CyberbotGUI
{
    public partial class MainWindow : Window
    {
       
        private readonly Chatbot _chatbot;
        private readonly ObservableCollection<ChatMessage> _messages;

        private bool _waitingForName = true;
        private bool _isPlaceholderActive = true;

        private const string PlaceholderText = "Type your message or ask about cybersecurity...";

        
        public MainWindow()
        {
            InitializeComponent();

            _chatbot = new Chatbot();
            _messages = new ObservableCollection<ChatMessage>();
            MessagesControl.ItemsSource = _messages;

            SetPlaceholder();

            Loaded += MainWindow_Loaded;
        }

       
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
           
            try { AudioPlayer.PlayGreeting(); } catch { /* skip if unavailable */ }

            // Initial welcome messages with a natural stagger
            AddBotMessage("Welcome to the Cybersecurity Awareness Bot! 🛡️");
            DelayedBotMessage("I'm here to help South African citizens stay safe online.", 700);
            DelayedBotMessage("Before we begin — what's your name?", 1400);
        }

        // ── Input handling ─────────────────────────────────────────────
        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                ProcessInput();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) => ProcessInput();

        private void ProcessInput()
        {
            if (_isPlaceholderActive) return;

            string input = InputTextBox.Text.Trim();
            if (!Validator.IsValid(input)) return;

            // Clear the input box
            InputTextBox.Text = "";

            // Add user message to chat
            AddUserMessage(input);

            // ── Name capture state ────────────────────────────────────
            if (_waitingForName)
            {
                _chatbot.UserName = input;
                _waitingForName = false;
                UserNameLabel.Text = $"👤  {input}";
                UpdateMemorySidebar();

                DelayedBotMessage($"Hello, {input}! 👋 Great to meet you.", 600);
                DelayedBotMessage(
                    "I'm your Cybersecurity Awareness Bot. You can ask me about:\n\n" +
                    "🔐 Passwords  •  🎣 Phishing  •  🕵️ Privacy  •  🦠 Malware\n" +
                    "🌐 VPNs  •  🔑 Two-Factor Auth  •  🖥️ Safe Browsing  •  🎭 Social Engineering\n\n" +
                    "Or use the quick-topic buttons on the right. What would you like to know?", 1300);
                return;
            }

            // ── Sentiment detection ───────────────────────────────────
            var (sentiment, acknowledgment) = SentimentDetector.Detect(input);
            ShowSentimentBar(sentiment);

            // ── Get chatbot response ──────────────────────────────────
            string response = _chatbot.GetResponse(input, sentiment);

            // Show sentiment acknowledgment first, then the main response
            if (!string.IsNullOrEmpty(acknowledgment))
            {
                DelayedBotMessage(acknowledgment, 500);
                DelayedBotMessage(response, 1100);
            }
            else
            {
                DelayedBotMessage(response, 550);
            }

            UpdateMemorySidebar();
        }

        // ── Message helpers ────────────────────────────────────────────
        private void AddBotMessage(string text)
        {
            _messages.Add(new ChatMessage
            {
                IsBot = true,
                Text = text,
                Timestamp = DateTime.Now.ToString("HH:mm")
            });
            ScrollToBottom();

            if (VoiceToggle.IsChecked == true)
                try { VoiceAssistant.Speak(text); } catch { }
        }

        private void AddUserMessage(string text)
        {
            _messages.Add(new ChatMessage
            {
                IsBot = false,
                Text = text,
                Timestamp = DateTime.Now.ToString("HH:mm")
            });
            ScrollToBottom();
        }

        /// <summary>
        /// Adds a bot message after a short delay to simulate a natural typing feel.
        /// </summary>
        private void DelayedBotMessage(string text, int delayMs)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(delayMs) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                AddBotMessage(text);
            };
            timer.Start();
        }

        private void ScrollToBottom()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                new Action(() => ChatScrollViewer.ScrollToEnd()));
        }

        // ── Sentiment bar ──────────────────────────────────────────────
        private void ShowSentimentBar(Sentiment sentiment)
        {
            if (sentiment == Sentiment.None)
            {
                SentimentBar.Visibility = Visibility.Collapsed;
                return;
            }

            SentimentBar.Visibility = Visibility.Visible;
            SentimentIcon.Text = SentimentDetector.GetEmoji(sentiment);
            SentimentText.Text = SentimentDetector.GetLabel(sentiment);

            SentimentBar.Background = sentiment switch
            {
                Sentiment.Worried    => new SolidColorBrush(Color.FromRgb(0x3D, 0x1A, 0x1A)),
                Sentiment.Frustrated => new SolidColorBrush(Color.FromRgb(0x3D, 0x28, 0x0A)),
                Sentiment.Confused   => new SolidColorBrush(Color.FromRgb(0x10, 0x1C, 0x3A)),
                Sentiment.Curious    => new SolidColorBrush(Color.FromRgb(0x0A, 0x2A, 0x18)),
                Sentiment.Happy      => new SolidColorBrush(Color.FromRgb(0x0A, 0x2A, 0x18)),
                _                    => new SolidColorBrush(Color.FromRgb(0x16, 0x1B, 0x22))
            };

            SentimentText.Foreground = sentiment switch
            {
                Sentiment.Worried    => new SolidColorBrush(Color.FromRgb(0xFF, 0x7B, 0x72)),
                Sentiment.Frustrated => new SolidColorBrush(Color.FromRgb(0xF7, 0x8B, 0x1E)),
                Sentiment.Confused   => new SolidColorBrush(Color.FromRgb(0x79, 0xC0, 0xFF)),
                Sentiment.Curious    => new SolidColorBrush(Color.FromRgb(0x3F, 0xB9, 0x50)),
                Sentiment.Happy      => new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x88)),
                _                    => new SolidColorBrush(Color.FromRgb(0xCD, 0xD9, 0xE5))
            };
        }

        private void DismissSentiment_Click(object sender, RoutedEventArgs e)
            => SentimentBar.Visibility = Visibility.Collapsed;

        // ── Memory sidebar ─────────────────────────────────────────────
        private void UpdateMemorySidebar()
        {
            MemoryUserName.Text = string.IsNullOrEmpty(_chatbot.UserName) ? "—" : _chatbot.UserName;
            MemoryFavTopic.Text = string.IsNullOrEmpty(_chatbot.FavouriteTopic) ? "—" : _chatbot.FavouriteTopic;

            // Refresh topics list
            TopicsControl.ItemsSource = null;
            TopicsControl.ItemsSource = _chatbot.TopicsDiscussed;
        }

        // ── Quick topic buttons ────────────────────────────────────────
        private void QuickTopic_Click(object sender, RoutedEventArgs e)
        {
            if (_waitingForName) return; // Ignore quick topics until name is set

            if (sender is Button btn && btn.Tag is string query)
            {
                // Inject the query directly as if the user typed it
                InputTextBox.Text = query;
                _isPlaceholderActive = false;
                InputTextBox.Foreground = new SolidColorBrush(Color.FromRgb(0xCD, 0xD9, 0xE5));
                ProcessInput();
            }
        }

        // ── Clear chat ─────────────────────────────────────────────────
        private void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            _messages.Clear();
            SentimentBar.Visibility = Visibility.Collapsed;

            if (!_waitingForName)
                AddBotMessage($"Chat cleared! I still remember you, {_chatbot.UserName}. What would you like to know about cybersecurity?");
        }

        // ── Placeholder text ───────────────────────────────────────────
        private void InputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_isPlaceholderActive)
            {
                InputTextBox.Text = "";
                InputTextBox.Foreground = new SolidColorBrush(Color.FromRgb(0xCD, 0xD9, 0xE5));
                _isPlaceholderActive = false;
            }
        }

        private void InputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
                SetPlaceholder();
        }

        private void SetPlaceholder()
        {
            InputTextBox.Text = PlaceholderText;
            InputTextBox.Foreground = new SolidColorBrush(Color.FromRgb(0x48, 0x4F, 0x58));
            _isPlaceholderActive = true;
        }
    }
}
