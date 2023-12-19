using System.Text.Json;

// Parse arguments
var (command, param) = args.Length switch
{
    0 => throw new InvalidOperationException("Usage: your_bittorrent.sh <command> <param>"),
    1 => throw new InvalidOperationException("Usage: your_bittorrent.sh <command> <param>"),
    _ => (args[0], args[1])
};


DecodeEncode(param);
 void DecodeEncode(dynamic encodedValue) //l5:helloi52ee
{
    // Parse command and act accordingly
    if (command == "decode")
    {
        // You can use print statements as follows for debugging, they'll be visible when running tests.
        //Console.WriteLine("Logs from your program will appear here!");

        //encodedValue = param;
        if (string.IsNullOrEmpty(encodedValue) || encodedValue.StartsWith("e"))
        {
            Console.WriteLine("finished");
        }

        if (Char.IsDigit(encodedValue[0]))
        {
            var (value, _) = DecodeString(encodedValue);
            Console.WriteLine("Decode String: " + value);
        }
        else if (encodedValue[0] == 'i')
        {
            var (value, _) = DecodeNumber(encodedValue);
            Console.WriteLine("Decode number: " + value);

        }
        else if (encodedValue[0] == 'l')
        {
            var (value, _) = DecodeList(encodedValue);
            foreach (var item in value)
            {
                Console.WriteLine("Decode list: " + item);

            }

        }

    }
}


    (string value, string rest) DecodeString(string encodedValue)
{
    var colonIndex = encodedValue.IndexOf(':');
    if (colonIndex != -1)
    {
        var strLength = int.Parse(encodedValue[..colonIndex]);
        var strValue = encodedValue.Substring(colonIndex + 1, strLength);
        string restString;
        Console.WriteLine(JsonSerializer.Serialize(strValue));

        if (strValue.Length + 2 < encodedValue.Length)
        {
            restString = encodedValue.Substring(strValue.Length + colonIndex + 1);
            return (strValue, restString);
        }

        return (strValue, null!);

    }



    else
    {
        throw new InvalidOperationException("Invalid encoded value: " + encodedValue);
    }
}

(string value, string rest) DecodeNumber(string encodedValue)
{
    var endIndex = encodedValue.IndexOf('e');

    if (endIndex != -1)
    {
        var strValue = encodedValue.Substring(1, endIndex - 1);
        if (endIndex + 1 < encodedValue.Length)
        {
            var restString = encodedValue.Substring(endIndex + 1);

            restString = restString.TrimStart('e');
            return (strValue, restString);
        }

        return (strValue, null!);
    }
    else
    {
        throw new InvalidOperationException("Invalid encoded value: " + encodedValue);
    }
}

(List<string> values, string rest) DecodeList(string encodedValue)
{
    var items = new List<string>();
    string restOfstring = string.Empty;



    if (!encodedValue.StartsWith('e'))
    {
        var colonIndex = encodedValue.IndexOf(':');
        if (colonIndex == -1)
            return (Enumerable.Empty<string>().ToList(), string.Empty);

        var value = encodedValue[1..colonIndex];
        var strLength = int.Parse(value.ToString());
        var strValue = encodedValue.Substring(colonIndex + 1, strLength);
        items.Add(strValue);
        var restString = encodedValue.Substring(strValue.Length + colonIndex + 1);

        restString = restString.Remove(restString.Length - 1);
        while (!string.IsNullOrEmpty(restString) && !restString.StartsWith('e'))
        {
            if (restString.StartsWith('i'))
            {
                var (val, rest) = DecodeNumber(restString);
                restString = rest;
                items.Add(val);
            }
            else if (Char.IsDigit(restString[0]))
            {
                var (val, rest) = DecodeString(restString);
                restString = rest;
                items.Add(val);
            }
        }

        return (items, restOfstring);

    }

    return (items, null!);
}

