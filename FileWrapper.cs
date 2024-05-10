using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace vault_thing;

internal class FileWrapper(string fileName, byte[] bytes)
{
    public string FileName { get; } = fileName;
    public byte[] Bytes { get; } = bytes;
}
