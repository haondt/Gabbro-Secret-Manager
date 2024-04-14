#!/bin/bash

protoc StorageKeyRepresentation.proto \
    --csharp_out=. \
    --csharp_opt=file_extension=.pb.cs
