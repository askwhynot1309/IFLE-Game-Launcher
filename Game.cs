using System;
using System.Collections.Generic;

public class Game
{
    public int Id { get; set; }
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
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class GameVersion
{
    public string Version { get; set; }
    public string Description { get; set; }
    public DateTime VersionDate { get; set; }
}
