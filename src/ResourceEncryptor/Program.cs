using EncryptTools;

string input = args[0];
string intermediate = args[1];
string output = args[2];

string encryptionKey = args[3];
string encryptionIv = args[4];

PackageEncryptor.EncryptResources(input, intermediate, output, ref encryptionKey, ref encryptionIv);
