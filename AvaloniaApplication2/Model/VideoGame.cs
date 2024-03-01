using System;

namespace AvaloniaApplication2.Model;

// Making the VideoGame class serializable for storage and retrieval.
[Serializable]
public class VideoGame
{
    // Properties to represent the attributes of a videogame.
    // Video game's director.
    public string Director { get; set; }

    // Title of the video game.
    public string Title { get; set; }

    // Release year of the video game.
    public int ReleaseYear { get; set; }

    // Rating of the video game.
    public float Rating { get; set; }

    // Indicates if the video game supports multiplayer.
    public bool Multiplayer { get; set; }

    // Genre code of the video game.
    public char Genre { get; set; }

    // Description of the video game.
    public string Description { get; set; }

    // Byte array to store image data of the video game.
    public byte[] ImageData { get; set; }

    // Constructor to initialize a videogame object with the provided values.
    public VideoGame(string director, string title, int releaseYear, float rating, bool multiplayer, char genre,
        string description, byte[] imageData)
    {
        Director = director;
        Title = title;
        ReleaseYear = releaseYear;
        Rating = rating;
        Multiplayer = multiplayer;
        Genre = genre;
        Description = description;
        ImageData = imageData;
    }
    
    // Constructor to initialize a videogame object with no image.
    public VideoGame(string director, string title, int releaseYear, float rating, bool multiplayer, char genre,
        string description)
    {
        Director = director;
        Title = title;
        ReleaseYear = releaseYear;
        Rating = rating;
        Multiplayer = multiplayer;
        Genre = genre;
        Description = description;
    }

    // Default constructor for serialization purposes.
    public VideoGame()
    {
    }
    
   
}