$protoFile = "StorageKeyRepresentation.proto"
$outputDir = "."
$fileExtension = ".pb.cs"

& protoc $protoFile --csharp_out=$outputDir --csharp_opt="file_extension=$fileExtension"
