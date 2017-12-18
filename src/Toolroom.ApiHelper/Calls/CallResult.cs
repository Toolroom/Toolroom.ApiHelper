namespace Toolroom.ApiHelper
{
    public class CallResult<TData> : CallResultBase
    {
        public TData Data { get; set; }
    }
}