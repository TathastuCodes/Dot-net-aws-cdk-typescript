using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaGetCustomerAccountStatement;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public APIGatewayProxyResponse FunctionHandler( ILambdaContext context)
    {
        Console.WriteLine("entered to FunctionHandler");
        IAmazonS3 client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1 );
        
        string bucket = "customerbalancestatus2023";
        string fileName = "accountstatus.json";
        string responseData = "OK";
        try
        {
           // var stream = DownloadS3Objects(client, bucket, fileName);
            var stream = Task.Run<Stream>(async () => await DownloadS3Objects(client, bucket, fileName)).
                GetAwaiter().GetResult();
            //var stream = task.Result;
            if (stream != null)
            {
                Console.WriteLine("entered to ParseDatafromS3File call pipe");
                responseData = ParseDatafromS3File(stream);
                
            }
            else
            {
                responseData = "{\"errot\":\"Null data\"}";
            }
           
            // return responseData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error from Main : {ex.Message}");
            responseData = ex.Message;
        }
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = JsonSerializer.Serialize(responseData)
        };
        //responseData = "{\"name\":\"Sloka\",\"accountbalance\":\"$10000000000\"}";
        //return responseData;
    }

    /// <summary>
    /// Shows how to download an object from an Amazon S3 bucket to the
    /// local computer.
    /// </summary>
    /// <param name="client">An initialized Amazon S3 client object.</param>
    /// <param name="bucketName">The name of the bucket where the object is
    /// currently stored.</param>
    /// <param name="objectName">The name of the object to download.</param>
    /// <param name="filePath">The path, including filename, where the
    /// downloaded object will be stored.</param>
    /// <returns>A boolean value indicating the success or failure of the
    /// download process.</returns>
    public static async Task<bool> DownloadObjectFromBucketAsync(
        IAmazonS3 client,
        string bucketName,
        string objectName,
        string filePath)
    {
        // Create a GetObject request
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = objectName,
        };

        // Issue request and remember to dispose of the response
        using GetObjectResponse response = await client.GetObjectAsync(request);

        try
        {
            // Save object to local file
            var stream =  response.ResponseStream;
           // await response.WriteResponseStreamToFileAsync($"{filePath}\\{objectName}", true, CancellationToken.None);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            //Console.WriteLine($"Error saving {objectName}: {ex.Message}");
            //return false;
            throw ex;
        }
    }
    public async Task<Stream> DownloadS3Objects(IAmazonS3 client, string bucket,string fileName)
    {
        
        var request = new GetObjectRequest
        {
            BucketName = bucket,
            Key = fileName,
        };

        try
        {
            var response = await client.GetObjectAsync(bucket, fileName) ;
            Console.WriteLine("entered to DownloadS3Objects");            
            return response?.ResponseStream;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error from DownloadS3Objects : {ex.Message}");
            throw;
        }

       // return memoryStream;
       //ListObjectsV2Response response;
       //response=await  client.ListObjectsV2Async(request);
       //foreach(var item in response.S3Objects )
       //{
       //    transferUtility.Download("", bucket, item.Key);
       //}
    }
    protected string ParseDatafromS3File(Stream dataStream)
    {
        try
        {
            Console.WriteLine("entered to ParseDatafromS3File");
            MemoryStream memoryStream = new MemoryStream();
            string jsonString = string.Empty;
            if (dataStream != null)
            {
                using (Stream responseStream = dataStream)
                {
                    responseStream.CopyTo(memoryStream);
                    jsonString = System.Text.Encoding.ASCII.GetString(memoryStream.ToArray());
                }
                Console.WriteLine($"jsonstring : {jsonString}");
            }
            else
            {
                Console.WriteLine("null data stream could not be parsed");
            }
            return jsonString ?? "<json>ParseDatafromS3File</json>";
        }
        catch {
            throw;
        }
    }



}
