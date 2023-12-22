using System.Text.Json;

// Parse arguments
var (command, param) = args.Length switch
{
    0 => throw new InvalidOperationException("Usage: your_bittorrent.sh <command> <param>"),
    1 => throw new InvalidOperationException("Usage: your_bittorrent.sh <command> <param>"),
    _ => (args[0], args[1])
};


DecodeEncode(param);

(string, string) DecodeEncode(dynamic encodedValue) //l5:helloi52ee
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
            var (value, remainder) = DecodeString(encodedValue);
            return ((value, remainder));
        }
        else if (encodedValue[0] == 'i')
        {
            var (value, remainder) = DecodeNumber(encodedValue);
            return ((value, remainder));
        }
        else if (encodedValue[0] == 'l')
        {
            var (value, remainder) = DecodeList(encodedValue);
            foreach (var item in value)
            {
                Console.WriteLine("Decode list: " + JsonSerializer.Serialize(item));
            }

            return ((value.ToString()!, remainder));
        }
        else if (encodedValue[0] == 'd')
        {
            var value = DecodeDictionary(encodedValue);
            foreach (var item in value)
            {
                Console.WriteLine("Dict list: " + JsonSerializer.Serialize(item.Key)  + "=> " + JsonSerializer.Serialize(item.Value));
            }

            return ((value.ToList().ToString()!, null!));
        }

    }
        return (null!, null!);
}


(string value, string rest) DecodeString(string encodedValue)
{
    var colonIndex = encodedValue.IndexOf(':');
    if (colonIndex != -1)
    {
        var strLength = int.Parse(encodedValue[..colonIndex]);
        var strValue = encodedValue.Substring(colonIndex + 1, strLength);
        string restString;
        //Console.WriteLine(JsonSerializer.Serialize(strValue));

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


    var restOfString = encodedValue[1..^1];

    while (!string.IsNullOrEmpty(restOfString) && !restOfString.StartsWith('e'))
    {
        var (value, rest) = DecodeEncode(restOfString);
        items.Add(value);
        restOfString = rest;
    }

    return (items, restOfString);
}

Dictionary<string, string> DecodeDictionary(string encodedValue)
{
    //try to split the string with :d3:foo3:bar5:helloi52ee

    var items = new Dictionary<string, string>();


    var restOfString = encodedValue[1..^1];

    while (!string.IsNullOrEmpty(restOfString) && !restOfString.StartsWith('e'))
    {
        var (key, rest) = DecodeEncode(restOfString);
        var (value, reminder) = DecodeEncode(rest);
        items.Add(key, value);
        restOfString = reminder;
    }

    return items;
}



