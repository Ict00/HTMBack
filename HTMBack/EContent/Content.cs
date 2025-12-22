namespace HTMBack.EContent;

public record Content(string Type = "text/html", string Text = "<h1>No</h1>", int ResponseMsg = 200, bool KeepAlive = false);