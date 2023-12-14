using HtmlAgilityPack;
using System.Globalization;

namespace TNEB.Shutdown.Scrapper
{
    public class Utils
    {
        private static HttpClient _httpClient = new HttpClient();

        private static DateTimeOffset ParseDate(string date)
        {
            return DateTimeOffset.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }

        private static DateTimeOffset ParseDateTime(string dateTime, bool isAM)
        {
            string input = $"{dateTime} {(isAM ? "AM" : "PM")} {Constants.TIMEZONE}".Replace("  ", " ");
            string format = "dd-MM-yyyy hh:mm tt zzz";

            return DateTimeOffset.ParseExact(input, format, null);
        }

        private static Task<string> GetFormHtml()
        {
            return _httpClient.GetStringAsync(Constants.TNEB_FORM_URL);
        }

        public static async Task<Circle[]> GetCircles()
        {
            string html = await GetFormHtml();

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            HtmlNodeCollection options = htmlDoc.DocumentNode.SelectNodes("//select/option");

            List<Circle> circles = new List<Circle>();

            foreach (HtmlNode node in options)
            {
                if (node.GetAttributeValue("value", string.Empty).Equals("A"))
                {
                    continue;
                }

                circles.Add(new Circle(node.InnerText, node.GetAttributeValue("value", string.Empty)));
            }

            return circles.ToArray();
        }

        public static string ResolveCaptcha(byte[] imageBuffer)
        {
            using (Tesseract.TesseractEngine engine = new Tesseract.TesseractEngine("./data", "eng", Tesseract.EngineMode.Default))
            {
                using (Tesseract.Pix pix = Tesseract.Pix.LoadFromMemory(imageBuffer))
                {
                    using (Tesseract.Page page = engine.Process(pix))
                    {
                        return page.GetText().Trim();
                    }
                }
            }
        }

        public static async Task<string> GetCaptcha()
        {
            Stream stream = await _httpClient.GetStreamAsync(Constants.TNEB_CAPTCHA_URL);

            MemoryStream memoryStream = new MemoryStream();

            await stream.CopyToAsync(memoryStream);

            File.WriteAllBytes("Captcha.jpg", memoryStream.ToArray());

            return ResolveCaptcha(memoryStream.ToArray());
        }

        public static async Task<ViewState> GetViewState()
        {
            string html = await GetFormHtml();

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            HtmlNode formNode = htmlDoc.DocumentNode.SelectSingleNode("//form");
            if (formNode == null)
            {
                throw new Exception("Unable to find form node.");
            }

            HtmlNode viewStateNode = formNode.SelectSingleNode("//*[@id=\"j_id1:javax.faces.ViewState:0\"]");
            if (viewStateNode == null)
            {
                throw new Exception("Unable to find view state node.");
            }

            string formId = formNode.GetAttributeValue("id", string.Empty);
            if (string.IsNullOrEmpty(formId))
            {
                throw new Exception("Unable to find form id.");
            }

            string viewState = viewStateNode.GetAttributeValue("value", string.Empty);
            if (string.IsNullOrEmpty(viewState))
            {
                throw new Exception("Unable to find view state.");
            }

            return new ViewState(formId, viewState);
        }

        public static async Task<Schedule[]> GetSchedules(string circleCode)
        {
            ViewState viewState = await GetViewState();
            string captcha = await GetCaptcha();

            // Create FormData
            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add(viewState.Id, viewState.Id);
            formData.Add($"{viewState.Id}:appcat_focus", "");
            formData.Add($"{viewState.Id}:appcat_input", circleCode);
            formData.Add($"{viewState.Id}:cap", captcha);
            formData.Add($"{viewState.Id}:submit3", "");
            formData.Add("javax.faces.ViewState", viewState.State);

            FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(formData);

            HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(Constants.TNEB_FORM_URL, formUrlEncodedContent);

            string html = await httpResponseMessage.Content.ReadAsStringAsync();

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            HtmlNodeCollection rowNodes = htmlDoc.DocumentNode.SelectNodes("//table/tbody/tr");

            if (rowNodes == null || rowNodes.Count == 0)
            {
                return new Schedule[0];
            }

            return rowNodes.TakeWhile(node => node != null).Select((rowNode) =>
            {
                // Parse first column as date in Constants.TIMEZONE
                DateTimeOffset date = ParseDate(rowNode.ChildNodes[0].InnerText);
                string town = rowNode.ChildNodes[1].InnerText;
                string subStation = rowNode.ChildNodes[2].InnerText;
                string feeder = rowNode.ChildNodes[3].InnerText;
                string location = rowNode.ChildNodes[4].InnerText;
                string typeOfWork = rowNode.ChildNodes[5].InnerText;
                DateTimeOffset from = ParseDateTime(rowNode.ChildNodes[6].InnerText, true);
                DateTimeOffset to = ParseDateTime(rowNode.ChildNodes[7].InnerText, false);

                return new Schedule(date, from, to, town, subStation, feeder, location, typeOfWork);
            }).ToArray();
        }
    }
}
