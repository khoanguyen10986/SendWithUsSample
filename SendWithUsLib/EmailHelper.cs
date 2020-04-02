using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace SendWithUsLib
{
    public static class EmailHelper
    {
        public static string ToJson<T>(this T model)
        {
            string json = JsonConvert.SerializeObject(model, Converter.Settings);
            return json;
        }

        public static T FromJson<T>(this string json)
        {
            T model = JsonConvert.DeserializeObject<T>(json, Converter.Settings);
            return model;
        }

        public static string ToHtmlTemplate(this string htmlFileName)
        {
            string templatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"Templates/{htmlFileName}");
            var templateHtmlString = File.ReadAllText(templatePath);
            return templateHtmlString;
        }

        public static EmailSetting ToEmailSetting()
        {
            string emailSettingPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "emailSetting.json");
            var json = File.ReadAllText(emailSettingPath);
            var setting = JsonConvert.DeserializeObject<EmailSetting>(json, Converter.Settings);
            return setting;
        }
    }
}