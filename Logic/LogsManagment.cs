using Data;
using Domain;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Logic
{
    public static class LogsManagment
    {
        private static readonly SemaphoreSlim Locker = new SemaphoreSlim(1, 1);
        public static async Task<List<object>> GetAll()
        {
            var listobj = new List<object>();
            var usersLogs = UserLogsRepo.GetAll();
            var gamesLogs = GameLogsRepo.GetAll();

            await Locker.WaitAsync();
            try
            {
                if (usersLogs != null)
                {
                    foreach (Logs<User> logU in usersLogs)
                    {
                        listobj.Add(logU);
                    }
                }

                if (gamesLogs != null)
                {
                    foreach (Logs<Game> logG in gamesLogs)
                    {
                        listobj.Add(logG);
                    }
                }
            }

            finally
            {
                Locker.Release();
            }

            return listobj;
        }

        public static async Task<Logs<User>> GetLogByIdUser(int id)
        {
            return await UserLogsRepo.GetAsync(id);

        }

        public static async Task<Logs<Game>> GetLogByIdGame(int id)
        {
            return await GameLogsRepo.GetAsync(id);
        }

    }
}
