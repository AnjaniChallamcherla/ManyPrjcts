using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TalentErrorHandling
{
    internal class GetDateTimeInCDS
    {

        internal async Task<DateTime> GetWorkerCreatedUpdate(string workerNumber)
        {
            DateTime workerCreatedTime = DateTime.Now;
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Constants.LogicAppCDSWorkflowURL))
                {
                    request.Content = new StringContent("{\"workerNumber\":\"" + workerNumber + "\",\"entityType\":\"Worker\"}", Encoding.UTF8, "application/json");
                    using (var response = await client.SendAsync(request))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var getDateTime = await response.Content.ReadAsStringAsync();

                            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(getDateTime);
                            workerCreatedTime = DateTime.Parse(json["createdDateTime"].ToString());
                        }
                    }
                }
            }

            return workerCreatedTime;
        }

    }
}
