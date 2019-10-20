using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace Coinbase2PP
{
    class Coinbase2PP
    {
        public enum CoinbaseCsvType
        {
            Unknown,
            Coinbase,
            CoinbasePro
        }

        //Coinbase CSV starts with "Transactions" but Data with "Timestamp"
        private const string CoinbaseFirstColumn = "Transactions";
        private const string CoinbaseFirstDataColumn = "\nTimestamp";

        //CoinbasePro CSV starts with "trade id" and is the start of the Data
        private const string CoinbaseProFirstColumn = "trade id";

        private CsvHelper.Configuration.Configuration cfg;

        public void InitCsvHelperConfig()
        {
            cfg = new CsvHelper.Configuration.Configuration()
            {
                   HasHeaderRecord = true,
                   IgnoreBlankLines = true,
                   HeaderValidated = null,
                   MissingFieldFound = null,
            };
        }

        private List<CoinbaseProCSV> ReadCoinbaseProCSV(string filePath)
        {
            using StreamReader tr = new StreamReader(filePath);
            using CsvReader csvReader = new CsvReader(tr, cfg, false);
            return csvReader.GetRecords<CoinbaseProCSV>().ToList();
        }

        private List<CoinbaseCSV> ReadCoinbaseCSV(string filePath,
                                                  bool avoidFirstLines = true)
        {
            using TextReader reader = new StreamReader(filePath);

            string data = reader.ReadToEnd();
            if (avoidFirstLines)
            {
                int dataIdx = data.IndexOf(CoinbaseFirstDataColumn,
                                           StringComparison.Ordinal);
                data = data[(dataIdx+1)..];
            }

            using TextReader tr = new StringReader(data);
            using CsvReader csvReader = new CsvReader(tr, cfg, false);
            return csvReader.GetRecords<CoinbaseCSV>().ToList();
        }

        private string GetPPSecurityName(string cryptoCurrency,
                                         string currency)
        {
            if (cryptoCurrency == CoinbaseCurrency.BTC.ToString() ||
                cryptoCurrency == CoinbaseCurrency.LTC.ToString() ||
                cryptoCurrency == CoinbaseCurrency.ETH.ToString())
            {
                return string.Format("{0}-{1}",
                                     cryptoCurrency,
                                     currency);
            }
            return null;
        }

        private string GetPPType(string amount)
        {
            if (amount.StartsWith('-'))
            {
                return PPType.Sell.ToString();
            }
            return PPType.Buy.ToString();
        }

        private string GetPPAbs(string value)
        {
            double parsed = double.Parse(value, CultureInfo.InvariantCulture);
            return Math.Abs(parsed).ToString();
        }

        private List<PortofolioPerformanceCSV> GetPPRecordsFromCoinbaseRecords(List<CoinbaseCSV> records)
        {
            List<PortofolioPerformanceCSV> ppRecords =
                new List<PortofolioPerformanceCSV>();
            foreach(CoinbaseCSV rec in records.Where(r => !string.IsNullOrEmpty(r.TransferTotal)))
            {
                ppRecords.Add(new PortofolioPerformanceCSV()
                {
                    Date = rec.Timestamp,
                    SecurityName = GetPPSecurityName(rec.Currency,
                                                     rec.TransferTotalCurrency),
                    Note = CoinbaseCsvType.Coinbase.ToString() + ": "
                         + rec.Notes,
                    Fees = rec.TransferFee,
                    Value = rec.TransferTotal,
                    Type = GetPPType(rec.Amount),
                    Shares = GetPPAbs(rec.Amount)
                });
            }

            return ppRecords;
        }

        private List<PortofolioPerformanceCSV> GetPPRecordsFromCoinbaseProRecords(List<CoinbaseProCSV> records)
        {
            List<PortofolioPerformanceCSV> ppRecords =
                new List<PortofolioPerformanceCSV>();
            foreach (CoinbaseProCSV rec in records)
            {
                ppRecords.Add(new PortofolioPerformanceCSV()
                {
                    Date = rec.CreatedAt,
                    SecurityName = GetPPSecurityName(rec.SizeUnit,
                                                     rec.PriceFeeTotalUnit),
                    Note = CoinbaseCsvType.CoinbasePro.ToString(),
                    Fees = rec.Fee,
                    Value = GetPPAbs(rec.Total),
                    Type = rec.Side == "BUY" ? PPType.Buy.ToString() :
                                               PPType.Sell.ToString(),
                    Shares = GetPPAbs(rec.Size)
                });
            }

            return ppRecords;
        }

        private bool WritePortofolioPerformanceCSV(List<PortofolioPerformanceCSV> ppRecords,
                                                   string filePath)
        {
            using (TextWriter writer = new StreamWriter(filePath))
            {
                using CsvWriter csvWriter = new CsvWriter(writer);
                csvWriter.WriteRecords(ppRecords);
            }
            
            return true;
        }

        public CoinbaseCsvType CheckCoinbaseCSVType(string fileIn)
        {
            using TextReader reader = new StreamReader(fileIn);
            string data = reader.ReadLine();
            if (data.StartsWith(CoinbaseFirstColumn, StringComparison.Ordinal))
            {
                return CoinbaseCsvType.Coinbase;
            }
            if (data.StartsWith(CoinbaseProFirstColumn, StringComparison.Ordinal))
            {
                return CoinbaseCsvType.CoinbasePro;
            }

            return CoinbaseCsvType.Unknown;
        }

        public void ProcessFile(CoinbaseCsvType csvType,
                                string fileIn, string fileOut)
        {
            Console.WriteLine("################################");
            Console.WriteLine("Parsing {0} - '{1}'!", csvType.ToString(), fileIn) ;

            List<CoinbaseCSV> records = null;
            List<CoinbaseProCSV> proRecords = null;
            try
            {
                if (csvType == CoinbaseCsvType.Coinbase)
                {
                    records = ReadCoinbaseCSV(fileIn);
                }
                else if (csvType == CoinbaseCsvType.CoinbasePro)
                {
                    proRecords = ReadCoinbaseProCSV(fileIn);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            List<PortofolioPerformanceCSV> ppRecords;
            if ((proRecords != null) && proRecords.Any())
            {
                ppRecords = GetPPRecordsFromCoinbaseProRecords(proRecords);
            }
            else if (records != null && records.Any())
            {
                ppRecords = GetPPRecordsFromCoinbaseRecords(records);
            }
            else
            {
                Console.WriteLine("NO Coinbase RECORDS FOUND !");
                return;
            }

            if ((ppRecords == null) || !ppRecords.Any())
            {
                Console.WriteLine("NO PortofolioPerformance RECORDS !");
                return;
            }

            if (!WritePortofolioPerformanceCSV(ppRecords, fileOut))
            {
                Console.WriteLine("Error while converting Coinbase Pro CSV as PP CSV !");
                return;
            }

            return ;
        }
    }

    class MainProgram
    {
        static void Main(string[] args)
        {
            Coinbase2PP coinbase2PP = new Coinbase2PP();

            if (args.Length < 2)
            {
                DisplayHelper();
            }

            List<FileInfo> fileToProcess = new List<FileInfo>();
            try
            {
                for (int i = 0; i < args.Length; i += 2)
                {

                    switch (args[i])
                    {
                        /*FileIn */
                        case "-i":

                            if (fileToProcess.Any())
                            {
                                throw new ArgumentException("Either -i or -d !");
                            }
                            FileInfo f = new FileInfo(args[i + 1]);
                            fileToProcess.Add(f);
                            break;

                        case "-d":
                            if (fileToProcess.Any())
                            {
                                throw new ArgumentException("Either -i or -d !");
                            }

                            foreach (string filename in Directory.EnumerateFiles(args[i + 1]))
                            {
                                FileInfo fi = new FileInfo(filename);
                                if (fi.Extension.ToLower() == ".csv")
                                {
                                    fileToProcess.Add(fi);
                                }
                            }
                            break;

                        default:
                            throw new ArgumentException(string.Format("Invalid Argument '{0}'",
                                                                      args[i]));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                DisplayHelper();
                Finish();
            }

            coinbase2PP.InitCsvHelperConfig();

            DirectoryInfo outputDir = null;

            foreach (FileInfo file in fileToProcess)
            {
                if (outputDir == null)
                {
                    outputDir = Directory.CreateDirectory(
                        Path.Combine(file.DirectoryName, "output"));
                }

                string filename = file.Name.Substring(0, file.Name.LastIndexOf('.'));
                string fileout = string.Format("{0}/{1}{2}",
                                               outputDir,
                                               (filename + "_pp"),
                                               file.Extension);

                Coinbase2PP.CoinbaseCsvType csvType = coinbase2PP.CheckCoinbaseCSVType(file.FullName);

                switch (csvType)
                {
                    case Coinbase2PP.CoinbaseCsvType.Coinbase:
                    case Coinbase2PP.CoinbaseCsvType.CoinbasePro:
                        coinbase2PP.ProcessFile(csvType, file.FullName, fileout);
                        break;

                    case Coinbase2PP.CoinbaseCsvType.Unknown:
                    default:
                        InvalidFileIn(file.FullName);
                        break;
                }
            }
            Finish();
        }

        static void InvalidFileIn(string filename)
        {
            Console.WriteLine("Unrecognized Coinbase CSV FileType for '{0}' - File is ignored !",
                filename);
        }

        static void DisplayHelper()
        {
            Console.WriteLine("Coinbase2PP -i FILEPATH [-d DIRECTORY]");
            Finish();
        }

        static void Finish()
        {
            Console.WriteLine("Press any key to exit !");
            Console.ReadKey();
        }
    }
}
