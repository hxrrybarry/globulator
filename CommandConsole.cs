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
using Newtonsoft.Json.Linq;
using static System.Formats.Asn1.AsnWriter;
using System.Linq.Expressions;

namespace vault_thing;

public class CommandConsole()
{
    private const string VERSION = "b1.0.0";

    static JObject json = JObject.Parse(File.ReadAllText("config.json"));

    private string Password = "";
    public string CurrentDirectory = json.Value<string>("defaultdir");
    public string[] AllFilesInCurrentPath = Directory.GetFiles(json.Value<string>("defaultdir"));

    public bool isMuted = json.Value<bool>("mute");

    public string CurrentGlobName = "";
    private string CurrentGlobPath = "";
    private List<FileWrapper> CurrentGlobContents = new();

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

    public (string, Color) ProcessCommand(string command, (string, string) fileDetails)
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
                "dl" => DeleteFile(args[1..]),
                "globulate" => MoveFileToGlob(args[1], args[2], args[3..]),
                "extract" => ExtractFromGlob(args[1], args[2], args[3..]),
                "peek" => ViewGlobContents(args[1..]),
                "def" => SetDefaultDirectory(),
                "open" => OpenFile(args[1]),
                "ver" => (VERSION, Color.MistyRose),
                "sl" => SelectFile(args[1..]),
                "commit" => WriteToSelectedFile(fileDetails),
                "togglemute" => ToggleMute(),
                _ => CommandNotFoundError(args[0])
            };
        } catch (IndexOutOfRangeException) {
            return ("There was an error parsing command arguments.", Color.Red);
        }
    }

    // since the usual parsing for commands works by splitting on a space, we have to pass in an array of the rest of the arguments
    private static (string, Color) Echo(string[] message)
    {
        string output = string.Join(' ', message);
        return (output, Color.MistyRose);
    }

    private (string, Color) ToggleMute()
    {
        isMuted ^= true;

        Dictionary<string, dynamic> newConfig = new()
        {
            { "defaultdir", CurrentDirectory },
            { "mute", isMuted }
        };

        string json = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
        File.WriteAllText("config.json", json);

        return ($"Mute is now set to '{isMuted}'.", Color.MistyRose);
    }

    // the password is used as an encryption seed for each .glob
    private (string, Color) Login(string password)
    {
        Password = password;
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
            { "ls", " <type> - Lists all files / directories in current directory depending on the value of <type>. <type = -f | -d | -g> where -f is all files, -d is all directories and -g is all globs. Use F11 and F12 to scroll through each file." },
            { "mk", " <type> <name> - Creates file of <type> with name <name>. <type = -f | -g> where -f is for a file and -g is for a glob." },
            { "dl", " <file> - Deletes file at <file>." },
            { "globulate", " <file> <copy? = -m | -c> <glob> - Moves <file> to <glob>. <copy = -c> indicates to copy the file, and if <glob> is left blank, the currently selected glob is assumed." },
            { "extract", " <file ?= *> <copy? = -m | -c> <glob> - Extracts <file> from <glob>. <copy = -c> indicates to copy the file, and if <glob> is left blank, the currently selected glob is assumed. <file = *> indicates all files should be extracted." },
            { "peek", " <glob> - Views contents of <glob>, and sets it to the currently selected glob. If <glob> is left blank, the currently selected glob is assumed."},
            { "def", " - Sets current directory to the default." },
            { "open", " <file> - Opens <file> in default application." },
            { "ver", " - Displays current software version." },
            { "sl", " <file> - Selects <file> for viewing / editing on the side. Use F11 and F12 to scroll through each file." },
            { "commit", " - Writes changes from selected file." },
            { "togglemute", " - Toggles mute." }
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
            try {
                CurrentGlobContents = JsonConvert.DeserializeObject<List<FileWrapper>>(json);
            } catch (JsonReaderException) {
                return ("There was an error deserializing glob contents. This very likely means the password provided was incorrect.", Color.Red);
            }

            string contents = "\n\n";
            foreach (FileWrapper f in CurrentGlobContents)
                contents += "> " + f.FileName + $" ({f.Bytes.Length} B)" + '\n';

            return (contents, Color.MistyRose);
        }
        
        return ($"Error! Glob '{stringGlob}' does not exist!", Color.Red);
    }
    
    private void ExtractSingleFile(FileWrapper file, string isCopy, string globPath)
    {
        // create a new file containing the decompressed byte data of file.Bytes and write
        File.WriteAllBytes(CurrentDirectory + '\\' + file.FileName, Decompress(file.Bytes));

        if (isCopy == "-m")
        {
            CurrentGlobContents.Remove(file);
            // get the byte data of the json serialized object [JsonConvert.SerializeObject()]
            // encrypt it [ByteCipher.XOR()]
            // compress it [Zip()]
            byte[] jsonBytes = Zip(ByteCipher.XOR(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(CurrentGlobContents, Formatting.Indented)), Password));
            File.WriteAllBytes(globPath, jsonBytes);
        }    
    }

    // since the usual parsing for commands works by splitting on a space, we have to pass in an array of the rest of the arguments
    private (string, Color) ExtractFromGlob(string fileName, string isCopy, string[] glob)
    {
        if (!(isCopy == "-c" || isCopy == "-m"))
            return ($"Argument '{isCopy}' is invalid!", Color.Red);

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

            try {
                // get all files in selected .glob file
                CurrentGlobContents = JsonConvert.DeserializeObject<List<FileWrapper>>(json);
            } catch (JsonReaderException) {
                return ("There was an error deserializing glob contents. This very likely means the password provided was incorrect.", Color.Red);
            }

            if (fileName != "*")
            {
                // acquire file by file name
                FileWrapper file = CurrentGlobContents.FirstOrDefault(file => file.FileName == fileName);

                if (file is null)
                    return ($"File '{fileName}' does not exist in glob '{glob}'!", Color.Red);

                ExtractSingleFile(file, isCopy, globPath);
            }
            
            else
            {
                // we have to define an amount here and do a manual loop because CurrentGlobContents.Count may change as files are extracted
                if (isCopy == "-m")
                {
                    int count = CurrentGlobContents.Count;
                    for (int _ = 0; _ < count; _++)
                        ExtractSingleFile(CurrentGlobContents[0], isCopy, globPath);
                }
                                    
                else
                    foreach (FileWrapper file in CurrentGlobContents)
                        ExtractSingleFile(file, isCopy, globPath);
            }
                
            return ($"Extracted '{fileName}' from glob '{globName}'.", Color.Green);
        }
        
        return ($"Error! Glob '{globName}' does not exist!", Color.Red);
    }

    // since the usual parsing for commands works by splitting on a space, we have to pass in an array of the rest of the arguments
    private (string, Color) MoveFileToGlob(string fileName, string isCopy, string[] glob)
    {
        if (!(isCopy == "-c" || isCopy == "-m"))
            return ($"Argument '{isCopy}' is invalid!", Color.Red);

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

            List<FileWrapper> files;
            try {
                // get all files in selected .glob file
                files = JsonConvert.DeserializeObject<List<FileWrapper>>(json);
            } catch (JsonReaderException) {
                return ("There was an error deserializing glob contents. This very likely means the password provided was incorrect.", Color.Red);
            }

            // create a new instance of the file with compressed byte data
            FileWrapper file = new(fileName, Compress(File.ReadAllBytes(filePath)));

            files.Add(file);

            // refer to previous
            byte[] jsonBytes = Zip(ByteCipher.XOR(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(files, Formatting.Indented)), Password));
            File.WriteAllBytes(globPath, jsonBytes);

            if (isCopy == "-m")
                File.Delete(filePath);

            return ($"Moved file '{fileName}' to glob '{globName}'.", Color.Green);
        }
        else
            return ($"Error! Either file '{fileName}' or glob '{globName}' does not exist!", Color.Red);
    }
    #endregion

    #region Files
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
        Dictionary<string, dynamic> newConfig = new()
        {
            { "defaultdir", CurrentDirectory },
            { "mute", isMuted }
        };

        string json = JsonConvert.SerializeObject(newConfig, Formatting.Indented);

        File.WriteAllText("config.json", json);
        return ("Set current directory to default.", Color.Green);
    }

    // -f refers to files
    // -d refers to directories
    // -g refers to globs
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
                if (file.Name.EndsWith("glob"))
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
                    if (file.Name.EndsWith("glob"))
                        allGlobs += "> " + file.Name + $" ({file.Length} B)" + '\n';

                stringAllFiles += allGlobs;
                break;
            default:
                return ($"Invalid argument '{type[0]}'.", Color.Red);
        };

        return (stringAllFiles, Color.MistyRose);
    }

    private (string, Color) SelectFile(string[] path)
    {
        string? stringPath = ParseDirectoryRequest(string.Join(' ', path));

        if (stringPath is null)
            return ($"File '{string.Join(' ', path)}' does not exist!", Color.Red);
        
        if (stringPath.EndsWith("png") || stringPath.EndsWith("jpg") || stringPath.EndsWith("jpeg") || stringPath.EndsWith("bmp") || stringPath.EndsWith("gif"))
            Main.SetPictureBoxImage(stringPath);
        else
            Main.SetViewingBoxText(File.ReadAllText(stringPath), stringPath);

        return ($"Currently viewing '{stringPath}'.", Color.MistyRose);
    }

    private static (string, Color) WriteToSelectedFile((string, string) fileDetails)
    {
        if (fileDetails.Item2.EndsWith("png") || fileDetails.Item2.EndsWith("jpg") || fileDetails.Item2.EndsWith("jpeg") || fileDetails.Item2.EndsWith("bmp") || fileDetails.Item2.EndsWith("gif") || fileDetails.Item2.EndsWith("glob"))
            return ("Selected file is not a text file!", Color.Red);

        File.WriteAllText(fileDetails.Item2, fileDetails.Item1);
        return ($"Wrote to file '{fileDetails.Item2}'.", Color.Green);
    }

    private (string, Color) DeleteFile(string[] path)
    {
        string? stringPath = ParseDirectoryRequest(string.Join(' ', path));

        if (stringPath is null)
            return ($"File '{string.Join(' ', path)}' does not exist.", Color.Red);

        File.Delete(stringPath);

        return ($"Deleted file '{stringPath}'.", Color.Green);
    }

    private string? ParseDirectoryRequest(string requestedPath)
    {
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
        return ($"Changed current directory scope to '{stringPath}'.", Color.Green);   
    }
    #endregion

    private static (string, Color) CommandNotFoundError(string command)
    {    
        return ($"Error! Command '{command}' not found!", Color.Red);
    }
}
