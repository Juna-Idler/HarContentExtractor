using System;
using System.IO;
using System.Text.Json;


if (args.Length < 1) return;
string filename = args[0];
string folder = filename.Substring(0, filename.LastIndexOf(".")) + '\\';


using JsonDocument document = JsonDocument.Parse(File.ReadAllText(filename));
JsonElement log = document.RootElement.GetProperty("log");
JsonElement entries = log.GetProperty("entries");

foreach (JsonElement e in entries.EnumerateArray())
{
    try
    {
        JsonElement request = e.GetProperty("request");
        JsonElement response = e.GetProperty("response");

        Uri url = new(request.GetProperty("url").GetString());
        JsonElement content = response.GetProperty("content");

        if (content.TryGetProperty("text", out JsonElement text_element))
        {
            string text = text_element.GetString();

            string path = folder + url.Host + url.AbsolutePath.Replace('/', '\\');
            if (path.LastIndexOf('\\') == path.Length - 1)
                path += "index";
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (content.TryGetProperty("encoding", out JsonElement encoding_element))
            {
                string encoding = encoding_element.GetString();
                if (encoding == "base64")
                {
                    byte[] b = Convert.FromBase64CharArray(text.ToCharArray(), 0, text.Length);
                    File.WriteAllBytes(path, b);
                }
            }
            else
            {
                File.WriteAllText(path, text);
            }
        }
    }
    catch (Exception)
    { }
}


