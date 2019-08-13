using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Client;
using Abitech.NextApi.Model.Filtering;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Tests.Common;
using Abitech.NextApi.Server.Tests.Filtering;
using Xunit;

namespace Abitech.NextApi.Server.Tests
{
    public class SystemTests : NextApiTest
    {
        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task TestSupportedPermissions(NextApiTransport transport)
        {
            var client = await GetClient(transport);

            var permissions = await client.SupportedPermissions();
            Assert.Equal(new[] {"permission1", "permission2"}, permissions);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task TestNextApiUserAccessor(NextApiTransport transport)
        {
            // case: not authorized
            {
                var client = await GetClient(transport);

                var userId = await client.Invoke<int?>("Test", "GetCurrentUser");
                Assert.Null(userId);
            }
            // case: authorized as user 1
            {
                var client = await GetClient(transport, "1");
                var userId = await client.Invoke<int?>("Test", "GetCurrentUser");
                Assert.Equal(1, userId.Value);
            }
            // case: authorized as user 2
            {
                var client = await GetClient(transport, "2");
                var userId = await client.Invoke<int?>("Test", "GetCurrentUser");
                Assert.Equal(2, userId.Value);
            }
        }

        [Fact]
        public async Task FilterTests()
        {
            var data = TestSource.GetData();

            // filter: entity => entity.ReferenceModel.Name.ToString().Contains("Model") &&
            //                   (entity.Number == 1 || entity.Number == 2) &&
            //                   (new [] {5,6,10}).Contains("Number")
            var filter = new FilterBuilder()
                .Contains("ReferenceModel.Name", "Model")
                .Or(f => f
                    .MoreThan<int>("Number", 1)
                    .LessThan<int>("Number", 2)
                    .Contains("Number", "4")
                )
                .In<int>("Number", new[] {5, 6, 10})
                .Build();
            
            var filter2_0 = new FilterBuilder()
                .Contains("Name", "testModel")
                .Build();
            
            var filter2_1 = new FilterBuilder()
                .Contains("Name", "testmodel")
                .Build();
            
            var filter2_2 = new FilterBuilder()
                .Contains("Name", "Testmodel")
                .Build();
            
            var filter2_3 = new FilterBuilder()
                .Contains("Name", "TESTMODEL")
                .Build();

            var expression = filter.ToLambdaFilter<TestModel>();
            var expression2_0 = filter2_0.ToLambdaFilter<TestModel>();
            var expression2_1 = filter2_1.ToLambdaFilter<TestModel>();
            var expression2_2 = filter2_2.ToLambdaFilter<TestModel>();
            var expression2_3 = filter2_3.ToLambdaFilter<TestModel>();

            var filtered = data.Where(expression).ToList();

            var filtered2_0 = data.Where(expression2_0).ToList();
            var filtered2_1 = data.Where(expression2_1).ToList();
            var filtered2_2 = data.Where(expression2_2).ToList();
            var filtered2_3 = data.Where(expression2_3).ToList();
            
            Assert.True(filtered2_0.Count==500);
            Assert.True(filtered2_1.Count==500);
            Assert.True(filtered2_2.Count==500);
            Assert.True(filtered2_3.Count==500);

            Assert.True(filtered.Count == 3);
            Assert.True(filtered.All(e => e.Number == 5 || e.Number == 6 || e.Number == 10));
        }

        [Fact]
        public async Task FilterIntContainsTest()
        {
            var data = TestSource.GetData();
            var filter = new FilterBuilder()
                .Contains("Number", "423")
                .Build();
            
            var expression = filter.ToLambdaFilter<TestModel>();

            var filtered = data.Where(expression).ToList();

            Assert.True(filtered.Count == 1);
            Assert.Equal(423, filtered.FirstOrDefault()?.Number);
        }
        
    }
}
