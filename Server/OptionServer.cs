using Domain;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Microsoft.Extensions.DependencyInjection;
using Logic;
using RabbitMQ.Client;

namespace Server
{

    public class OptionServer
    {

        public static async Task OptionStart(UserService.UserServiceClient clientuser)
        {

            while (true)
            {
                ShowMenu();
                int selectedOption;
                var optStr = Console.ReadLine();
                bool optionWasValid = int.TryParse(optStr, out selectedOption);

                if (optionWasValid)
                {
                    switch (selectedOption)
                    {
                        case 1:
                            await NewUserAsync(clientuser);
                            break;
                        case 2:
                            await DeleteUserAsync(clientuser);
                            break;
                        case 3:
                           await UpdateUser(clientuser);
                            break;
                        case 4:
                            CatalogGame();
                            break;
                        case 5:
                            BuyGame();
                            break;
                        case 6:
                            GamesUsersPurchased();
                            break;
                        case 7:
                            SearchGameTitle();
                            break;
                        case 8:
                            SearchGameGender();
                            break;
                        case 9:
                            SearchGameQualification();
                            break;
                        case 10:
                            return;
                    }
                }
            }
        }

        private static int ValidUserPostion(List<User> users)
        {
            bool isnumberuser;
            int position;
            do
            {
                Console.WriteLine("Enter position:");
                var selectposition = Console.ReadLine().Trim();
                isnumberuser = int.TryParse(selectposition, out position);
            }
            while (!isnumberuser || position > users.Count || position <= 0);

            return position;
        }

        private static int ValidGamePostion(List<Game> games)
        {
            bool isnumberuser;
            int position;
            do
            {
                Console.WriteLine("Enter position:");
                var selectposition = Console.ReadLine().Trim();
                isnumberuser = int.TryParse(selectposition, out position);
            }
            while (!isnumberuser || position > games.Count || position <= 0);

            return position;
        }

        private static void SearchGameQualification()
        {

            var countgame = GameLogicService.GetAll().Count;
            if (countgame == 0)
            {
                Console.WriteLine(">> No games published <<");
                Console.WriteLine();
            }
            else
            {
                bool isnumberuser;
                double gamequalification;
                do
                {
                    Console.WriteLine("Write game qualification:");
                    var qualification = Console.ReadLine().Trim();
                    isnumberuser = double.TryParse(qualification, out gamequalification);
                }
                while (!isnumberuser);

                var gamelist= GameLogicService.GetByQualyfication(gamequalification);
                if (gamelist == null)
                {
                    Console.WriteLine(">> There are no game with that qualification <<");
                }
                else
                {
                    var index = 1;
                    foreach (var g in gamelist)
                    {
                        Console.WriteLine($"{index++}){g}");
                    }
                }
            }
        }

        private static void SearchGameGender()
        {
            var games = GameLogicService.GetAll();
            if (games.Count == 0)
            {
                Console.WriteLine(">> No games published <<");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Write game gender:");
                var gamegender = Console.ReadLine();
                var gamesgender = GameLogicService.GetByGender(gamegender);
                if (gamesgender == null)
                {
                    Console.WriteLine(">> There are no game with that gender <<");
                }
                else 
                {
                    var index = 1;
                    foreach (var g in gamesgender)
                    {
                        Console.WriteLine($"{index++}){g}");
                    }
                }
            }
        }

        private static void SearchGameTitle()
        {
           
            var games = GameLogicService.GetAll();
            if (games.Count == 0)
            {
                Console.WriteLine(">> No games published <<");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Write game title:");
                var gametitle = Console.ReadLine();
                var game = GameLogicService.GetByTitle(gametitle);
                if (game == null)
                {
                    Console.WriteLine(">> There are no game with that title <<");
                }
                else
                {
                    Console.WriteLine($"{game}");
                    Console.WriteLine();
                }

            }
        }

        private static async Task NewUserAsync(UserService.UserServiceClient client)
        {
           
            Console.WriteLine("Enter a name new user");
            string name = Console.ReadLine().Trim();

            User user = new User();
            user.Name = name;

           var result = await UserLogicService.AddUserAsync(user);

            if (result > 0)
            {
                var logsevent = new Logs<User>
                {
                    Object = user,
                    Description = "NewUser",
                    Date = DateTime.Now,
                };

                PublisherSucriberQueue.SendMessages(logsevent);

                Console.WriteLine("User was added successfully");
            }
            else 
            {
                Console.WriteLine("User is alredy registered");
            }

            //GRPC
            var addUserReply = await client.AddUserAsync(new AddUserRequest { Name = name });
            Console.WriteLine($"GRPC server response: {addUserReply.Message}");
           
        }

        private static async Task UpdateUser(UserService.UserServiceClient client)
        {
           
            var users = UserLogicService.GetAll();

            if (users.Count == 0)
            {
                Console.WriteLine(">> No logged in users <<");
                Console.WriteLine();
            }
            else 
            {
                int index = 1;
                foreach (var user in users)
                {
                    Console.WriteLine($"{index++})Id: {user.Id} - Name: {user.Name}");
                }
                Console.WriteLine(">> Selected name user position <<");

                var position = ValidUserPostion(users);
                var userold = UserLogicService.GetAll().ElementAt(position - 1);
                
                Console.WriteLine(">> New name user <<");
                var newname = Console.ReadLine();
                User newuser = new User
                {
                    Name = newname,
                    Games = userold.Games
                };

                await UserLogicService.UpdateAsync(userold, newuser);

                var logsevent = new Logs<User>
                {
                    Object = userold,
                    Description = "UpdateUser",
                    Date = DateTime.Now,
                };

                PublisherSucriberQueue.SendMessages(logsevent);

                //GRPC
                var updateUserReply = client.Update(new UpdateUserRequest {Id = userold.Id, NewName = newuser.Name});
                Console.WriteLine($"GRPC server response: {updateUserReply.Message}");
            }
        }

        private static async Task DeleteUserAsync(UserService.UserServiceClient client)
        {
            var users = UserLogicService.GetAll();

            if (users is null)
            {
                Console.WriteLine(">> No logged in users <<");
                Console.WriteLine();
            }
            else
            {
                int index = 1;
                foreach (var user in users)
                {
                    Console.WriteLine($"{index++})Id: {user.Id} - Name: {user.Name}");
                }
                Console.WriteLine(">> Select position from the list of users for deletion <<");

                var position = ValidUserPostion(users);

                var selectedUser = users.ElementAt(position-1);

                await UserLogicService.RemoveAsync(selectedUser.Id);

                var logsevent = new Logs<User>
                {
                    Object = selectedUser,
                    Description = "DeleteUser",
                    Date = DateTime.Now,
                };

                PublisherSucriberQueue.SendMessages(logsevent);

                //GRPC
                var deleteUserReply = client.Delete(new DeleteUserRequest { Id = selectedUser.Id });
                Console.WriteLine($"GRPC server response: {deleteUserReply.Message}");

            }
        }

        private static void GamesUsersPurchased()
        {
            var users = UserLogicService.GetAll();

            if (users is null)
            {
                Console.WriteLine(">> No logged in users <<");
                Console.WriteLine();
            }
            else
            {
                var index = 1;
                foreach (var user in users)
                {
                    Console.WriteLine($"{index++}){user.Name}");
                    var gamesPurchasedUser = UserLogicService.ListUserGamesPurchased(user);
                    if (gamesPurchasedUser is null)
                    {
                        Console.WriteLine($"\t>> I don't buy games <<");
                    }
                    else
                    {
                        foreach (var gamesUser in gamesPurchasedUser)
                        {
                            Console.WriteLine($"{gamesUser}");
                            Console.WriteLine();
                        }
                    }
                }
            }
        }

        private static void CatalogGame()
        {
            var games = UserLogicService.GetAll();
            if (games is null)
            {
                Console.WriteLine(">> No games published <<");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Games published:");
                var index = 1;
                foreach (var game in games)
                {
                    Console.WriteLine($"{index++}){game}");
                }
            }
        }

        private static void BuyGame()
        {

            var games = GameLogicService.GetAll();
            if (games.Count == 0 )
            {
                Console.WriteLine(">> No games to sell <<");
            }
            else
            {
                var users = UserLogicService.GetAll();
                if (users.Count == 0)
                {
                    Console.WriteLine(">> No logged in users <<");
                    Console.WriteLine();
                }
                else
                {
                    var indexuser = 1;
                    foreach (var user in users)
                    {
                        Console.WriteLine($"{indexuser++}){user}");
                    }
                    Console.WriteLine();

                    Console.WriteLine("Select user logged in to purchase:");

                    var position = ValidUserPostion(users);

                    var userbuy = UserLogicService.GetAll().ElementAt(position - 1);
                    User userrepo = UserLogicService.GetUser(userbuy.Name);

                    var indexgames = 1;
                    foreach (var game in games)
                    {
                        Console.WriteLine($"{indexgames++}){game}");
                    }
                    Console.WriteLine("Select the game you want to buy:");

                    var positiongame = ValidGamePostion(games);

                    var gamebuy = GameLogicService.GetAll().ElementAt(positiongame - 1);
                    userrepo.AddPurchasedGame(gamebuy);
                    Console.WriteLine(">> Congratulations I buy a new game !!! <<");
                    Console.WriteLine();
                }
            }
        }

        public static void ShowMenu()
        {
            Console.WriteLine("Server Option:");
            Console.WriteLine("1) New User");
            Console.WriteLine("2) Delete User");
            Console.WriteLine("3) Update User");
            Console.WriteLine("4) See games catalog");
            Console.WriteLine("5) Buy game");
            Console.WriteLine("6) See the games that the users purchased");
            Console.WriteLine("7) Search game by title");
            Console.WriteLine("8) Search game by gender");
            Console.WriteLine("9) Search game by qualification");
            Console.WriteLine("10) Exit");
        }




    }
}
