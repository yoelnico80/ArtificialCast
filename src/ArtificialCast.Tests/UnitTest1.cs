global using static ArtificialCast.ArtificialCast;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ArtificialCast;
using Xunit;

namespace ArtificialCast.Tests
{
    public class Spaceship
    {
        public string Model { get; set; }
        public int Health { get; set; }
        public int Speed { get; set; }
        public List<string> Weapons { get; set; }
    }

    public class Song
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public int DurationInSeconds { get; set; }
    }

    public class Playlist
    {
        public string Name { get; set; }
        public List<Song> Songs { get; set; } = new();
    }

    public class OldBloatedUser
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string[] Interests { get; set; }
    }
    public class UserContent
    {
        public string Bio { get; set; }
        public string ProfilePictureUrl { get; set; }
        public List<string> Posts { get; set; } = new();
    }
    public class UserCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
    public class UserPreferences
    {
        public string Theme { get; set; }
        public bool NotificationsEnabled { get; set; }
        public string Language { get; set; }
    }
    public class NewUser
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public UserPreferences Preferences { get; set; }
        public UserCredentials Credentials { get; set; }
        public UserContent Content { get; set; }
    }

    public class UnitTest1
    {
        JsonSerializerOptions WriteIntendedJson = new JsonSerializerOptions { WriteIndented = true };

        [Fact]
        public async Task Test1()
        {
            var oldUser = await AF<OldBloatedUser>();
            var oldUserPrefs = await AF<UserPreferences>();
            var newUser = await AC<OldBloatedUser, NewUser>(oldUser);
            newUser = await AM<NewUser, UserPreferences, NewUser>(newUser, oldUserPrefs);
            var AFUser = await AF<NewUser>();
            var playlist = await AM<NewUser, NewUser, Playlist>(newUser, AFUser);
            var song = await AQ<Song[], Song>([..playlist.Songs], "Find the single longest song, please. No array.");

            Console.WriteLine($"Old User: {JsonSerializer.Serialize(oldUser, WriteIntendedJson)}");
            Console.WriteLine($"Old User Prefs: {JsonSerializer.Serialize(oldUserPrefs, WriteIntendedJson)}");
            Console.WriteLine($"New User: {JsonSerializer.Serialize(newUser, WriteIntendedJson)}");
            Console.WriteLine($"AF User: {JsonSerializer.Serialize(AFUser, WriteIntendedJson)}");
            Console.WriteLine($"Playlist: {JsonSerializer.Serialize(playlist, WriteIntendedJson)}");
            Console.WriteLine($"Song: {JsonSerializer.Serialize(song, WriteIntendedJson)}");
        }
    }
}
