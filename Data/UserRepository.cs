using Domain;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Data
{
    public static class UserRepository
    {
        private static readonly List<User> users = new List<User>();
        private static readonly SemaphoreSlim Locker = new SemaphoreSlim(1, 1);

        public static async Task<int> AddAsync(User user)
        {

            await Locker.WaitAsync();
            try
            {
                users.Add(user);
                return user.Id;
            }
            finally
            {
                Locker.Release();
            }
        }

        public static bool Exists(string username)
        {
            return users.Any(u => string.Equals(u.Name, username, StringComparison.OrdinalIgnoreCase));
        }

        public static List<User> GetListUser()
        {
            return users;
        }

        public static User GetUser(string username)
        {
            return users.FirstOrDefault(u => string.Equals(u.Name, username, StringComparison.OrdinalIgnoreCase));
        }

        public static bool Remove(string username)
        {

            var remove = GetUser(username);

            return users.Remove(remove);

        }

        public static bool Modify(User userold, User usernew)
        {
            lock (Locker)
            {
                if (Exists(usernew.Name)) return false;

                var remove = GetUser(userold.Name);

                users.Remove(remove);
                users.Add(usernew);

                return true;
            }
        }


        public static User Get(int id)
        {
            return users.FirstOrDefault(user => user.Id == id);
        }

        public static User Find(User user)
        {
            return users.FirstOrDefault(u => string.Equals(u.Name, user.Name, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<bool> UpdateAsync([NotNull] User old, [NotNull] User @new)
        {
            await Locker.WaitAsync();
            try
            {
                var user = Get(old.Id);
                user.Games = @new.Games;
                user.Name = @new.Name;
                return true;
            }
            finally
            {

                Locker.Release();

            }
        }
        public static List<User> GetAll(int gameId)
        {
            return GetAll()
                .Where(x => x.Games.Select(game => game.Id).Contains(gameId))
                .ToList();
        }
        public static List<User> GetAll()
        {
            return ImmutableList<User>.Empty.AddRange(users).ToList();
        }

        public static List<Game> ListUserGamesPurchased(User user)
        {
            return user.PurchasedGame;
        }

        public static User GetUserName(int id)
        {
            return users.FirstOrDefault(u => Equals(u.Id, id));
        }



    }






}

