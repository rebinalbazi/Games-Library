namespace Games_Library
{
    public class Game
    {
        public string Name { get; set; }
        public string Genre { get; set; }
        public string Platform { get; set; }
        public string Release_Date { get; set; }
        public string Description { get; set; }
        public string Meta_Score { get; set; }
        public string User_Score { get; set; }
        public string Cover_Path { get; set; }


        public Game(string name, string genre, string platform, string release_date, string description, string meta_score, string user_score, string cover_path)
        {
            Name = name.Substring(1);
            Genre = genre;
            Platform = platform;
            Release_Date = release_date;
            Description = description;
            Meta_Score = meta_score;
            User_Score = user_score;
            Cover_Path = cover_path.Remove(cover_path.Length - 1, 1);
        }
    }
}
