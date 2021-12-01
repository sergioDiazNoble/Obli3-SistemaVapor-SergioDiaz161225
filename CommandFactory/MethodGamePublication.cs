using StringProtocolLibrary;

namespace CommandFactory
{
    /*public class MethodGamePublication: AbstractMethod
    {
        private readonly HeaderHandler _headerHander;
        private readonly SocketHandler _socketHandler;

        public MethodGamePublication(HeaderHandler headerHandler, SocketHandler socketHandler,byte[] data) 
        {

            _headerHander = headerHandler;
            _socketHandler = socketHandler;

        }

        public  GamePublication() 
        {
            var header = new Header
            {
                Type = HeaderType.Request,
                Command = CommandConstants.GamePublication,
                DataLength = data.Length,
            };

            var encodeHeader = headerHandler.GetHeaderBytes(header);
            socketHandler.Send(encodeHeader);
            socketHandler.Send(data);

            var dataheader = socketHandler.Read(sizeheader);
            var headerrecive = headerHandler.DecodeHeader(dataheader);


        }



    }*/
}
