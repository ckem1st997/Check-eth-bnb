
using NBitcoin;
using Nethereum.HdWallet;
using Nethereum.Web3;

namespace Check_eth_bnb
{
    internal class Program
    {
        // Biến để cache dữ liệu
        private static List<string> cachedData = new List<string>();
        private static Web3 web3 = new Web3("https://bsc-dataseed.binance.org/");
        static async Task Main(string[] args)
        {

            Console.WriteLine("Input number thread:");
            int number = int.Parse(Console.ReadLine());

            List<string> data = Wordlist.English.GetWords().ToList();
            // Tạo ra 5 Task để chạy hàm Check
            Task[] tasks = new Task[number < 1 ? 1 : number];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    // Gọi hàm Check và đợi kết quả
                    await Check(data);
                });
            }

            // Đợi cho tất cả các Task hoàn thành
            await Task.WhenAll(tasks);

            Console.WriteLine("Tất cả các luồng đã hoàn thành.");


        }
        static async Task Check(List<string> words)
        {
            string currentDirectory = Environment.CurrentDirectory;
            List<string> rd = new List<string>();

            string mnemonicWords = "";
            int count = 0;
            int seedNum = 12;

            Random random = new Random();
            while (true)
            {

                rd = new List<string>();
                var listRd = new List<int>();
                mnemonicWords = string.Empty;
                for (int i = 0; i < seedNum; i++)
                {
                    bool b = true;
                    while (b)
                    {
                        int randomIndex = random.Next(2048);
                        var check = listRd.Where(x => x == randomIndex);
                        if ((check == null || !check.Any()))
                        {
                            rd.Add(randomIndex.ToString());
                            listRd.Add(randomIndex);
                            mnemonicWords = mnemonicWords + " " + words[randomIndex];
                            b = false;
                        }
                    }

                }
                mnemonicWords = mnemonicWords.Trim();
                if (!(!string.IsNullOrEmpty(mnemonicWords) && (mnemonicWords.Split(" ").Length == 12 || mnemonicWords.Split(" ").Length == 24))) continue;
                try
                {
                    count++;
                    var listAddress = new List<string>();

                    // Tạo một ví mới từ seed
                    Wallet wallet = new Wallet(mnemonicWords, null);
                    string accountAddress44 = wallet.GetAccount(0).Address;
                    //if (!string.IsNullOrEmpty(accountAddress44))
                    //{
                    //    listAddress.Add(accountAddress44);
                    //}
                    // Tạo và kiểm tra các loại địa chỉ khác nhau
                    try
                    {
                        // Tạo một task để lấy số dư tài khoản
                        Task<decimal> getBalanceTask = Task.Run(async () =>
                        {
                            var balance = await web3.Eth.GetBalance.SendRequestAsync(accountAddress44);
                            return Web3.Convert.FromWei(balance.Value);
                        });

                        decimal etherAmount = await getBalanceTask;
                        Console.WriteLine($"[{count}]-{accountAddress44}|{etherAmount}");

                        if (etherAmount > 0)
                        {
                            string output = $"12 Seed: {mnemonicWords} | address:{String.Join(", ", listAddress)}";
                            string filePath = Path.Combine(currentDirectory, "btc-wallet.txt");

                            await using (StreamWriter sw = File.AppendText(filePath))
                            {
                                await sw.WriteLineAsync(output);
                            }
                            Console.WriteLine($"Thông tin đã được ghi vào file cho địa chỉ: {String.Join(", ", listAddress)}");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("\nException Caught!");
                        Console.WriteLine("Message :{0} ", e.Message);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }

        }

        static async Task DeriveAndCheckBalance(string listAddress, string csvFilePath, string mnemonicWords)
        {
            try
            {
                // Tạo một task để lấy số dư tài khoản
                Task<decimal> getBalanceTask = Task.Run(async () =>
                {
                    var balance = await web3.Eth.GetBalance.SendRequestAsync(listAddress);
                    return Web3.Convert.FromWei(balance.Value);
                });

                decimal etherAmount = await getBalanceTask;
                if (etherAmount > 0)
                {
                    string output = $"12 Seed: {mnemonicWords} | address:{String.Join(", ", listAddress)}";

                    string currentDirectory = Environment.CurrentDirectory;
                    string projectRootDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(currentDirectory).FullName).FullName).FullName;
                    string filePath = Path.Combine(projectRootDirectory, "btc-wallet.txt");

                    await using (StreamWriter sw = File.AppendText(filePath))
                    {
                        await sw.WriteLineAsync(output);
                    }
                    Console.WriteLine($"Thông tin đã được ghi vào file cho địa chỉ: {String.Join(", ", listAddress)}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        static async Task<List<string>> GetDataAsync(string filePath)
        {
            // Nếu dữ liệu đã được cache, trả về dữ liệu từ cache
            if (cachedData != null && cachedData.Count > 0)
            {
                Console.WriteLine("Lấy dữ liệu từ cache.");
                return cachedData;
            }

            // Nếu chưa có dữ liệu trong cache, đọc từ file
            Console.WriteLine("Đọc dữ liệu từ file và cache nó.");
            cachedData = new List<string>();

            // Kiểm tra xem file có tồn tại không
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File không tồn tại.");
                return cachedData;
            }

            // Đọc file và lưu vào cache
            using (StreamReader reader = new StreamReader(filePath))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    cachedData.Add(line);
                }
            }

            return cachedData;
        }

    }
}
