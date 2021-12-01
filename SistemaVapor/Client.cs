using SimpleProtocolShared;
using StringProtocolLibrary;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.IO;
using Common;
using System.Threading.Tasks;

namespace Client
{
    internal static class Client
    {

        public static NetworkStreamHandler networkStreamHandler;
        private static bool NotFinished = true;
        private static HeaderHandler headerHandler = new HeaderHandler();
        private static int sizeheader = HeaderConstants.TypeLength + HeaderConstants.CommandLength + HeaderConstants.DataLength;
         
        static async Task Main(string[] args)
        {

                    
            if (ConnetServer(out var tcpClient)) return;
            networkStreamHandler = new NetworkStreamHandler(tcpClient.GetStream());

            await UsersLogin();

            while (NotFinished)
            {
                
                PrintMenu();
                string selectedOption = Console.ReadLine();
                int option;
                bool validOption = int.TryParse(selectedOption, out option);

                if (validOption) 
                {
                    switch (option)
                    {
                        case 1:
                            await MethodGamePublicationAsync();
                            break;
                        case 2:
                            await MethodGameRemoveAsync();
                            break;
                        case 3:
                            await MethodGameModificationAsync ();
                            break;
                        case 4:
                            await MethodGameSearchAsync();
                            break;
                        case 5:
                            await MethodGameQualifyAsync ();
                            break;
                        case 6:
                            await MethodDetailGameAsync ();
                            break;
                        case 7:
                            await UploadServerGameCoverAsync ();
                            break;
                        case 8:
                            await DownloadServerGameCoverAsync ();
                            break;
                        case 10:
                            await MethodExitAsync ();
                            NotFinished = false;
                            break;
                        default:
                            Console.WriteLine("¡¡¡ No valid option received !!!");
                            break;

                    }    
   
                }
            
            }
        }

        private static async Task UsersLogin()
        {
            var existsname = false;
            while (!existsname)
            {
                Console.WriteLine("Enter a name to login");
                string namelogin = Console.ReadLine().Trim();

                var data = Encoding.ASCII.GetBytes($"{namelogin}");

                var header = new Header
                {
                    Type = HeaderType.Request,
                    Command = CommandConstants.Login,
                    DataLength = data.Length,
                };

                var encodeHeader = headerHandler.GetHeaderBytes(header);

                await networkStreamHandler.SendAsync(encodeHeader);
                await networkStreamHandler.SendAsync(data);
                var dataheader = await networkStreamHandler.ReadAsync(sizeheader);
                var headerrecive = headerHandler.DecodeHeader(dataheader);
                var datadata = await networkStreamHandler.ReadAsync(headerrecive.DataLength);
                var printdata = Encoding.ASCII.GetString(datadata);
                if (printdata.Equals("Your name login was successfully registered"))
                {
                    Console.WriteLine(">> " + printdata + " <<");
                    existsname = true;

                }
                else
                {
                    Console.WriteLine(">> " + printdata + " <<");
                    existsname = true;
                }
            }
  
        }

        private static async Task MethodExitAsync()
        {
            var header = new Header
            {
                Type = HeaderType.Request,
                Command = CommandConstants.Exit,
                DataLength = 0,
            };

            var encodeHandler = headerHandler.GetHeaderBytes(header);
            await networkStreamHandler.SendAsync(encodeHandler);
            
        }

        private static bool ConnetServer(out TcpClient tcpClient)
        {
            string ipAddress = ConfigurationData.ConfigIP();
            int port = ConfigurationData.ConfigPort();

            var clientIpEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), 0);
            tcpClient = new TcpClient(clientIpEndPoint);
            var socketIpEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

            try
            {
               
               tcpClient.Connect(socketIpEndPoint);
                
               
            }
            catch (SocketException)
            {
                Console.WriteLine($"No server online");
                return true;
            }
            return false;
        }

        public static async Task MethodGamePublicationAsync()
        {
            var data = DataGamePublication();

            var header = new Header
            {
                Type = HeaderType.Request,
                Command = CommandConstants.GamePublication,
                DataLength = data.Length,
            };

            var encodeHeader = headerHandler.GetHeaderBytes(header);

  
           await networkStreamHandler.SendAsync(encodeHeader);
           await networkStreamHandler .SendAsync(data);
           var dataheader = await networkStreamHandler.ReadAsync(sizeheader);
           var headerrecive = headerHandler.DecodeHeader(dataheader);
           var datadata = await networkStreamHandler.ReadAsync(headerrecive.DataLength);
           var toprintdata = Encoding.ASCII.GetString(datadata);
           Console.WriteLine();
           Console.WriteLine(">>" + toprintdata + "<<");
        }

        public static byte[] DataGamePublication()
        {
          
            Console.WriteLine("What's the game title?");
            string title = Console.ReadLine().Trim();
            Console.WriteLine("What's the game gender?");
            string gender = Console.ReadLine().Trim();
            Console.WriteLine("What's the game synopsis?");
            string synopsis = Console.ReadLine();
            string cover = "No charge cover";
            var datagame = Encoding.ASCII.GetBytes($"{title}#{gender}#{0}#{synopsis}#{cover}");
           
            return datagame;
        }

        private static async Task MethodGameRemoveAsync()
        {
            var listgames = await HandleListGamePublicationAsync();
            if (listgames != null)
            {
                PrintListGame(listgames);

                bool isnumber;
                int position;
                Console.WriteLine("What game title do you want to delete?");
                do
                {
                    Console.WriteLine("Enter position:");
                    var selectposition = Console.ReadLine();
                    isnumber = int.TryParse(selectposition, out position);
                }
                while (!isnumber || position > listgames.Count || position <= 0);

                var datagame = Encoding.ASCII.GetBytes($"{position}#");

                var header = new Header
                {
                    Type = HeaderType.Request,
                    Command = CommandConstants.RemoveGame,
                    DataLength = datagame.Length,
                };

     
                    var encodeHeader = headerHandler.GetHeaderBytes(header);
                    await networkStreamHandler.SendAsync(encodeHeader);
                    await networkStreamHandler.SendAsync(datagame);

                    var headerremovebyte = await networkStreamHandler.ReadAsync(sizeheader);
                    var headerremove = headerHandler.DecodeHeader(headerremovebyte);

                    var dataremove = await networkStreamHandler.ReadAsync(headerremove.DataLength);
                    var dataprinter = Encoding.ASCII.GetString(dataremove);
                    Console.WriteLine(">>" + dataprinter + "<<");
            }
        }

        private static async Task<List<List<string>>> HandleListGamePublicationAsync()
        {
            var header = new Header
            {
                Type = HeaderType.Request,
                Command = CommandConstants.ListGame,
                DataLength = 0,
            };


            await networkStreamHandler.SendAsync(headerHandler.GetHeaderBytes(header));

            var bufferheader = await networkStreamHandler.ReadAsync(sizeheader);
            var headerlist = headerHandler.DecodeHeader(bufferheader);
            var databyte = await networkStreamHandler.ReadAsync(headerlist.DataLength);
            
            if (databyte.Length == 0)
            {
               Console.WriteLine(">> No game published <<");
               Console.WriteLine();
               return null;
            }

            var dataStr = Encoding.UTF8.GetString(databyte);
            var listgames = dataStr.Split(',').
                            Select(line => {
                               var split = line.Split('#');
                               return new List<string> {split[0],split[1], split[2],split[3],split[4],split[5]};
                            }).ToList();

            return listgames;            
        }
        private static void PrintListGame(List<List<string>> games)
        {
            for(var index = 0; index < games.Count(); index++)
            {
                var game = games.ElementAt(index);
                Console.WriteLine($"{index + 1}) - Id: {game[0]} - Title: {game[1]} - Gender: {game[2]} - Ranking Public: {game[3]} - Synopsis: {game[4]} - Cover: {game[5]}");
            }
        }

        private static async Task MethodGameModificationAsync()
        {
            var listgames = await HandleListGamePublicationAsync();
            if (listgames != null)
            {
                PrintListGame(listgames);

                bool isnumber;
                int position;
                Console.WriteLine("Select game you want to modify");
                do
                {
                    Console.WriteLine("Enter position:");
                    var selectposition = Console.ReadLine();
                    isnumber = int.TryParse(selectposition, out position);
                }
                while (!isnumber || position > listgames.Count || position <= 0);

                var datagamenewbyte = DataGamePublication();

                var datagamenewstr = Encoding.ASCII.GetString(datagamenewbyte);
                                                
                var datagameoldnew = Encoding.ASCII.GetBytes($"{position}#{datagamenewstr}");

                var header = new Header
                {
                    Type = HeaderType.Request,
                    Command = CommandConstants.ModifyGame,
                    DataLength = datagameoldnew.Length,
                };

                    var encodeHeader = headerHandler.GetHeaderBytes(header);
                    await networkStreamHandler.SendAsync(encodeHeader);
                    await networkStreamHandler.SendAsync(datagameoldnew);

                    var datamodify = await networkStreamHandler.ReadAsync(sizeheader);
                    var headerdecod = headerHandler.DecodeHeader(datamodify);
                    var databyte = await networkStreamHandler.ReadAsync(headerdecod.DataLength);
                    var dataprinter = Encoding.ASCII.GetString(databyte);
                    Console.WriteLine();
                    Console.WriteLine(">> " + dataprinter + " <<");
                
            }
        }

        private static async Task MethodGameSearchAsync()
        {
            var listgames = await HandleListGamePublicationAsync();
            if (listgames != null)
            {
                Console.WriteLine("Enter the following data to search for a game");
                Console.WriteLine("Enter title:");
                var title = Console.ReadLine().Trim();

                Console.WriteLine("Enter genero:");
                var gender = (Console.ReadLine().Trim());
                
                bool isnumberuser;
                double gameranking;
                do
                {
                    Console.WriteLine("Enter ranking:");
                    var ranking = Console.ReadLine().Trim();
                    isnumberuser = double.TryParse(ranking, out gameranking);
                }
                while (!isnumberuser);

                var datagame = Encoding.ASCII.GetBytes($"{title}#");

                var header = new Header
                {
                    Type = HeaderType.Request,
                    Command = CommandConstants.GameSearch,
                    DataLength = datagame.Length
                };


                    await networkStreamHandler.SendAsync(headerHandler.GetHeaderBytes(header));
                    await networkStreamHandler.SendAsync(datagame);

                    var headerrecivesearch = await networkStreamHandler.ReadAsync(sizeheader);
                    var headerrecivegame = headerHandler.DecodeHeader(headerrecivesearch);


                if (headerrecivegame.DataLength == 0)
                {
                    Console.WriteLine($">> Game not found {title} <<");
                }
                else
                {
                    var datarecive = await networkStreamHandler.ReadAsync(headerrecivegame.DataLength);
                    var datarecivesearch = Encoding.ASCII.GetString(datarecive).Split('#');

                    if (datarecivesearch[2] == gender && int.Parse(datarecivesearch[3]) == gameranking)
                    {
                        Console.Write("     Id: ");
                        Console.WriteLine(datarecivesearch[0]);
                        Console.Write("     Title: ");
                        Console.WriteLine(datarecivesearch[1]);
                        Console.Write("     Gender: ");
                        Console.WriteLine(datarecivesearch[2]);
                        Console.Write("     Ranking: ");
                        Console.WriteLine(datarecivesearch[3]);
                        Console.Write("     Synopsis: ");
                        Console.WriteLine(datarecivesearch[4]);
                        Console.Write("     Cover: ");
                        Console.WriteLine(datarecivesearch[5]);
                    }
                    else
                    {
                        Console.Write("Game does not match ranking or gender");
                    }

                }
           
            }
        }

        private static async Task MethodGameQualifyAsync()
        {
            var listgames = await HandleListGamePublicationAsync();
            if (listgames != null)
            {
                PrintListGame(listgames);
                       
                bool isnumber;
                int position;
                Console.WriteLine("Select game to add a brief comment and note");

                do
                {
                    Console.WriteLine("Enter position:");
                    var selectposition = Console.ReadLine();
                    isnumber = int.TryParse(selectposition, out position);
                }
                while (!isnumber || position > listgames.Count || position <= 0);

                Console.WriteLine("Enter username");
                var username = Console.ReadLine().Trim();

                Console.WriteLine("Enter comment:");
                var comment = Console.ReadLine();

                bool validposition = false;
                var note = 0;
                string select = "";
                while (!validposition || (1 > note || 10 < note))
                {
                    Console.WriteLine("Define your note between 1 - 10");
                    select = Console.ReadLine();
                    validposition = int.TryParse(select, out note);
                }

                var data = Encoding.ASCII.GetBytes($"{position}#{comment}#{note}#{username}");

                var header = new Header
                {
                    Type = HeaderType.Request,
                    Command = CommandConstants.GameQualify,
                    DataLength = data.Length
                };

                var encodeHeader = headerHandler.GetHeaderBytes(header);
                await networkStreamHandler.SendAsync(encodeHeader);
                await networkStreamHandler.SendAsync(data);

                var headerread = await networkStreamHandler .ReadAsync(sizeheader);
                var decodeheader = headerHandler.DecodeHeader(headerread);

                var datarecive = await networkStreamHandler .ReadAsync(decodeheader.DataLength);
                var recivemessage = Encoding.ASCII.GetString(datarecive);
                Console.WriteLine(">> " + recivemessage + " <<");

            }
        }

        private static async Task MethodDetailGameAsync()
        {
            var listgames = await HandleListGamePublicationAsync();
            if (listgames != null)
            {
                PrintListGame(listgames);

                bool isnumber;
                int position;

                Console.WriteLine("What game title do you want to see the detail?");
                do
                {
                    Console.WriteLine("Enter position:");
                    var selectposition = Console.ReadLine();
                    isnumber = int.TryParse(selectposition, out position);
                }
                while (!isnumber || position > listgames.Count || position <= 0);

                var gamebyte = Encoding.ASCII.GetBytes($"{position}#");

                var header = new Header
                {
                    Type = HeaderType.Request,
                    Command = CommandConstants.GameDetail,
                    DataLength = gamebyte.Length
                };

               
                    var encodeHeader = headerHandler.GetHeaderBytes(header);
                    await networkStreamHandler.SendAsync(encodeHeader);
                    await networkStreamHandler.SendAsync(gamebyte);

                    var reciveheader = await networkStreamHandler.ReadAsync(sizeheader);
                    var headerrecive = headerHandler.DecodeHeader(reciveheader);

                    var recivemessage = await networkStreamHandler.ReadAsync(headerrecive.DataLength);
                    var listdetails = Encoding.ASCII.GetString(recivemessage);

                if (listdetails == "" )
                {
                    Console.WriteLine(">> No details to show for this game <<");
                }
                else
                {
                    var listreview = listdetails.Split(',').
                        Select(line =>
                        {
                            var split = line.Split('#');
                            return new List<string> {split[0],split[1],split[2]};
                    }).ToList();

                    for (var index = 0; index < listreview.Count(); index++)
                    {
                        var review = listreview.ElementAt(index);
                        Console.WriteLine($"{index + 1}) - Username: {review[0]} - Comment: {review[1]} - Note: {review[2]}");
                    }
                }

            }
        }
        private static async Task UploadServerGameCoverAsync()
        {
            var listgames = await HandleListGamePublicationAsync();
            if (listgames != null)
            {
                PrintListGame(listgames);

                bool isnumber;
                int position;

                do
                {
                    Console.WriteLine("Enter position:");
                    var selectposition = Console.ReadLine();
                    isnumber = int.TryParse(selectposition, out position);
                }
                while (!isnumber || position > listgames.Count || position <= 0);

                var gamebyte = Encoding.ASCII.GetBytes($"{position}#");

                var header = new Header
                {
                    Type = HeaderType.Request,
                    Command = CommandConstants.UploadGameCover,
                    DataLength = gamebyte.Length
                };

                
                var encodeHeader = headerHandler.GetHeaderBytes(header);
                await networkStreamHandler.SendAsync(encodeHeader);
                await networkStreamHandler.SendAsync(gamebyte);

                Console.WriteLine("Enter the path of the file to send");
                string path = Console.ReadLine();

                while (!File.Exists(path))
                {
                    Console.WriteLine("Your path not exist try again");
                    path = Console.ReadLine();
                }

                var fileInfo = new FileInfo(path);
                string fileName = fileInfo.Name;
                byte[] fileNameData = Encoding.UTF8.GetBytes(fileName);
                int fileNameLength = fileNameData.Length;
                byte[] fileNameLengthData = BitConverter.GetBytes(fileNameLength);
                // 1.- Envío el largo del nombre del archivo
                await networkStreamHandler.SendAsync(fileNameLengthData);
                // 2.- Envío el nombre del archivo
                await networkStreamHandler .SendAsync(fileNameData);

                Console.WriteLine("Sending file to server...");
                var fileCommunication = new FileCommunicationHandler(networkStreamHandler);
                await fileCommunication.SendFileAsync(path);
                Console.WriteLine("Done sending file");
            }
        }
        private static async Task DownloadServerGameCoverAsync()
        {
            var listgames = await HandleListGamePublicationAsync();
           if (listgames != null)
            {
                PrintListGame(listgames);

                bool isnumber;
                int position;

                do
                {
                    Console.WriteLine("Enter position:");
                    var selectposition = Console.ReadLine();
                    isnumber = int.TryParse(selectposition, out position);
                }
                while (!isnumber || position > listgames.Count || position <= 0);

                var game = listgames.ElementAt(position - 1);

                if (game[5] == "No charge cover")
                {
                    Console.WriteLine(">> ¡¡Firts you have to upload file to server!! <<");
                }
                else
                {
                    var gamebyte = Encoding.ASCII.GetBytes($"{game[0]}#");

                    var header = new Header
                    {
                        Type = HeaderType.Request,
                        Command = CommandConstants.DownloadGameCover,
                        DataLength = gamebyte.Length
                    };

                    var encodeHeader = headerHandler.GetHeaderBytes(header);
                    await networkStreamHandler.SendAsync(encodeHeader);
                    await networkStreamHandler.SendAsync(gamebyte);

                    var fileCommunication = new FileCommunicationHandler(networkStreamHandler);
                    await fileCommunication.ReceiveFileAsync();

                    Console.WriteLine("Done receiving file");

                }
                
            }
        }

        private static void PrintMenu()
        {
            Console.WriteLine();
            Console.WriteLine("Choose one of the following options:");
            Console.WriteLine("1) Game Publication");
            Console.WriteLine("2) Remove game");
            Console.WriteLine("3) Modify game");
            Console.WriteLine("4) Search game");
            Console.WriteLine("5) Qulalify game");
            Console.WriteLine("6) Detail of a game");
            Console.WriteLine("7) Upload a game cover to the server");
            Console.WriteLine("8) Download a game cover to the client");
            Console.WriteLine("10) Exit");
            Console.WriteLine();           
        }
    }
}
