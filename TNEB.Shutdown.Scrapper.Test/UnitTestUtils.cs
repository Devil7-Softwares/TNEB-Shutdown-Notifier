namespace TNEB.Shutdown.Scrapper.Test
{
    public class UnitTestUtils
    {
        [Fact]
        public async Task ShouldFetchCircles()
        {
            ScrappedCircle[] circles = await Utils.GetCircles();
            Assert.NotEmpty(circles);
        }

        [Fact]
        public void ShouldResolveCaptchas()
        {
            DirectoryInfo directoryInfo = new("Captcha");
            Assert.True(directoryInfo.Exists);

            FileInfo[] files = directoryInfo.GetFiles("*.jpg");
            Assert.NotEmpty(files);

            foreach (FileInfo file in files)
            {
                byte[] imageBuffer = File.ReadAllBytes(file.FullName);
                string captcha = Utils.ResolveCaptcha(imageBuffer);
                Assert.NotNull(captcha);
                Assert.NotEmpty(captcha);
                Assert.Equal(file.Name.Split('.')[0], captcha);
            }
        }

        [Fact]
        public async Task ShouldFetchSchedules()
        {
            ScrappedSchedule[] schedules1 = await Utils.GetSchedules("0435");
            ScrappedSchedule[] schedules2 = await Utils.GetSchedules("0430");
            ScrappedSchedule[] schedules3 = await Utils.GetSchedules("0432");
            ScrappedSchedule[] schedules4 = await Utils.GetSchedules("0400");
            ScrappedSchedule[] schedules5 = await Utils.GetSchedules("0401");
            ScrappedSchedule[] schedules6 = await Utils.GetSchedules("0402");
            ScrappedSchedule[] schedules7 = await Utils.GetSchedules("0404");
            ScrappedSchedule[] schedules8 = await Utils.GetSchedules("0406");

            ScrappedSchedule[] schedules = [.. schedules1, .. schedules2, .. schedules3, .. schedules4, .. schedules5, .. schedules6, .. schedules7, .. schedules8];

            Assert.NotEmpty(schedules);
        }
    }
}