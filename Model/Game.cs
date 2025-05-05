using System;
using System.Collections.Generic;

public class PlayablePackage
{
    public string Id { get; set; }
    public decimal Price { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrderCode { get; set; }
    public string PaymentMethod { get; set; }
    public bool IsActivated { get; set; }
    public string Status { get; set; }
    public GamePackageInfo GamePackageInfo { get; set; }
}

public class GamePackageInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Duration { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; }
    public List<Game> GameList { get; set; }
}
public class Game
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string VideoUrl { get; set; }
    public int PlayCount { get; set; }
    public string DownloadUrl { get; set; }
    public string ImageUrl { get; set; }
    public List<Category> Categories { get; set; }
    public List<GameVersion> Versions { get; set; }
}

public class Category
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class GameVersion
{
    public string Version { get; set; }
    public string Description { get; set; }
    public DateTime ReleaseDate { get; set; }
}

class Settings
{
    public string DownloadFolder { get; set; }
}
