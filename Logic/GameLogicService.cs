using Data;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Logic
{
    public static class GameLogicService
    {
        private static readonly SemaphoreSlim Locker = new SemaphoreSlim(1, 1);


        public static async Task<int> AddGameAsync(Game game)
        {

            await Locker.WaitAsync();
            try
            {
                if (Exists(game)) return -1;

                var result = await GameRepository.AddAsync(game);

                return result;
            }
            finally
            {
                Locker.Release();
            }

        }

        public static bool Exists(Game game)
        {
            return GameRepository.Exists(game);
        }

        public static List<Game> GetAll()
        {
            return GameRepository.GetAll();
        }

        public static Game Get(int id)
        {
            return GameRepository.Get(id);
        }

        public static async Task<int> RemoveAsync(int id)
        {
            await Locker.WaitAsync();
            try
            {
                var gameToDelete = GameRepository.Get(id);

                if (gameToDelete is null)
                {
                    return -1;
                }
                else
                {
                    GameRepository.Remove(gameToDelete.Id);

                }

                return gameToDelete.Id;
            }
            finally
            {
                Locker.Release();
            }
        }


        public static bool Update(Game old, Game @new)
        {
            if (!(GameRepository.Find(@new) is null)) return false;

            var gameToReplace = GameRepository.Get(old.Id);
            if (gameToReplace is null) return false;

            GameRepository.Update(gameToReplace, @new);

            return true;
        }

        public static List<Game> GetByQualyfication(double calification)
        {
            return GameRepository.GetByQualification(calification);
        }

        public static List<Game> GetByGender(string gamegender)
        {
            return GameRepository.GetByGender(gamegender);
        }

        public static Game GetByTitle(string gametitle)
        {
            return GameRepository.GetByTitle(gametitle);
        }

        public static Task<bool> Remove(Game game)
        {
            return GameRepository.Remove(game);
        }


    }
}