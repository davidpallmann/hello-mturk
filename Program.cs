using Amazon.MTurk;
using Amazon.MTurk.Model;

string SANDBOX_URL = "https://mturk-requester-sandbox.us-east-1.amazonaws.com";
//string PROD_URL = "https://mturk-requester.us-east-1.amazonaws.com";

var config = new AmazonMTurkConfig()
{
    RegionEndpoint = Amazon.RegionEndpoint.USEast1,
    ServiceURL = SANDBOX_URL // PROD_URL
};

var mturkClient = new AmazonMTurkClient(config);

var command = (args.Length > 0) ? args[0] : null;

if (command == "balance")
{
    GetAccountBalanceRequest request = new GetAccountBalanceRequest();
    GetAccountBalanceResponse balance = await mturkClient.GetAccountBalanceAsync(request);
    Console.WriteLine("Your account balance is $" + balance.AvailableBalance);
}
else if (command == "add")
{
    string questionXML = System.IO.File.ReadAllText(@"question.xml");

    var hitRequest = new CreateHITRequest()
    {
        LifetimeInSeconds = 60*60*3,
        AssignmentDurationInSeconds = 60*3,
        Reward = "0.03",
        Title = "Type the (one or two words) you see in the image",
        Description = "Enter the highlighted text in the oval",
        Question = questionXML,
        MaxAssignments = 3
    };

    var createResponse = mturkClient.CreateHITAsync(hitRequest);
    var HITId = createResponse.Result.HIT.HITId;
    Console.WriteLine($"HIT created - Id: {HITId}");
    Console.WriteLine($"Worker link: https://workersandbox.mturk.com/projects/{createResponse.Result.HIT.HITTypeId}/tasks");

    var cont = String.Empty;
    Console.WriteLine("Press any key to update, or Q to quit.");
    do
    {
        var statusResponse = mturkClient.GetHITAsync(new GetHITRequest { HITId = HITId });
        var HIT = statusResponse.Result.HIT;
        Console.WriteLine($"Status: {HIT.HITStatus}, Expiration: {HIT.Expiration}, Available: {HIT.NumberOfAssignmentsAvailable}, Pending: {HIT.NumberOfAssignmentsPending}, Completed: {HIT.NumberOfAssignmentsCompleted}");
    } while (Console.ReadKey().KeyChar != 'Q');

}
else if (command=="results")
{
    var listRequest = new ListAssignmentsForHITRequest()
    {
        HITId = args[1]
    };
    var listResponse = await mturkClient.ListAssignmentsForHITAsync(listRequest);
    foreach(var assignment in listResponse.Assignments)
    {
        Console.WriteLine($"Worker Id: {assignment.WorkerId}, Answer: {assignment.Answer}");
    }
}
else
{
    Console.WriteLine("dotnet run -- add ................ creates a new Human Intelligence Task and monitors status");
    Console.WriteLine("dotnet run -- balance ............ shows account balance");
    Console.WriteLine("dotnet run -- results [hit-id] ... shows results for HIT");
}




