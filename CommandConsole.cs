using System.Text;
using System.Net.NetworkInformation;
using System.DirectoryServices;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using vault_thing;
using System.Diagnostics.Eventing.Reader;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using WMPLib;

namespace vault_thing;

public class CommandConsole()
{
    private const string VERSION = "a1.1.0";

    private string Password = "";
    public string CurrentDirectory = File.ReadAllText("defaultdir.txt");

    public string CurrentGlobName = "";
    private string CurrentGlobPath = "";
    private List<FileWrapper> CurrentGlobContents = new();

    private void PlayTimeCircuitsSFX()
    {
        WindowsMediaPlayer player = new()
        {
            URL = "time_circuits.mp3"
        };
        player.controls.play();
    }

    #region Compression
    // nicked a good deal of this
    public static void CopyTo(Stream src, Stream dest)
    {
        byte[] bytes = new byte[4096];

        int cnt;

        while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            dest.Write(bytes, 0, cnt);
    }

    public static byte[] Zip(byte[] bytes)
    {
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
            CopyTo(msi, gs);

        return mso.ToArray();
    }

    public static byte[] Unzip(byte[] bytes)
    {
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            CopyTo(gs, mso);

        return mso.ToArray();
    }

    private static byte[] Compress(byte[] data)
    {
        MemoryStream output = new();
        using (DeflateStream dStream = new(output, CompressionLevel.SmallestSize))
            dStream.Write(data, 0, data.Length);
        return output.ToArray();
    }

    private static byte[] Decompress(byte[] data)
    {
        MemoryStream input = new(data);
        MemoryStream output = new();
        using (DeflateStream dStream = new(input, CompressionMode.Decompress))
            dStream.CopyTo(output);
        return output.ToArray();
    }
    #endregion

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
                "globulate" => MoveFileToGlob(args[1], args[2..]),
                "extract" => ExtractFromGlob(args[1], args[2..]),
                "peek" => ViewGlobContents(args[1..]),
                "def" => SetDefaultDirectory(),
                "open" => OpenFile(args[1]),
                "ver" => (VERSION, Color.MistyRose),
                _ => CommandNotFoundError(args[0])
            };
        } catch (Exception ex) {
            return (ex.ToString(), Color.Red);
        }
    }

    // since the usual parsing for commands works by splitting on a space, we have to pass in an array of the rest of the arguments
    private static (string, Color) Echo(string[] message)
    {
        string output = string.Join(' ', message);
        return (output, Color.MistyRose);
    }

    // the password is used as an encryption seed for each .glob
    private (string, Color) Login(string password)
    {
        Password = password;
        PlayTimeCircuitsSFX();
        return ("Login successful.", Color.Green);
    }

    // since the usual parsing for commands works by splitting on a space, we have to pass in an array of the rest of the arguments
    private static (string, Color) HelpCommand(string[] command)
    {
        Dictionary<string, string> commands = new()
        {
            { "echo", " <message> - Repeats <message> in terminal." },
            { "login", " <password> - Sets current glob encryption key to <password>." },
            { "help", " <command = *> - Displays help on <command>, by default will display all commands." },
            { "cd", " <directory> - Changes current directory scope to <directory>. <directory = '^'> retrieves parent directory." },
            { "ls", " <type> - Lists all files / directories in current directory depending on the value of <type>. <type = -f | -d | -g> where -f is all files, -d is all directories and -g is all globs." },
            { "mk", " <type> <name> - Creates file of <type> with name <name>. <type = -f | -g> where -f is all files and -g is all globs." },
            { "globulate", " <file> <glob> - Moves <file> to <glob>. If <glob> is left blank, the currently selected glob is assumed." },
            { "extract", " <file> <glob> - Extracts <file> from <glob>. If <glob> is left blank, the currently selected glob is assumed." },
            { "peek", " <glob> - Views contents of <glob>, and sets it to the currently selected glob. If <glob> is left blank, the currently selected glob is assumed."},
            { "def", " - Sets current directory to the default." },
            { "open", " <file> - Opens <file> in default application." },
            { "ver", " - Displays current software version." }
        };

        // checks if user is requesting help for specific command
        string response = "\n";
        if (command.Length == 0)    // the length being zero indicates the user has inputted no argument, and should therefore display everything
        {
            foreach (KeyValuePair<string, string> kvPair in commands)
                response += $"\n> {kvPair.Key}{kvPair.Value}\n";

            return (response, Color.MistyRose);
        }

        try {
            response = commands[command[0]];
            return (command[0] + response, Color.MistyRose);
        } catch (KeyNotFoundException) {
            return CommandNotFoundError(command[0]);
        }
    }

    #region Glob
    // since the usual parsing for commands works by splitting on a space, we have to pass in an array of the rest of the arguments
    private (string, Color) ViewGlobContents(string[] glob)
    {
        string stringGlob;
        if (glob.Length > 0)    // length greater than zero indicates user has inputted a path for the .glob
        {
            stringGlob = glob[0];
            CurrentGlobPath = ParseDirectoryRequest(stringGlob);
        }
        else
            stringGlob = CurrentGlobName;

        if (File.Exists(CurrentGlobPath))
        {
            // read all the byte data of a file [File.ReadAllBytes()]
            // decompress the encrypted data [Unzip()]
            // decrypt [ByteCipher.XOR()]
            // and finally read the json [Encoding.ASCII.GetString()]
            // it's worth noting that each file's byte data is also compressed separately, but we don't need to do that here since we're only reading-
            // - the file name and size
            CurrentGlobName = stringGlob;
            string json = Encoding.ASCII.GetString(ByteCipher.XOR(Unzip(File.ReadAllBytes(CurrentGlobPath)), Password));

            // get all files in selected .glob file and read file name and size
            CurrentGlobContents = JsonConvert.DeserializeObject<List<FileWrapper>>(json);

            string contents = "\n\n";
            foreach (FileWrapper f in CurrentGlobContents)
                contents += "> " + f.FileName + $" ({f.Bytes.Length} B)" + '\n';

            PlayTimeCircuitsSFX();
            return (contents, Color.MistyRose);
        }
        
        return ($"Error! Glob '{stringGlob}' does not exist!", Color.Red);
    }
    
    // since the usual parsing for commands works by splitting on a space, we have to pass in an array of the rest of the arguments
    private (string, Color) ExtractFromGlob(string fileName, string[] glob)
    {
        string globPath;
        string globName;
        if (glob.Length > 0) // length greater than zero indicates user has inputted a path for the .glob
        {
            globPath = ParseDirectoryRequest(glob[0]);
            globName = glob[0];
        }   
        else
        {
            globPath = CurrentGlobPath;
            globName = CurrentGlobName;
        }
            
        if (File.Exists(globPath))
        {
            // refer to above method for reading file json
            string json = Encoding.ASCII.GetString(ByteCipher.XOR(Unzip(File.ReadAllBytes(CurrentGlobPath)), Password));

            // get all files in selected .glob file
            List<FileWrapper> files = JsonConvert.DeserializeObject<List<FileWrapper>>(json);

            // acquire file by file name
            FileWrapper file = files.FirstOrDefault(file => file.FileName == fileName);

            if (file is null)
                return ($"File '{fileName}' does not exist in glob '{glob}'!", Color.Red);

            // create a new file containing the decompressed byte data of file.Bytes and write
            File.WriteAllBytes(CurrentDirectory + '\\' + fileName, Decompress(file.Bytes));

            files.Remove(file);

            // get the byte data of the json serialized object [JsonConvert.SerializeObject()]
            // encrypt it [ByteCipher.XOR()]
            // compress it [Zip()]
            byte[] jsonBytes = Zip(ByteCipher.XOR(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(files, Formatting.Indented)), Password));

            File.WriteAllBytes(globPath, jsonBytes);

            return ($"Extracted '{file.FileName}' from glob '{globName}'.", Color.Green);
        }
        
        return ($"Error! Glob '{globName}' does not exist!", Color.Red);
    }

    // since the usual parsing for commands works by splitting on a space, we have to pass in an array of the rest of the arguments
    private (string, Color) MoveFileToGlob(string fileName, string[] glob)
    {
        
        string? filePath = ParseDirectoryRequest(fileName);

        string? globPath;
        string globName;
        if (glob.Length > 0) // length greater than zero indicates user has inputted a path for the .glob
        {
            globPath = ParseDirectoryRequest(glob[0]);
            globName = glob[0];
        }        
        else
        {
            globPath = CurrentGlobPath;
            globName = CurrentGlobName;
        }

        if (File.Exists(filePath) && File.Exists(globPath))
        {
            // refer to previous
            string json = Encoding.ASCII.GetString(ByteCipher.XOR(Unzip(File.ReadAllBytes(CurrentGlobPath)), Password));

            // get all files in selected .glob file
            List<FileWrapper> files = JsonConvert.DeserializeObject<List<FileWrapper>>(json);

            // create a new instance of the file with compressed byte data
            FileWrapper file = new(fileName, Compress(File.ReadAllBytes(filePath)));

            files.Add(file);

            // refer to previous
            byte[] jsonBytes = Zip(ByteCipher.XOR(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(files, Formatting.Indented)), Password));
            File.WriteAllBytes(globPath, jsonBytes);

            File.Delete(filePath);

            return ($"Moved file '{fileName}' to glob '{globName}'.", Color.Green);
        }
        else
            return ($"Error! Either file '{fileName}' or glob '{globName}' does not exist!", Color.Red);
    }
    #endregion

    #region Files
    // -f refers to files
    // -d refers to directories
    // -g refers to globs
    private (string, Color) OpenFile(string path)
    {
        string file = ParseDirectoryRequest(path);

        using Process p = new();

        p.StartInfo.FileName = "explorer";
        p.StartInfo.Arguments = "\"" + file + "\"";
        p.Start();

        return ($"Opened file '{file}'.", Color.Green);
    }

    private (string, Color) SetDefaultDirectory()
    {
        File.WriteAllText(Directory.GetCurrentDirectory() + '\\' + "defaultdir.txt", CurrentDirectory);
        return ("Set current directory to default.", Color.Green);
    }

    private (string, Color) MakeFile(string type, string name)
    {
        string filePath = CurrentDirectory + '\\' + name;

        try {
            if (type == "-f")
                File.Create(filePath).Dispose();
            else if (type == "-d")
                Directory.CreateDirectory(filePath);
            else if (type == "-g")
                File.WriteAllBytes(filePath + ".glob", Zip(ByteCipher.XOR(Encoding.ASCII.GetBytes("[]"), Password)));
                            
        } catch (Exception ex) {
            return ($"An error occurred whilst attempting to create file '{name}'. '{ex}'", Color.Red);
        }

        return ($"Created file '{name}'.", Color.Green);
    }

    private (string, Color) ListFiles(string[] type)
    {
        string stringAllFiles = "\n\n";
        DirectoryInfo dInfo = new(CurrentDirectory);

        if (type.Length == 0)
        {
            stringAllFiles += "Files:\n";

            FileInfo[] allFiles = dInfo.GetFiles();

            string allGlobs = "";

            foreach (FileInfo file in allFiles)
            {
                if (file.Name.EndsWith(".glob"))
                    allGlobs += "> " + file.Name + $" ({file.Length} B)" + '\n';
                else
                    stringAllFiles += "> " + file.Name + $" ({file.Length} B)" + '\n';
            }
                
            stringAllFiles += "\nDirectories:\n";

            DirectoryInfo[] allDirs = dInfo.GetDirectories();

            foreach (DirectoryInfo dir in allDirs)
                stringAllFiles += "> " + dir.Name + '\n';

            stringAllFiles += "\nGlobs:\n" + allGlobs;
            return (stringAllFiles, Color.MistyRose);
        }

        switch(type[0])
        {
            case "-d":
                DirectoryInfo[] allDirs = dInfo.GetDirectories();

                foreach (DirectoryInfo dir in allDirs)
                    stringAllFiles += "> "+ dir.Name + '\n';
                break;
            case "-f":
                FileInfo[] allFiles = dInfo.GetFiles();

                foreach (FileInfo file in allFiles)
                    stringAllFiles += "> " + file.Name + $" ({file.Length} B)" + '\n';
                break;
            case "-g":
                allFiles = dInfo.GetFiles();

                string allGlobs = "";

                foreach (FileInfo file in allFiles)
                    if (file.Name.EndsWith(".glob"))
                        allGlobs += "> " + file.Name + $" ({file.Length} B)" + '\n';

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

        // '^' indicates user is requesting to go to parent directory
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
        PlayTimeCircuitsSFX();
        return ($"Changed current directory scope to '{stringPath}'.", Color.Green);   
    }
    #endregion

    private static (string, Color) CommandNotFoundError(string command)
    {
        return ($"Error! Command '{command}' not found!", Color.Red);
    }
}
