var builder = DistributedApplication.CreateBuilder(args);

var pgvector = builder.AddConnectionString("pgvector");
var api = builder.AddProject<Projects.AIBusinessSupport_API>("aibusinesssupportapi");

builder.AddJavaScriptApp("aibusinesssupportweb", "../AIBusinessSupport.Web", "start")
	.WithHttpEndpoint(targetPort: 4200)
	.WithReference(api)
	.WithReference(pgvector)
	.WaitFor(api);


builder.Build().Run();
