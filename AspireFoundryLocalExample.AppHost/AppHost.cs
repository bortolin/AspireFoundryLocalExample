using Projects;

var builder = DistributedApplication.CreateBuilder(args);


var foundry = builder.AddAzureAIFoundry("foundry").RunAsFoundryLocal().FoundryLocalInstall();

var chat = foundry.AddDeployment("chat", "phi-3.5-mini", "1", "Microsoft");

//var codeagent = foundry.AddDeployment("codeagent", "qwen2.5-coder-0.5b", "1", "Microsoft");

var chatWebApp = builder.AddProject<ChatWebApp>("chatwebapp").WithReference(chat).WaitFor(chat);

builder.Build().Run();
