using Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Data
{
    public static class GameLogsRepo
    {
        public static List<Logs<Game>> gamesLogs = new List<Logs<Game>>();
        private static readonly SemaphoreSlim Locker = new SemaphoreSlim(1, 1);

        public static async Task<bool> AddAsync(Logs<Game> obj)
        {
            await Locker.WaitAsync();
            try
            {
                gamesLogs.Add(obj);
                return true;
            }
            finally
            {
                Locker.Release();
            }
        }

        public static async Task<List<Logs<Game>>> GetAllAsync()
        {
            await Locker.WaitAsync();
            try
            {
                return gamesLogs;
            }
            finally
            {
                Locker.Release();
            }

        }

        public static List<Logs<Game>> GetAll()
        {
            return gamesLogs;
        }


        public static async Task<Logs<Game>> GetAsync(int id)
        {
            await Locker.WaitAsync();
            try
            {
                return gamesLogs.Find(glogs => glogs.Id == id);
            }
            finally
            {
                Locker.Release();
            }



        }

    }

}

