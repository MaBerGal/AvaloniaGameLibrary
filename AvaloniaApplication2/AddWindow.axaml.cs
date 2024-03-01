using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaApplication2.Model;

namespace AvaloniaApplication2
{
    // Class that allows user's input in order to add a new video game to the list.
    public partial class AddWindow : Window
    {
        // Slider for selecting the rating of the game.
        private Slider ratingSlider;

        // TextBlock to display the selected rating value.
        private TextBlock ratingValueTextBlock;

        // Image control for picking an image.
        private Image coolImagePicker;

        // ComboBox for selecting the genre of the game.
        private ComboBox genreComboBox;

        // Variable to store the selected image path.
        private string selectedImagePath;

        public AddWindow()
        {
            // Initialize the window components.
            InitializeComponent(); 
            // Initialize the genre selection ComboBox.
            InitializeGenreComboBox(); 
            
            // Set the application icon
            string iconPath = "../../../icon_add_window.png";
            Icon = new WindowIcon(iconPath);
        }

        // Method for initializing window components programatically.
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            // Find and initialize the rating slider.
            ratingSlider = this.FindControl<Slider>("RatingSlider");

            // Find and initialize the text block for rating.
            ratingValueTextBlock = this.FindControl<TextBlock>("RatingValueTextBlock");

            // Find and initialize the image picker.
            coolImagePicker = this.FindControl<Image>("CoolImagePicker");

            // Find and initialize the genre selection ComboBox.
            genreComboBox = this.FindControl<ComboBox>("GenreComboBox");

            // Find and initialize the DatePicker and textboxes.
            ReleaseYearDatePicker = this.FindControl<DatePicker>("ReleaseYearDatePicker");
            DirectorTextBox = this.FindControl<TextBox>("DirectorTextBox");
            TitleTextBox = this.FindControl<TextBox>("TitleTextBox");
            DescriptionTextBox = this.FindControl<TextBox>("DescriptionTextBox");
            
            // Initialize radio buttons.
            MultiplayerYesRadioButton = this.FindControl<RadioButton>("MultiplayerYesRadioButton");
            MultiplayerNoRadioButton = this.FindControl<RadioButton>("MultiplayerNoRadioButton");
            
            // Initialize the image placeholder text.
            ImagePlaceholder = this.FindControl<TextBlock>("ImagePlaceholder");
            
            // Subscribe to the rating slider value change event.
            // This is one of the reasons we're initializing programatically, since adding this event in the
            // .xaml file was proving to be troublesome.
            ratingSlider.ValueChanged += RatingSlider_ValueChanged;
        }

        // Method to populate the genre combobox.
        private void InitializeGenreComboBox()
        {
            // Clear any existing items.
            genreComboBox.Items.Clear();

            // Add genres from the dictionary to the ComboBox.
            foreach (var genre in GenresDictionary.GenreDictionary)
            {
                genreComboBox.Items.Add(new ComboBoxItem
                {
                    Content = genre.Value,
                    Tag = genre.Key.ToString()
                });
            }

            // Select the first genre by default.
            if (genreComboBox.Items.Count > 0)
                genreComboBox.SelectedIndex = 0;
        }
            
        // Method to create a VideoGame object given the input.
        private VideoGame CreateVideoGameFromInput()
        {
            // Extract input data from controls.
            string director = DirectorTextBox.Text;
            string title = TitleTextBox.Text;
            int releaseYear = ReleaseYearDatePicker.SelectedDate?.Year ?? DateTime.Now.Year;
            float rating = (float)ratingSlider.Value;
            bool multiplayer = MultiplayerYesRadioButton.IsChecked ?? false;
            char genre = GetSelectedGenre();
            string description = DescriptionTextBox.Text;

            // Extract image data if an image is selected.
            byte[] imageData = null;

            // If an image has been picked in the OpenFileDialog.
            if (!string.IsNullOrEmpty(selectedImagePath))
            {
                // Use FileStream to read the image directly into the byte array.
                using (var fileStream = new FileStream(selectedImagePath, FileMode.Open, FileAccess.Read))
                {
                    imageData = new byte[fileStream.Length];
                    fileStream.Read(imageData, 0, imageData.Length);
                }
            }
            // If no image has been picked.
            else
            {
                // Go to the project's working directory to search for the default image.
                string executableDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string projectDirectory = Path.Combine(executableDirectory, "..", "..","..");
                string defaultImagePath = Path.Combine(projectDirectory, "noimage.png");

                // Check if the default image exists.
                if (File.Exists(defaultImagePath))
                {
                    // If it exists, read it into the byte array.
                    using (var fileStream = new FileStream(defaultImagePath, FileMode.Open, FileAccess.Read))
                    {
                        imageData = new byte[fileStream.Length];
                        fileStream.Read(imageData, 0, imageData.Length);
                    }
                }
            }

            // Create a new VideoGame object with the retrieved data.
            VideoGame newGame = new VideoGame(director, title, releaseYear, rating, multiplayer, genre, description, 
                imageData);

            // Return the new video game.
            return newGame;
        }

        // Method for obtaining the selected genre option in the combobox.
        private char GetSelectedGenre()
        {
            // Verify that it is indeed a combobox item and that the Tag is of type string, then initializes a variable
            // with it.
            if (genreComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string genreCode)
            {
                // Return the first character (char) from the Tag aka the key.
                return genreCode[0];
            }

            // Default to 'O' (Other) if no genre is selected.
            return 'O';
        }
        
        // Event method to check if a limit of 40 characters has been reached in the comboboxes.
        // Prevents further input if so.
        private void LimitChecker(object? sender, TextChangedEventArgs textChangedEventArgs)
        {
            // Get the TextBox that is sending the information.
            TextBox textBox = (TextBox) sender;
            // Establish a maximum character limit.
            int maxLength = 40;

            // If the contents equal or exceed the character limit.
            if (textBox.Text.Length >= maxLength)
            {
                // Remove the last character.
                textBox.Text = textBox.Text.Substring(0, maxLength - 1);
                // Set the caret position at the end.
                textBox.CaretIndex = textBox.Text.Length;
                // Prevent further input when the maximum length is reached.
                textChangedEventArgs.Handled = true;
            }
        }

        // Event method for when the Save button is clicked.
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new VideoGame object using the input data.
            VideoGame newGame = CreateVideoGameFromInput();

            // Add the new game to the MainWindow's list of video games.
            (Owner as MainWindow)?.AddVideoGame(newGame);

            // Close the AddWindow.
            Close();
        }
        
        // Event method for when the Remove Image button is clicked.
        private void RemoveImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Remove the currently selected image.
            coolImagePicker.Source = null;
            // Remove the selected path to the image.
            selectedImagePath = null;
            // Reset the placeholder text.
            ImagePlaceholder.Text = "Click here to add an image";
        }

        // Event method for when the slider is dragged.
        private void RatingSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // Update the text block with the current value of the slider, formatted to 1 decimal.
            ratingValueTextBlock.Text = $"{ratingSlider.Value:F1}";
        }

        // Event method for when the image container/OpenFileDialog control is clicked.
        private async void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Create a new OpenFileDialog and admit different image extensions.
            // Only allows one image to be picked at a time. If multiple are chosen, it'll retrieve the first one.
            var openFileDialog = new OpenFileDialog
            {
                Filters =
                {
                    new FileDialogFilter { Name = "Image Files", Extensions = { "jpg", "jpeg", "png", "bmp", "gif" } }
                },
                AllowMultiple = false
            };

            // Show the OpenFileDialog asynchronously and wait for a result.
            var chosenImages = await openFileDialog.ShowAsync(this);
            if (coolImagePicker == null)
            {
                // For debugging.
                Console.WriteLine("The coolest image picker ever is null, for some reason.");
            }

            // If an image has indeed been chosen, get the first element from the chosen images (first image selected)
            // and save it to the selectedImagePath class attribute. Then, show the image using the BitMap class.
            // Remove the placeholder text when there's an image in place.
            if (chosenImages != null && chosenImages.Length > 0)
            {
                selectedImagePath = chosenImages[0];
                coolImagePicker.Source = new Bitmap(selectedImagePath);
                ImagePlaceholder.Text = "";
            }
        }
        
        // Event method for when the pointer hovers over the Cancel textblock.
        private void InputElement_OnPointerEntered(object? sender, PointerEventArgs e)
        {
            ((TextBlock)sender!).Foreground = Brushes.Blue;
        }
        
        // Event method for when the pointer leaves the Cancel textblock.
        private void InputElement_OnPointerExited(object? sender, PointerEventArgs e)
        {
            ((TextBlock)sender!).Foreground = Brushes.DodgerBlue;
        }
        
        // Event method for when the pointer clicks the Cancel textblock.
        private void Cancel_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            Close();
        }

        // Event method for when the pointer hovers over the Border control inside of which the image is shown.
        private void BorderPointerEntered(object sender, PointerEventArgs e)
        {
            if (sender is Border border)
            {
                // Change the background color when the pointer enters.
                border.Background = Brushes.DodgerBlue;
            }
        }

        // Event method for when the pointer leaves over the Border control.
        private void BorderPointerExited(object sender, PointerEventArgs e)
        {
            if (sender is Border border)
            {
                // Change the background color back to the original color when the pointer exits.
                border.Background = Brushes.DarkGray;
            }
        }
        
    }

}
