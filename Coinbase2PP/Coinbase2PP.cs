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
        private const string tsp = "\nTimestamp";

        static List<CoinbaseCSV> ReadCoinbaseCSV(string filePath,
                                                 bool avoidFirstLines = true)
        {
            using TextReader reader = new StreamReader(filePath);

            string data = reader.ReadToEnd();
            if (avoidFirstLines)
            {
                int dataIdx = data.IndexOf(tsp,
                                           StringComparison.Ordinal);
                data = data[(dataIdx+1)..];
            }

            CsvHelper.Configuration.Configuration cfg =
                new CsvHelper.Configuration.Configuration()
                {
                    HasHeaderRecord = true,
                    IgnoreBlankLines = true,
                    HeaderValidated = null,
                    MissingFieldFound = null,
                };

            using TextReader tr = new StringReader(data);
            using CsvReader csvReader = new CsvReader(tr, cfg, false);
            return csvReader.GetRecords<CoinbaseCSV>().ToList();
        }

        protected static string GetPPSecurityName(string cryptoCurrency,
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

        static string GetPPType(string amount)
        {
            if (amount.StartsWith('-'))
            {
                return PPType.Sell.ToString();
            }
            return PPType.Buy.ToString();
        }

        static string GetPPShares(string amount)
        {
            double parsed = double.Parse(amount, CultureInfo.InvariantCulture);
            return Math.Abs(parsed).ToString();
        }

        static bool WritePortofolioPerformanceCSV(List<CoinbaseCSV> records,
                                                  string filePath)
        {
            List<PortofolioPerformanceCSV> ppRecords =
                new List<PortofolioPerformanceCSV>();
            foreach (CoinbaseCSV rec in records)
            {
                ppRecords.Add(new PortofolioPerformanceCSV()
                {
                    Date = rec.Timestamp,
                    SecurityName = GetPPSecurityName(rec.Currency,
                                                     rec.TransferTotalCurrency),
                    Note = rec.Notes,
                    Fees = rec.TransferFee,
                    Value = rec.TransferTotal,
                    Type = GetPPType(rec.Amount),
                    Shares = GetPPShares(rec.Amount)
                });
            }

            using (TextWriter writer = new StreamWriter(filePath))
            {
                using CsvWriter csvWriter = new CsvWriter(writer);
                csvWriter.WriteRecords(ppRecords);
            }
            
            return true;
        }

        static void ProcessFile(string fileIn, string fileOut)
        {
            Console.WriteLine("################################");
            Console.WriteLine("Parsing '{0}'!", fileIn) ;

            List<CoinbaseCSV> records;
            try
            {
                records = ReadCoinbaseCSV(fileIn);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            if (records == null || !records.Any())
            {
                Console.WriteLine("NO RECORDS FOUND !");
                return ;
            }

            if (!WritePortofolioPerformanceCSV(records, fileOut))
            {
                Console.WriteLine("Error while converting Coinbase CSV as PP CSV !");
                return;
            }

            Console.WriteLine("Coinbase CSV has been succesfully converted as PP CSV !");
            return ;
        }

        static void Main(string[] args)
        {
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
            catch(Exception e)
            {
                Console.WriteLine(e);
                DisplayHelper();
                Finish();
            }

            DirectoryInfo outputDir = null;

            foreach(FileInfo file in fileToProcess)
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
                                               file.Extension) ;
                ProcessFile(file.FullName, fileout);
            }
            Finish();
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
