using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace HelloWorld
{
    class Program
    {
        // connection string to the Event Hubs namespace
        private const string connectionString = "";
        // name of the event hub
        private const string eventHubName = "test";
        // number of events to be sent to the event hub
        private const int numOfEvents = 3;

        // The Event Hubs client types are safe to cache and use as a singleton for the lifetime
        // of the application, which is best practice when events are being published or read regularly.
        static EventHubProducerClient producerClient;
        
        static async Task Main()
        {
            Console.WriteLine("Hello World!");

            // Create a producer client that you can use to send events to an event hub
            producerClient = new EventHubProducerClient(connectionString, eventHubName);

            // Create a batch of events 
            using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

            for (int i = 1; i <= numOfEvents; i++)
            {
                if (! eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes($"Event {i}"))))
                {
                    // if it is too large for the batch
                    throw new Exception($"Event {i} is too large for the batch and cannot be sent.");
                }
            }

            try
            {
                // Use the producer client to send the batch of events to the event hub
                await producerClient.SendAsync(eventBatch);
                Console.WriteLine($"A batch of {numOfEvents} events has been published.");
            }
            finally
            {
                await producerClient.DisposeAsync();
            }


            // var connectionString = "<< CONNECTION STRING FOR THE EVENT HUBS NAMESPACE >>";
            // var eventHubName = "<< NAME OF THE EVENT HUB >>";

            // await using (var producer = new EventHubProducerClient(connectionString, eventHubName))
            // {
            //     string[] partitionIds = await producer.GetPartitionIdsAsync();
            // }
        }
    }
}