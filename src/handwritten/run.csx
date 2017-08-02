#r "Newtonsoft.Json"

using System.Net.Http.Headers;
using System.Configuration;

private static readonly string key = ConfigurationManager.AppSettings["SubscriptionKey"];
//private static readonly string endpoint = ConfigurationManager.AppSettings["Url"];
// private static readonly string endpoint = ConfigurationManager.AppSettings["QueryParams"];

public async static Task<string> Run(Stream myBlob, string name, TraceWriter log)
{
    log.Info($"Calling computer vision for {name}...");

    var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

    var array = await ToByteArrayAsync(myBlob);

    var payload = new ByteArrayContent(array);
    payload.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/octet-stream");

    var endpoint = "https://westus.api.cognitive.microsoft.com/vision/v1.0/";
    var queryParams = "ocr?language=en&detectOrientation=true";// "/analyze?visualFeatures=ImageType,Faces,Adult,Categories,Color,Tags,Description"
    var results = await client.PostAsync(endpoint + queryParams, payload);

    log.Info("Status code " + results.StatusCode);

    var obj = await results.Content.ReadAsAsync<object>();
    var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

    log.Info($"Results for {name} : {json}");

    return json;
}

// Converts a stream to a byte array
private async static Task<byte[]> ToByteArrayAsync(Stream stream)
{
    var length = stream.Length > Int32.MaxValue ? Int32.MaxValue : Convert.ToInt32(stream.Length);
    var buffer = new Byte[length];
    await stream.ReadAsync(buffer, 0, length);
    stream.Position = 0;
    return buffer;
}