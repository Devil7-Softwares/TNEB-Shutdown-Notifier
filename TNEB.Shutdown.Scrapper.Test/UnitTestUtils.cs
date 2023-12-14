namespace TNEB.Shutdown.Scrapper.Test
{
    public class UnitTestUtils
    {
        [Fact]
        public async Task ShouldFetchCircles()
        {
            Circle[] circles = await Utils.GetCircles();
            Assert.NotEmpty(circles);
        }

        [Fact]
        public void ShouldResolveCaptchas()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo("Captcha");
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
            Schedule[] schedules1 = await Utils.GetSchedules("0435");
            Schedule[] schedules2 = await Utils.GetSchedules("0430");
            Schedule[] schedules3 = await Utils.GetSchedules("0432");
            Schedule[] schedules4 = await Utils.GetSchedules("0400");
            Schedule[] schedules5 = await Utils.GetSchedules("0401");
            Schedule[] schedules6 = await Utils.GetSchedules("0402");
            Schedule[] schedules7 = await Utils.GetSchedules("0404");
            Schedule[] schedules8 = await Utils.GetSchedules("0406");

            Schedule[] schedules = schedules1.Concat(schedules2).Concat(schedules3).Concat(schedules4).Concat(schedules5).Concat(schedules6).Concat(schedules7).Concat(schedules8).ToArray();

            Assert.NotEmpty(schedules);
        }
    }
}