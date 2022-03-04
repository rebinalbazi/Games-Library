namespace Games_Library
{
    public class Game
    {
        public string Name { get; set; }
        public string Platform { get; set; }
        public int Release_Year { get; set; }
        public string Meta_Score { get; set; }
        public string User_Review { get; set; }

        public Game(string name, string platform, int release_year, string meta_score, string user_review)
        {
            Name = name;
            Platform = platform;
            Release_Year = release_year;
            Meta_Score = meta_score;
            User_Review = user_review;
        }
    }
}
