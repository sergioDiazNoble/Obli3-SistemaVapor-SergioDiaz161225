using Domain;
using Grpc.Core;
using Logic;
using System.Threading.Tasks;


namespace ServerAdmin
{
    public class UserGrpcService : UserService.UserServiceBase
    {


        public override async Task<TextResponse> AddUser(AddUserRequest request, ServerCallContext context)
        {
            var id = await UserLogicService.AddUserAsync(new User { Name = request.Name });

            string message = id > 0 ? $"User added {request.Name} successfully " : $"User {request.Name} not was added";

            return await Task.FromResult(new TextResponse
            {
                Message = message
            });
        }

        public override async Task<TextResponse> Delete(DeleteUserRequest userRequest, ServerCallContext context)
        {
           
            var id = await UserLogicService.RemoveAsync(userRequest.Id);

            string message = id > 0 ? $"User delet successfully " : $"User not was delete";

            return await Task.FromResult(new TextResponse
            {
                Message = message
            });


        }

        public override async Task<TextResponse> Update(UpdateUserRequest request, ServerCallContext context)
        {
            var oldUser = new User(request.Id);
            var newUser = new User(request.Id)
            {
                Name = request.NewName
            };

            var updated = await UserLogicService.UpdateAsync(oldUser, newUser);

            string message = updated ? $"User update {request.NewName} successfully " : $"User not {request.NewName}  was update";

            return await Task.FromResult(new TextResponse
            {
                Message = message
            });
        }
    }
}

