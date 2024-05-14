﻿namespace vault_thing;

internal class FileWrapper(string fileName, byte[] bytes)
{
    public string FileName { get; } = fileName;
    public byte[] Bytes { get; } = bytes;

    public static bool FileOrDirectoryExists(string path) => Directory.Exists(path) || File.Exists(path);
}
