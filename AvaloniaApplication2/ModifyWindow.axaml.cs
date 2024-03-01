using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaApplication2.Model;

namespace AvaloniaApplication2
{
    // Class that allows users to modify an existing video game.
    public partial class ModifyWindow : Window
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
        
        // Flag to check whether the game to be modified has an image already.
        private bool hasImage;

        // For working with the game to be modified.
        private VideoGame originalGame;

        // Constructor that initializes the ModifyWindow with an existing VideoGame.
        public ModifyWindow(VideoGame game)
        {
            // Initialize the different controls, attributes as well as the combobox.
            InitializeComponent(game);
            InitializeGenreComboBox();
            // Pre-select the genre in the ComboBox based on the original game's genre.
            SelectGenreInComboBox(game.Genre.ToString());

            // Store the original game.
            originalGame = game;

            // Check if the game has an image originally (can't compare to null, so we check its length).
            hasImage = originalGame.ImageData?.Length > 0;
            
            // Set the application icon
            string iconPath = "../../../icon_modify_window.png";
            Icon = new WindowIcon(iconPath);
        }

        // Method to initialize the components of the window programmatically.
        private void InitializeComponent(VideoGame game)
        {
            AvaloniaXamlLoader.Load(this);

            // Find and initialize the slider, its textblock, the image and combobox controls.
            ratingSlider = this.FindControl<Slider>("RatingSlider");
            ratingValueTextBlock = this.FindControl<TextBlock>("RatingValueTextBlock");
            coolImagePicker = this.FindControl<Image>("CoolImagePicker");
            genreComboBox = this.FindControl<ComboBox>("GenreComboBox");
            
            // Find and initialize the DatePicker plus the textboxes.
            ReleaseYearDatePicker = this.FindControl<DatePicker>("ReleaseYearDatePicker");
            DirectorTextBox = this.FindControl<TextBox>("DirectorTextBox");
            TitleTextBox = this.FindControl<TextBox>("TitleTextBox");
            DescriptionTextBox = this.FindControl<TextBox>("DescriptionTextBox");

            // Find and initialize the multiplayer choice radio buttons.
            MultiplayerYesRadioButton = this.FindControl<RadioButton>("MultiplayerYesRadioButton");
            MultiplayerNoRadioButton = this.FindControl<RadioButton>("MultiplayerNoRadioButton");

            // Find and initialize the image placeholder text.
            ImagePlaceholder = this.FindControl<TextBlock>("ImagePlaceholder");

            // Pre-fill the input fields with the data from the original game.
            DirectorTextBox.Text = game.Director;
            TitleTextBox.Text = game.Title;
            ReleaseYearDatePicker.SelectedDate = new DateTime(game.ReleaseYear, 1, 1);
            ratingSlider.Value = game.Rating;
            DescriptionTextBox.Text = game.Description;

            // Subscribe to the rating slider value change event.
            // This is one of the reasons we're initializing programatically, since adding this event in the
            // .xaml file was proving to be troublesome.
            ratingSlider.ValueChanged += RatingSlider_ValueChanged;

            // Pre-select the multiplayer radio button based on the original game's multiplayer value.
            if (game.Multiplayer)
            {
                MultiplayerYesRadioButton.IsChecked = true;
            }
            else
            {
                MultiplayerNoRadioButton.IsChecked = true;
            }

            // Pre-load the image into the Image control if the game has an image.
            if (game.ImageData != null && game.ImageData.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream(game.ImageData))
                {
                    coolImagePicker.Source = new Bitmap(ms);
                }
            }
        }

        // Method to initialize the genre ComboBox.
        private void InitializeGenreComboBox()
        {
            // Clear any existing items.
            genreComboBox.Items.Clear();

            // Add genres from the dictionary to the ComboBox.
            foreach (var genreEntry in GenresDictionary.GenreDictionary)
            {
                genreComboBox.Items.Add(new ComboBoxItem
                {
                    Content = genreEntry.Value,
                    Tag = genreEntry.Key.ToString() 
                });
            }

            // Select the first genre by default.
            if (genreComboBox.Items.Count > 0)
                genreComboBox.SelectedIndex = 0;
        }

        // Method to select a genre in the ComboBox.
        private void SelectGenreInComboBox(string genreCode)
        {
            // Iterate through the different items in the combobox.
            foreach (ComboBoxItem item in genreComboBox.Items)
            {
                // Verify that each key is a string, initialize the tag variable with it and compare it to the game's
                // genre key.
                if (item.Tag is string tag && tag.Equals(genreCode))
                {
                    // Make it match the game's genre if there's a coincidence.
                    genreComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        // Method to modify the original VideoGame with input from the user.
        private void ModifyOriginalGameWithInput()
        {
            // Extract input data from controls.
            string director = DirectorTextBox.Text;
            string title = TitleTextBox.Text;
            int releaseYear = ReleaseYearDatePicker.SelectedDate?.Year ?? DateTime.Now.Year;
            float rating = (float)ratingSlider.Value;
            bool multiplayer = MultiplayerYesRadioButton.IsChecked ?? false;
            char genre = GetSelectedGenre();
            string description = DescriptionTextBox.Text;
            byte[] imageData = null;
            
            // Extract image data if an image is selected.
            if (!string.IsNullOrEmpty(selectedImagePath))
            {
                // Use FileStream to read the image directly into a byte array.
                using (var fileStream = new FileStream(selectedImagePath, FileMode.Open, FileAccess.Read))
                {
                    imageData = new byte[fileStream.Length];
                    fileStream.Read(imageData, 0, imageData.Length);
                }
            }
            // If there's no image selected and the game has no image, proceed with loading the default one.
            else if (!hasImage)
            {
                // Load the default image (noimage.png) by moving to the project's working directory.
                string executableDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string projectDirectory = Path.Combine(executableDirectory, "..", "..", "..");
                string defaultImagePath = Path.Combine(projectDirectory, "noimage.png");

                // Check if the default image exists. Read it into the array if so.
                if (File.Exists(defaultImagePath))
                {
                    using (var fileStream = new FileStream(defaultImagePath, FileMode.Open, FileAccess.Read))
                    {
                        imageData = new byte[fileStream.Length];
                        fileStream.Read(imageData, 0, imageData.Length);
                    }
                }
            }

            // Modify the properties of the original VideoGame object with the new ones.
            originalGame.Director = director;
            originalGame.Title = title;
            originalGame.ReleaseYear = releaseYear;
            originalGame.Rating = rating;
            originalGame.Multiplayer = multiplayer;
            originalGame.Genre = genre;
            originalGame.Description = description;

            // Update the image if there's new image data, be it a new image or the default one.
            if (imageData != null) originalGame.ImageData = imageData;
        }

        // Method to get the selected genre from the ComboBox.
        private char GetSelectedGenre()
        {
            // Check that it's indeed a combobox item and then check that it's a string. Save the string to a variable.
            // Retrieve the first character from the string, aka the key.
            if (genreComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string genreCode)
            {
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

        // Event method for the Save button click.
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Modify the existing VideoGame object with the updated input data.
            ModifyOriginalGameWithInput();

            // Close the ModifyWindow.
            Close(true);
        }

        // Event method for the Remove Image button click.
        private void RemoveImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Remove the currently selected image.
            coolImagePicker.Source = null;
            // Remove the selected image's path.
            selectedImagePath = null;
            // Add the placeholder text back.
            ImagePlaceholder.Text = "Click here to add an image";
            // Set the flag to indicate that there is no image.
            hasImage = false; 
        }

        // Event method for slider value change (aka sliding the slider).
        private void RatingSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            // Update the text block with the current value of the slider, formatted to show 1 decimal.
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
