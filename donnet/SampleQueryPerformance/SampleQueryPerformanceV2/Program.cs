/*
 * 
 * Sample demonstrate the performance of the queries when index are used and without indexing
 * Using appropriate indexing will have high impact on the query performance and cost 
 * 
 * Refer to following documents for more insights on the indexing 
 * https://learn.microsoft.com/en-us/azure/cosmos-db/index-policy
 * https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/performance-tips?tabs=trace-net-core
 * https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-manage-indexing-policy?tabs=dotnetv3%2Cpythonv3
 * 
 */


using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SampleQueryPerformanceV2
{
    internal class Program
    {

        private const string Endpoint = "https://testquerytuning.documents.azure.com:443/";
        private const string Key = "WBpPB7oxOuCpR9zILtwZJiNQ7YfcGDZoiVBweQSQomUldG3KSzUHuDgWCrU8z8QDdrkhaBE5987GACDbrIYQmg==";
        private const string DatabaseName = "RetailIngest";
        private const string CollectionWithIndex= "WebsiteMetrics"; //Collect with index on Price property
        private const string CollectionWithOutIndex = "WebsiteMetricsNoIndex";//Collect without index on Price property
        private static DocumentClient client;

        static void Main(string[] args)
        {
 
            try
            {
                // Create a new instance of the DocumentClient
                client = new DocumentClient(new Uri(Endpoint), Key);
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
            catch (DocumentClientException cre)
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
        }
        private static async Task GetQueryPerformance(string databaseId, string collectionId)
        {
            try
            {
                // Get a reference to the database and collection
                var collection = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

                string query = "select * from c where c.Price >=300";
                // Setting PopulateQueryMetrics to true in the FeedOptions
                FeedOptions feedOptions = new FeedOptions
                {
                    PopulateQueryMetrics = true,
                    EnableCrossPartitionQuery = true
                };


                IDocumentQuery<dynamic> queryResult = client.CreateDocumentQuery(collection, query, feedOptions).AsDocumentQuery();
                if (queryResult.HasMoreResults)
                {
                    // Execute one continuation of the query
                    var feedResponse = await queryResult.ExecuteNextAsync();

                    // This dictionary maps the partitionId to the QueryMetrics of that query
                    IReadOnlyDictionary<string, QueryMetrics> partitionIdToQueryMetrics = feedResponse.QueryMetrics;
                    Console.WriteLine("Query Used# {0}", query);
                    Console.WriteLine("Database Name# {0}", databaseId);
                    Console.WriteLine("Collection Name# {0}", collectionId);
                    Console.WriteLine("\n******RU's Utilized:{0}******", feedResponse.RequestCharge);
                    
                    Console.WriteLine("\n");
                    Console.WriteLine("\nQuery Metrics**************************************");
                    // At this point you have QueryMetrics which you can serialize using .ToString()
                    foreach (KeyValuePair<string, QueryMetrics> kvp in partitionIdToQueryMetrics)
                    {
                        string partitionId = kvp.Key;
                        QueryMetrics queryMetrics = kvp.Value;
                        Console.WriteLine("{0}:{1}", partitionId, queryMetrics);
                    }
                }
            }
            catch (DocumentClientException cre)
            {
                Console.WriteLine(cre.ToString());
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
 
        }
    }
}
