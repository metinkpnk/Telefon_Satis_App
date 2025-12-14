using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TelefonSatısApp
{
    public partial class ThemedMessageBox : Window
    {
        private MessageBoxResult _result = MessageBoxResult.None;

        private ThemedMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage image)
        {
            InitializeComponent();
            TitleText.Text = title;
            MessageText.Text = message;
            ConfigureIcon(image);
            ConfigureButtons(buttons);
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            var dialog = new ThemedMessageBox(message, title, buttons, image);
            var owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? Application.Current.MainWindow;
            if (owner != null)
            {
                dialog.Owner = owner;
            }
            dialog.WindowStartupLocation = dialog.Owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen;
            dialog.ShowDialog();
            return dialog._result;
        }

        private void ConfigureIcon(MessageBoxImage image)
        {
            Brush badgeBrush = (Brush)FindResource("PrimaryBrush");
            string glyph = "i";

            switch (image)
            {
                case MessageBoxImage.Warning:
                    badgeBrush = (Brush)FindResource("WarningBrush");
                    glyph = "!";
                    break;
                case MessageBoxImage.Error:
                    badgeBrush = (Brush)FindResource("ErrorBrush");
                    glyph = "!";
                    break;
                case MessageBoxImage.Question:
                    badgeBrush = (Brush)FindResource("PrimaryDarkBrush");
                    glyph = "?";
                    break;
                case MessageBoxImage.Information:
                default:
                    badgeBrush = (Brush)FindResource("PrimaryBrush");
                    glyph = "i";
                    break;
            }

            IconBadge.Background = badgeBrush;
            IconGlyph.Text = glyph;
        }

        private void ConfigureButtons(MessageBoxButton buttons)
        {
            SecondaryButton.Visibility = Visibility.Collapsed;
            PrimaryButton.Tag = MessageBoxResult.OK;
            SecondaryButton.Tag = MessageBoxResult.Cancel;

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    PrimaryButton.Content = "Tamam";
                    PrimaryButton.Tag = MessageBoxResult.OK;
                    break;
                case MessageBoxButton.OKCancel:
                    PrimaryButton.Content = "Tamam";
                    PrimaryButton.Tag = MessageBoxResult.OK;
                    SecondaryButton.Content = "İptal";
                    SecondaryButton.Tag = MessageBoxResult.Cancel;
                    SecondaryButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNo:
                    PrimaryButton.Content = "Evet";
                    PrimaryButton.Tag = MessageBoxResult.Yes;
                    SecondaryButton.Content = "Hayır";
                    SecondaryButton.Tag = MessageBoxResult.No;
                    SecondaryButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    PrimaryButton.Content = "Evet";
                    PrimaryButton.Tag = MessageBoxResult.Yes;
                    SecondaryButton.Content = "Hayır";
                    SecondaryButton.Tag = MessageBoxResult.No;
                    SecondaryButton.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            if (PrimaryButton.Tag is MessageBoxResult result)
            {
                _result = result;
            }
            Close();
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            if (SecondaryButton.Tag is MessageBoxResult result)
            {
                _result = result;
            }
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _result = SecondaryButton.Visibility == Visibility.Visible
                    ? (SecondaryButton.Tag as MessageBoxResult?) ?? MessageBoxResult.Cancel
                    : MessageBoxResult.Cancel;
                Close();
            }
            else if (e.Key == Key.Enter)
            {
                _result = PrimaryButton.Tag is MessageBoxResult result ? result : MessageBoxResult.OK;
                Close();
            }
        }
    }
}

