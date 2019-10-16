using System;
using CsvHelper.Configuration.Attributes;

namespace Coinbase2PP
{
    public enum PortofolioPerformanceCsvType
    {
        AccountTransactions = 0,
        PorfofolioTransactions,
        Securities,
        HistoricalQuotes,
        SecuritiesAccount
    }

    public enum PPType
    {
        Buy,
        Deposit,
        Dividend,
        Fees,
        [Name("Fees Refund")]
        FeesRefund,
        Interest,
        [Name("Interest Charge")]
        InterestCharge,
        Removal,
        Sell,
        [Name("Tax Refund")]
        TaxRefund,
        Taxes,
        [Name("Transfer (Inbound)")]
        TransferInbound,
        [Name("Transfer (Outbound)")]
        TransferOutbound,
    }

    public class PortofolioPerformanceCSV
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public string ISIN { get; set; }
        public string TickerSymbol { get; set; }
        public string WKN { get; set; }
        public string SecurityName { get; set; }
        public string Value { get; set; }
        public string TransactionCurrency { get; set; }
        public string Fees { get; set; }
        public string Taxes { get; set; }
        public string GrossAmount { get; set; }
        public string CurrencyGrossAmount { get; set; }
        public string ExchangeRate { get; set; }
        public string Shares { get; set; }
        public string Type { get; set; }
        public string Note { get; set; }
    }
}
