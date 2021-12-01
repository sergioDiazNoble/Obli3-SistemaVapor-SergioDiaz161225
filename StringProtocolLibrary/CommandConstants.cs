using System;
using System.Collections.Generic;
using System.Text;

namespace StringProtocolLibrary
{
   public enum CommandConstants
   {
        GamePublication = 1,
        RemoveGame = 2,
        ModifyGame = 3,
        GameSearch = 4,
        GameQualify = 5,
        GameDetail = 6,
        UploadGameCover = 7,
        DownloadGameCover = 8,
        ListGame = 9,
        Exit = 10,
        Login = 11
   }
}
