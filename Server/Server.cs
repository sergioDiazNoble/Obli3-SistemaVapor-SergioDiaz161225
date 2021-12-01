using SimpleProtocolShared;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using StringProtocolLibrary;
using System.Text;
using Domain;
using System.Linq;
using Client;
using Common;
using System.IO;
using System.Threading.Tasks;
using Data;
using Grpc.Net.Client;
using RabbitMQ.Client;
using Logic;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using LogsServer;

namespace Server
{
    public class Server
    {
        public static bool _exit = false;
        private static HeaderHandler headerHandler = new HeaderHandler();
        private static int sizeheader = HeaderConstants.TypeLength + HeaderConstants.CommandLength + HeaderConstants.DataLength;
        public static readonly List<NetworkStreamHandler> ConnectedClients = new List<NetworkStreamHandler>();
        private static object locker = new object();

        private static GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:8001");
        private static GameService.GameServiceClient clientgame = new GameService.GameServiceClient(channel);
        private static UserService.UserServiceClient clientuser = new UserService.UserServiceClient(channel);

        private static ConnectionFactory factory = new ConnectionFactory { HostName = "localhost" };
        private static IConnection connection = factory.CreateConnection();
        private static IModel channelmq = connection.CreateModel();


        public static void Main(string[] args)
        {

            PublisherSucriberQueue.StartChannel(channelmq);
            
            var startServer = Task.Run(()=> StartServer());
            var optionServer = Task.Run(() => OptionServer.OptionStart(clientuser));

            PublisherSucriberQueue.ReceiveMessages();


            Task.WaitAll(startServer, optionServer);
        }


        public static void StartServer()
        {
            var ipAddress = ConfigurationData.ConfigIP();
            var port = ConfigurationData.ConfigPort();

            var serverIpEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress),port);
            var tcpListener = new TcpListener(serverIpEndPoint);
            
            
            Console.WriteLine("Server Started..");

            Task.Run(() => ListenForConnections(tcpListener));
            
        }

        private static NetworkStreamHandler AddNewConnection(TcpClient tcpClient)
        {
            lock (locker)
            {
                var networkStreamHandler = new NetworkStreamHandler(tcpClient.GetStream());
                ConnectedClients.Add(networkStreamHandler);
                return networkStreamHandler;
            }
        }

        private static void ListenForConnections(TcpListener tcpListener)
        {
            
            while (!_exit)
            {
                try
                {
                    tcpListener.Start(1);
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    tcpListener.Stop();
                    var networkStreamHandler = AddNewConnection(tcpClient);
                    Task.Run(() => HandleClientAsync(networkStreamHandler));
                    
                }
                catch (SocketException se)
                {
                    Console.WriteLine("The server is closing");
                    _exit = true;
                }
            }
            foreach (var socketClient in ConnectedClients)
            {
                lock (locker)
                {
                    try
                    {
                        socketClient.Shutdown();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Socket client is already closed");
                    }
                    Console.WriteLine("Exit of Main Thread...");
                }

            }
        }

        private static async Task HandleClientAsync(NetworkStreamHandler networkStreamHandler)
        {
            
            try
            {                           
                while (!_exit)
                {
                    var encodeHeader = await networkStreamHandler.ReadAsync(sizeheader);
                    var header = headerHandler.DecodeHeader(encodeHeader);
                    var data = await networkStreamHandler.ReadAsync(header.DataLength);

                    switch (header.Command)
                    {
                            case CommandConstants.GamePublication:
                                await GamePublicationAsync(data, networkStreamHandler);
                                break;

                            case CommandConstants.RemoveGame:
                                await RemoveGameAsync(data, networkStreamHandler);
                                break;

                            case CommandConstants.ModifyGame:
                                await ModifyGameAsync(data, networkStreamHandler);
                                break;

                            case CommandConstants.GameSearch:
                                await GameSearchAsync(data, networkStreamHandler);
                                break;

                            case CommandConstants.GameQualify:
                                await GameQualifyAsync(data, networkStreamHandler);
                                break;

                            case CommandConstants.GameDetail:
                                await GameDetailAsync(data, networkStreamHandler);
                                break;

                            case CommandConstants.UploadGameCover:
                                await UploadGameCoverAsync(data, networkStreamHandler);
                                break;

                            case CommandConstants.DownloadGameCover:
                                await DownloadGameCoverAsync(data, networkStreamHandler);
                                break;

                            case CommandConstants.ListGame:
                                await ListGameAsync(data, networkStreamHandler);
                                break;

                            case CommandConstants.Exit:
                                Exit(data, networkStreamHandler);
                                _exit = true;
                                break;
                            case CommandConstants.Login:
                                await LoginAsync(data, networkStreamHandler);
                                break;
                    }
                }
                   
            }
            catch (SocketException)
            {
                Console.WriteLine("Client closed the connection");
            }
            catch (Exception)
            {
                Console.WriteLine("An error occcurred! Client has been disconnected by the server");
            }
        }

        private static async Task LoginAsync(byte[] data, NetworkStreamHandler networkStreamHandler)
        {
           var dataname = Encoding.UTF8.GetString(data).Split("#");

            var username = dataname[0];

            Header headerresponse = new Header
            {
                Type = HeaderType.Response,
                Command = CommandConstants.Login,
                DataLength = 0
            };

            var responseok = Encoding.ASCII.GetBytes("Your name login was successfully registered");
            var responsefailure = Encoding.ASCII.GetBytes("Your name is already registered");

            if (UserLogicService.Exists(username))
            {
                headerresponse.DataLength = responsefailure.Length;
                await networkStreamHandler.SendAsync(headerHandler.GetHeaderBytes(headerresponse));
                await networkStreamHandler.SendAsync(responsefailure);
            }
            else
            {
                User user = new User
                {
                    Name = dataname[0]
                };

                await UserLogicService.AddUserAsync(user);

                var logsevent = new Logs<User>
                {
                    Object = user,
                    Description = "NewUser",
                    Date = DateTime.Now,
                };

                PublisherSucriberQueue.SendMessages(logsevent);

                //GRPC
                var addUserReply = await clientuser.AddUserAsync(new AddUserRequest { Name = user.Name });
                Console.WriteLine($"GRPC server response: {addUserReply.Message}");


                headerresponse.DataLength = responseok.Length;
                await networkStreamHandler.SendAsync(headerHandler.GetHeaderBytes(headerresponse));
                await networkStreamHandler.SendAsync(responseok);
            }

        }

        private static void Exit(byte[] data, NetworkStreamHandler networkStreamHandler)
        {
            lock (locker)
            {
                networkStreamHandler.Shutdown();
                ConnectedClients.Remove(networkStreamHandler);
                Console.WriteLine("Killed connection");
            }
        }

        private static async Task GamePublicationAsync(byte[] databyte,NetworkStreamHandler networkStreamHandler) 
        {
            var data = Encoding.UTF8.GetString(databyte).Split("#");
            

            var titlegame = data[0];
            if (GameLogicService.GetByTitle(titlegame) is null)
            {
                var game = new Game()
                {
                    Title = data[0],
                    Gender = data[1],
                    RankingPublic = double.Parse(data[2]),
                    Synopsis = data[3],
                    CoverName = data[4]
                };

                await GameLogicService.AddGameAsync(game);

                var logsevent = new Logs<Game>
                {
                    Object = game,
                    Description = "NewGame",
                    Date = DateTime.Now,
                };

                PublisherSucriberQueue.SendMessages(logsevent);

                var responseok = Encoding.ASCII.GetBytes("Your game was successfully registered");

                Header headerresponse = new Header
                {
                    Type = HeaderType.Response,
                    Command = CommandConstants.GamePublication,
                    DataLength = responseok.Length
                };

                await networkStreamHandler.SendAsync(headerHandler.GetHeaderBytes(headerresponse));
                await networkStreamHandler.SendAsync(responseok);
            }
            else
            {
                var responsefailure = Encoding.ASCII.GetBytes("Game title is already published");

                Header headerresponse = new Header
                {
                    Type = HeaderType.Response,
                    Command = CommandConstants.GamePublication,
                    DataLength = responsefailure.Length
                };

                await networkStreamHandler.SendAsync(headerHandler.GetHeaderBytes(headerresponse));
                await networkStreamHandler.SendAsync(responsefailure);

            }

            //GRPC
            var addGameReply = await clientgame.AddGameAsync(new AddGameRequest { Title = data[0],Gender = data[1],Synopsis = data[2]});
            Console.WriteLine($"GRPC server response: {addGameReply.Message}");


        }

        private static async Task RemoveGameAsync(byte[] data, NetworkStreamHandler networkStreamHandler)
        {
            var dataArg = Encoding.UTF8.GetString(data).Split("#");

            var gamerIdremove = dataArg[0]; 

            var result = await GameLogicService.RemoveAsync(int.Parse(gamerIdremove));
            var gameToDelete =  GameLogicService.Get(int.Parse(gamerIdremove));

            if (result != -1)
            {
                var responseremove = Encoding.ASCII.GetBytes("Your game was removed");

                var logsevent = new Logs<Game>
                {
                    Object = gameToDelete,
                    Description = "DeleteGame",
                    Date = DateTime.Now,
                };

                PublisherSucriberQueue.SendMessages(logsevent);


                Header headerremove = new Header
                {
                    Type = HeaderType.Response,
                    Command = CommandConstants.RemoveGame,
                    DataLength = responseremove.Length
                };

                await networkStreamHandler.SendAsync(headerHandler.GetHeaderBytes(headerremove));
                await networkStreamHandler.SendAsync(responseremove);
 
            }

            //GRPC
            var deleteGameReply =  await clientgame.DeleteAsync(new DeleteGameRequest {Id = int.Parse(gamerIdremove)});
            Console.WriteLine($"GRPC server response: {deleteGameReply.Message}");

        }

        private static async Task ModifyGameAsync(byte[] data, NetworkStreamHandler networkStreamHandler)
        {
            var datastr = Encoding.ASCII.GetString(data);

            var listgames = datastr.Split('#');

            var gameoldId = int.Parse(listgames[0]);
            var gameTitlenew = listgames[1];
            var gameGendernew = listgames[2];
            var gameRankingPublicnew = listgames[3];
            var gameSynopsisnew = listgames[4];
            var gameCoverNamenew = listgames[5];

            var gameold = GameLogicService.Get(gameoldId);

            var gameNew = new Game()
            {
                Title = gameTitlenew,
                Gender = gameGendernew,
                RankingPublic = double.Parse(gameRankingPublicnew),
                Synopsis = gameSynopsisnew,
                CoverName = gameCoverNamenew
            };

            if (GameLogicService.Update(gameold, gameNew))
            {
                var responseok = Encoding.ASCII.GetBytes(" Your game was modified correctly ");

                var logsevent = new Logs<Game>
                {
                    Object = gameNew,
                    Description = "UpdateGame",
                    Date = DateTime.Now,
                };

                PublisherSucriberQueue.SendMessages(logsevent);

                Header header = new Header
                {
                    Type = HeaderType.Response,
                    Command = CommandConstants.ModifyGame,
                    DataLength = responseok.Length
                };

                await networkStreamHandler.SendAsync(headerHandler.GetHeaderBytes(header));
                await networkStreamHandler.SendAsync(responseok);

            }
            else
            {
                var responsefail = Encoding.ASCII.GetBytes(" The new game is published or old game is not published!! ");

                Header header = new Header
                {
                    Type = HeaderType.Response,
                    Command = CommandConstants.ModifyGame,
                    DataLength = responsefail.Length
                };

                await networkStreamHandler.SendAsync(headerHandler.GetHeaderBytes(header));
                await networkStreamHandler.SendAsync(responsefail);
            }

            //GRPC
            var updateGameReply =  await clientgame.UpdateAsync(new UpdateGameRequest {Id = gameold.Id,NewTitle = gameNew.Title,NewGender = gameNew.Synopsis});
            Console.WriteLine($"GRPC server response: {updateGameReply.Message}");


        }

        private static async Task ListGameAsync(byte[] data, NetworkStreamHandler networkStreamHandler)
        {
            
           var listgam = GameLogicService.GetAll();
            var stringgames = string.Join(',', listgam.Select(game => $"{game.Id}#{game.Title}#{game.Gender}#{game.RankingPublic}#{game.Synopsis}#{game.CoverName}").ToArray());
            var listgambytes = Encoding.ASCII.GetBytes(stringgames);

              var headerlistgame = new Header
              {
                 Type = HeaderType.Response,
                 Command = CommandConstants.ListGame,
                 DataLength = listgambytes.Length
              };

            var encodelistheader = headerHandler.GetHeaderBytes(headerlistgame);

            await networkStreamHandler.SendAsync(encodelistheader);
            await networkStreamHandler .SendAsync(listgambytes);
       
        }

        private static async Task GameSearchAsync(byte[] data, NetworkStreamHandler networkStreamHandler)
        {
            var dataArgs = Encoding.ASCII.GetString(data).Split("#");

            var title = dataArgs[0];
      
            var gameresult = GameLogicService.GetByTitle(title);

             if (!(gameresult is null))
             {
                 var gamereposearch = Encoding.ASCII.GetBytes($"{gameresult.Id}#{gameresult.Title}#{gameresult.Gender}#{gameresult.RankingPublic}#{gameresult.Synopsis}#{gameresult.CoverName}");

                 var headersearch = new Header
                 {
                     Type = HeaderType.Response,
                     Command = CommandConstants.GameSearch,
                     DataLength = gamereposearch.Length
                 };


                await networkStreamHandler.SendAsync(headerHandler.GetHeaderBytes(headersearch));
                await networkStreamHandler.SendAsync(gamereposearch);
                 
             }
             else
             {
                 var headerempty = new Header
                 {
                     Type = HeaderType.Response,
                     Command = CommandConstants.GameSearch,
                     DataLength = 0
                 };

                await networkStreamHandler.SendAsync(headerHandler.GetHeaderBytes(headerempty));
             }
        }

        private static async Task GameQualifyAsync(byte[] data, NetworkStreamHandler networkStreamHandler)
        {
            var dataArgs = Encoding.ASCII.GetString(data).Split("#");

             var gameId = dataArgs[0];
             var comment = dataArgs[1];
             var note = int.Parse(dataArgs[2]);
             var username = dataArgs[3];
            
             var game = GameLogicService.Get(int.Parse(gameId));

             var review = new Review
             {
                 Username = username,
                 Comment = comment,
                 Note = note
             };

             game.AddListReview(review);

             var messageclient = Encoding.ASCII.GetBytes("Game review saved successfully");

             var header = new Header
             {
                 Type = HeaderType.Response,
                 Command = CommandConstants.GameQualify,
                 DataLength = messageclient.Length
             };

             var encodeHeader = headerHandler.GetHeaderBytes(header);

             await networkStreamHandler.SendAsync(encodeHeader);
             await networkStreamHandler.SendAsync(messageclient);
             
         }

         private static async Task GameDetailAsync(byte[] data,NetworkStreamHandler networkStreamHandler)
         {
            var dataArgs = Encoding.ASCII.GetString(data).Split("#");
            var gameId = dataArgs[0];

            var gamerepo = GameLogicService.Get(int.Parse(gameId));

             var listreview = gamerepo.GetListReview();
             var stringreview = string.Join(',', listreview.Select(review => $"{review.Username}#{review.Comment}#{review.Note}").ToArray());
             var listgambytes = Encoding.ASCII.GetBytes(stringreview);

             var header = new Header
             {
                 Type = HeaderType.Response,
                 Command = CommandConstants.GameDetail,
                 DataLength = listgambytes.Length
             };

             var encodeHeader = headerHandler.GetHeaderBytes(header);

             
             await networkStreamHandler.SendAsync(encodeHeader);
             await networkStreamHandler.SendAsync(listgambytes);
             
         }
         private static async Task DownloadGameCoverAsync(byte[] data, NetworkStreamHandler networkStreamHandler)
         {
            var gameIdstr = Encoding.ASCII.GetString(data).Split("#");
            var gameId = int.Parse(gameIdstr[0]);

            var gameresult = GameLogicService.Get(gameId);
            string name = gameresult.CoverName;

            string namefile = @name;
            string path = Path.GetFullPath(namefile);
            if (File.Exists(path))
            {
                var fileCommunication = new FileCommunicationHandler(networkStreamHandler);
                await fileCommunication.SendFileAsync(path);
            }
         }

         private static async Task UploadGameCoverAsync(byte[] data, NetworkStreamHandler networkStreamHandler)
         {

            var gameIdstr = Encoding.ASCII.GetString(data).Split("#");
            var gameId = int.Parse(gameIdstr[0]);

            var gameresult = GameLogicService.Get(gameId);

            // 1.- Recibir el largo del nombre del archivo
            byte[] fileNameLengthData = await networkStreamHandler.ReadAsync(ProtocolSpecification.FileNameSize);
            int fileNameLength = BitConverter.ToInt32(fileNameLengthData);
            // 2.- Recibir el nombre del archivo
            byte[] fileNameData = await networkStreamHandler.ReadAsync(fileNameLength);
            string fileName = Encoding.UTF8.GetString(fileNameData);

            gameresult.SetNameCover(fileName);

            var fileCommunication = new FileCommunicationHandler(networkStreamHandler);
            await fileCommunication.ReceiveFileAsync();
         
         }
    }
}
