using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Domain
{
    public class Game
    {
        private static int _idCounter = 0;
        public List<Review> reviews;
        public Game(int id)
        {
            this.Id = id;
            reviews = new List<Review>();
        }
        public Game() : this(++_idCounter)
        {
        }
        public int Id { get; }


        private static readonly object Locker = new object();
        public string Title { get; set; }
        public string Gender { get; set; }
        public string Synopsis { get; set; }
            
        public string CoverName  {get ; set;}

        public string GetNameCover()
        {
            return CoverName;
        }

        public void SetNameCover(string name)
        {
            CoverName = name;
        }

        public double averagenote { get; set; }
        public double RankingPublic 
        {
            get
            {
                return averagenote;
            }
            set 
            {
                averagenote = value;
            }
        }
     
        public bool ContainsByName(IEnumerable<Game> games, string game)
        {
            return games.Any(g => string.Equals(g.Title, game, StringComparison.OrdinalIgnoreCase));
        }

        public override string ToString()
        {
            return $"\tId:{Id}\n\tTitle: {Title}\n\tGender:{Gender}\n\tRankingPublic:{RankingPublic}\n\tSynopsis:{Synopsis}\n\tCover:{CoverName}\n";
        }

        public void AddListReview (Review review)
        {
            lock (Locker) 
            {
                reviews.Add(review);
                averagenote = AverageCalculate(reviews);
            }
            
        }

        public IEnumerable<Review> GetListReview()
        {
            lock (Locker) 
            {
                return reviews;
            }
        
        }

        public List<Review> GetLisReview()
        {
            return reviews;
        }

        public double AverageCalculate(List<Review> reviews) 
        {
            var total = reviews.Count();

            if (total == 0)
            {
                return total;
            }
            else
            {
                var totalnote = 0;
                foreach (var review in reviews)
                {
                    totalnote += review.Note;
                }

                return (double)totalnote / total;
            }
        }

    }
}

  
