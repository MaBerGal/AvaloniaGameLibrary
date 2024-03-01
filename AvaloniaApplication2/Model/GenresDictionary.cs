
using System.Collections.Generic;

namespace AvaloniaApplication2.Model;

// Class to provide a dictionary mapping genre codes to genre names.
internal class GenresDictionary
{
    // Static dictionary storing the video game genres (char -> string).
    public static Dictionary<char, string> GenreDictionary { get; } = new Dictionary<char, string>
    {
        { 'O', "Other" },
        { 'A', "Action" },
        { 'D', "Adventure" },
        { 'R', "Role-Playing Game (RPG)" },
        { 'S', "Simulation" },
        { 'T', "Strategy" },
        { 'P', "Sports" },
        { 'F', "Fighting" },
        { 'U', "Puzzle" },
        { 'L', "Survival" },
        { 'N', "Visual Novel" },
        { 'I', "Rhythm" },
        { 'K', "Mystery" },
        { '7', "Action-Adventure" },
        { 'V', "Metroidvania" }
    };
}
