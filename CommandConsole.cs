using System.Text;
using System.Net.NetworkInformation;
using System.DirectoryServices;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;
using vault_thing;
using System.Diagnostics.Eventing.Reader;

namespace vault_thing;

public class CommandConsole()
{
    private string Password = "";
    public string CurrentDirectory = Directory.GetCurrentDirectory();

    public (string, Color) ProcessCommand(string command)
    {
        string[] args = command.Split(' ');

        try {
            return args[0] switch
            {
                "echo" => Echo(args[1..]),
                "login" => Login(args[1]),
                "help" => HelpCommand(args[1..]),
                "cd" => ChangeDirectory(args[1..]),
                "ls" => ListFiles(args[1..]),
                "mk" => MakeFile(args[1], args[2]),
                "globulate" => MoveFileToGlob(args[1], args[2]),
                "extract" => ExtractFromGlob(args[1], args[2]),
                "view" => ViewGlobContents(args[1]),
                _ => CommandNotFoundError(args[0])
            };
        } catch (IndexOutOfRangeException) {
            return ("There was an error processing the given arguments.", Color.Red);
        }
    }

    private static (string, Color) HelpCommand(string[] command)
    {
        Dictionary<string, string> commands = new()
        {
            { "echo", " <message> - Repeats <message> in terminal." },
            { "login", " <password> - Sets current glob encryption key to <password>." },
            { "help", " <command = *> - Displays help on <command>, by default will display all commands." },
            { "cd", " <directory> - Changes current directory scope to <directory>. <directory> = '^' retrieves parent directory." },
            { "ls", " <type> - Lists all files / directories in current directory depending on the value of <type>." },
            { "mk", " <type> <name> - Creates file of <type> with name <name>." },
            { "globulate", " <file> <glob> - Moves <file> to <glob>." },
            { "extract", " <file> <glob> - Extracts <file> from <glob>." },
            { "view", " <glob> - Views contents of <glob>."}
        };

        // checks if user is requesting help for specific command
        string response = "";
        if (command.Length == 0)
        {
            foreach (KeyValuePair<string, string> kvPair in commands)
                response += $"\n{kvPair.Key}{kvPair.Value}";

            return (response, Color.MistyRose);
        }

        try {
            response = commands[command[0]];
            return (command[0] + response, Color.MistyRose);
        } catch (KeyNotFoundException) {
            return CommandNotFoundError(command[0]);
        }
    }

    private (string, Color) ViewGlobContents(string glob)
    {
        string globPath = ParseDirectoryRequest(glob);

        if (File.Exists(globPath))
        {
            string json = File.ReadAllText(globPath);

            // get all files in selected .glob file
            List<FileWrapper> files = JsonConvert.DeserializeObject<List<FileWrapper>>(json);

            string allFiles = "\n";

            foreach (FileWrapper f in files)
                allFiles += $"{f.FileName}\n";

            return (allFiles, Color.MistyRose);
        }
        else
            return ($"Error! Glob '{glob}' does not exist!", Color.Red);
    }

    private (string, Color) ExtractFromGlob(string fileName, string glob)
    {
        string globPath = ParseDirectoryRequest(glob);

        if (File.Exists(globPath))
        {
            string json = File.ReadAllText(globPath);

            // get all files in selected .glob file
            List<FileWrapper> files = JsonConvert.DeserializeObject<List<FileWrapper>>(json);

            FileWrapper file = files.FirstOrDefault(file => file.FileName == fileName);

            if (file is null)
                return ($"File '{fileName}' does not exist in glob '{glob}'!", Color.Red);

            File.WriteAllBytes(CurrentDirectory + '\\' + fileName, file.Bytes);

            files.Remove(file);
            json = JsonConvert.SerializeObject(files, Formatting.Indented);
            File.WriteAllText(globPath, json);

            return ($"Extracted '{file.FileName}' from glob '{glob}'.", Color.Green);
        }
        else
            return ($"Error! Glob '{glob}' does not exist!", Color.Red);
    }

    private (string, Color) MoveFileToGlob(string fileName, string glob)
    {
        string? globPath = ParseDirectoryRequest(glob);
        string? filePath = ParseDirectoryRequest(fileName);

        if (File.Exists(filePath) && File.Exists(globPath))
        {
            string json = File.ReadAllText(globPath);

            // get all files in selected .glob file
            List<FileWrapper> files = JsonConvert.DeserializeObject<List<FileWrapper>>(json);

            FileWrapper file = new(fileName, File.ReadAllBytes(filePath));

            files.Add(file);

            json = JsonConvert.SerializeObject(files, Formatting.Indented);
            File.WriteAllText(globPath, json);
            File.Delete(filePath);

            return ($"Moved file '{fileName}' to glob '{glob}'.", Color.Green);
        }
        else
            return ($"Error! Either file '{fileName}' or glob '{glob}' does not exist!", Color.Red);
    }

    private (string, Color) MakeFile(string type, string name)
    {
        string? filePath = ParseDirectoryRequest(name);

        if (filePath is null)
            return ($"File '{name}' could not be created.", Color.Red);

        try {
            if (type == "-f")
                File.Create(filePath).Dispose();
            else if (type == "-d")
                Directory.CreateDirectory(filePath);
            else if (type == "-g")
                File.WriteAllText(filePath + ".glob", "[]");
                
        } catch (Exception ex) {
            return ($"An error occurred whilst attempting to create file '{name}'. '{ex}'", Color.Red);
        }

        return ($"Created file '{name}'.", Color.Green);
    }

    private (string, Color) ListFiles(string[] type)
    {
        string stringAllFiles = "\n";
        DirectoryInfo dInfo = new(CurrentDirectory);

        if (type.Length == 0)
        {
            stringAllFiles += "Files:\n";

            FileInfo[] allFiles = dInfo.GetFiles();

            string allGlobs = "";

            foreach (FileInfo file in allFiles)
            {
                if (file.Name.EndsWith(".glob"))
                    allGlobs += file.Name + '\n';
                else
                    stringAllFiles += file.Name + '\n';
            }
                
            stringAllFiles += "\nDirectories:\n";

            DirectoryInfo[] allDirs = dInfo.GetDirectories();

            foreach (DirectoryInfo dir in allDirs)
                stringAllFiles += dir.Name + '\n';

            stringAllFiles += "\nGlobs:\n" + allGlobs;
            return (stringAllFiles, Color.MistyRose);
        }

        switch(type[0])
        {
            case "-d":
                DirectoryInfo[] allDirs = dInfo.GetDirectories();

                foreach (DirectoryInfo dir in allDirs)
                    stringAllFiles += dir.Name + '\n';
                break;
            case "-f":
                FileInfo[] allFiles = dInfo.GetFiles();

                foreach (FileInfo file in allFiles)
                    stringAllFiles += file.Name + '\n';
                break;
            case "-g":
                allFiles = dInfo.GetFiles();

                string allGlobs = "";

                foreach (FileInfo file in allFiles)
                    if (file.Name.EndsWith(".glob"))
                        allGlobs += file.Name + '\n';

                stringAllFiles += allGlobs;
                break;
            default:
                return ($"Invalid argument '{type[0]}'.", Color.Red);
        };

        return (stringAllFiles, Color.MistyRose);
    }

    private string? ParseDirectoryRequest(string requestedPath)
    {
        requestedPath = requestedPath.Trim();
        string _currentDirectory = CurrentDirectory;

        if (requestedPath == "^")
            _currentDirectory = Directory.GetParent(CurrentDirectory).FullName;
        else if (Directory.Exists(CurrentDirectory + '\\' + requestedPath) || File.Exists(CurrentDirectory + '\\' + requestedPath))
            _currentDirectory += '\\' + requestedPath;
        // attempt to see if the absolute path exists
        else if (Directory.Exists(requestedPath) || File.Exists(requestedPath))
            _currentDirectory = requestedPath;
        else
            return null;

        return _currentDirectory;
    }

    private (string, Color) ChangeDirectory(string[] path)
    {
        string stringPath = string.Join(' ', path);

        string? nextDirectory = ParseDirectoryRequest(stringPath);
        if (nextDirectory is null)
            return ($"Directory '{stringPath}' does not exist!", Color.Red);
       
        CurrentDirectory = nextDirectory;
        return ($"Changed current directory scope to '{stringPath}'.", Color.Green);   
    }

    private static (string, Color) CommandNotFoundError(string command)
    {
        return ($"Error! Command '{command}' not found!", Color.Red);
    }

    private static (string, Color) Echo(string[] message)
    {
        string output = "";
        foreach (string arg in message) 
            output += $"{arg} ";

        return (output, Color.MistyRose);
    }

    private (string, Color) Login(string password)
    {
        Password = password;
        return ("Login successful.", Color.Green);
    }
}
