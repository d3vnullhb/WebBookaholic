namespace Bookaholic.Models
{
    public class PaymentInformationModel
    {
        public string OrderType { get; set; } = "billpayment";
        public double Amount { get; set; }
        public string OrderDescription { get; set; } = "";
        public string Name { get; set; } = "";
    }
}
