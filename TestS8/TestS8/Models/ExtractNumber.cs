using Microsoft.Extensions.Primitives;

namespace TestS8.Models
{
    public class ExtractNumber
    {
        public float Extract( String result)
        {
            if (!string.IsNullOrEmpty(result))
            {

                dynamic jsonResult = Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                if (jsonResult != null && jsonResult.accuracy != null)
                {
                    //  accuracy
                    return (float)jsonResult.accuracy;
                }
            }
            return 0;
        }
    }
}
