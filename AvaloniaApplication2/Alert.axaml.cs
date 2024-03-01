using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaApplication2
{
    // Class for displaying a custom alert dialog.
    public partial class Alert : Window
    {

        // Property to control the visibility of the Cancel button.
        public bool ShowCancelButton { get; set; } = true;
        
        // Receive a title and content for the dialog, as well as a bool indicating whether to show the Cancel button.
        // or not.
        public Alert(string title, string content, bool showCancelButton)
        {
            // Initialize controls (aka the content).
            InitializeComponent();
            Title = title;
            AlertText.Text = content;
            ShowCancelButton = showCancelButton;
            // Initialize buttons (aka mainly checking if the Cancel button will be shown or not).
            InitializeButtons();
            // Set the WindowStartupLocation to CenterOwner.
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            
            // Set the application icon
            string iconPath = "../../../icon_alert_window.png";
            Icon = new WindowIcon(iconPath);
        }

        // Method for initializing controls.
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            // Find and initialize the content.
            AlertText = this.FindControl<TextBlock>("AlertText");
        }

        // Method for initializing the dialog options.
        private void InitializeButtons()
        {
            // Find and initialize the buttons.
            Button okButton = this.FindControl<Button>("OkButton");
            Button cancelButton = this.FindControl<Button>("CancelButton");
            
            // Set visibility of the Cancel button based on the property.
            if (!ShowCancelButton)
            {
                cancelButton.IsVisible = false;
                cancelButton.IsEnabled = false;
            }
        }

        // Event method for when the OK button is clicked.
        private void OkButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Close with a true result.
            Close(true); 
        }

        // Event method for when the Cancel button is clicked.
        private void CancelButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Close with a false result.
            Close(false);
        }
    }
}