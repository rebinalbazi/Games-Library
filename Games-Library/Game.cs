namespace Games_Library
{
    public class Game
    {
        public string Name { get; set; }
        public string Genre { get; set; }
        public string Platform { get; set; }
        public string Release_Day { get; set; }
        public int Release_Year { get; set; }
        public string Rating_Score { get; set; }
        public string Cover_Path { get; set; }

        public Game(string name, string genre, string platform, string release_day,int release_year, string rating_score, string cover_path)
        {
            Name = name.Substring(1);
            Genre = genre;
            Platform = platform;
            Release_Day = release_day;
            Release_Year = release_year;
            Rating_Score = rating_score;
            Cover_Path = cover_path.Remove(cover_path.Length - 1, 1);
        }
    }
}
