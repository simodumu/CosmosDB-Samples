/*
 * 
 * Sample demonstrate the performance of the queries when index are used and without indexing
 * Using appropriate indexing will have high impact on the query performance and cost 
 * 
 * Refer to following documents for more insights on the indexing 
 * https://learn.microsoft.com/en-us/azure/cosmos-db/index-policy
 * https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-manage-indexing-policy?tabs=dotnetv3%2Cpythonv3
 * https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/performance-tips-dotnet-sdk-v3?tabs=trace-net-core
 * https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/index-metrics
 * 
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace SampleQueryPerformanceV3
{
    internal class Program
    {
        private const string Endpoint = "https://testquerytuning.documents.azure.com:443/";
        private const string Key = "WBpPB7oxOuCpR9zILtwZJiNQ7YfcGDZoiVBweQSQomUldG3KSzUHuDgWCrU8z8QDdrkhaBE5987GACDbrIYQmg==";
        private const string DatabaseName = "RetailIngest";
        private const string CollectionWithIndex = "WebsiteMetrics"; //Collect with index on Price property
        private const string CollectionWithOutIndex = "WebsiteMetricsNoIndex";//Collect without index on Price property
        private static CosmosClient client;
        static void Main(string[] args)
        {

            try
            {
                // Create a new instance of the DocumentClient
                client = new CosmosClient(Endpoint, Key);
                Console.WriteLine("=====================================================");
                Console.WriteLine("=====================================================");
                Console.WriteLine("Query metric for sample query with indexed properties");
                GetQueryPerformance(DatabaseName, CollectionWithIndex).Wait();
                Console.WriteLine("=====================================================\n\n");

                Console.WriteLine("=====================================================");
                Console.WriteLine("=====================================================");
                Console.WriteLine("Query metric for sample query without indexed properties");
                GetQueryPerformance(DatabaseName, CollectionWithOutIndex).Wait();
                Console.WriteLine("=====================================================\n\n");
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            Console.ReadKey();
            return;
        }//main
        private static async Task GetQueryPerformance(string databaseId, string collectionId)
        {
            try
            {
                // Get a reference to the database and collection
                var database = client.GetDatabase(databaseId);
                var collection = database.GetContainer(collectionId);
               
                // Setting PopulateQueryMetrics to true in the FeedOptions
                var requestOptions = new QueryRequestOptions
                {
                    PopulateIndexMetrics = true
                };
                string query = "select * from c where c.Price >=300";
                var queryResult = collection.GetItemQueryIterator<dynamic>(query, requestOptions: requestOptions);
                if (queryResult.HasMoreResults)
                {
                    // Execute one continuation of the query
                    var feedResponse = await queryResult.ReadNextAsync();
                    Console.WriteLine("Query Used# {0}", query);
                    Console.WriteLine("Database Name# {0}", databaseId);
                    Console.WriteLine("Collection Name# {0}", collectionId);
                    Console.WriteLine("\n******RU's Utilized:{0}******", feedResponse.RequestCharge);
                    
                    Console.WriteLine("\n");
                    Console.WriteLine("Index Metrics:{0} \n", feedResponse.IndexMetrics);

                    Console.WriteLine("\nDiagnostic Details**************************************");
                    Console.WriteLine(feedResponse.Diagnostics);
                }


            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }

        }

    }//class
}//namespace
