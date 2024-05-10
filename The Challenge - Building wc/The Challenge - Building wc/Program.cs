
using System.Text;

bool exitRequested = false;

while (!exitRequested)
{
    // Display prompt
    Write("Enter command and filename (e.g., 'command filename'): ");
    string input = ReadLine()!;

    /// Split the input into command, option, and filename
    string[] parts = input.Split(' ');

    //if (parts.Length < 2 || parts.Length > 3)
    //{
    //    Console.WriteLine("Invalid input format. Please enter 'command option filename'.");
    //    continue;
    //}

    string command = parts[0].ToLower();
    string option = parts.Length == 3 ? parts[1].ToLower() : null!;
    string filename = parts[parts.Length - 1];

    // Process command
    switch (command)
    {
        case "wc":
            ReadFile(filename, option);
            WriteLine($" command {command}. and file name = {filename}");
            break;
        
        case "exit":
            exitRequested = true;
            break;
        default:
            WriteLine("Unknown command. Available commands: read, write, exit");
            break;
    }

    static void ReadFile(string filename, string option)
    {
        try
        {

            byte[] contentBytes = null!;
            string[] contentLines = null!;
            string[] word = null!;
            char[] character = null!;

            // Check if filename is provided
            if (string.IsNullOrEmpty(filename))
            {
                // Read from standard input
                string input = Console.In.ReadToEnd();

                // Proceed with the same logic based on the provided option
                switch (option)
                {
                    case "-c":
                        contentBytes = Encoding.UTF8.GetBytes(input);
                        Console.WriteLine($"Bytes read from standard input: {contentBytes.Length}");
                        break;
                    case "-l":
                        contentLines = input.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                        Console.WriteLine($"Lines read from standard input: {contentLines.Length}");
                        break;
                    case "-w":
                        word = input.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        Console.WriteLine($"Words read from standard input: {word.Length}");
                        break;
                    case "-m":
                        character = input.ToCharArray();
                        Console.WriteLine($"Characters read from standard input: {character.Length}");
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
            else
            {
                switch (option)
                {

                    case "-c":
                        contentBytes = File.ReadAllBytes(filename);
                        Console.WriteLine($"Bytes read from {filename}: {contentBytes?.Length}");
                        break;
                    case "-l":
                        contentLines = File.ReadAllLines(filename);
                        Console.WriteLine($"Lines read from {filename}: {contentLines.Length}");
                        break;
                    case "-w":
                        var contentWord = File.ReadAllText(filename);
                        word = contentWord.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        Console.WriteLine($"word read from {filename}: {word.Length}");
                        break;

                    case "-m":
                        var contentChar = File.ReadAllText(filename);
                        character = contentChar.ToCharArray();
                        Console.WriteLine($"word read from {filename}: {character.Length}");
                        break;
                    default:
                        contentBytes = File.ReadAllBytes(filename);
                        contentLines = File.ReadAllLines(filename);
                        contentWord = File.ReadAllText(filename);
                        word = contentWord.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        contentChar = File.ReadAllText(filename);
                        character = contentChar.ToCharArray();
                        Console.WriteLine($"Bytes read from {filename}: {contentBytes?.Length}");
                        Console.WriteLine($"Lines read from {filename}: {contentLines?.Length}");
                        Console.WriteLine($"Words read from {filename}: {word?.Length}");
                        Console.WriteLine($"Characters read from {filename}: {character?.Length}");
                        break;
                }

            }

        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"File '{filename}' not found.");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
        }
    }

}

