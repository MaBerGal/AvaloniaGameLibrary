using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaApplication2.Model;
using Newtonsoft.Json;

namespace AvaloniaApplication2
{
    // Class for showing and managing elements in the frame that is central to the application.
    public partial class MainWindow : Window
    {
        // For storing the full list of video games.
        private List<VideoGame> originalVideoGames;
        // For storing the search results/filtered list of videogames.
        private List<VideoGame> filteredVideoGames;
        // Property for storing the current list's index.
        private int currentIndex = 0;
        // Flag that tracks whether changes have been made or not in order to turn on/off the Save button.
        private bool changesMade = false;
        // For tracking which game was selected in the image grid.
        private Button previouslySelectedButton; 
        // For filtering. No filter by default.
        private string selectedFilter = "Original"; 

        // Constructor for the MainWindow.
        public MainWindow()
        {
            // Find and initialize components (not programmatically).
            InitializeComponent();
            // Initialize the secondary list.
            filteredVideoGames = new List<VideoGame>();
            // Load stored data.
            LoadData();
            // Add elements to the combobox.
            PopulateFilterComboBox();
            // Display the current game's data.
            DisplayCurrentGame();
            // Create the left-side buttons to facilitate navigation.
            CreateGameButtons();
            // Update the appearance of the first button (if there is one).
            if (filteredVideoGames.Count > 0)
            {
                UpdateGameButtonsAppearance(GetGameButtonAtIndex(0), currentIndex == 0);
            }
            // Activate or deactivate the different buttons depending on given conditions.
            UpdateButtonState();
            
            // Set the application icon
            string iconPath = "../../../icon_main_window.png";
            Icon = new WindowIcon(iconPath);
        }

        // Method to load data from the designated file (databank.data) with fallback to the project's bin folder.
        private void LoadData()
        {
            // Get the path to the Documents folder using the SpecialFolder property from Environment.
            string documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Combine the Documents folder path with the file name using the Combine method.
            string dataFilePath = Path.Combine(documentsFolderPath, "databank.data");

            // Checks if the file exists in the Documents folder.
            if (File.Exists(dataFilePath))
            {
                using (FileStream fs = new FileStream(dataFilePath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    originalVideoGames = (List<VideoGame>)formatter.Deserialize(fs);
                }
            }
            else
            {
                // If the file does not exist in Documents, try loading it from the project's bin folder.
                string binFolderPath = AppDomain.CurrentDomain.BaseDirectory;
                dataFilePath = Path.Combine(binFolderPath, "databank.data");

                if (File.Exists(dataFilePath))
                {
                    using (FileStream fs = new FileStream(dataFilePath, FileMode.Open))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        originalVideoGames = (List<VideoGame>)formatter.Deserialize(fs);
                    }
                }
                else
                {
                    // If the file is not found in either location, initialize with an empty list.
                    originalVideoGames = new List<VideoGame>();
                }
            }

            // Initialize videoGames as a copy of the original list.
            filteredVideoGames = new List<VideoGame>(originalVideoGames);
        }

        // Method to save data to the file in the Documents folder.
        private void SaveData()
        {
            try
            {
                // Get the path to the Documents folder.
                string documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                // Combine the Documents folder path with the file name.
                string dataFilePath = Path.Combine(documentsFolderPath, "databank.data");

                // Combination of FileStream and BinaryFormatter to overwrite/create and serialize the file respectively.
                using (FileStream fs = new FileStream(dataFilePath, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fs, originalVideoGames);
                }
            }
            catch (IOException ex)
            {
                // Handle I/O related issues (e.g., disk full, file in use).
                ShowAlertWindow(this, $"Error saving data: {ex.Message}", "Error", false);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle unauthorized access (e.g., lack of permissions).
                ShowAlertWindow(this, $"Unauthorized access error: {ex.Message}", "Error", false);
            }
            catch (Exception ex)
            {
                // Handle other exceptions.
                ShowAlertWindow(this, $"Unexpected error: {ex.Message}", "Error", false);
            }
        }
        
        // Method to display the current game on the UI.
        private void DisplayCurrentGame()
        {
            // Check if there are games to display.
            if (filteredVideoGames.Count > 0)
            {
                // Save the current game in the list to a temporary object.
                VideoGame currentGame = filteredVideoGames[currentIndex];

                // Initialization of labels, verifying that the game's data exists and setting placeholders when it doesn't.
                Title.Text = string.IsNullOrWhiteSpace(currentGame.Title) ? "Title not specified" : currentGame.Title;
                Director.Text = string.IsNullOrWhiteSpace(currentGame.Director) ? "Not specified" : currentGame.Director;
                ReleaseYear.Text = currentGame.ReleaseYear > 0 ? currentGame.ReleaseYear.ToString() : "Not specified";
                
                // Format rating to one decimal place.
                Rating.Text = currentGame.Rating.ToString("0.0");

                // Display "Yes" or "No" based on the Multiplayer property.
                Multiplayer.Text = currentGame.Multiplayer ? "Yes" : "No";

                // Use the genre char to get the full genre name from the dictionary using the TryGetValue method.
                if (GenresDictionary.GenreDictionary.TryGetValue(currentGame.Genre, out string genreName))
                {
                    Genre.Text = genreName;
                }
                else
                {
                    // If the genre char is not in the dictionary, set it to "Other".
                    Genre.Text = GenresDictionary.GenreDictionary['O'];
                }

                Description.Text = string.IsNullOrWhiteSpace(currentGame.Description) ? "Not specified" : currentGame.Description;

                // Update the count label.
                Count.Text = $"Game {currentIndex + 1} of {filteredVideoGames.Count}";

                // Handle loading the image, if there is one.
                if (currentGame.ImageData != null && currentGame.ImageData.Length > 0)
                {
                    // Use MemoryStream to load the image into the GUI component.
                    using (MemoryStream ms = new MemoryStream(currentGame.ImageData))
                    {
                        GameImage.Source = new Bitmap(ms);
                    }
                }
                else
                {
                    // Set GameImage.Source to null when there is no image.
                    GameImage.Source = null;
                }
            }
            else
            {
                // If there are no games, display placeholders for everything.
                Title.Text = "Title not specified";
                Director.Text = "Not specified";
                ReleaseYear.Text = "Not specified";
                Rating.Text = "Not specified";
                Multiplayer.Text = "Not specified";
                Genre.Text = "Not specified";
                Description.Text = "Not specified";
                Count.Text = "No games available";

                // Set GameImage.Source to null when there are no games.
                GameImage.Source = null;
            }
        }
        
        // Method to add elements to the filters combobox.
        private void PopulateFilterComboBox()
        {
            // Populate the ComboBox with video game attributes
            OrderComboBox.Items.Add("Title");
            OrderComboBox.Items.Add("Director");
            OrderComboBox.Items.Add("Release Year");
            OrderComboBox.Items.Add("Rating");
            OrderComboBox.Items.Add("Multiplayer");
            OrderComboBox.Items.Add("Genre");
            
        }
        
        // Method to sort the videogames given a specific filter.
        private void SortVideoGames(string selectedFilter)
        {
            switch (selectedFilter)
            {
                case "Title":
                    filteredVideoGames = filteredVideoGames.OrderBy(game => game.Title).ToList();
                    break;
                case "Director":
                    filteredVideoGames = filteredVideoGames.OrderBy(game => game.Director).ToList();
                    break;
                case "Release Year":
                    filteredVideoGames = filteredVideoGames.OrderBy(game => game.ReleaseYear).ToList();
                    break;
                case "Rating":
                    filteredVideoGames = filteredVideoGames.OrderBy(game => game.Rating).ToList();
                    break;
                case "Multiplayer":
                    filteredVideoGames = filteredVideoGames.OrderBy(game => game.Multiplayer).ToList();
                    break;
                case "Genre":
                    filteredVideoGames = filteredVideoGames.OrderBy(game => game.Genre).ToList();
                    break;
                default:
                    break;
            }
        }

        // Method to activate or deactivate buttons.
        private void UpdateButtonState()
        {
            // In the first place, check if there are any video games.
            bool hasVideoGames = filteredVideoGames.Count > 0;

            // Enable or disable navigation buttons based on the presence of video games.
            First.IsEnabled = hasVideoGames;
            Last.IsEnabled = hasVideoGames;
            Previous.IsEnabled = hasVideoGames;
            Next.IsEnabled = hasVideoGames;

            // Enable or disable the Save Changes button based on whether changes have been made.
            SaveChanges.IsEnabled = changesMade;

            // Enable or disable the Delete button based on whether there are games to delete or not.
            Delete.IsEnabled = hasVideoGames;

            // Enable or disable the Modify button based on whether there are games to delete or not.
            Modify.IsEnabled = hasVideoGames;

        }
        
        // Method to create buttons representing each game in the list. They hold the corresponding game's image.
        private void CreateGameButtons()
        {
            // Clear existing buttons.
            GamesGrid.Children.Clear(); 

            // Number of columns and padding.
            int numColumns = 4;
            int paddingBetweenElements = 2; 

            // Iterate through the different games, creating a button control programmatically for each one.
            for (int i = 0; i < filteredVideoGames.Count; i++)
            {
                var button = new Button
                {
                    // Store index as Tag for later reference.
                    Tag = i 
                };

                // Check if the game has image data. If it does, add the image to the button.
                if (filteredVideoGames[i].ImageData != null && filteredVideoGames[i].ImageData.Length > 0)
                {
                    button.Content = new Image
                    {
                        Source = new Bitmap(new MemoryStream(filteredVideoGames[i].ImageData)),
                        Width = 100,
                        Height = 100,
                        Stretch = Stretch.UniformToFill
                    };
                }
                // If it doesn't, adjust the size so it matches that of image-filled button and display a placeholder.
                else
                {
                    button.Width = 118;
                    button.Height = 113;
                    button.Content = "No Image";
                }

                // Attach a click event.
                button.Click += GameButton_Click;

                // Set margin to add space around the button.
                button.Margin = new Thickness(paddingBetweenElements);

                // Add button to the lefthand grid.
                GamesGrid.Children.Add(button); 

                // Calculate row and column indices.
                int row = i / numColumns;
                int col = i % numColumns;

                // Check if a new row needs to be added (after 4 columns).
                if (row >= GamesGrid.RowDefinitions.Count)
                {
                    GamesGrid.RowDefinitions.Add(new RowDefinition());
                }

                // Set the row and column indices for the button.
                Grid.SetRow(button, row);
                Grid.SetColumn(button, col);

                // Update the appearance of the button.
                UpdateGameButtonsAppearance(button, currentIndex == i);
            }
        }
        
        // Method to get the game button at a specific index.
        private Button GetGameButtonAtIndex(int index)
        {
            // If the passed index is valid and it's lesser than the amount of game buttons,
            // return the button at that index.
            if (index >= 0 && index < GamesGrid.Children.Count)
            {
                return (Button)GamesGrid.Children[index];
            }
            return null;
        }
        
        // Method to highlight a specific button using a border.
        private void UpdateGameButtonsAppearance(Button button, bool isSelected)
        {
            // If there was a previously selected button, clear its border.
            if (previouslySelectedButton != null)
            {
                previouslySelectedButton.BorderBrush = null;
                previouslySelectedButton.BorderThickness = new Thickness(0);
            }

            // For the newly selected button, add a border to it.
            if (isSelected)
            {
                // Add a border to the selected button
                button.BorderBrush = Brushes.Blue; 
                button.BorderThickness = new Thickness(2);
            }

            // Set the current button as the previously selected button.
            previouslySelectedButton = isSelected ? button : null;
        }
        
        // Method to update the game buttons grid with the elements from the secondary filtered list.
        private void UpdateGamesGrid(List<VideoGame> filteredGames)
        {
            // Clear existing buttons from the grid.
            GamesGrid.Children.Clear();

            // Add buttons for the filtered games, following similar logic as the CreateGameButtons method.
            for (int i = 0; i < filteredGames.Count; i++)
            {
                var button = new Button
                {
                    Content = new Image
                    {
                        Source = new Bitmap(new MemoryStream(filteredGames[i].ImageData)),
                        Width = 100,
                        Height = 100,
                        Stretch = Stretch.UniformToFill
                    },
                    Tag = i
                };

                // Attach click event.
                button.Click += GameButton_Click;

                // Set margin to add space around the button.
                button.Margin = new Thickness(2);

                // Add button to the grid.
                GamesGrid.Children.Add(button); 

                // Calculate row and column indices.
                int row = i / 4; 
                int col = i % 4;

                // Check if a new row needs to be added after 4 columns.
                if (row >= GamesGrid.RowDefinitions.Count)
                {
                    GamesGrid.RowDefinitions.Add(new RowDefinition());
                }

                // Set the row and column indices for the button.
                Grid.SetRow(button, row);
                Grid.SetColumn(button, col);

                // Update the appearance of the button.
                UpdateGameButtonsAppearance(button, currentIndex == i);
            }
        }
        
        // Method to add a new videogame to the list.
        public void AddVideoGame(VideoGame newGame)
        {
            // Add the new game to both the original list and the filtered list,
            originalVideoGames.Add(newGame);
            filteredVideoGames.Add(newGame);
            
            // Changes have been made, so set the flag to true (will activate the Save Changes button).
            changesMade = true;

            // Re-create the game buttons to reflect the changes.
            CreateGameButtons();

            // Refresh the information on screen.
            DisplayCurrentGame();

            // Highlight the game at the current index if it was selected before.
            UpdateGameButtonsAppearance(GetGameButtonAtIndex(currentIndex), currentIndex == currentIndex);
            
            // Activate/deactivate buttons.
            UpdateButtonState();
        }

        // Method to show an alert dialog window. Made static so it works better with run-time exceptions.
        private static void ShowAlertWindow(Window owner, string title, string content, bool showCancelButton)
        {
            var window = new Alert(title, content, showCancelButton);
            window.ShowDialog(owner);
        }
        
        // Method to show an alert dialog window. Made asynchronous to make it work with the program's flow, 
        // allowing it to give back a result depending on what option the user clicks.
        private async Task<bool> ShowConfirmationDialog(string title, string content, bool showCancelButton)
        {
            // Show
            var confirmationDialog = new Alert(title, content, showCancelButton);
    
            // Show the dialog and wait for the user's response.
            var response = await confirmationDialog.ShowDialog<bool>(this);

            return response;
        }

        // Method to make the Add Window visible.
        private static void ShowAddWindow(Window owner)
        {
            var window = new AddWindow();
            window.ShowDialog(owner);
        }
        
        // Method to make the Modify Window visible. Also stores a carbon copy of the game to be modified so it can be
        // compared to the modified one to verify whether changes have been made by using the JsonConvert class
        // from the Newtonsoft Json nuGet.
        private async void ShowModifyWindow()
        {
            // In the first place, the list must have at least one element.
            if (filteredVideoGames.Count > 0)
            {
                // Get the selected game and store it in two objects.
                VideoGame selectedGame = filteredVideoGames[currentIndex];
                VideoGame previousGameForComparison = selectedGame;
                // Serialize one of them into Json format.
                string originalJson = JsonConvert.SerializeObject(previousGameForComparison);

                // Create an instance of the Modify Window with the selected game.
                var modifyWindow = new ModifyWindow(selectedGame);

                // Show the Modify Window and wait for the user's response.
                var response = await modifyWindow.ShowDialog<bool>(this);

                // Check the result from the Modify Window (always true if user pressed Save).
                if (response)
                {
                    // Serialize the modified object.
                    string modifiedJson = JsonConvert.SerializeObject(selectedGame);
                    
                    // If the Json strings don't match, that means changes have been made.
                    if (originalJson != modifiedJson)
                    {
                        changesMade = true;
                    }

                    // Perform the usual updating of controls.
                    UpdateButtonState();
                    DisplayCurrentGame();
                    CreateGameButtons();
                    
                }
            }
        }

        // Event method for handling clicks on the Add button.
        private void Add_OnClick(object? sender, RoutedEventArgs e)
        {
            ShowAddWindow(this);
        }

        // Event method for handling clicks on the Modify button.
        private void Modify_OnClick(object? sender, RoutedEventArgs e)
        {
            ShowModifyWindow();
        }
        
        // Event method for handling clicks on the Delete button. Will ask for confirmation from the user.
        private async void Delete_OnClick(object? sender, RoutedEventArgs e)
        {
            // List must have at least one element.
            if (filteredVideoGames.Count > 0)
            {
                // Show confirmation dialog to confirm the user's choice.
                var response = await ShowConfirmationDialog("Confirmation", "Are you sure you want to delete this game?", true);

                // Check the result from the dialog.
                // If user clicked OK, proceed with deletion.
                if (response)
                {
                    VideoGame deletedGame = filteredVideoGames[currentIndex];
                    filteredVideoGames.RemoveAt(currentIndex);
                    originalVideoGames.Remove(deletedGame);

                    // Perform the usual updates and set the changes flag to true.
                    CreateGameButtons();
                    changesMade = true;
                    UpdateButtonState();

                    // Adjust the currentIndex if it goes out of bounds.
                    if (currentIndex >= filteredVideoGames.Count && currentIndex != 0)
                    {
                        currentIndex = filteredVideoGames.Count - 1;
                    }

                    // Refresh the information on screen.
                    DisplayCurrentGame();

                    // If there's at least one element remaining in the list, highlight the game button at the
                    // current index.
                    if (filteredVideoGames.Count != 0)
                    {
                        UpdateGameButtonsAppearance(GetGameButtonAtIndex(currentIndex), currentIndex == currentIndex);
                    }
                }
            }
        }

        // Event method for handling clicks on the Save Changes button.
        private void SaveChanges_OnClick(object? sender, RoutedEventArgs e)
        {
            try
            {
                // Call the SaveData method to save the changes.
                SaveData();

                // Reset the changesMade flag.
                changesMade = false;
        
                // Deactivate the Save Changes button.
                UpdateButtonState();

                // Display a success dialog with only one option (OK).
                var savedChangesDialog = new Alert("Changes saved successfully", "Success", false);
        
                // Set ShowCancelButton to false to hide the Cancel button.
                savedChangesDialog.ShowCancelButton = false;
                savedChangesDialog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                // Handle exceptions generally and display an error message.
                ShowAlertWindow(this, $"Error saving changes: {ex.Message}", "Error", false);
            }
        }
        
        
        private void First_OnClick(object? sender, RoutedEventArgs e)
        {
            if (filteredVideoGames.Count > 0)
            {
                // Unselect the current game button.
                UpdateGameButtonsAppearance(GetGameButtonAtIndex(currentIndex), false); 
                // Go to the first position in the list.
                currentIndex = 0;
                DisplayCurrentGame();
                // Select the new game button.
                UpdateGameButtonsAppearance(GetGameButtonAtIndex(currentIndex), true); 
                UpdateButtonState();
            }
        }

        // Event method for handling clicks on the Last button.
        private void Last_OnClick(object? sender, RoutedEventArgs e)
        {
            if (filteredVideoGames.Count > 0)
            {
                // Unselect the current button.
                UpdateGameButtonsAppearance(GetGameButtonAtIndex(currentIndex), false); 
                // Go to the last position in the list.
                currentIndex = filteredVideoGames.Count - 1;
                DisplayCurrentGame();
                // Select the new button.
                UpdateGameButtonsAppearance(GetGameButtonAtIndex(currentIndex), true); 
                UpdateButtonState();
            }
        }

        // Event method for handling clicks on the Previous button.
        private void Previous_OnClick(object? sender, RoutedEventArgs e)
        {
            if (filteredVideoGames.Count > 0)
            {
                // Unselect the current button.
                UpdateGameButtonsAppearance(GetGameButtonAtIndex(currentIndex), false); 
                // Goes to the previous position, adding the total count of items and calculating the module 
                // with the same count in order to avoid going out of bounds.
                currentIndex = (currentIndex - 1 + filteredVideoGames.Count) % filteredVideoGames.Count;
                DisplayCurrentGame();
                // Select the new button.
                UpdateGameButtonsAppearance(GetGameButtonAtIndex(currentIndex), true); 
                UpdateButtonState();
            }
        }

        // Event method for handling clicks on the Next button.
        private void Next_OnClick(object? sender, RoutedEventArgs e)
        {
            if (filteredVideoGames.Count > 0)
            {
                // Unselect the current button.
                UpdateGameButtonsAppearance(GetGameButtonAtIndex(currentIndex), false); 
                // Goes to the next position, dividing it by the count of games to avoid going out of bounds.
                currentIndex = (currentIndex + 1) % filteredVideoGames.Count;
                DisplayCurrentGame();
                // Select the new button.
                UpdateGameButtonsAppearance(GetGameButtonAtIndex(currentIndex), true);
                UpdateButtonState();
            }
        }

        // Event method for handling clicks on the game buttons.
        private void GameButton_Click(object sender, RoutedEventArgs e)
        {
            // If it's indeed a button and the key for the button is an int, create a variable called
            // index with its value.
            if (sender is Button button && button.Tag is int index)
            {
                // Set the current index to the button's index in the grid group.
                currentIndex = index;
                // Perform the usual updates and highlighting this button.
                DisplayCurrentGame();
                UpdateGameButtonsAppearance(button, true);
            }
        }

        // Event method for handling focus gain on the search bar.
        private void SearchTextBox_GotFocus(object sender, GotFocusEventArgs e)
        {
            // Verify that it's a TextBox title and whether it contains the placeholder text.
            if (sender is TextBox textBox && textBox.Text.Contains("Search by title"))
            {
                // Remove all the text if so.
                textBox.Text = string.Empty;
            }
        }
        
        // Event method for handling focus loss on the search bass.
        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Verify that it's a TextBox title and whether it's empty.
            if (sender is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
            {
                // If empty, add back the original placeholder. The unicode represents a magnifying glass.
                textBox.Text = "\ud83d\udd0e Search by title";
            }
        }

        // Event method for handling changes in the text contents of the search bar.
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Verify that the event is indeed being triggered from a TextBox type.
            if (sender is TextBox textBox)
            {
                // Get the entered text. Turn it to lower case or empty it if there's no text (just in case).
                string searchText = textBox.Text?.ToLower() ?? "";

                // Update the video games list based on the entered text using the Where method.
                filteredVideoGames = originalVideoGames
                    .Where(game => game != null && game.Title != null && game.Title.ToLower().Contains(searchText))
                    .ToList();

                // If the search text is empty or equals the placeholder, show all original games.
                if (string.IsNullOrWhiteSpace(searchText) || searchText.Contains("search by title"))
                {
                    filteredVideoGames = new List<VideoGame>(originalVideoGames);
                }

                // Sort the videoGames list based on the selected filter.
                SortVideoGames(selectedFilter);

                // Update the window with the filtered and sorted games.
                UpdateGamesGrid(filteredVideoGames);

                // Adjust currentIndex if it goes out of bounds.
                if (currentIndex >= filteredVideoGames.Count && currentIndex != 0)
                {
                    currentIndex = filteredVideoGames.Count - 1;
                }

                // There might be cases where the index goes below zero. This conditional prevents that.
                if (currentIndex < 0)
                {
                    currentIndex = 0;
                }

                // Perform the usual updating tasks.
                DisplayCurrentGame();
                UpdateButtonState();
            }
        }
        
        // Method for handling changes in the selection of combobox items.
        private void OrderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Verify that the event triggerer is in fact a ComboBox type.
            if (sender is ComboBox comboBox)
            {
                // Get the selected filter from the ComboBox. Verify that it's a string just in case.
                selectedFilter = comboBox.SelectedItem as string;

                // Sort the videoGames list based on the selected filter.
                SortVideoGames(selectedFilter);

                // Update the window with the filtered and sorted games.
                UpdateGamesGrid(filteredVideoGames);

                // Reset the index and display the current game if there's at least one element in the list.
                if (filteredVideoGames.Count > 0)
                {
                    currentIndex = 0;
                }

                // Perform the usual updating tasks.
                DisplayCurrentGame();
                UpdateButtonState();
            }
        }
        
    }
}
