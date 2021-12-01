using Data;
using Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Logic
{

    public static class UserLogicService
    {
        private static readonly SemaphoreSlim Locker = new SemaphoreSlim(1, 1);

        public static async Task<int> AddUserAsync(User user)
        {
            await Locker.WaitAsync();
            try
            {
                if (UserRepository.Exists(user.Name)) return -1;

                var result = await UserRepository.AddAsync(user);

                return result;

            }
            finally
            {
                Locker.Release();
            }
        }

        public static List<User> GetAll()
        {
            return UserRepository.GetListUser();
        }

        public static bool Exists(string name)
        {
            return UserRepository.Exists(name);
        }

        public static async Task<int?> RemoveAsync(int id)
        {
            await Locker.WaitAsync();
            try
            {
                var userToDelete = UserRepository.Get(id);

                if (!Exists(userToDelete.Name)) return -1;

                UserRepository.Remove(userToDelete.Name);

                return userToDelete.Id;
            }
            finally
            {
                Locker.Release();
            }
        }

        public static async Task<bool> UpdateAsync(User old, User @new)
        {
            if (!(UserRepository.Find(@new) is null)) return false;

            var userToReplace = UserRepository.Get(old.Id);

            if (userToReplace is null) return false;

            return await UserRepository.UpdateAsync(userToReplace, @new);
        }

        public static User Find(User user)
        {
            if (user is null) return null;
            return UserRepository.Find(user);
        }

        public static List<Game> ListUserGamesPurchased(User user)
        {
            return UserRepository.ListUserGamesPurchased(user);
        }

        public static User GetUser(string username)
        {
            return UserRepository.GetUser(username);
        }

        public static User GetUserName(int id)
        {
            return UserRepository.GetUserName(id);
        }


        public static User GetId(int id)
        {
            return UserRepository.Get(id);
        }

    }
}
