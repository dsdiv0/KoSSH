using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.Security.Cryptography;
/*
 THIS ONE IS WITHOUT CHECKING THE hash of the file to determine if its duplicate or not.
class KindleSSH
{
  public static void UploadToK()
  {
    HashSet<string> presentFiles = [];
    SftpClient client = new("192.168.29.194", 8022, "u0_a197", "1811"); //the params are ip add, port, username, password.
    client.Connect();
    string bookpath = "/home/cronyx/Downloads/The Horus Heresy"; // path to the files on the main machine.
    int counter = 0;
    foreach (ISftpFile file in client.ListDirectory("/data/data/com.termux/files/home/storage/shared/TWRP"))
    {
      string fname = Path.GetFileName(file.FullName);
      presentFiles.Add(fname);
      Console.WriteLine($"Found Files{fname}");
    }
    foreach (string file in Directory.GetFiles(bookpath, "*.epub", SearchOption.AllDirectories)) // loops through each file path as the GetFiles method returns a string[] which contains all the filepaths.
    {
      string filename = Path.GetFileName(file); // grabbing one file at a time.
      if (!presentFiles.Contains(filename))
      {
        FileStream fs = File.OpenRead(file); // this is always gonna run to load the raw data in the stream so that it can then send the raw data over the ssh (needs citations)
        client.UploadFile(fs, Path.Combine("/data/data/com.termux/files/home/storage/shared/TWRP", filename));
        counter++;
        // uploads the files to the target device , the file name needs to be passed after the path or else it will never upload.
        Console.WriteLine($"Uploaded File {file}, files uploaded total {counter}");
      }
    }
    client.Disconnect();
    Console.WriteLine($"Uploaded Files {counter}");
  }
  public static void Main()
  {
    UploadToK();
  }
}
*/

//Using SHA256 to determine if duplicates are present or not.
class KindleSSH
{
  public static void UploadToK()
  {
    HashSet<string> presentFiles = [];

    SftpClient client = new("yourIpHere", 0 /*add port here */, "username of the device you're connecting to", "password"); //the params are ip add, port, username, password.
    client.Connect();

    string bookpath = "/home/user/...."; // path to the files on the main machine.
    int counter = 0;
    foreach (ISftpFile file in client.ListDirectory("/mnt/us/....")) // path to the files on target machine.
    {
      if (file.IsRegularFile) // check to see if the file is a file indeed (lol).
      {
        // computing the SHA256 hash in order to make sure no duplicates exist or files is not overwitten.
        SHA256 sha = SHA256.Create();
        string fname = Path.GetFileName(file.FullName);
        SftpFileStream sfs = client.OpenRead(file.FullName);
        byte[] hash = sha.ComputeHash(sfs);
        string hashcode = Convert.ToHexStringLower(hash);
        presentFiles.Add(hashcode); // adds files to the HashSet in order to use it later to check and prevent duplicate files or overwriting the files.
        Console.WriteLine($"Found Files {fname}");
        sfs.Close();
        counter++;
      }
    }
    Console.WriteLine($"Total files found {counter}");
    counter = 0;
    foreach (string file in Directory.GetFiles(bookpath, "*.epub", SearchOption.AllDirectories)) // searches for files with extension .epub in every subdirectory in the target directory.
    {
      string filename = Path.GetFileName(file);
      FileStream fs = File.OpenRead(file); // grabbing one file at a time.

      string dupli = Convert.ToHexStringLower(SHA256.Create().ComputeHash(fs));
      fs.Position = 0;
      if (!presentFiles.Contains(dupli)) // checks if the files already exist, if they dont, it uploads the files.
      {
        // this is always gonna run to load the raw data in the stream so that it can then send the raw data over the ssh (needs citations)
        client.UploadFile(fs, Path.Combine("/mnt/us/..", filename)); // add the target path here.
        counter++;

        // uploads the files to the target device, the file name needs to be passed after the path or else it will never upload.
        Console.WriteLine($"Uploaded File {file}");
      }
      fs.Flush();
      fs.Close();
    }

    client.Disconnect();
    Console.WriteLine($"Uploaded Files {counter}");
  }

  public static void Main()
  {
    UploadToK();
  }
}

