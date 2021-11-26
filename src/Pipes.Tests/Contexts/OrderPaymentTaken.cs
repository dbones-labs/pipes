namespace Pipes.Tests.Contexts
{
    public class OrderPaymentTaken : MessageBase
    {
        public decimal Total { get; set; }
        public string CustomerId { get; set; }
    }
}