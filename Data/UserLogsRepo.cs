using Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Data
{
    public static class UserLogsRepo
    {
        public static List<Logs<User>> usersLogs = new List<Logs<User>>();
        private static readonly SemaphoreSlim Locker = new SemaphoreSlim(1, 1);

        public static async Task<bool> AddAsync(Logs<User> obj)
        {
            await Locker.WaitAsync();
            try
            {
                usersLogs.Add(obj);
                return true;
            }
            finally
            {
                Locker.Release();
            }
        }

        public static async Task<List<Logs<User>>> GetAllAsync()
        {
            await Locker.WaitAsync();
            try
            {
                return usersLogs;
            }
            finally
            {
                Locker.Release();
            }

        }

        public static List<Logs<User>> GetAll()
        {
            return usersLogs;
        }

        public static async Task<Logs<User>> GetAsync(int id)
        {
            await Locker.WaitAsync();
            try
            {
                return usersLogs.SingleOrDefault(ulogs => ulogs.Id == id);
            }
            finally
            {
                Locker.Release();
            }
        }


    }
}
