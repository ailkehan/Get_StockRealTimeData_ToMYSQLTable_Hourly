using System.Text.Json;
using System.Text;
using MySql.Data.MySqlClient;

List<string> shareList = new List<string>() { "AAPL", "GOOG", "MSFT" };

var timeToConvert = DateTime.Now; 
var est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
DateTime targetTime = TimeZoneInfo.ConvertTime(timeToConvert, est);
DateTime time1 = targetTime;

var timer = new PeriodicTimer(TimeSpan.FromSeconds(3600)); // Once per 60 minute(3600 second)
do
{      //This program will start running at 09:30 and end at 16:00(Nasdaq trading hours).
       //It will not run on saturdays(6=saturday) and sundays(0=sunday)
    if ((time1.Hour >= 9) & (time1.Hour <= 16) & (((int)time1.DayOfWeek) != 6) & (((int)time1.DayOfWeek) != 0))
    {
        if (time1.Hour == 9)
        {
            if (time1.Minute >= 30)
            {
                //Business logic
                foreach (string str in shareList)
                {
                    Console.WriteLine(str);
                    string json1 = "";
                    json1 = await JSONConv.Program.Pr1(str);
                    await JSONConv.Program.Pr2(json1, str);
                }
            }
            else
            {
                Console.WriteLine("Time is before 09:30" + time1);
                break; // Exit if program is executed before 09:30
            }
        }
        else
        {
            //Business logic
            foreach (string str in shareList)
            {
                Console.WriteLine(str);
                string json1 = "";

                json1 = await JSONConv.Program.Pr1(str);

                await JSONConv.Program.Pr2(json1, str);
            }
        }
    }
    else
    {
        Console.WriteLine("Hour=" + time1.Hour + "DayofWeek" + (int)time1.DayOfWeek);
        break;
    }
} while (await timer.WaitForNextTickAsync());

namespace JSONConv
{
    public class Program
    {
        async public static Task<string> Pr1(string sticker)
        {
            HttpClient client = new HttpClient();
            string jsonresponseBody = "";
            Console.WriteLine("sticker=" + sticker);
            jsonresponseBody = await client.GetStringAsync("https://financialmodelingprep.com/api/v3/quote/" + sticker + "?&apikey=API-KEY");
            
            jsonresponseBody = jsonresponseBody.Replace("[", "");
            jsonresponseBody = jsonresponseBody.Replace("]", "");
            jsonresponseBody = jsonresponseBody.Replace("null", "0");

            return jsonresponseBody;
        }

        async public static Task Pr2(string jsonresponseBody, string sticker)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonresponseBody);
            MemoryStream stream = new MemoryStream(byteArray);
            var myDeserializedClass = JsonSerializer.Deserialize<Root>(stream);

            double timestmp = myDeserializedClass.timestamp;
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            dateTime = dateTime.AddSeconds(timestmp -18000); // Subtract 5 hour (18000 seconds) from UTC timestamp to get Eastern Standard Time 

            string print_the_Date = dateTime.Year.ToString() + dateTime.Month.ToString().PadLeft(2, '0') + dateTime.Day.ToString().PadLeft(2, '0') +
                                    dateTime.Hour.ToString().PadLeft(2, '0') + dateTime.Minute.ToString().PadLeft(2, '0') + dateTime.Second.ToString().PadLeft(2, '0'); ;
            // Console.WriteLine(print_the_Date + sticker);
            if (sticker.Length > 5)
                sticker = sticker.Remove(5);
                        
            var str = print_the_Date + " " + ((decimal)myDeserializedClass.open).ToString() + " " + myDeserializedClass.price.ToString() + " " + myDeserializedClass.dayHigh.ToString() + " " +
                  myDeserializedClass.dayLow.ToString() + " " + myDeserializedClass.previousClose.ToString() + " " + myDeserializedClass.volume.ToString() + "\n";
            
            // insert the str fields into table(for example nasdaqdb.AAPL table);    

            string cs = "server=localhost;user=root;password=pass;database=nasdaqdb;Allow User Variables=True;";
            using var con = new MySqlConnection(cs);
            con.Open();
            
            decimal open1 = 0.0M;
            open1 = (decimal)myDeserializedClass.open;
            var price1 = (decimal)myDeserializedClass.price;
            var high1 = (decimal)myDeserializedClass.dayHigh;
            var low1 = (decimal)myDeserializedClass.dayLow;
            var close1 = (decimal)myDeserializedClass.previousClose;
            var volume1 = myDeserializedClass.volume;
            var ticker = sticker;
            Console.WriteLine("Open=" + open1 + "sticker=" + sticker + "ticker=" + ticker);
            var sql = "INSERT INTO " + sticker + "(ticker, datetime, open, price, high, low , prevclose, volume) " +
                      "VALUES(?ticker, ?dateTime, ?open, ?price90, ?high, ?low, ?prevclose, ?volume);";
            Console.WriteLine("sql=" + sql);
            MySqlCommand cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("?sticker", sticker);
            cmd.Parameters.AddWithValue("?ticker", ticker);
            cmd.Parameters.AddWithValue("?datetime", dateTime);
            cmd.Parameters.AddWithValue("?open", open1);
            cmd.Parameters.AddWithValue("?price90", price1);
            cmd.Parameters.AddWithValue("?high", high1);
            cmd.Parameters.AddWithValue("?low", low1);
            cmd.Parameters.AddWithValue("?prevclose", close1);
            cmd.Parameters.AddWithValue("?volume", volume1);
            cmd.ExecuteNonQuery();

        }
    }
    
    public class Root
    {
        public string? symbol { get; set; }
        public string? name { get; set; }
        public double price { get; set; }
        public double changesPercentage { get; set; }
        public double change { get; set; }
        public double dayLow { get; set; }
        public double dayHigh { get; set; }
        public double yearHigh { get; set; }
        public double yearLow { get; set; }
        public double marketCap { get; set; }
        public double priceAvg50 { get; set; }
        public double priceAvg200 { get; set; }
        public int volume { get; set; }
        public int avgVolume { get; set; }
        public string? exchange { get; set; }
        public double open { get; set; }
        public double previousClose { get; set; }
        public double eps { get; set; }
        public double pe { get; set; }
        //   public DateTime earningsAnnouncement { get; set; }
        public long sharesOutstanding { get; set; }
        public int timestamp { get; set; }
    }

}
