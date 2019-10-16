using System;
using CsvHelper.Configuration.Attributes;

namespace Coinbase2PP
{
    public class CoinbaseCSV
    {
        public string Timestamp { get; set; }
        public string Balance { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string To { get; set; }
        public string Notes { get; set; }
        [Name("Instantly Exchanged")]
        public string InstantlyExchanged { get; set; }
        [Name("Transfer Total")]
        public string TransferTotal { get; set; }
        [Name("Transfer Total Currency")]
        public string TransferTotalCurrency { get; set; }
        [Name("Transfer Fee")]
        public string TransferFee { get; set; }
        [Name("Transfer Fee Currency")]
        public string TransferFeeCurrency { get; set; }
        [Name("Transfer Payment Method")]
        public string TransferPaymentMethod { get; set; }
        [Name("Transfer ID")]
        public string TransferID { get; set; }
        [Name("Order Price")]
        public string OrderPrice { get; set; }
        [Name("Order Currency")]
        public string OrderCurrency { get; set; }
        [Name("Order Total")]
        public string OrderTotal { get; set; }
        [Name("Order Tracking Code")]
        public string OrderTrackingCode { get; set; }
        [Name("Order Custom Parameter")]
        public string OrderCustomParameter { get; set; }
        [Name("Order Paid Out")]
        public string OrderPaidOut { get; set; }
        [Name("Recurring Payment ID")]
        public string RecurringPaymentID { get; set; }
        [Name("Coinbase ID")]
        public string CoinbaseID { get; set; }
        [Name("Transaction Hash")]
        public string TransactionHash { get; set; }
    }

    public enum CoinbaseCurrency
    {
        BTC,
        LTC,
        ETH
    }
}
