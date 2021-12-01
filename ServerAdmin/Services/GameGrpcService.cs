using Domain;
using Grpc.Core;
using Logic;
using ServerAdmin.Protos;
using System.Threading.Tasks;

namespace ServerAdmin.Services
{
    public class GameGrpcService : GameService.GameServiceBase
    {
      
        public override async Task<TextResponseGame> AddGame(AddGameRequest request, ServerCallContext context)
        {

            var game = new Game
            {
                Title = request.Title,
                Gender = request.Gender,
                Synopsis = request.Synopsis,                
            };

            var addedId = await GameLogicService.AddGameAsync(game);

            string message = addedId > 0 ? $"Game added successfully " : $"Game not was added";

            return await Task.FromResult(new TextResponseGame
            {
                Message = message
            });
        }


        public override async Task<TextResponseGame> Delete(DeleteGameRequest request, ServerCallContext context)
        {
            var result = await GameLogicService.RemoveAsync(request.Id);

            string message = result > 0 ? $"Game delete successfully " : $"Game not was delete";

            return await Task.FromResult(new TextResponseGame
            {
                Message = message
            });
        }

        public override async Task<TextResponseGame> Update(UpdateGameRequest request, ServerCallContext context)
        {
            var result = -1;
            var addedId = -1;

            var gameold =  GameLogicService.Get(request.Id);
            if (gameold != null)
            {
                var game = new Game
                {
                    Title = request.NewTitle,
                    Gender = request.NewGender,
                    Synopsis = request.NewSynopsis,
                };

                addedId = await GameLogicService.AddGameAsync(game);
                result = await GameLogicService.RemoveAsync(request.Id);
            }

            string message = (result > 0 && addedId > 0) ? $"Game update successfully " : $"Game not was update";

            return await Task.FromResult(new TextResponseGame
            {
                Message = message
            });

        }

    }
}
