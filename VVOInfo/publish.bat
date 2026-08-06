
@rem # For Raspberry Pi 64-bit OS (Highly recommended)
@rem dotnet publish -r linux-arm64 -c Release --self-contained

@rem # For Raspberry Pi 32-bit OS
@rem dotnet publish -r linux-arm -c Release --self-contained


@rem # For Raspberry Pi 64-Bit OS (recommended)
dotnet publish -r linux-arm64 -c Release --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

@rem # For Raspberry Pi 32-Bit OS
@rem dotnet publish -r linux-arm -c Release --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
