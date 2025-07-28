{ pkgs ? import <nixpkgs> { } }:

with pkgs;
let

dotnet = dotnet-sdk; 

in mkShell {
  name = "avalonia-env";
  packages = (with pkgs; [
    dotnet
    avalonia
    fontconfig
    alsa-lib
    glew
    udev
    gnumake 
    vulkan-headers
    vulkan-loader
    vulkan-validation-layers
    vulkan-tools
    vulkan-tools-lunarg
    powershell
  ]) ++ (with pkgs.xorg; [
   libX11
    libICE
    libSM
    libXi
    libXcursor
    libXext
    libXrandr  ]);

    DOTNET_ROOT = "${dotnet}";
}
