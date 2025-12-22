using HTMBack;

var server = new Server("http://+:8080/");
server.RegisterComponent("text.html", "text", true, true);
server.AddFile("www/files/main.css", "main.css", true, "stylesheet");
server.AddPage("main.html", "/");
server.AddVar("Var", (_, _, _) => "World");

await server.Start();
Console.ReadLine();