namespace TNEB.Shutdown.Scrapper.Sample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TimeZoneInfo timezoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

            Console.WriteLine(timezoneInfo.BaseUtcOffset.TotalHours);

            Console.WriteLine("Fetching circles...");
            ScrappedCircle[] circles = Utils.GetCircles().Result;
            Console.WriteLine($"Fetched {circles.Length} circles.");

            string circleCode = string.Empty;
            while (string.IsNullOrEmpty(circleCode) && circles.FirstOrDefault((circle) => circle.Value.Equals(circleCode)) == null)
            {
                foreach (ScrappedCircle circle in circles)
                {
                    Console.WriteLine($"{circle.Value} - {circle.Name}");
                }
                Console.Write("Enter circle code: ");
                circleCode = Console.ReadLine()!;
            }

            Console.WriteLine("Fetching schedules...");
            ScrappedSchedule[] schedules = Utils.GetSchedules(circleCode).Result;

            Console.WriteLine($"Fetched {schedules.Length} schedules.");

            foreach (ScrappedSchedule schedule in schedules)
            {
                Console.WriteLine($"{schedule.Date} - {schedule.From} - {schedule.To} - {schedule.Town} - {schedule.SubStation} - {schedule.Feeder} - {schedule.Location} - {schedule.TypeOfWork}");
            }
        }
    }
}
