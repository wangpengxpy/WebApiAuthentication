using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using System.Threading.Tasks;
using System.Net.Http;

namespace CustmParameterBindingUnitTest
{

    public class UnitTest1
    {
        [Fact]
        public async Task TestMethod1()
        {
            string url = "http://localhost:7114/api/product/PostRawContent";
      
            string post = "Hello World";

            var httpClient = new HttpClient();
            var content = new StringContent(post);
            var response = await httpClient.PostAsync(url, content);
 

            string result = await response.Content.ReadAsStringAsync();
  
            Xunit.Assert.Equal(result, "\"" + post + "\"");
        }
    }
}
