using Domain;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Data
{
    public static class GameRepository
    {
        private static readonly List<Game> games = new List<Game>();
        private static readonly SemaphoreSlim Locker = new SemaphoreSlim(1, 1);

        public static async Task<int> AddAsync(Game game)
        {
            await Locker.WaitAsync();
            try
            {
                games.Add(game);
                return game.Id;

            }
            finally
            {
                Locker.Release();
            }
        }


        public static async Task<bool> UpdateAsync(Game game)
        {
            await Locker.WaitAsync();
            try
            {
                var outdated = Get(game.Id);
                outdated.Title = game.Title;
                outdated.Synopsis = game.Synopsis;
                outdated.Gender = game.Gender;
                outdated.RankingPublic = game.RankingPublic;
                outdated.reviews = game.reviews;
                return true;
            }
            finally
            {
                Locker.Release();
            }
        }

        public static Game Get(int id)
        {
            return GetAll().FirstOrDefault(game => game.Id == id);
        }

        public static List<Game> GetAll()
        {
            return ImmutableList<Game>.Empty.AddRange(games).ToList();
        }

        public static async Task<bool> RemoveAsync(Game game)
        {
            await Locker.WaitAsync();
            try
            {
                if (!Exists(game)) return false;
                var toRemove = Find(game);
                games.Remove(toRemove);
                return true;
            }
            finally
            {
                Locker.Release();
            }
        }

        public static int Remove(int id)
        {
            var toRemove = Get(id);
            games.Remove(toRemove);

            return id;
        }


        public static bool Exists(Game game)
        {
            return games.Any(g => string.Equals(g.Title, game.Title, StringComparison.OrdinalIgnoreCase) || g.Id == game.Id);
        }

        public static Game Find(Game game)
        {
            return games.FirstOrDefault(g => string.Equals(g.Title, game.Title, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<bool> Remove(Game game)
        {
            await Locker.WaitAsync();
            try
            {
                var remove = Find(game);
                return games.Remove(remove);
            }
            finally
            {
                Locker.Release();
            }
        }

        public static List<Game> GetListGame()
        {

            return games;

        }

        public static Game GetByTitle(string gametitle)
        {
            return games.FirstOrDefault(g => string.Equals(g.Title, gametitle, StringComparison.OrdinalIgnoreCase));
        }

        public static List<Game> GetByGender(string gamegender)
        {
            return games.FindAll(g => string.Equals(g.Gender, gamegender, StringComparison.OrdinalIgnoreCase));
        }

        public static List<Game> GetByQualification(double qualification)
        {
            return games.FindAll(g => Equals(g.averagenote, qualification));
        }

        public static Game GetGame(Game game)
        {
            return games.Find(x => x.Title.Contains(game.Title) && x.Gender.Contains(game.Gender) && x.RankingPublic == game.RankingPublic);
        }

        public static Task<bool> Update([NotNull] Game old, [NotNull] Game @new)
        {
            var game = Get(old.Id);
            game.Title = @new.Title;
            game.Gender = @new.Gender;
            game.RankingPublic = @new.RankingPublic;
            game.Synopsis = @new.Synopsis;
            game.CoverName = @new.CoverName;

            return Task.FromResult(true);
        }

    }

}

