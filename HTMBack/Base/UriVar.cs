namespace HTMBack.Base;

public record UriVar(string Name, string Content);

public static class UriVarExts
{
    private enum Writing
    {
        Name,
        Value,
        Nothing
    }

    public static string? GetByName(this List<UriVar> uriVars, string name)
    {
        foreach (var i in uriVars)
        {
            if (i.Name == name)
            {
                return i.Content;
            }
        }
        
        return null;
    }
    
    public static List<UriVar> GetVars(this string str)
    {
        List<UriVar> vars = [];
        string currentName = "";
        string currentValue = "";
        Writing now = Writing.Nothing;

        foreach (var i in str)
        {
            switch (now)
            {
                case Writing.Nothing:
                    if (i == '?')
                    {
                        now = Writing.Name;
                    }
                    break;
                case Writing.Name:
                    if (i == '=')
                    {
                        now = Writing.Value;
                        break;
                    }

                    currentName += i;
                    break;
                case Writing.Value:
                    if (i == '?')
                    {
                        now = Writing.Name;
                        if (currentName != "" && currentValue != "")
                        {
                            vars.Add(new UriVar(currentName, currentValue));
                            currentName = "";
                            currentValue = "";
                        }
                        break;
                    }
                    
                    currentValue += i;
                    break;
            }
        }
        
        if (currentName != "" && currentValue != "")
        {
            vars.Add(new UriVar(currentName, currentValue));
        }

        return vars;
    }
}