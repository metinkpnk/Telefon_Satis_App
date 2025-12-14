using System.Windows;

namespace TelefonSatısApp
{
    public partial class TemaliMesajPenceresi : Window
    {
        public TemaliMesajPenceresi(string title, string message)
        {
            InitializeComponent();
            TxtTitle.Text = title;
            TxtMessage.Text = message;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}

