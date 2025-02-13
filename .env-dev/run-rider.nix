{ pkgs ? import <nixpkgs> {} }:


(pkgs.buildFHSEnv {
  name = "rider-env";
  targetPkgs = pkgs: (with pkgs; [
    dotnet-sdk
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

  multiPkgs = pkgs: (with pkgs; [
   udev
   alsa-lib
  ]);

  runScript = "nohup rider &";
}).env
